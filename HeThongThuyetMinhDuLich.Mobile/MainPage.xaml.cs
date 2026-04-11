using HeThongThuyetMinhDuLich.Mobile.Models;
using HeThongThuyetMinhDuLich.Mobile.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System.Net;
using MauiMap = Microsoft.Maui.Controls.Maps.Map;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class MainPage : ContentPage
{
    private static readonly TimeSpan GeofenceCooldown = TimeSpan.FromMinutes(2);
    private const double DefaultPoiRadiusKm = 0.6;
    private const double DefaultUserRadiusKm = 1;
    private const double MinimumViewportDegrees = 0.01;

    private readonly MobileApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly ObservableCollection<DiemThamQuanItem> _diemThamQuan = [];
    private readonly ObservableCollection<NoiDungItem> _noiDung = [];
    private readonly ObservableCollection<NgonNguItem> _ngonNguItems = [];
    private readonly List<NoiDungItem> _allNoiDung = [];
    private readonly Dictionary<int, NgonNguItem> _ngonNguMap = [];
    private readonly Dictionary<int, DateTime> _lastAutoTriggerUtcByPoi = [];

    private MauiMap? _mapView;
    private IDispatcherTimer? _gpsTimer;
    private IDispatcherTimer? _syncTimer;
    private Location? _currentLocation;
    private DiemThamQuanItem? _nearestPoi;
    private bool _isRefreshingGps;
    private bool _isSyncing;
    private bool _isConnectivitySubscribed;
    private bool _isHandlingPoiSelection;
    private bool _isUpdatingLanguageSelection;
    private int _selectedDisplayMaNgonNgu;
    private int _selectedAudioMaNgonNgu;

    public MainPage(MobileApiClient apiClient, AuthSession authSession)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _authSession = authSession;
        PoiCollection.ItemsSource = _diemThamQuan;
        ContentCollection.ItemsSource = _noiDung;
        DisplayLanguagePicker.ItemsSource = _ngonNguItems;
        AudioLanguagePicker.ItemsSource = _ngonNguItems;
        InitializeMap();
        ConfigureToolbar();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ConfigureToolbar();

        EnsureConnectivitySubscription();

        try
        {
            await LoadNgonNguAsync();
            await LoadDiemThamQuanAsync();
            await SyncOfflineStateWithRetriesAsync();
            await RefreshGpsAsync(requestPermissionIfNeeded: true, showErrors: false);
            StartGpsTimer();
            StartSyncTimer();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Loi", $"Khoi dong that bai: {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopGpsTimer();

        if (_syncTimer is not null)
        {
            _syncTimer.Stop();
            _syncTimer = null;
        }

        if (_isConnectivitySubscribed)
        {
            Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
            _isConnectivitySubscribed = false;
        }
    }

    private void EnsureConnectivitySubscription()
    {
        if (_isConnectivitySubscribed)
        {
            return;
        }

        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        _isConnectivitySubscribed = true;
    }

    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet || _isSyncing)
        {
            return;
        }

        try
        {
            _isSyncing = true;
            await SyncOfflineStateWithRetriesAsync();
        }
        catch
        {
            // ignore reconnect sync failures
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void InitializeMap()
    {
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            _mapView = null;
            GpsStatusLabel.Text = "Vi tri: map chi ho tro Android/iOS. Ban dang chay tren Windows.";
            return;
        }

        try
        {
            _mapView = new MauiMap { IsShowingUser = true };
            MapHost.Children.Clear();
            MapHost.Children.Add(_mapView);
            UpdateMapViewport();
        }
        catch
        {
            _mapView = null;
            GpsStatusLabel.Text = "Vi tri: map khong ho tro tren thiet bi nay.";
        }
    }

    private async Task LoadNgonNguAsync()
    {
        var items = await _apiClient.GetNgonNguAsync();
        _ngonNguItems.Clear();
        _ngonNguMap.Clear();

        _ngonNguItems.Add(new NgonNguItem
        {
            MaNgonNgu = 0,
            MaNgonNguQuocTe = "",
            TenNgonNgu = "Tat ca ngon ngu"
        });

        foreach (var item in items)
        {
            _ngonNguItems.Add(item);
            _ngonNguMap[item.MaNgonNgu] = item;
        }

        var defaultLang = items.FirstOrDefault(x => x.LaMacDinh) ?? items.FirstOrDefault();
        SetUnifiedLanguageSelection(defaultLang?.MaNgonNgu ?? 0);
        UpdateLanguageStateLabel();
    }

    private void StartSyncTimer()
    {
        if (_syncTimer is not null)
        {
            return;
        }

        _syncTimer = Dispatcher.CreateTimer();
        _syncTimer.Interval = TimeSpan.FromMinutes(10);
        _syncTimer.Tick += async (_, _) =>
        {
            if (_isSyncing)
            {
                return;
            }

            _isSyncing = true;
            try
            {
                await SyncOfflineStateWithRetriesAsync();
                var items = await _apiClient.GetDiemThamQuanAsync();
                if (items.Count != _diemThamQuan.Count ||
                    !_diemThamQuan.Select(x => (x.MaDiem, x.NgayCapNhat)).SequenceEqual(items.Select(x => (x.MaDiem, x.NgayCapNhat))))
                {
                    _diemThamQuan.Clear();
                    foreach (var item in items)
                    {
                        _diemThamQuan.Add(item);
                    }

                    UpdateNearestPoi();
                    RenderMapPins();
                    UpdateMapViewport();
                }
            }
            catch
            {
                // ignore sync failures
            }
            finally
            {
                _isSyncing = false;
            }
        };

        _syncTimer.Start();
    }

    private async Task SyncOfflineStateWithRetriesAsync()
    {
        var poiIds = _diemThamQuan.Select(x => x.MaDiem).ToList();
        var retryDelays = new[]
        {
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(5)
        };

        foreach (var delay in retryDelays)
        {
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }

            try
            {
                await _apiClient.SyncOfflineStateAsync(poiIds);
                return;
            }
            catch when (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return;
            }
            catch
            {
                // retry until delays are exhausted
            }
        }
    }

    private void StartGpsTimer()
    {
        if (_gpsTimer is not null)
        {
            return;
        }

        _gpsTimer = Dispatcher.CreateTimer();
        _gpsTimer.Interval = TimeSpan.FromSeconds(10);
        _gpsTimer.Tick += async (_, _) => await RefreshGpsAsync(requestPermissionIfNeeded: false, showErrors: false);
        _gpsTimer.Start();
    }

    private void StopGpsTimer()
    {
        if (_gpsTimer is null)
        {
            return;
        }

        _gpsTimer.Stop();
        _gpsTimer = null;
    }

    private async Task LoadDiemThamQuanAsync()
    {
        try
        {
            SetLoading(true);
            _diemThamQuan.Clear();

            var items = await _apiClient.GetDiemThamQuanAsync();
            foreach (var item in items)
            {
                _diemThamQuan.Add(item);
            }

            UpdateNearestPoi();
            RenderMapPins();
            UpdateMapViewport();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Loi", $"Khong tai duoc danh sach diem: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task RefreshGpsAsync(bool requestPermissionIfNeeded = true, bool showErrors = true)
    {
        if (_isRefreshingGps)
        {
            return;
        }

        _isRefreshingGps = true;

        try
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permission != PermissionStatus.Granted && requestPermissionIfNeeded)
            {
                permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (permission != PermissionStatus.Granted)
            {
                GpsStatusLabel.Text = "Vi tri: ban chua cap quyen GPS.";
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request) ?? await Geolocation.Default.GetLastKnownLocationAsync();
            if (location is null)
            {
                GpsStatusLabel.Text = "Vi tri: khong lay duoc toa do hien tai.";
                return;
            }

            _currentLocation = location;
            GpsStatusLabel.Text = $"Vi tri: {location.Latitude:F6}, {location.Longitude:F6}";
            UpdateNearestPoi();
            RenderMapPins();
            UpdateMapViewport();
            await CheckAndTriggerGeofenceAsync();
        }
        catch (Exception ex)
        {
            if (showErrors)
            {
                await ShowAlertAsync("Loi GPS", $"Khong cap nhat duoc vi tri: {ex.Message}", "OK");
            }
            else
            {
                GpsStatusLabel.Text = $"Vi tri: cap nhat that bai ({ex.Message})";
            }
        }
        finally
        {
            _isRefreshingGps = false;
        }
    }

    private async Task LoadNoiDungAsync(int maDiem)
    {
        try
        {
            SetLoading(true);
            _allNoiDung.Clear();

            var items = await _apiClient.GetNoiDungByDiemAsync(maDiem);
            _allNoiDung.AddRange(items);
            await EnsureLanguageSupportedForCurrentPoiAsync();
            ApplyLanguageFilter();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Loi", $"Khong tai duoc noi dung: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task SelectPoiAsync(DiemThamQuanItem poi, bool autoPlay, string triggerType, bool focusMap)
    {
        if (_isHandlingPoiSelection)
        {
            return;
        }

        _isHandlingPoiSelection = true;

        try
        {
        SelectedPoiLabel.Text = $"Da chon: {poi.TenDiem} ({poi.MaDinhDanh})";

        if (focusMap)
        {
            FocusMapOnPoi(poi);
        }

        if (!ReferenceEquals(PoiCollection.SelectedItem, poi))
        {
            PoiCollection.SelectedItem = poi;
        }

        await LoadNoiDungAsync(poi.MaDiem);

        if (!autoPlay)
        {
            return;
        }

        var preferredContent = ResolvePreferredContent();
        if (preferredContent is null)
        {
            await ShowAlertAsync("Thong bao", "Diem nay chua co noi dung de phat.", "OK");
            return;
        }

        await PlayNoiDungAsync(preferredContent, triggerType);
        }
        finally
        {
            _isHandlingPoiSelection = false;
        }
    }

    private void ApplyLanguageFilter()
    {
        _noiDung.Clear();

        var filtered = _selectedDisplayMaNgonNgu == 0
            ? _allNoiDung
            : _allNoiDung.Where(x => x.MaNgonNgu == _selectedDisplayMaNgonNgu).ToList();

        foreach (var item in filtered)
        {
            _noiDung.Add(item);
        }

        UpdateLanguageStateLabel();
    }

    private async Task CheckAndTriggerGeofenceAsync()
    {
        if (_currentLocation is null || _nearestPoi is null)
        {
            return;
        }

        var distanceKm = Location.CalculateDistance(
            _currentLocation,
            new Location((double)_nearestPoi.ViDo, (double)_nearestPoi.KinhDo),
            DistanceUnits.Kilometers);

        var distanceMeters = distanceKm * 1000;
        if (distanceMeters > (double)_nearestPoi.BanKinhKichHoat)
        {
            return;
        }

        if (_lastAutoTriggerUtcByPoi.TryGetValue(_nearestPoi.MaDiem, out var lastTriggerUtc) &&
            DateTime.UtcNow - lastTriggerUtc < GeofenceCooldown)
        {
            return;
        }

        var contents = await _apiClient.GetNoiDungByDiemAsync(_nearestPoi.MaDiem);
        _allNoiDung.Clear();
        _allNoiDung.AddRange(contents);
        await EnsureLanguageSupportedForCurrentPoiAsync();
        ApplyLanguageFilter();

        var autoItem = ResolvePreferredContent();
        if (autoItem is null)
        {
            return;
        }

        _lastAutoTriggerUtcByPoi[_nearestPoi.MaDiem] = DateTime.UtcNow;
        SelectedPoiLabel.Text = $"Auto geofence: {_nearestPoi.TenDiem}";
        await PlayNoiDungAsync(autoItem, "gps");
    }

    private NoiDungItem? ResolvePreferredContent()
    {
        return _selectedAudioMaNgonNgu == 0
            ? _allNoiDung.FirstOrDefault()
            : _allNoiDung.FirstOrDefault(x => x.MaNgonNgu == _selectedAudioMaNgonNgu) ?? _allNoiDung.FirstOrDefault();
    }

    private NoiDungItem ResolvePlaybackContent(NoiDungItem requestedItem)
    {
        if (_selectedAudioMaNgonNgu == 0 || requestedItem.MaNgonNgu == _selectedAudioMaNgonNgu)
        {
            return requestedItem;
        }

        return _allNoiDung.FirstOrDefault(x => x.MaDiem == requestedItem.MaDiem && x.MaNgonNgu == _selectedAudioMaNgonNgu)
            ?? requestedItem;
    }

    private void SetUnifiedLanguageSelection(int maNgonNgu)
    {
        _selectedDisplayMaNgonNgu = maNgonNgu;
        _selectedAudioMaNgonNgu = maNgonNgu;

        var selectedItem = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == maNgonNgu)
                           ?? _ngonNguItems.FirstOrDefault();

        _isUpdatingLanguageSelection = true;
        try
        {
            DisplayLanguagePicker.SelectedItem = selectedItem;
            AudioLanguagePicker.SelectedItem = selectedItem;
        }
        finally
        {
            _isUpdatingLanguageSelection = false;
        }
    }

    private async Task EnsureLanguageSupportedForCurrentPoiAsync()
    {
        if (_allNoiDung.Count == 0 || _selectedDisplayMaNgonNgu == 0)
        {
            return;
        }

        if (_allNoiDung.Any(x => x.MaNgonNgu == _selectedDisplayMaNgonNgu))
        {
            return;
        }

        var availableLanguages = _allNoiDung
            .Select(x => x.MaNgonNgu)
            .Distinct()
            .Select(id => _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == id) ?? new NgonNguItem
            {
                MaNgonNgu = id,
                TenNgonNgu = $"Ngon ngu {id}"
            })
            .ToList();

        if (availableLanguages.Count == 0)
        {
            return;
        }

        var choices = availableLanguages.Select(x => x.TenNgonNgu).ToArray();
        var selectedName = await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.DisplayActionSheetAsync(
                "POI nay khong ho tro ngon ngu da chon. Hay chon ngon ngu khac de xem va nghe.",
                "Huy",
                null,
                choices));

        if (string.IsNullOrWhiteSpace(selectedName) || selectedName == "Huy")
        {
            SetUnifiedLanguageSelection(0);
            return;
        }

        var selectedLanguage = availableLanguages.FirstOrDefault(x => x.TenNgonNgu == selectedName);
        if (selectedLanguage is null)
        {
            SetUnifiedLanguageSelection(0);
            return;
        }

        SetUnifiedLanguageSelection(selectedLanguage.MaNgonNgu);
    }

    private async Task PlayNoiDungAsync(NoiDungItem item, string triggerType)
    {
        var startedAt = DateTime.UtcNow;
        var played = false;
        string? audioError = null;

        try
        {
            var audioUrl = await _apiClient.GetPlayableAudioSourceAsync(item);
            if (!string.IsNullOrWhiteSpace(audioUrl))
            {
                try
                {
                    PlayAudioInsideApp(audioUrl);
                    played = true;
                }
                catch (Exception ex)
                {
                    audioError = ex.Message;
                    played = false;
                }
            }

            if (!played && item.ChoPhepTTS && !string.IsNullOrWhiteSpace(item.NoiDungVanBan))
            {
                AudioPlayerCard.IsVisible = false;
                await SpeakTextAsync(item);
                played = true;
            }

        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Loi phat noi dung", ex.Message, "OK");
            played = false;
        }

        if (!played)
        {
            var detail = string.IsNullOrWhiteSpace(audioError) ? string.Empty : $"\nChi tiet audio: {audioError}";
            await ShowAlertAsync("Thong bao", $"Khong the phat audio cho noi dung nay.{detail}", "OK");
            return;
        }

        try
        {
            await _apiClient.CreateLichSuPhatAsync(new LichSuPhatCreateRequest
            {
                MaNguoiDung = _authSession.MaNguoiDung,
                MaDiem = item.MaDiem,
                MaNoiDung = item.MaNoiDung,
                CachKichHoat = triggerType,
                ThoiGianBatDau = startedAt,
                ThoiLuongDaNghe = item.ThoiLuongGiay
            });
        }
        catch
        {
            // ignore post history errors
        }
    }

    private void PlayAudioInsideApp(string audioUrl)
    {
        var safeUrl = WebUtility.HtmlEncode(audioUrl);
        AudioPlayerCard.IsVisible = true;
        AudioWebView.Source = new HtmlWebViewSource
        {
            Html = $"""
                   <html><body style='margin:0;padding:0;background:#fff;'>
                   <audio controls autoplay style='width:100%;height:42px;'>
                     <source src="{safeUrl}" type="audio/mpeg" />
                   </audio>
                   </body></html>
                   """
        };
    }

    private async Task HandleQrValueAsync(string qrValue)
    {
        var value = qrValue.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            await ShowAlertAsync("Thong bao", "Nhap hoac scan ma QR truoc.", "OK");
            return;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUrl))
        {
            var normalizedUrl = _apiClient.ResolveAudioUrl(absoluteUrl.ToString());
            if (!string.IsNullOrWhiteSpace(normalizedUrl))
            {
                SelectedPoiLabel.Text = "QR -> Dang phat audio truc tiep";
                PlayAudioInsideApp(normalizedUrl);
                return;
            }
        }

        try
        {
            SetLoading(true);
            var result = await _apiClient.LookupQrAsync(value);
            if (result?.DiemThamQuan is null)
            {
                await ShowAlertAsync("Khong tim thay", "Ma QR khong hop le hoac khong ton tai.", "OK");
                return;
            }

            SelectedPoiLabel.Text = $"QR -> {result.DiemThamQuan.TenDiem}";
            FocusMapOnPoi(result.DiemThamQuan);

            _allNoiDung.Clear();
            _allNoiDung.AddRange(result.NoiDung);
            await EnsureLanguageSupportedForCurrentPoiAsync();
            ApplyLanguageFilter();

            var firstContent = ResolvePreferredContent();
            if (firstContent is not null)
            {
                await PlayNoiDungAsync(firstContent, "qr");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Loi", $"Khong tra cuu duoc QR: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void RenderMapPins()
    {
        if (_mapView is null)
        {
            return;
        }

        _mapView.Pins.Clear();
        foreach (var poi in _diemThamQuan)
        {
            if (!TryCreatePoiLocation(poi, out var poiLocation))
            {
                continue;
            }

            var isNearest = _nearestPoi is not null && _nearestPoi.MaDiem == poi.MaDiem;
            var pin = new Pin
            {
                Label = isNearest ? $"Gan nhat: {poi.TenDiem}" : poi.TenDiem,
                Address = poi.DiaChi ?? string.Empty,
                Location = poiLocation,
                Type = PinType.Place
            };

            pin.MarkerClicked += async (_, e) =>
            {
                try
                {
                    e.HideInfoWindow = false;
                    await SelectPoiAsync(poi, autoPlay: true, triggerType: "manual", focusMap: false);
                }
                catch (Exception ex)
                {
                    await ShowAlertAsync("Loi map", ex.Message, "OK");
                }
            };

            _mapView.Pins.Add(pin);
        }
    }

    private void UpdateNearestPoi()
    {
        if (_currentLocation is null || _diemThamQuan.Count == 0)
        {
            _nearestPoi = null;
            NearestPoiLabel.Text = "POI gan nhat: chua xac dinh.";
            return;
        }

        _nearestPoi = _diemThamQuan
            .Where(p => TryCreatePoiLocation(p, out _))
            .OrderBy(p => Location.CalculateDistance(
                _currentLocation,
                new Location((double)p.ViDo, (double)p.KinhDo),
                DistanceUnits.Kilometers))
            .FirstOrDefault();

        if (_nearestPoi is null)
        {
            NearestPoiLabel.Text = "POI gan nhat: chua xac dinh.";
            return;
        }

        var distanceKm = Location.CalculateDistance(
            _currentLocation,
            new Location((double)_nearestPoi.ViDo, (double)_nearestPoi.KinhDo),
            DistanceUnits.Kilometers);

        NearestPoiLabel.Text = $"POI gan nhat: {_nearestPoi.TenDiem} (~{distanceKm * 1000:F0} m)";
    }

    private async void OnReloadClicked(object? sender, EventArgs e)
    {
        await LoadDiemThamQuanAsync();
    }

    private async void OnRefreshGpsClicked(object? sender, EventArgs e)
    {
        await RefreshGpsAsync(requestPermissionIfNeeded: true, showErrors: true);
    }

    private async void OnPoiSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DiemThamQuanItem poi)
        {
            return;
        }

        try
        {
            await SelectPoiAsync(poi, autoPlay: false, triggerType: "manual", focusMap: true);
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Loi chon diem", ex.Message, "OK");
        }
    }

    private void FocusMapOnPoi(DiemThamQuanItem poi)
    {
        if (_mapView is null || !TryCreatePoiLocation(poi, out var poiLocation))
        {
            return;
        }

        _mapView.MoveToRegion(MapSpan.FromCenterAndRadius(
            poiLocation,
            Distance.FromKilometers(DefaultPoiRadiusKm)));
    }

    private void UpdateMapViewport()
    {
        if (_mapView is null)
        {
            return;
        }

        var poiLocations = _diemThamQuan
            .Select(poi => TryCreatePoiLocation(poi, out var location) ? location : null)
            .OfType<Location>()
            .ToList();

        if (poiLocations.Count > 0)
        {
            MoveMapToLocations(poiLocations);
            return;
        }

        if (_currentLocation is not null)
        {
            _mapView.MoveToRegion(MapSpan.FromCenterAndRadius(
                _currentLocation,
                Distance.FromKilometers(DefaultUserRadiusKm)));
        }
    }

    private void MoveMapToLocations(params Location[] locations)
    {
        MoveMapToLocations((IReadOnlyCollection<Location>)locations);
    }

    private void MoveMapToLocations(IReadOnlyCollection<Location> locations)
    {
        if (_mapView is null || locations.Count == 0)
        {
            return;
        }

        if (locations.Count == 1)
        {
            _mapView.MoveToRegion(MapSpan.FromCenterAndRadius(
                locations.First(),
                Distance.FromKilometers(DefaultPoiRadiusKm)));
            return;
        }

        var minLatitude = locations.Min(location => location.Latitude);
        var maxLatitude = locations.Max(location => location.Latitude);
        var minLongitude = locations.Min(location => location.Longitude);
        var maxLongitude = locations.Max(location => location.Longitude);

        var latitudeSpan = Math.Max((maxLatitude - minLatitude) * 1.3, MinimumViewportDegrees);
        var longitudeSpan = Math.Max((maxLongitude - minLongitude) * 1.3, MinimumViewportDegrees);
        var center = new Location(
            (minLatitude + maxLatitude) / 2,
            (minLongitude + maxLongitude) / 2);

        _mapView.MoveToRegion(new MapSpan(center, latitudeSpan, longitudeSpan));
    }

    private static bool TryCreatePoiLocation(DiemThamQuanItem poi, out Location location)
    {
        var latitude = (double)poi.ViDo;
        var longitude = (double)poi.KinhDo;

        if (double.IsNaN(latitude) || double.IsNaN(longitude) ||
            latitude is < -90 or > 90 ||
            longitude is < -180 or > 180 ||
            (latitude == 0 && longitude == 0))
        {
            location = default!;
            return false;
        }

        location = new Location(latitude, longitude);
        return true;
    }

    private async void OnLookupQrClicked(object? sender, EventArgs e)
    {
        if (!await EnsureAuthenticatedForActionAsync("mo noi dung bang QR"))
        {
            return;
        }

        await HandleQrValueAsync(QrEntry.Text ?? string.Empty);
    }

    private async void OnScanQrClicked(object? sender, EventArgs e)
    {
        if (!await EnsureAuthenticatedForActionAsync("quet QR"))
        {
            return;
        }

        var cameraPermission = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (cameraPermission != PermissionStatus.Granted)
        {
            cameraPermission = await Permissions.RequestAsync<Permissions.Camera>();
        }

        if (cameraPermission != PermissionStatus.Granted)
        {
            await ShowAlertAsync("Thong bao", "Can cap quyen camera de quet QR.", "OK");
            return;
        }

        var value = await QrScannerPage.ScanAsync(Navigation);
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        QrEntry.Text = value;
        await HandleQrValueAsync(value);
    }

    private async void OnPlayContentClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: NoiDungItem item })
        {
            return;
        }

        if (!await EnsureAuthenticatedForActionAsync("phat audio"))
        {
            return;
        }

        var playbackItem = ResolvePlaybackContent(item);
        if (playbackItem.MaNoiDung != item.MaNoiDung)
        {
            SelectedPoiLabel.Text = $"Dang phat audio theo ngon ngu da chon: {playbackItem.TenNgonNgu ?? playbackItem.MaNgonNgu.ToString()}";
        }

        await PlayNoiDungAsync(playbackItem, "manual");
    }

    private async void OnDisplayLanguageChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingLanguageSelection)
        {
            return;
        }

        if (DisplayLanguagePicker.SelectedItem is not NgonNguItem selected)
        {
            SetUnifiedLanguageSelection(0);
        }
        else
        {
            SetUnifiedLanguageSelection(selected.MaNgonNgu);
        }

        await EnsureLanguageSupportedForCurrentPoiAsync();
        ApplyLanguageFilter();
    }

    private async void OnAudioLanguageChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingLanguageSelection)
        {
            return;
        }

        if (AudioLanguagePicker.SelectedItem is not NgonNguItem selected)
        {
            SetUnifiedLanguageSelection(0);
        }
        else
        {
            SetUnifiedLanguageSelection(selected.MaNgonNgu);
        }

        await EnsureLanguageSupportedForCurrentPoiAsync();
        ApplyLanguageFilter();
    }

    private void UpdateLanguageStateLabel()
    {
        var displayText = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == _selectedDisplayMaNgonNgu)?.TenNgonNgu ?? "Tat ca";
        var audioText = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == _selectedAudioMaNgonNgu)?.TenNgonNgu ?? "Tat ca";
        var hasDisplayContent = _selectedDisplayMaNgonNgu == 0 || _allNoiDung.Any(x => x.MaNgonNgu == _selectedDisplayMaNgonNgu);
        var hasAudioContent = _selectedAudioMaNgonNgu == 0 || _allNoiDung.Any(x => x.MaNgonNgu == _selectedAudioMaNgonNgu);
        var label = this.FindByName<Label>("LanguageStateLabel");
        if (label is not null)
        {
            var suffix = string.Empty;
            if (_allNoiDung.Count > 0)
            {
                if (!hasDisplayContent)
                {
                    suffix += " | Hien thi chua co du lieu";
                }

                if (!hasAudioContent)
                {
                    suffix += " | Audio se dung ban thay the";
                }
            }

            label.Text = $"Hien thi: {displayText} | Audio: {audioText}{suffix}";
        }
    }

    private async Task SpeakTextAsync(NoiDungItem item)
    {
        var options = new SpeechOptions();
        var isoCode = _ngonNguMap.TryGetValue(item.MaNgonNgu, out var language)
            ? language.MaNgonNguQuocTe
            : null;

        if (!string.IsNullOrWhiteSpace(isoCode))
        {
            var locale = (await TextToSpeech.Default.GetLocalesAsync())
                .FirstOrDefault(x => x.Language.StartsWith(isoCode, StringComparison.OrdinalIgnoreCase));
            if (locale is not null)
            {
                options.Locale = locale;
            }
        }

        await TextToSpeech.Default.SpeakAsync(item.NoiDungVanBan ?? string.Empty, options);
    }

    private void SetLoading(bool value)
    {
        LoadingIndicator.IsVisible = value;
        LoadingIndicator.IsRunning = value;
    }

    private async Task<bool> EnsureAuthenticatedForActionAsync(string actionName)
    {
        if (_authSession.IsAuthenticated)
        {
            return true;
        }

        var shouldLogin = await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.DisplayAlertAsync(
                "Can dang nhap",
                $"Ban can dang nhap de {actionName}.",
                "Dang nhap",
                "De sau"));

        if (shouldLogin)
        {
            await Shell.Current.GoToAsync(nameof(AuthPage));
        }

        return false;
    }

    private void ConfigureToolbar()
    {
        ToolbarItems.Clear();

        var accountText = _authSession.IsAuthenticated
            ? (_authSession.DisplayName ?? _authSession.TenDangNhap ?? "Tai khoan")
            : "Dang nhap";

        ToolbarItems.Add(new ToolbarItem
        {
            Text = accountText,
            Order = ToolbarItemOrder.Primary,
            Priority = 0,
            Command = new Command(async () => await HandleAccountToolbarAsync())
        });
    }

    private async Task HandleAccountToolbarAsync()
    {
        if (!_authSession.IsAuthenticated)
        {
            await Shell.Current.GoToAsync(nameof(AuthPage));
            return;
        }

        var action = await Shell.Current.DisplayActionSheetAsync(
            _authSession.DisplayName ?? _authSession.TenDangNhap ?? "Tai khoan",
            "Dong",
            null,
            "Dang xuat");

        if (action == "Dang xuat")
        {
            _authSession.SignOut();
            ConfigureToolbar();
            await ShowAlertAsync("Thong bao", "Da dang xuat tai khoan.", "OK");
        }
    }

    private static Task ShowAlertAsync(string title, string message, string cancel)
    {
        return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, cancel));
    }
}
