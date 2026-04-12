using HeThongThuyetMinhDuLich.Mobile.Models;
using HeThongThuyetMinhDuLich.Mobile.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Linq;
using MauiMap = Microsoft.Maui.Controls.Maps.Map;
#if ANDROID
using Android.Media;
#endif

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class MainPage : ContentPage
{
    private static readonly TimeSpan GeofenceCooldown = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan GpsTestStepInterval = TimeSpan.FromSeconds(6);
    private static readonly TimeSpan GpsRefreshInterval = TimeSpan.FromSeconds(5);
    private const double DefaultPoiRadiusKm = 0.6;
    private const double DefaultUserRadiusKm = 1;
    private const double MinimumViewportDegrees = 0.01;
    private const double MinimumEffectiveTriggerRadiusMeters = 45;
    private const double MaximumAccuracyBufferMeters = 25;
    private const string GpxTestAssetName = "vinh-khanh-food-tour.gpx";

    private readonly MobileApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly LanguageService _languageService;
    private readonly ObservableCollection<DiemThamQuanItem> _diemThamQuan = [];
    private readonly ObservableCollection<NoiDungItem> _noiDung = [];
    private readonly ObservableCollection<NgonNguItem> _ngonNguItems = [];
    private readonly ObservableCollection<string> _gpsTestEventLogs = [];
    private readonly List<NoiDungItem> _allNoiDung = [];
    private readonly Dictionary<int, NgonNguItem> _ngonNguMap = [];
    private readonly Dictionary<int, DateTime> _lastAutoTriggerUtcByPoi = [];
    private List<string> _fallbackAlternativeLanguageNames = [];
    private bool _isFallbackActive;
    private int? _resolvedContentMaNgonNgu;

    private MauiMap? _mapView;
    private IDispatcherTimer? _gpsTimer;
    private IDispatcherTimer? _gpsTestTimer;
    private IDispatcherTimer? _syncTimer;
    private Location? _currentLocation;
    private DiemThamQuanItem? _nearestPoi;
    private List<Location> _gpsTestTrack = new List<Location>();
    private IReadOnlyList<SimulationWaypoint> _gpsTestWaypoints = new List<SimulationWaypoint>();
    private readonly HashSet<int> _gpsTestTriggeredPoiIds = new HashSet<int>();
    private bool _isRefreshingGps;
    private bool _isSyncing;
    private bool _isConnectivitySubscribed;
    private bool _isHandlingPoiSelection;
    private bool _isUpdatingLanguageSelection;
    private bool _isLanguageSubscribed;
    private bool _isGpsTestRunning;
    private int _selectedDisplayMaNgonNgu;
    private int _gpsTestWaypointIndex;
    private int _gpsTestRunVersion;
    private string? _gpsTestModeName;
    private string? _gpsTestLastTriggeredPoiName;
    private bool _gpsTestLastStepTriggered;
#if ANDROID
    private MediaPlayer? _androidMediaPlayer;
#endif

    public MainPage(MobileApiClient apiClient, AuthSession authSession, LanguageService languageService)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _authSession = authSession;
        _languageService = languageService;
        PoiCollection.ItemsSource = _diemThamQuan;
        ContentCollection.ItemsSource = _noiDung;
        DisplayLanguagePicker.ItemsSource = _ngonNguItems;
        GpsTestLogCollection.ItemsSource = _gpsTestEventLogs;
        ApplyLocalization();
        InitializeMap();
        ConfigureToolbar();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        SubscribeLanguageChanges();
        ApplyLocalization();
        ConfigureToolbar();

        EnsureConnectivitySubscription();

        try
        {
            await LoadNgonNguAsync();
            await LoadDiemThamQuanAsync();
            await EnsureGpsTestTrackLoadedAsync();
            await SyncOfflineStateWithRetriesAsync();
            await RefreshGpsAsync(requestPermissionIfNeeded: true, showErrors: false);
            StartGpsTimer();
            StartSyncTimer();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(T("StartupErrorTitle"), _languageService.Format("StartupErrorBody", ex.Message), T("ConfirmAction"));
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopGpsTest(restartRealGps: false);
        StopGpsTimer();
        StopNativeAudioPlayback();
        UnsubscribeLanguageChanges();

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
            GpsStatusLabel.Text = T("GpsUnsupportedWindows");
            return;
        }

        try
        {
            _mapView = new MauiMap { IsShowingUser = true };
            MapHost.Children.Clear();
            MapHost.Children.Add(_mapView);
            UpdateGpsTestStatusLabel();
            UpdateMapViewport();
        }
        catch
        {
            _mapView = null;
            GpsStatusLabel.Text = T("GpsUnsupportedDevice");
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
            TenNgonNgu = T("AllLanguages")
        });

        foreach (var item in items)
        {
            _ngonNguItems.Add(item);
            _ngonNguMap[item.MaNgonNgu] = item;
        }

        RefreshPoiLocalization();

        var defaultLang = items.FirstOrDefault(x => string.Equals(x.MaNgonNguQuocTe, _languageService.CurrentLanguageCode, StringComparison.OrdinalIgnoreCase))
            ?? items.FirstOrDefault(x => x.LaMacDinh)
            ?? items.FirstOrDefault();
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

                    RefreshPoiLocalization();
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
        if (_isGpsTestRunning)
        {
            return;
        }

        if (_gpsTimer is not null)
        {
            return;
        }

        _gpsTimer = Dispatcher.CreateTimer();
        _gpsTimer.Interval = GpsRefreshInterval;
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

            RefreshPoiLocalization();
            UpdateNearestPoi();
            RenderMapPins();
            UpdateMapViewport();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(T("StartupErrorTitle"), _languageService.Format("PoiLoadErrorBody", ex.Message), T("ConfirmAction"));
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
                GpsStatusLabel.Text = T("LocationPermissionMissing");
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request) ?? await Geolocation.Default.GetLastKnownLocationAsync();
            if (location is null)
            {
                GpsStatusLabel.Text = T("LocationUnavailable");
                return;
            }

            _currentLocation = location;
            SetCurrentLocationText(location);
            UpdateNearestPoi();
            RenderMapPins();
            UpdateMapViewport();
            await CheckAndTriggerGeofenceAsync();
        }
        catch (Exception ex)
        {
            if (showErrors)
            {
                await ShowAlertAsync(T("GpsErrorTitle"), _languageService.Format("GpsErrorBody", ex.Message), T("ConfirmAction"));
            }
            else
            {
                GpsStatusLabel.Text = _languageService.Format("GpsErrorInline", ex.Message);
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
            ClearFallbackLanguageInfo();

            var fallbackInfo = await _apiClient.GetNoiDungFallbackAsync(maDiem, GetPreferredLanguageId());
            var items = await _apiClient.GetNoiDungByDiemAsync(maDiem, GetPreferredLanguageId());
            _allNoiDung.AddRange(items);

            if (fallbackInfo is not null)
            {
                ApplyFallbackLanguageInfo(fallbackInfo);
            }
            else
            {
                await EnsureLanguageSupportedForCurrentPoiAsync();
            }

            ApplyLanguageFilter();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(T("StartupErrorTitle"), _languageService.Format("ContentLoadErrorBody", ex.Message), T("ConfirmAction"));
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
            SelectedPoiLabel.Text = _languageService.Format("SelectedPoiFormat", poi.TenDiem, poi.MaDinhDanh);

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
                await ShowAlertAsync(T("NotificationTitle"), T("PoiNoContentBody"), T("ConfirmAction"));
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

        var preferredLanguageId = GetEffectiveContentLanguageId();

        var filtered = preferredLanguageId == 0
            ? _allNoiDung
            : _allNoiDung.Where(x => x.MaNgonNgu == preferredLanguageId).ToList();

        foreach (var item in filtered)
        {
            item.LocalizedLanguageText = _languageService.Format("ContentLanguageFormat", item.TenNgonNgu ?? item.MaNgonNgu.ToString());
            item.LocalizedPlayButtonText = T("PlayButton");
            _noiDung.Add(item);
        }

        RefreshContentBindings();
        UpdateLanguageStateLabel();
    }

    private async Task CheckAndTriggerGeofenceAsync()
    {
        await CheckAndTriggerGeofenceAsync(trackGpsTestTrigger: false);
    }

    private async Task<bool> CheckAndTriggerGeofenceAsync(bool trackGpsTestTrigger)
    {
        if (_currentLocation is null)
        {
            return false;
        }

        var candidates = _diemThamQuan
            .Where(poi => TryCreatePoiLocation(poi, out _))
            .Select(poi =>
            {
                TryCreatePoiLocation(poi, out var poiLocation);
                var distanceMeters = Location.CalculateDistance(_currentLocation, poiLocation, DistanceUnits.Kilometers) * 1000;
                var triggerRadiusMeters = GetEffectiveTriggerRadiusMeters(poi, _currentLocation);
                return new GeofenceCandidate(poi, distanceMeters, triggerRadiusMeters);
            })
            .Where(candidate => candidate.DistanceMeters <= candidate.TriggerRadiusMeters)
            .OrderBy(candidate => candidate.DistanceMeters)
            .ToList();

        if (candidates.Count == 0)
        {
            if (trackGpsTestTrigger)
            {
                var nearestCandidate = _diemThamQuan
                    .Where(poi => TryCreatePoiLocation(poi, out _))
                    .Select(poi =>
                    {
                        TryCreatePoiLocation(poi, out var poiLocation);
                        var distanceMeters = Location.CalculateDistance(_currentLocation, poiLocation, DistanceUnits.Kilometers) * 1000;
                        return new GeofenceCandidate(poi, distanceMeters, GetEffectiveTriggerRadiusMeters(poi, _currentLocation));
                    })
                    .OrderBy(candidate => candidate.DistanceMeters)
                    .FirstOrDefault();

                if (nearestCandidate is not null)
                {
                    AddGpsTestLog(_languageService.Format("GpsTestLogOutsideGeofence", nearestCandidate.Poi.TenDiem, nearestCandidate.DistanceMeters, nearestCandidate.TriggerRadiusMeters));
                }
            }

            return false;
        }

        foreach (var candidate in candidates)
        {
            var poi = candidate.Poi;
            if (_lastAutoTriggerUtcByPoi.TryGetValue(poi.MaDiem, out var lastTriggerUtc) &&
                DateTime.UtcNow - lastTriggerUtc < GeofenceCooldown)
            {
                if (trackGpsTestTrigger)
                {
                    AddGpsTestLog(_languageService.Format("GpsTestLogCooldown", poi.TenDiem));
                }

                continue;
            }

            _nearestPoi = poi;

            var contents = await _apiClient.GetNoiDungByDiemAsync(poi.MaDiem, GetPreferredLanguageId());
            var fallbackInfo = await _apiClient.GetNoiDungFallbackAsync(poi.MaDiem, GetPreferredLanguageId());
            _allNoiDung.Clear();
            _allNoiDung.AddRange(contents);

            if (fallbackInfo is not null)
            {
                ApplyFallbackLanguageInfo(fallbackInfo);
            }
            else
            {
                await EnsureLanguageSupportedForCurrentPoiAsync();
            }

            ApplyLanguageFilter();

            var autoItem = ResolvePreferredContent();
            if (autoItem is null)
            {
                continue;
            }

            _lastAutoTriggerUtcByPoi[poi.MaDiem] = DateTime.UtcNow;
            if (trackGpsTestTrigger)
            {
                _gpsTestTriggeredPoiIds.Add(poi.MaDiem);
                _gpsTestLastTriggeredPoiName = poi.TenDiem;
                _gpsTestLastStepTriggered = true;
                AddGpsTestLog(_languageService.Format("GpsTestLogGeofenceTriggered", poi.TenDiem, candidate.DistanceMeters));
            }

            SelectedPoiLabel.Text = _languageService.Format("AutoGeofenceFormat", poi.TenDiem);
            await PlayNoiDungAsync(autoItem, "gps");
            return true;
        }

        return false;
    }

    private NoiDungItem? ResolvePreferredContent()
    {
        var preferredLanguageId = GetEffectiveContentLanguageId();
        return preferredLanguageId == 0
            ? _allNoiDung.FirstOrDefault()
            : _allNoiDung.FirstOrDefault(x => x.MaNgonNgu == preferredLanguageId) ?? _allNoiDung.FirstOrDefault();
    }

    private NoiDungItem ResolvePlaybackContent(NoiDungItem requestedItem)
    {
        var preferredLanguageId = GetEffectiveContentLanguageId();
        if (preferredLanguageId == 0 || requestedItem.MaNgonNgu == preferredLanguageId)
        {
            return requestedItem;
        }

        return _allNoiDung.FirstOrDefault(x => x.MaDiem == requestedItem.MaDiem && x.MaNgonNgu == preferredLanguageId)
            ?? requestedItem;
    }

    private void SetUnifiedLanguageSelection(int maNgonNgu)
    {
        _selectedDisplayMaNgonNgu = maNgonNgu;

        var selectedItem = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == maNgonNgu)
                           ?? _ngonNguItems.FirstOrDefault();

        _isUpdatingLanguageSelection = true;
        try
        {
            DisplayLanguagePicker.SelectedItem = selectedItem;
        }
        finally
        {
            _isUpdatingLanguageSelection = false;
        }
    }

    private Task EnsureLanguageSupportedForCurrentPoiAsync()
    {
        ClearFallbackLanguageInfo();

        var preferredLanguageId = GetPreferredLanguageId();
        if (_allNoiDung.Count == 0 || preferredLanguageId == 0)
        {
            return Task.CompletedTask;
        }

        var availableLanguageIds = _allNoiDung
            .Select(x => x.MaNgonNgu)
            .Distinct()
            .ToList();

        if (availableLanguageIds.Count == 0)
        {
            return Task.CompletedTask;
        }

        var resolvedLanguageId = ResolveFallbackLanguageId(availableLanguageIds, preferredLanguageId);
        _resolvedContentMaNgonNgu = resolvedLanguageId > 0 ? resolvedLanguageId : null;

        if (resolvedLanguageId != preferredLanguageId)
        {
            var requestedLanguageName = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == preferredLanguageId)?.TenNgonNgu
                ?? _languageService.Format("LanguageFallbackGenerated", preferredLanguageId);
            var resolvedLanguageName = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == resolvedLanguageId)?.TenNgonNgu
                ?? _languageService.Format("LanguageFallbackGenerated", resolvedLanguageId);
            SelectedPoiLabel.Text = _languageService.Format("LanguageFallbackAppliedFormat", requestedLanguageName, resolvedLanguageName);
            _isFallbackActive = true;
        }

        _fallbackAlternativeLanguageNames = _ngonNguItems
            .Where(x => x.MaNgonNgu > 0 && x.MaNgonNgu != resolvedLanguageId && availableLanguageIds.Contains(x.MaNgonNgu))
            .Select(x => x.TenNgonNgu)
            .ToList();
        UpdateFallbackLanguageLabel();

        return Task.CompletedTask;
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
                    PlayAudioInsideApp(item, audioUrl);
                    played = true;
                    if (_isGpsTestRunning)
                    {
                        AddGpsTestLog(_languageService.Format("GpsTestLogPlaybackAudio", item.TieuDe ?? item.MaNoiDung.ToString(), item.TenNgonNgu ?? item.MaNgonNgu.ToString()));
                    }
                }
                catch (Exception ex)
                {
                    audioError = ex.Message;
                    played = false;
                    if (_isGpsTestRunning)
                    {
                        AddGpsTestLog(_languageService.Format("GpsTestLogPlaybackError", item.TieuDe ?? item.MaNoiDung.ToString(), ex.Message));
                    }
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
            await ShowAlertAsync(T("AudioPlayErrorTitle"), ex.Message, T("ConfirmAction"));
            played = false;
            if (_isGpsTestRunning)
            {
                AddGpsTestLog(_languageService.Format("GpsTestLogPlaybackError", item.TieuDe ?? item.MaNoiDung.ToString(), ex.Message));
            }
        }

        if (!played)
        {
            var detail = string.IsNullOrWhiteSpace(audioError) ? string.Empty : _languageService.Format("AudioErrorDetailPrefix", audioError);
            await ShowAlertAsync(T("NotificationTitle"), _languageService.Format("AudioUnavailableBody", detail), T("ConfirmAction"));
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

    private void PlayAudioInsideApp(NoiDungItem? item, string audioUrl)
    {
#if ANDROID
        if (TryPlayAudioNativelyOnAndroid(item, audioUrl))
        {
            AudioPlayerCard.IsVisible = false;
            return;
        }
#endif

        var safeUrl = WebUtility.HtmlEncode(audioUrl);
        AudioPlayerCard.IsVisible = true;
        AudioWebView.IsVisible = true;
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
            await ShowAlertAsync(T("NotificationTitle"), T("QrInvalidBody"), T("ConfirmAction"));
            return;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUrl))
        {
            var normalizedUrl = _apiClient.ResolveAudioUrl(absoluteUrl.ToString());
            if (!string.IsNullOrWhiteSpace(normalizedUrl))
            {
                SelectedPoiLabel.Text = T("DirectQrPlaying");
                PlayAudioInsideApp(item: null, normalizedUrl);
                return;
            }
        }

        try
        {
            SetLoading(true);
            ClearFallbackLanguageInfo();
            var result = await _apiClient.LookupQrAsync(value, GetPreferredLanguageId());
            if (result?.DiemThamQuan is null)
            {
                await ShowAlertAsync(T("QrNotFoundTitle"), T("QrNotFoundBody"), T("ConfirmAction"));
                return;
            }

            SelectedPoiLabel.Text = _languageService.Format("QrPoiSelectedFormat", result.DiemThamQuan.TenDiem);
            FocusMapOnPoi(result.DiemThamQuan);

            _allNoiDung.Clear();
            _allNoiDung.AddRange(result.NoiDung);

            if (result.MaNgonNguThucTe.HasValue && result.MaNgonNguThucTe.Value > 0)
            {
                ApplyFallbackLanguageInfo(new NoiDungFallbackResponse
                {
                    MaDiem = result.MaDiem,
                    MaNgonNguYeuCau = result.MaNgonNguYeuCau,
                    MaNgonNguThucTe = result.MaNgonNguThucTe,
                    NgonNguYeuCau = result.NgonNguYeuCau,
                    NgonNguThucTe = result.NgonNguThucTe,
                    NgonNguKhaDung = result.NgonNguKhaDung,
                    NgonNguThayThe = result.NgonNguThayThe,
                    NoiDung = result.NoiDung
                });
            }
            else
            {
                await EnsureLanguageSupportedForCurrentPoiAsync();
            }

            ApplyLanguageFilter();

            var firstContent = ResolvePreferredContent();
            if (firstContent is not null)
            {
                await PlayNoiDungAsync(firstContent, "qr");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(T("StartupErrorTitle"), _languageService.Format("QrLookupErrorBody", ex.Message), T("ConfirmAction"));
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
        _mapView.MapElements.Clear();

        if (_gpsTestTrack.Count > 1)
        {
            var completedCount = Math.Min(Math.Max(_gpsTestWaypointIndex, 0), _gpsTestTrack.Count);

            if (_isGpsTestRunning && completedCount > 1)
            {
                _mapView.MapElements.Add(CreatePolyline(_gpsTestTrack.Take(completedCount), "#0E7A6A", 6));
            }

            var remainingPoints = _isGpsTestRunning && completedCount > 0
                ? _gpsTestTrack.Skip(Math.Max(completedCount - 1, 0)).ToList()
                : _gpsTestTrack;

            if (remainingPoints.Count > 1)
            {
                _mapView.MapElements.Add(CreatePolyline(remainingPoints, _isGpsTestRunning ? "#7FA8F8" : "#2E5BCA", 4));
            }
        }

        foreach (var poi in _diemThamQuan)
        {
            if (!TryCreatePoiLocation(poi, out var poiLocation))
            {
                continue;
            }

            var isNearest = _nearestPoi is not null && _nearestPoi.MaDiem == poi.MaDiem;
            var pin = new Pin
            {
                Label = isNearest ? _languageService.Format("NearestPinFormat", poi.TenDiem) : poi.TenDiem,
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
                    await ShowAlertAsync(T("MapErrorTitle"), ex.Message, T("ConfirmAction"));
                }
            };

            _mapView.Pins.Add(pin);
        }

        if (_isGpsTestRunning && _currentLocation is not null)
        {
            _mapView.Pins.Add(new Pin
            {
                Label = T("GpsTestCurrentPin"),
                Location = _currentLocation,
                Type = PinType.SavedPin
            });
        }
    }

    private void UpdateNearestPoi()
    {
        if (_currentLocation is null || _diemThamQuan.Count == 0)
        {
            _nearestPoi = null;
            NearestPoiLabel.Text = T("NearestPoiUnknown");
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
            NearestPoiLabel.Text = T("NearestPoiUnknown");
            return;
        }

        var distanceKm = Location.CalculateDistance(
            _currentLocation,
            new Location((double)_nearestPoi.ViDo, (double)_nearestPoi.KinhDo),
            DistanceUnits.Kilometers);

        NearestPoiLabel.Text = _languageService.Format("NearestPoiFormat", _nearestPoi.TenDiem, distanceKm * 1000);
    }

    private async void OnReloadClicked(object? sender, EventArgs e)
    {
        await LoadDiemThamQuanAsync();
    }

    private async void OnRefreshGpsClicked(object? sender, EventArgs e)
    {
        if (_isGpsTestRunning)
        {
            await ShowAlertAsync(T("NotificationTitle"), T("GpsTestRunningBody"), T("ConfirmAction"));
            return;
        }

        await RefreshGpsAsync(requestPermissionIfNeeded: true, showErrors: true);
    }

    private async void OnStartGpsRouteTestClicked(object? sender, EventArgs e)
    {
        try
        {
            await EnsureGpsTestTrackLoadedAsync();
            if (_gpsTestTrack.Count == 0)
            {
                await ShowAlertAsync(T("GpsTestLoadErrorTitle"), T("GpsTestTrackMissingBody"), T("ConfirmAction"));
                return;
            }

            var waypoints = _gpsTestTrack
                .Select((location, index) => new SimulationWaypoint(location, $"route-{index + 1}"))
                .ToList();
            await StartGpsTestAsync(waypoints, T("GpsTestRouteModeName"));
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(T("GpsTestLoadErrorTitle"), _languageService.Format("GpsTestLoadErrorBody", ex.Message), T("ConfirmAction"));
        }
    }

    private async void OnStartPoiSweepTestClicked(object? sender, EventArgs e)
    {
        var waypoints = _diemThamQuan
            .Where(poi => TryCreatePoiLocation(poi, out _))
            .Select(poi =>
            {
                TryCreatePoiLocation(poi, out var location);
                return new SimulationWaypoint(location, poi.TenDiem);
            })
            .ToList();

        if (waypoints.Count == 0)
        {
            await ShowAlertAsync(T("GpsTestLoadErrorTitle"), T("GpsTestNoPoiBody"), T("ConfirmAction"));
            return;
        }

        await StartGpsTestAsync(waypoints, T("GpsTestPoiSweepModeName"));
    }

    private async void OnStopGpsTestClicked(object? sender, EventArgs e)
    {
        UpdateGpsTestStatusLabel(T("GpsTestStoppingStatus"));
        await Task.Yield();
        StopGpsTest(restartRealGps: true);
        UpdateGpsTestStatusLabel(T("GpsTestStoppedStatus"));
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
            await ShowAlertAsync(T("PoiSelectionErrorTitle"), ex.Message, T("ConfirmAction"));
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

        if (_isGpsTestRunning && _currentLocation is not null)
        {
            if (_nearestPoi is not null && TryCreatePoiLocation(_nearestPoi, out var nearestLocation))
            {
                MoveMapToLocations(_currentLocation, nearestLocation);
                return;
            }

            _mapView.MoveToRegion(MapSpan.FromCenterAndRadius(
                _currentLocation,
                Distance.FromKilometers(DefaultPoiRadiusKm)));
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

    private async Task EnsureGpsTestTrackLoadedAsync()
    {
        if (_gpsTestTrack.Count > 0)
        {
            return;
        }

        using var stream = await FileSystem.OpenAppPackageFileAsync(GpxTestAssetName);
        var document = XDocument.Load(stream);
        XNamespace ns = document.Root?.Name.Namespace ?? XNamespace.None;

        _gpsTestTrack = document
            .Descendants(ns + "trkpt")
            .Select(element =>
            {
                var latitude = double.Parse(element.Attribute("lat")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                var longitude = double.Parse(element.Attribute("lon")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                return new Location(latitude, longitude);
            })
            .Where(location => location.Latitude is >= -90 and <= 90 && location.Longitude is >= -180 and <= 180)
            .ToList();

        RenderMapPins();
        UpdateMapViewport();
        UpdateGpsTestStatusLabel();
    }

    private async Task StartGpsTestAsync(IReadOnlyList<SimulationWaypoint> waypoints, string modeName)
    {
        StopGpsTest(restartRealGps: false);
        StopGpsTimer();

        _lastAutoTriggerUtcByPoi.Clear();
        _gpsTestTriggeredPoiIds.Clear();
        _gpsTestEventLogs.Clear();
        _gpsTestLastTriggeredPoiName = null;
        _gpsTestLastStepTriggered = false;
        _gpsTestWaypoints = waypoints;
        _gpsTestWaypointIndex = 0;
        _gpsTestRunVersion++;
        _gpsTestModeName = modeName;
        _isGpsTestRunning = true;
        var currentRunVersion = _gpsTestRunVersion;

        AddGpsTestLog(_languageService.Format("GpsTestLogStarted", modeName, waypoints.Count));

        if (_gpsTestTimer is null)
        {
            _gpsTestTimer = Dispatcher.CreateTimer();
            _gpsTestTimer.Interval = GpsTestStepInterval;
            _gpsTestTimer.Tick += async (_, _) => await AdvanceGpsTestAsync(_gpsTestRunVersion);
        }

        UpdateGpsTestStatusLabel();
        await AdvanceGpsTestAsync(currentRunVersion);
        if (_isGpsTestRunning && currentRunVersion == _gpsTestRunVersion)
        {
            _gpsTestTimer.Start();
        }
    }

    private async Task AdvanceGpsTestAsync(int runVersion)
    {
        if (!_isGpsTestRunning || runVersion != _gpsTestRunVersion)
        {
            return;
        }

        if (_gpsTestWaypointIndex >= _gpsTestWaypoints.Count)
        {
            StopGpsTest(restartRealGps: true);
            UpdateGpsTestStatusLabel(_languageService.Format("GpsTestCompletedStatus", _gpsTestModeName ?? T("GpsTestRouteModeName"), _gpsTestTriggeredPoiIds.Count));
            return;
        }

        var waypoint = _gpsTestWaypoints[_gpsTestWaypointIndex];
        _gpsTestWaypointIndex++;
        _gpsTestLastStepTriggered = false;
        _currentLocation = waypoint.Location;
        SetCurrentLocationText(_currentLocation);
        UpdateNearestPoi();
        RenderMapPins();
        UpdateMapViewport();
        AddGpsTestLog(_languageService.Format("GpsTestLogStep", waypoint.Label, _gpsTestWaypointIndex, _gpsTestWaypoints.Count, waypoint.Location.Latitude, waypoint.Location.Longitude));
        await CheckAndTriggerGeofenceAsync(trackGpsTestTrigger: true);
        if (!_isGpsTestRunning || runVersion != _gpsTestRunVersion)
        {
            return;
        }

        if (!_gpsTestLastStepTriggered)
        {
            AddGpsTestLog(T("GpsTestLogNoTrigger"));
        }

        UpdateGpsTestStatusLabel();
    }

    private void StopGpsTest(bool restartRealGps)
    {
        _gpsTestRunVersion++;

        if (_gpsTestTimer is not null)
        {
            _gpsTestTimer.Stop();
        }

        if (_isGpsTestRunning)
        {
            AddGpsTestLog(_languageService.Format("GpsTestLogStopped", _gpsTestTriggeredPoiIds.Count));
        }

        _isGpsTestRunning = false;
        _gpsTestWaypoints = new List<SimulationWaypoint>();
        _gpsTestWaypointIndex = 0;
        _gpsTestModeName = null;
        _gpsTestLastTriggeredPoiName = null;
        _gpsTestLastStepTriggered = false;
        _nearestPoi = null;
        RenderMapPins();
        UpdateMapViewport();

        if (restartRealGps)
        {
            StartGpsTimer();
            MainThread.BeginInvokeOnMainThread(async () => await RefreshGpsAsync(requestPermissionIfNeeded: false, showErrors: false));
        }
    }

    private void UpdateGpsTestStatusLabel(string? overrideText = null)
    {
        if (GpsTestStatusLabel is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(overrideText))
        {
            GpsTestStatusLabel.Text = overrideText;
            UpdateGpsTestProgress();
            return;
        }

        if (!_isGpsTestRunning)
        {
            GpsTestStatusLabel.Text = _gpsTestTrack.Count > 0
                ? _languageService.Format("GpsTestReadyStatus", _gpsTestTrack.Count)
                : T("GpsTestIdleStatus");
            UpdateGpsTestProgress();
            return;
        }

        var status = _languageService.Format(
            "GpsTestRunningStatus",
            _gpsTestModeName ?? T("GpsTestRouteModeName"),
            Math.Min(_gpsTestWaypointIndex, _gpsTestWaypoints.Count),
            _gpsTestWaypoints.Count,
            _gpsTestTriggeredPoiIds.Count);

        if (!string.IsNullOrWhiteSpace(_gpsTestLastTriggeredPoiName))
        {
            status = string.Concat(status, " ", _languageService.Format("GpsTestLastTriggerSuffix", _gpsTestLastTriggeredPoiName));
        }

        GpsTestStatusLabel.Text = status;
        UpdateGpsTestProgress();
    }

    private void UpdateGpsTestProgress()
    {
        if (GpsTestProgressBar is null || GpsTestProgressLabel is null)
        {
            return;
        }

        var totalSteps = _isGpsTestRunning ? _gpsTestWaypoints.Count : _gpsTestTrack.Count;
        var completedSteps = _isGpsTestRunning ? Math.Min(_gpsTestWaypointIndex, _gpsTestWaypoints.Count) : 0;
        var progress = totalSteps > 0 ? (double)completedSteps / totalSteps : 0d;

        GpsTestProgressBar.Progress = progress;
        GpsTestProgressLabel.Text = _languageService.Format("GpsTestProgressFormat", completedSteps, totalSteps, progress * 100);
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
        await HandleQrValueAsync(QrEntry.Text ?? string.Empty);
    }

    private async void OnScanQrClicked(object? sender, EventArgs e)
    {
        var cameraPermission = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (cameraPermission != PermissionStatus.Granted)
        {
            cameraPermission = await Permissions.RequestAsync<Permissions.Camera>();
        }

        if (cameraPermission != PermissionStatus.Granted)
        {
            await ShowAlertAsync(T("NotificationTitle"), T("CameraPermissionBody"), T("ConfirmAction"));
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

        var playbackItem = ResolvePlaybackContent(item);
        if (playbackItem.MaNoiDung != item.MaNoiDung)
        {
            SelectedPoiLabel.Text = _languageService.Format("PlaybackLanguageFormat", playbackItem.TenNgonNgu ?? playbackItem.MaNgonNgu.ToString());
        }

        await PlayNoiDungAsync(playbackItem, "manual");
    }

    private async void OnDisplayLanguageChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingLanguageSelection)
        {
            return;
        }

        var selected = DisplayLanguagePicker.SelectedItem as NgonNguItem;
        ApplyPreferredLanguageSelection(selected?.MaNgonNgu ?? 0, syncAppLanguage: true);

        await EnsureLanguageSupportedForCurrentPoiAsync();
        ApplyLanguageFilter();
    }

    private void UpdateLanguageStateLabel()
    {
        var preferredLanguageId = GetPreferredLanguageId();
        var effectiveAudioLanguageId = GetEffectiveContentLanguageId();
        var displayText = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == preferredLanguageId)?.TenNgonNgu ?? T("AllLanguages");
        var audioText = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == effectiveAudioLanguageId)?.TenNgonNgu ?? displayText;
        var hasDisplayContent = preferredLanguageId == 0 || _allNoiDung.Any(x => x.MaNgonNgu == preferredLanguageId);
        var hasAudioContent = effectiveAudioLanguageId == 0 || _allNoiDung.Any(x => x.MaNgonNgu == effectiveAudioLanguageId);
        var label = this.FindByName<Label>("LanguageStateLabel");
        if (label is not null)
        {
            var suffix = string.Empty;
            if (_allNoiDung.Count > 0)
            {
                if (!hasDisplayContent)
                {
                    suffix += T("LanguageMissingDisplaySuffix");
                }

                if (!hasAudioContent)
                {
                    suffix += T("LanguageMissingAudioSuffix");
                }
            }

            label.Text = $"{_languageService.Format("LanguageDisplayFormat", displayText)} | {_languageService.Format("LanguageAudioFormat", audioText)}{suffix}";
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

        if (_isGpsTestRunning)
        {
            AddGpsTestLog(_languageService.Format("GpsTestLogPlaybackTts", item.TieuDe ?? item.MaNoiDung.ToString(), item.TenNgonNgu ?? item.MaNgonNgu.ToString()));
        }
    }

    private static Polyline CreatePolyline(IEnumerable<Location> points, string colorHex, float strokeWidth)
    {
        var polyline = new Polyline
        {
            StrokeColor = Color.FromArgb(colorHex),
            StrokeWidth = strokeWidth
        };

        foreach (var point in points)
        {
            polyline.Geopath.Add(point);
        }

        return polyline;
    }

    private void AddGpsTestLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _gpsTestEventLogs.Insert(0, $"[{timestamp}] {message}");

        while (_gpsTestEventLogs.Count > 30)
        {
            _gpsTestEventLogs.RemoveAt(_gpsTestEventLogs.Count - 1);
        }
    }

    private void StopNativeAudioPlayback()
    {
#if ANDROID
        if (_androidMediaPlayer is null)
        {
            return;
        }

        try
        {
            if (_androidMediaPlayer.IsPlaying)
            {
                _androidMediaPlayer.Stop();
            }
        }
        catch
        {
            // best effort
        }

        _androidMediaPlayer.Reset();
        _androidMediaPlayer.Release();
        _androidMediaPlayer.Dispose();
        _androidMediaPlayer = null;
#endif
    }

#if ANDROID
    private bool TryPlayAudioNativelyOnAndroid(NoiDungItem? item, string audioUrl)
    {
        if (string.IsNullOrWhiteSpace(audioUrl))
        {
            return false;
        }

        try
        {
            StopNativeAudioPlayback();

            var player = new MediaPlayer();
            var audioAttributesBuilder = new AudioAttributes.Builder();
            if (audioAttributesBuilder is null)
            {
                player.Dispose();
                return false;
            }

            var audioAttributes = audioAttributesBuilder
                .SetUsage(AudioUsageKind.Media)
                ?.SetContentType(AudioContentType.Music)
                ?.Build();

            if (audioAttributes is null)
            {
                player.Dispose();
                return false;
            }

            player.SetAudioAttributes(audioAttributes);

            player.Prepared += (_, _) => player.Start();
            player.Completion += (_, _) =>
            {
                if (ReferenceEquals(_androidMediaPlayer, player))
                {
                    StopNativeAudioPlayback();
                }
            };
            player.Error += (_, args) =>
            {
                if (_isGpsTestRunning)
                {
                    AddGpsTestLog(_languageService.Format("GpsTestLogPlaybackError", item?.TieuDe ?? item?.MaNoiDung.ToString() ?? audioUrl, $"MediaPlayer {args.What}/{args.Extra}"));
                }

                StopNativeAudioPlayback();
                args.Handled = true;
            };

            if (!string.IsNullOrWhiteSpace(item?.TepAmThanhNoiBo) &&
                Uri.TryCreate(item.TepAmThanhNoiBo, UriKind.Absolute, out var localUri) &&
                string.Equals(localUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase) &&
                File.Exists(localUri.LocalPath))
            {
                player.SetDataSource(localUri.LocalPath);
            }
            else
            {
                player.SetDataSource(audioUrl);
            }

            player.PrepareAsync();
            _androidMediaPlayer = player;
            return true;
        }
        catch (Exception ex)
        {
            StopNativeAudioPlayback();
            if (_isGpsTestRunning)
            {
                AddGpsTestLog(_languageService.Format("GpsTestLogPlaybackError", item?.TieuDe ?? item?.MaNoiDung.ToString() ?? audioUrl, ex.Message));
            }

            return false;
        }
    }
#endif

    private static double GetEffectiveTriggerRadiusMeters(DiemThamQuanItem poi, Location currentLocation)
    {
        var baseRadiusMeters = Math.Max((double)poi.BanKinhKichHoat, MinimumEffectiveTriggerRadiusMeters);
        var accuracyBufferMeters = Math.Clamp(currentLocation.Accuracy ?? 0d, 0d, MaximumAccuracyBufferMeters);
        return baseRadiusMeters + accuracyBufferMeters;
    }

    private void SetLoading(bool value)
    {
        LoadingIndicator.IsVisible = value;
        LoadingIndicator.IsRunning = value;
    }

    private void ConfigureToolbar()
    {
        ToolbarItems.Clear();

        ToolbarItems.Add(new ToolbarItem
        {
            Text = _languageService.CurrentLanguage.ShortLabel,
            Order = ToolbarItemOrder.Primary,
            Priority = 0,
            Command = new Command(async () => await ShowLanguageMenuAsync())
        });

        var accountText = _authSession.IsAuthenticated
            ? (_authSession.DisplayName ?? _authSession.TenDangNhap ?? T("AccountFallback"))
            : T("LoginToolbarText");

        ToolbarItems.Add(new ToolbarItem
        {
            Text = accountText,
            Order = ToolbarItemOrder.Primary,
            Priority = 1,
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
            _authSession.DisplayName ?? _authSession.TenDangNhap ?? T("AccountFallback"),
            T("CloseAction"),
            null,
            T("LogoutAction"));

        if (action == T("LogoutAction"))
        {
            _authSession.SignOut();
            ConfigureToolbar();
            await ShowAlertAsync(T("NotificationTitle"), T("LogoutSuccess"), T("ConfirmAction"));
        }
    }

    private void SubscribeLanguageChanges()
    {
        if (_isLanguageSubscribed)
        {
            return;
        }

        _languageService.LanguageChanged += OnLanguageChanged;
        _isLanguageSubscribed = true;
    }

    private void UnsubscribeLanguageChanges()
    {
        if (!_isLanguageSubscribed)
        {
            return;
        }

        _languageService.LanguageChanged -= OnLanguageChanged;
        _isLanguageSubscribed = false;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            ApplyLocalization();
            ConfigureToolbar();
            SyncContentLanguageWithAppLanguage();
            await EnsureLanguageSupportedForCurrentPoiAsync();
            ApplyLanguageFilter();
        });
    }

    private void ApplyLocalization()
    {
        Title = T("MainPageTitle");
        HeroTitleLabel.Text = T("MainHeroTitle");
        HeroSubtitleLabel.Text = T("MainHeroSubtitle");
        ReloadButton.Text = T("ReloadPoints");
        RefreshGpsButton.Text = T("RefreshGps");
        LocationStatusTitleLabel.Text = T("LocationStatusTitle");
        LanguageSectionTitleLabel.Text = T("LanguageSectionTitle");
        GpsTestSectionTitleLabel.Text = T("GpsTestSectionTitle");
        StartGpsRouteTestButton.Text = T("GpsTestRouteButton");
        StartPoiSweepTestButton.Text = T("GpsTestPoiSweepButton");
        StopGpsTestButton.Text = T("GpsTestStopButton");
        GpsTestProgressTitleLabel.Text = T("GpsTestProgressTitle");
        GpsTestLogTitleLabel.Text = T("GpsTestLogTitle");
        DisplayLanguageTitleLabel.Text = T("LanguagePreferenceLabel");
        DisplayLanguagePicker.Title = T("LanguagePreferencePickerTitle");
        QrSectionTitleLabel.Text = T("QrSectionTitle");
        QrEntry.Placeholder = T("QrPlaceholder");
        OpenQrButton.Text = T("OpenQr");
        ScanQrButton.Text = T("ScanQr");
        PoiSectionTitleLabel.Text = T("PoiListTitle");
        AudioPlayerTitleLabel.Text = T("AudioPlayerTitle");
        ContentSectionTitleLabel.Text = T("ContentSectionTitle");
        RefreshPoiLocalization();
        RefreshContentLocalization();

        if (_currentLocation is null)
        {
            GpsStatusLabel.Text = DeviceInfo.Platform == DevicePlatform.WinUI
                ? T("GpsUnsupportedWindows")
                : T("DefaultGpsStatus");
        }
        else
        {
            SetCurrentLocationText(_currentLocation);
        }

        if (PoiCollection.SelectedItem is not DiemThamQuanItem selectedPoi)
        {
            SelectedPoiLabel.Text = T("SelectedPoiDefault");
        }
        else
        {
            SelectedPoiLabel.Text = _languageService.Format("SelectedPoiFormat", selectedPoi.TenDiem, selectedPoi.MaDinhDanh);
        }

        UpdateNearestPoi();
        UpdateLanguageStateLabel();
        UpdateFallbackLanguageLabel();
        UpdateGpsTestStatusLabel();
    }

    private void ApplyFallbackLanguageInfo(NoiDungFallbackResponse fallbackInfo)
    {
        _resolvedContentMaNgonNgu = fallbackInfo.MaNgonNguThucTe.HasValue && fallbackInfo.MaNgonNguThucTe.Value > 0
            ? fallbackInfo.MaNgonNguThucTe.Value
            : null;

        _isFallbackActive = fallbackInfo.MaNgonNguYeuCau.HasValue &&
            fallbackInfo.MaNgonNguThucTe.HasValue &&
            fallbackInfo.MaNgonNguYeuCau.Value > 0 &&
            fallbackInfo.MaNgonNguYeuCau != fallbackInfo.MaNgonNguThucTe;

        if (_isFallbackActive)
        {
            var requestedLanguageName = fallbackInfo.NgonNguYeuCau?.TenNgonNgu
                ?? _languageService.Format("LanguageFallbackGenerated", fallbackInfo.MaNgonNguYeuCau!.Value);
            var resolvedLanguageName = fallbackInfo.NgonNguThucTe?.TenNgonNgu
                ?? _languageService.Format("LanguageFallbackGenerated", fallbackInfo.MaNgonNguThucTe!.Value);
            SelectedPoiLabel.Text = _languageService.Format("LanguageFallbackAppliedFormat", requestedLanguageName, resolvedLanguageName);
        }

        _fallbackAlternativeLanguageNames = (fallbackInfo.NgonNguThayThe ?? [])
            .Where(x => x.MaNgonNgu > 0)
            .Select(x => x.TenNgonNgu)
            .Distinct()
            .ToList();

        UpdateFallbackLanguageLabel();
    }

    private void ClearFallbackLanguageInfo()
    {
        _isFallbackActive = false;
        _resolvedContentMaNgonNgu = null;
        _fallbackAlternativeLanguageNames = [];
        UpdateFallbackLanguageLabel();
    }

    private void UpdateFallbackLanguageLabel()
    {
        if (FallbackLanguageLabel is null)
        {
            return;
        }

        if (_fallbackAlternativeLanguageNames.Count == 0)
        {
            FallbackLanguageLabel.Text = string.Empty;
            FallbackLanguageLabel.IsVisible = false;
            return;
        }

        var alternativesText = string.Join(", ", _fallbackAlternativeLanguageNames);
        FallbackLanguageLabel.Text = _isFallbackActive
            ? $"{T("LanguageFallbackPrompt")} {_languageService.Format("LanguageFallbackAlternativesFormat", alternativesText)}"
            : _languageService.Format("LanguageAvailableAlternativesFormat", alternativesText);
        FallbackLanguageLabel.IsVisible = true;
    }

    private void SyncContentLanguageWithAppLanguage()
    {
        var matchedLanguage = _ngonNguItems.FirstOrDefault(x => string.Equals(x.MaNgonNguQuocTe, _languageService.CurrentLanguageCode, StringComparison.OrdinalIgnoreCase));
        if (matchedLanguage is not null)
        {
            ApplyPreferredLanguageSelection(matchedLanguage.MaNgonNgu, syncAppLanguage: false);
        }
    }

    private int GetPreferredLanguageId()
    {
        if (_selectedDisplayMaNgonNgu > 0)
        {
            return _selectedDisplayMaNgonNgu;
        }

        return ResolveAppLanguageItem()?.MaNgonNgu ?? 0;
    }

    private int GetEffectiveContentLanguageId()
    {
        var resolvedLanguageId = _resolvedContentMaNgonNgu.GetValueOrDefault();
        return resolvedLanguageId > 0
            ? resolvedLanguageId
            : GetPreferredLanguageId();
    }

    private NgonNguItem? ResolveAppLanguageItem()
    {
        return _ngonNguItems.FirstOrDefault(x =>
            x.MaNgonNgu > 0 &&
            string.Equals(x.MaNgonNguQuocTe, _languageService.CurrentLanguageCode, StringComparison.OrdinalIgnoreCase));
    }

    private int ResolveFallbackLanguageId(IReadOnlyCollection<int> availableLanguageIds, int requestedLanguageId)
    {
        if (availableLanguageIds.Count == 0)
        {
            return 0;
        }

        if (requestedLanguageId > 0 && availableLanguageIds.Contains(requestedLanguageId))
        {
            return requestedLanguageId;
        }

        var defaultLanguageId = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu > 0 && x.LaMacDinh)?.MaNgonNgu ?? 0;
        if (defaultLanguageId > 0 && availableLanguageIds.Contains(defaultLanguageId))
        {
            return defaultLanguageId;
        }

        var vietnameseLanguageId = _ngonNguItems.FirstOrDefault(x =>
            x.MaNgonNgu > 0 &&
            string.Equals(x.MaNgonNguQuocTe, "vi", StringComparison.OrdinalIgnoreCase))?.MaNgonNgu ?? 0;
        if (vietnameseLanguageId > 0 && availableLanguageIds.Contains(vietnameseLanguageId))
        {
            return vietnameseLanguageId;
        }

        return availableLanguageIds.First();
    }

    private void ApplyPreferredLanguageSelection(int maNgonNgu, bool syncAppLanguage)
    {
        var preferredLanguageId = maNgonNgu > 0
            ? maNgonNgu
            : ResolveAppLanguageItem()?.MaNgonNgu ?? _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu > 0)?.MaNgonNgu ?? 0;

        SetUnifiedLanguageSelection(preferredLanguageId);

        if (!syncAppLanguage)
        {
            return;
        }

        var selectedLanguage = _ngonNguItems.FirstOrDefault(x => x.MaNgonNgu == preferredLanguageId);
        if (selectedLanguage is null || string.IsNullOrWhiteSpace(selectedLanguage.MaNgonNguQuocTe))
        {
            return;
        }

        _languageService.SetLanguage(selectedLanguage.MaNgonNguQuocTe);
    }

    private void SetCurrentLocationText(Location location)
    {
        GpsStatusLabel.Text = _languageService.Format("CurrentLocationCoordinates", location.Latitude, location.Longitude);
    }

    private void RefreshPoiLocalization()
    {
        foreach (var poi in _diemThamQuan)
        {
            poi.CoordinateText = _languageService.Format("CoordinateFormat", poi.ViDo, poi.KinhDo);
        }

        var selectedItem = PoiCollection.SelectedItem;
        PoiCollection.ItemsSource = null;
        PoiCollection.ItemsSource = _diemThamQuan;
        PoiCollection.SelectedItem = selectedItem;
    }

    private void RefreshContentLocalization()
    {
        foreach (var item in _allNoiDung)
        {
            item.LocalizedLanguageText = _languageService.Format("ContentLanguageFormat", item.TenNgonNgu ?? item.MaNgonNgu.ToString());
            item.LocalizedPlayButtonText = T("PlayButton");
        }

        RefreshContentBindings();
    }

    private void RefreshContentBindings()
    {
        ContentCollection.ItemsSource = null;
        ContentCollection.ItemsSource = _noiDung;
    }

    private string T(string key) => _languageService.GetText(key);

    private async Task ShowLanguageMenuAsync()
    {
        var actions = _languageService.SupportedLanguages
            .Select(x => $"{x.ShortLabel} - {x.NativeName}")
            .ToArray();

        var selectedAction = await Shell.Current.DisplayActionSheetAsync(
            T("LanguageActionTitle"),
            T("CancelAction"),
            null,
            actions);

        var selectedLanguage = _languageService.SupportedLanguages.FirstOrDefault(x => $"{x.ShortLabel} - {x.NativeName}" == selectedAction);
        if (selectedLanguage is null)
        {
            return;
        }

        _languageService.SetLanguage(selectedLanguage.Code);
    }

    private static Task ShowAlertAsync(string title, string message, string cancel)
    {
        return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, cancel));
    }

    private sealed record SimulationWaypoint(Location Location, string Label);
    private sealed record GeofenceCandidate(DiemThamQuanItem Poi, double DistanceMeters, double TriggerRadiusMeters);
}
