using HeThongThuyetMinhDuLich.Mobile.Models;
using HeThongThuyetMinhDuLich.Mobile.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using MauiMap = Microsoft.Maui.Controls.Maps.Map;
using System.Linq;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class MainPage : ContentPage
{
    private static readonly TimeSpan GeofenceCooldown = TimeSpan.FromMinutes(2);
    private readonly MobileApiClient _apiClient;
    private readonly ObservableCollection<DiemThamQuanItem> _diemThamQuan = [];
    private readonly ObservableCollection<NoiDungItem> _noiDung = [];
    private readonly Dictionary<int, DateTime> _lastAutoTriggerUtcByPoi = [];
    private MauiMap? _mapView;
    private IDispatcherTimer? _gpsTimer;
    private Location? _currentLocation;
    private DiemThamQuanItem? _nearestPoi;
    private IDispatcherTimer? _syncTimer;
    public MainPage(MobileApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
        PoiCollection.ItemsSource = _diemThamQuan;
        ContentCollection.ItemsSource = _noiDung;
        InitializeMap();
    }

    private bool _isSyncing;

    private void StartSyncTimer()
    {
        if (_syncTimer is not null)
            return;

        _syncTimer = Dispatcher.CreateTimer();
        _syncTimer.Interval = TimeSpan.FromSeconds(30);

        _syncTimer.Tick += async (_, _) =>
        {
            if (_isSyncing) return; // 🔥 tránh chạy chồng

            _isSyncing = true;

            try
            {
                var items = await _apiClient.GetDiemThamQuanAsync();

                // update nếu có thay đổi
                if (items.Count != _diemThamQuan.Count ||
                    !_diemThamQuan.Select(x => (x.MaDiem, x.NgayCapNhat))
                        .SequenceEqual(items.Select(x => (x.MaDiem, x.NgayCapNhat))))
                {
                    _diemThamQuan.Clear();
                    foreach (var item in items)
                    {
                        _diemThamQuan.Add(item);
                    }

                    RenderMapPins();
                    UpdateNearestPoi();
                }
            }
            catch
            {
                // tránh crash
            }
            finally
            {
                _isSyncing = false;
            }
        };

        _syncTimer.Start();
    }    
    private void InitializeMap()
    {
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            _mapView = null;
            GpsStatusLabel.Text = "Vi tri: map khong ho tro tren Windows.";
            return;
        }

        try
        {
            _mapView = new MauiMap
            {
                IsShowingUser = true
            };
            MapHost.Children.Clear();
            MapHost.Children.Add(_mapView);
        }
        catch
        {
            _mapView = null;
            GpsStatusLabel.Text = "Vi tri: map khong ho tro tren thiet bi nay.";
        }
    }

    protected override async void OnAppearing()
{
    base.OnAppearing();

    try
    {
        // 🟢 Load + sync dữ liệu (API → SQLite → UI)
        await LoadDiemThamQuanAsync();

        // 📍 Cập nhật GPS
        await RefreshGpsAsync();

        // ⏱️ Timer GPS (đã có sẵn của bạn)
        StartGpsTimer();

        // 🔄 Start sync timer (chỉ chạy 1 lần)
        StartSyncTimer();
    }
    catch (Exception ex)
    {
        await DisplayAlertAsync("Loi", $"Khoi dong that bai: {ex.Message}", "OK");
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
}

    private void StartGpsTimer()
    {
        if (_gpsTimer is not null)
        {
            return;
        }

        _gpsTimer = Dispatcher.CreateTimer();
        _gpsTimer.Interval = TimeSpan.FromSeconds(10);
        _gpsTimer.Tick += async (_, _) => await RefreshGpsAsync();
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

            RenderMapPins();
            UpdateNearestPoi();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Loi", $"Khong tai duoc danh sach diem: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task RefreshGpsAsync()
    {
        try
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permission != PermissionStatus.Granted)
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
            _mapView?.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1)));
            RenderMapPins();
            UpdateNearestPoi();
            await CheckAndTriggerGeofenceAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Loi GPS", $"Khong cap nhat duoc vi tri: {ex.Message}", "OK");
        }
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
        var item = contents.FirstOrDefault();
        if (item is null)
        {
            return;
        }

        _lastAutoTriggerUtcByPoi[_nearestPoi.MaDiem] = DateTime.UtcNow;
        SelectedPoiLabel.Text = $"Auto geofence: {_nearestPoi.TenDiem}";
        _noiDung.Clear();
        foreach (var content in contents)
        {
            _noiDung.Add(content);
        }

        await PlayNoiDungAsync(item, "gps");
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
            var isNearest = _nearestPoi is not null && _nearestPoi.MaDiem == poi.MaDiem;
            var label = isNearest ? $"Gan nhat: {poi.TenDiem}" : poi.TenDiem;

            _mapView.Pins.Add(new Pin
            {
                Label = label,
                Address = poi.DiaChi ?? string.Empty,
                Location = new Location((double)poi.ViDo, (double)poi.KinhDo),
                Type = PinType.Place
            });
        }
    }

    private void UpdateNearestPoi()
    {
        if (_currentLocation is null || _diemThamQuan.Count == 0)
        {
            NearestPoiLabel.Text = "POI gan nhat: chua xac dinh.";
            return;
        }

        _nearestPoi = _diemThamQuan
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
        RenderMapPins();
    }

    private async Task LoadNoiDungAsync(int maDiem)
    {
        try
        {
            SetLoading(true);
            _noiDung.Clear();
            var items = await _apiClient.GetNoiDungByDiemAsync(maDiem);
            foreach (var item in items)
            {
                _noiDung.Add(item);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Loi", $"Khong tai duoc noi dung: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task PlayNoiDungAsync(NoiDungItem item, string triggerType)
    {
        var startedAt = DateTime.UtcNow;
        var played = false;

        var audioUrl = _apiClient.ResolveAudioUrl(item.DuongDanAmThanh);
        if (!string.IsNullOrWhiteSpace(audioUrl))
        {
            try
            {
                await Launcher.Default.OpenAsync(audioUrl);
                played = true;
            }
            catch
            {
                played = false;
            }
        }

        if (!played && !string.IsNullOrWhiteSpace(item.NoiDungVanBan) && item.ChoPhepTTS)
        {
            await TextToSpeech.Default.SpeakAsync(item.NoiDungVanBan);
            played = true;
        }

        if (!played)
        {
            await DisplayAlertAsync("Thong bao", "Khong the phat audio/TTS cho noi dung nay.", "OK");
            return;
        }

        try
        {
            await _apiClient.CreateLichSuPhatAsync(new LichSuPhatCreateRequest
            {
                MaNguoiDung = null,
                MaDiem = item.MaDiem,
                MaNoiDung = item.MaNoiDung,
                CachKichHoat = triggerType,
                ThoiGianBatDau = startedAt,
                ThoiLuongDaNghe = item.ThoiLuongGiay
            });
        }
        catch
        {
            // no-op
        }
    }

    private async void OnReloadClicked(object? sender, EventArgs e)
    {
        await LoadDiemThamQuanAsync();
    }

    private async void OnRefreshGpsClicked(object? sender, EventArgs e)
    {
        await RefreshGpsAsync();
    }

    private async void OnPoiSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DiemThamQuanItem poi)
        {
            return;
        }

        SelectedPoiLabel.Text = $"Da chon: {poi.TenDiem} ({poi.MaDinhDanh})";
        _mapView?.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location((double)poi.ViDo, (double)poi.KinhDo),
            Distance.FromKilometers(0.6)));
        await LoadNoiDungAsync(poi.MaDiem);
    }

    private async void OnLookupQrClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(QrEntry.Text))
        {
            await DisplayAlertAsync("Thong bao", "Nhap ma QR truoc khi tim.", "OK");
            return;
        }

        try
        {
            SetLoading(true);
            var result = await _apiClient.LookupQrAsync(QrEntry.Text);
            if (result?.DiemThamQuan is null)
            {
                await DisplayAlertAsync("Khong tim thay", "Ma QR khong hop le hoac khong ton tai.", "OK");
                return;
            }

            SelectedPoiLabel.Text = $"QR -> {result.DiemThamQuan.TenDiem} ({result.GiaTriQR})";
            _mapView?.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location((double)result.DiemThamQuan.ViDo, (double)result.DiemThamQuan.KinhDo),
                Distance.FromKilometers(0.6)));

            _noiDung.Clear();
            foreach (var item in result.NoiDung)
            {
                _noiDung.Add(item);
            }

            var firstContent = result.NoiDung.FirstOrDefault();
            if (firstContent is not null)
            {
                await PlayNoiDungAsync(firstContent, "qr");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Loi", $"Khong tra cuu duoc QR: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnSpeakTtsClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: NoiDungItem item })
        {
            return;
        }

        await PlayNoiDungAsync(item, "manual");
    }

    private async void OnPlayAudioClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: NoiDungItem item })
        {
            return;
        }

        await PlayNoiDungAsync(item, "manual");
    }

    private void SetLoading(bool value)
    {
        LoadingIndicator.IsVisible = value;
        LoadingIndicator.IsRunning = value;
    }
}
