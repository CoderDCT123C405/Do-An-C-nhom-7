using HeThongThuyetMinhDuLich.Mobile.Models;
using HeThongThuyetMinhDuLich.Mobile.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using MauiMap = Microsoft.Maui.Controls.Maps.Map;
#if ANDROID
using Android.Media;
#endif

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class MainPage : ContentPage
{   
    private string _baseUrl;
    private string GetFullUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";

        return _baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }
    private static readonly TimeSpan GeofenceCooldown = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan GeofenceSuppressionAfterStop = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan GpsTestStepInterval = TimeSpan.FromSeconds(6);
    private static readonly TimeSpan GpsRefreshInterval = TimeSpan.FromSeconds(5);
    private const double DefaultPoiRadiusKm = 0.6;
    private const double DefaultUserRadiusKm = 1;
    private const double MinimumViewportDegrees = 0.01;
    private const double MinimumEffectiveTriggerRadiusMeters = 45;
    private const double MaximumAccuracyBufferMeters = 25;
    private const string GpxTestAssetName = "vinh-khanh-food-tour.gpx";

   private readonly SyncService _syncService;
    private readonly MobileApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly LanguageService _languageService;
    private readonly ObservableCollection<DiemThamQuanItem> _diemThamQuan = [];
    private readonly ObservableCollection<DiemThamQuanItem> _visibleDiemThamQuan = [];
    private readonly ObservableCollection<NoiDungItem> _noiDung = [];
    private readonly ObservableCollection<HinhAnhDiemThamQuanItem> _selectedPoiImages = [];
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
    private Location? _currentLocation;
    private DiemThamQuanItem? _nearestPoi;
    private List<Location> _gpsTestTrack = new List<Location>();
    private IReadOnlyList<SimulationWaypoint> _gpsTestWaypoints = new List<SimulationWaypoint>();
    private readonly HashSet<int> _gpsTestTriggeredPoiIds = new HashSet<int>();
    private bool _isRefreshingGps;
    private bool _isConnectivitySubscribed;
    private bool _isHandlingPoiSelection;
    private bool _isUpdatingLanguageSelection;
    private bool _isLanguageSubscribed;
    private bool _isGpsTestRunning;
    private CancellationTokenSource? _playbackCancellationTokenSource;
    private int _selectedDisplayMaNgonNgu;
    private int _gpsTestWaypointIndex;
    private int _gpsTestRunVersion;
    private int _playbackRequestVersion;
    private DateTime? _suppressAutoGeofenceUntilUtc;
    private string? _gpsTestModeName;
    private string? _gpsTestLastTriggeredPoiName;
    private string _poiSearchText = string.Empty;
    private bool _gpsTestLastStepTriggered;
    private DiemThamQuanItem? _selectedPoi;
#if ANDROID
    private MediaPlayer? _androidMediaPlayer;
    private readonly object _androidMediaPlayerGate = new();
#endif

    public MainPage(MobileApiClient apiClient, AuthSession authSession, LanguageService languageService)
    {   
        InitializeComponent();
        _apiClient = apiClient;
        _authSession = authSession;
        _languageService = languageService;
        _baseUrl = Preferences.Get("api_url", "http://10.0.2.2:5000");
        _syncService = new SyncService(_apiClient, _baseUrl);
        PoiCollection.ItemsSource = _visibleDiemThamQuan;
        ContentCollection.ItemsSource = _noiDung;
        PoiGalleryCollection.ItemsSource = _selectedPoiImages;
        DisplayLanguagePicker.ItemsSource = _ngonNguItems;
        GpsTestLogCollection.ItemsSource = _gpsTestEventLogs;
        ApplyLocalization();
        InitializeMap();
        ConfigureToolbar();
    }

   protected override async void OnAppearing()
{
    base.OnAppearing();

    try
    {
        SubscribeLanguageChanges();
        ApplyLocalization();
        ConfigureToolbar();

        EnsureConnectivitySubscription();

        await LoadNgonNguAsync();
        await LoadDiemThamQuanAsync();

        if (_diemThamQuan.Count > 0)
        {
            var firstPoi = _diemThamQuan.First();
            await SelectPoiAsync(firstPoi, false, "auto", false);
        }

        // 🔥 sync lần đầu
        _ = _syncService.SyncAllAsync();
        // 🔥 auto sync
        _syncService.StartAutoSync();

        await EnsureGpsTestTrackLoadedAsync();
        await RefreshGpsAsync(true, false);

        StartGpsTimer();
    }
    catch (Exception ex)
    {
        await DisplayAlertAsync("Loi", ex.Message, "OK");
    }
}
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopGpsTest(restartRealGps: false);
        StopGpsTimer();
        StopPlayback();
        UnsubscribeLanguageChanges();
       

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
        try
        {
            if (e.NetworkAccess != NetworkAccess.Internet)
                return;

            await _syncService.SyncAllAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("CONNECT ERROR: " + ex.Message);
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
            ApplyPoiSearchFilter();
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
            _selectedPoi = poi;
            SelectedPoiLabel.Text = _languageService.Format("SelectedPoiFormat", poi.TenDiem, poi.MaDinhDanh);
            UpdateSelectedPoiMeta(poi);

            if (focusMap)
            {
                FocusMapOnPoi(poi);
            }

            if (!ReferenceEquals(PoiCollection.SelectedItem, poi))
            {
                PoiCollection.SelectedItem = poi;
            }

            await LoadPoiImagesAsync(poi.MaDiem);
            await LoadNoiDungAsync(poi.MaDiem);

            if (!autoPlay)
            {
                return;
            }

            var selectedLanguageId = await PromptPoiLanguageSelectionAsync();
            if (!selectedLanguageId.HasValue)
            {
                return;
            }

            ApplySelectedContentLanguage(
                selectedLanguageId.Value,
                _languageService.Format("PoiLanguageSelectedFormat", poi.TenDiem, ResolveLanguageDisplayName(selectedLanguageId.Value)));

            ApplyLanguageFilter();

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

    private void OnPoiSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _poiSearchText = e.NewTextValue?.Trim() ?? string.Empty;
        ApplyPoiSearchFilter();
    }

    private void ApplyPoiSearchFilter()
    {
        var query = NormalizeSearchText(_poiSearchText);
        var selectedPoiId = (PoiCollection.SelectedItem as DiemThamQuanItem)?.MaDiem;

        IEnumerable<DiemThamQuanItem> filtered = string.IsNullOrWhiteSpace(query)
            ? _diemThamQuan
            : _diemThamQuan.Where(poi => MatchesPoiSearch(poi, query)).ToList();

        _visibleDiemThamQuan.Clear();
        foreach (var poi in filtered)
        {
            _visibleDiemThamQuan.Add(poi);
        }

        if (selectedPoiId.HasValue)
        {
            PoiCollection.SelectedItem = _visibleDiemThamQuan.FirstOrDefault(x => x.MaDiem == selectedPoiId.Value);
        }
    }

    private static bool MatchesPoiSearch(DiemThamQuanItem poi, string normalizedQuery)
    {
        return NormalizeSearchText(poi.TenDiem).Contains(normalizedQuery, StringComparison.Ordinal)
            || NormalizeSearchText(poi.MaDinhDanh).Contains(normalizedQuery, StringComparison.Ordinal)
            || NormalizeSearchText(poi.DiaChi).Contains(normalizedQuery, StringComparison.Ordinal)
            || NormalizeSearchText(poi.MoTaNgan).Contains(normalizedQuery, StringComparison.Ordinal);
    }

    private static string NormalizeSearchText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
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

        if (!trackGpsTestTrigger &&
            _suppressAutoGeofenceUntilUtc is DateTime suppressUntilUtc &&
            DateTime.UtcNow < suppressUntilUtc)
        {
            return false;
        }

        var candidates = _diemThamQuan
            .Where(poi => TryCreatePoiLocation(poi, out _)) // bắt đầu của đoạn trigger gps
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
            _selectedPoi = poi;

            var contents = await _apiClient.GetNoiDungByDiemAsync(poi.MaDiem, GetPreferredLanguageId());
            var fallbackInfo = await _apiClient.GetNoiDungFallbackAsync(poi.MaDiem, GetPreferredLanguageId());
            _allNoiDung.Clear();
            _allNoiDung.AddRange(contents);
            await LoadPoiImagesAsync(poi.MaDiem);
            UpdateSelectedPoiMeta(poi);

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
            await PlayNoiDungAsync(autoItem, "gps"); // kết thúc trigger gps
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

    private async Task<int?> PromptPoiLanguageSelectionAsync()
    {
        var supportedLanguages = GetSupportedContentLanguages(_ngonNguItems).ToList();
        return await PromptLanguageSelectionAsync(supportedLanguages, T("PoiLanguageActionTitle"));
    }

    private async Task<int?> PromptQrLanguageSelectionAsync(QrLookupResponse result)
    {
        var supportedLanguages = GetSupportedContentLanguages(result.NgonNguKhaDung).ToList();
        return await PromptLanguageSelectionAsync(supportedLanguages, T("QrLanguageActionTitle"));
    }

    private async Task<int?> PromptLanguageSelectionAsync(IReadOnlyList<NgonNguItem> supportedLanguages, string title)
    {
        if (supportedLanguages.Count == 0)
        {
            return null;
        }

        if (supportedLanguages.Count == 1)
        {
            return supportedLanguages[0].MaNgonNgu;
        }

        var actionMap = supportedLanguages.ToDictionary(
            ResolveLanguageDisplayName,
            x => x.MaNgonNgu,
            StringComparer.Ordinal);

        var selectedAction = await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.DisplayActionSheetAsync(
                title,
                T("CancelAction"),
                null,
                actionMap.Keys.ToArray()));

        if (string.IsNullOrWhiteSpace(selectedAction) || selectedAction == T("CancelAction"))
        {
            return null;
        }

        return actionMap.TryGetValue(selectedAction, out var languageId) ? languageId : null;
    }

    private IEnumerable<NgonNguItem> GetSupportedContentLanguages(IEnumerable<NgonNguItem>? preferredLanguages)
    {
        var languageIds = _allNoiDung
            .Where(x => x.MaNgonNgu > 0)
            .Select(x => x.MaNgonNgu)
            .Distinct()
            .ToHashSet();

        if (languageIds.Count == 0)
        {
            return [];
        }

        var languages = (preferredLanguages ?? [])
            .Where(x => languageIds.Contains(x.MaNgonNgu))
            .Concat(_ngonNguItems.Where(x => x.MaNgonNgu > 0 && languageIds.Contains(x.MaNgonNgu)))
            .GroupBy(x => x.MaNgonNgu)
            .Select(x => x.First())
            .ToList();

        foreach (var languageId in languageIds)
        {
            if (languages.Any(x => x.MaNgonNgu == languageId))
            {
                continue;
            }

            languages.Add(new NgonNguItem
            {
                MaNgonNgu = languageId,
                TenNgonNgu = ResolveLanguageDisplayName(languageId)
            });
        }

        return languages
            .OrderByDescending(x => x.LaMacDinh)
            .ThenBy(ResolveLanguageDisplayName)
            .ToList();
    }

    private void ApplySelectedContentLanguage(int languageId, string statusText)
    {
        ApplyPreferredLanguageSelection(languageId, syncAppLanguage: false);
        _resolvedContentMaNgonNgu = languageId;
        _isFallbackActive = false;
        _fallbackAlternativeLanguageNames = _allNoiDung
            .Where(x => x.MaNgonNgu > 0 && x.MaNgonNgu != languageId)
            .Select(x => ResolveLanguageDisplayName(x.MaNgonNgu))
            .Distinct()
            .ToList();

        SelectedPoiLabel.Text = statusText;
        UpdateFallbackLanguageLabel();
    }

    private void ApplyQrLanguageSelection(int languageId, string poiName)
    {
        ApplySelectedContentLanguage(
            languageId,
            _languageService.Format(
                "QrLanguageSelectedFormat",
                poiName,
                ResolveLanguageDisplayName(languageId)));
    }

    private string ResolveLanguageDisplayName(NoiDungItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.TenNgonNgu))
        {
            return item.TenNgonNgu;
        }

        return ResolveLanguageDisplayName(item.MaNgonNgu);
    }

    private string ResolveLanguageDisplayName(NgonNguItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.TenNgonNgu))
        {
            return item.TenNgonNgu;
        }

        return ResolveLanguageDisplayName(item.MaNgonNgu);
    }

    private string ResolveLanguageDisplayName(int languageId)
    {
        if (_ngonNguMap.TryGetValue(languageId, out var language) && !string.IsNullOrWhiteSpace(language.TenNgonNgu))
        {
            return language.TenNgonNgu;
        }

        return _languageService.Format("LanguageFallbackGenerated", languageId);
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
        var playbackRequestVersion = BeginPlaybackRequest();
        var cancellationToken = _playbackCancellationTokenSource?.Token ?? CancellationToken.None;
        var played = false;
        string? audioError = null;
        var ttsEngineError = false;

        try
        {
            var audioUrl = await _apiClient.GetPlayableAudioSourceAsync(item);
            if (!IsPlaybackRequestCurrent(playbackRequestVersion))
            {
                return;
            }

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
                try
                {
                    await SpeakTextAsync(item, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex) when (IsTtsEngineInitializationError(ex))
                {
                    ttsEngineError = true;
                    audioError = ex.Message;
                    played = false;

                    await ShowAlertAsync(
                        T("AudioPlayErrorTitle"),
                        _languageService.Format("TtsEngineUnavailableBody", ex.Message),
                        T("ConfirmAction"));
                }

                if (ttsEngineError)
                {
                    return;
                }

                played = true;
            }

        }
        catch (OperationCanceledException)
        {
            return;
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

        if (!IsPlaybackRequestCurrent(playbackRequestVersion))
        {
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
            AudioPlayerCard.IsVisible = true;
            AudioWebView.IsVisible = false;
            AudioWebView.Source = null;
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

        try
        {
            SetLoading(true);
            ClearFallbackLanguageInfo();
            var result = await _apiClient.LookupQrAsync(value);
            if (result?.DiemThamQuan is null)
            {
                await ShowAlertAsync(T("QrNotFoundTitle"), T("QrNotFoundBody"), T("ConfirmAction"));
                return;
            }

            _selectedPoi = result.DiemThamQuan;
            SelectedPoiLabel.Text = _languageService.Format("QrPoiSelectedFormat", result.DiemThamQuan.TenDiem);
            UpdateSelectedPoiMeta(result.DiemThamQuan);
            FocusMapOnPoi(result.DiemThamQuan);
            PoiCollection.SelectedItem = _visibleDiemThamQuan.FirstOrDefault(x => x.MaDiem == result.DiemThamQuan.MaDiem);
            await LoadPoiImagesAsync(result.DiemThamQuan.MaDiem);

            _allNoiDung.Clear();
            _allNoiDung.AddRange(result.NoiDung);

            if (_allNoiDung.Count == 0)
            {
                ApplyLanguageFilter();
                await ShowAlertAsync(T("NotificationTitle"), T("QrNoContentBody"), T("ConfirmAction"));
                return;
            }

            var selectedLanguageId = await PromptQrLanguageSelectionAsync(result);
            if (!selectedLanguageId.HasValue)
            {
                return;
            }

            ApplyQrLanguageSelection(selectedLanguageId.Value, result.DiemThamQuan.TenDiem);

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
            await SelectPoiAsync(poi, autoPlay: true, triggerType: "manual", focusMap: true);
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

        _suppressAutoGeofenceUntilUtc = null;
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

        StopPlayback();
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
            _suppressAutoGeofenceUntilUtc = DateTime.UtcNow.Add(GeofenceSuppressionAfterStop);
            SuppressImmediateGeofenceRetrigger();
            StartGpsTimer();
            MainThread.BeginInvokeOnMainThread(() => _ = ResumeRealGpsAfterStopAsync());
        }
    }

    private void SuppressImmediateGeofenceRetrigger()
    {
        if (_currentLocation is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var poi in _diemThamQuan)
        {
            if (!TryCreatePoiLocation(poi, out var poiLocation))
            {
                continue;
            }

            var distanceMeters = Location.CalculateDistance(_currentLocation, poiLocation, DistanceUnits.Kilometers) * 1000;
            var triggerRadiusMeters = GetEffectiveTriggerRadiusMeters(poi, _currentLocation);
            if (distanceMeters <= triggerRadiusMeters)
            {
                _lastAutoTriggerUtcByPoi[poi.MaDiem] = now;
            }
        }
    }

    private async Task ResumeRealGpsAfterStopAsync()
    {
        try
        {
            await RefreshGpsAsync(requestPermissionIfNeeded: false, showErrors: false);
        }
        catch
        {
            // best effort refresh after stopping simulated GPS
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
            UpdateGpsTestUiState();
            return;
        }

        if (!_isGpsTestRunning)
        {
            GpsTestStatusLabel.Text = _gpsTestTrack.Count > 0
                ? _languageService.Format("GpsTestReadyStatus", _gpsTestTrack.Count)
                : T("GpsTestIdleStatus");
            UpdateGpsTestProgress();
            UpdateGpsTestUiState();
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
        UpdateGpsTestUiState();
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

    private void UpdateGpsTestUiState()
    {
        if (StartGpsRouteTestButton is null || StartPoiSweepTestButton is null || StopGpsTestButton is null)
        {
            return;
        }

        var canStart = !_isGpsTestRunning;
        StartGpsRouteTestButton.IsEnabled = canStart;
        StartPoiSweepTestButton.IsEnabled = canStart;
        StopGpsTestButton.IsEnabled = !canStart;

        StartGpsRouteTestButton.Opacity = canStart ? 1 : 0.55;
        StartPoiSweepTestButton.Opacity = canStart ? 1 : 0.55;
        StopGpsTestButton.Opacity = canStart ? 0.6 : 1;

        if (GpsTestModeBadgeLabel is null || GpsTestModeBadgeBorder is null)
        {
            return;
        }

        if (_isGpsTestRunning)
        {
            GpsTestModeBadgeLabel.Text = T("GpsTestModeRunning");
            GpsTestModeBadgeLabel.TextColor = Color.FromArgb("#166246");
            GpsTestModeBadgeBorder.Background = Color.FromArgb("#DDF6EA");
            return;
        }

        if (_gpsTestTrack.Count > 0)
        {
            GpsTestModeBadgeLabel.Text = T("GpsTestModeReady");
            GpsTestModeBadgeLabel.TextColor = Color.FromArgb("#25548B");
            GpsTestModeBadgeBorder.Background = Color.FromArgb("#E5F0FF");
            return;
        }

        GpsTestModeBadgeLabel.Text = T("GpsTestModeIdle");
        GpsTestModeBadgeLabel.TextColor = Color.FromArgb("#5E738D");
        GpsTestModeBadgeBorder.Background = Color.FromArgb("#EEF3F8");
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

    private void OnStopAudioClicked(object? sender, EventArgs e)
    {
        StopPlayback();
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

    private async Task SpeakTextAsync(NoiDungItem item, CancellationToken cancellationToken)
    {
        AudioPlayerCard.IsVisible = true;
        AudioWebView.IsVisible = false;
        AudioWebView.Source = null;

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

        await TextToSpeech.Default.SpeakAsync(item.NoiDungVanBan ?? string.Empty, options, cancellationToken);
        HidePlaybackUi();

        if (_isGpsTestRunning)
        {
            AddGpsTestLog(_languageService.Format("GpsTestLogPlaybackTts", item.TieuDe ?? item.MaNoiDung.ToString(), item.TenNgonNgu ?? item.MaNgonNgu.ToString()));
        }
    }

    private static bool IsTtsEngineInitializationError(Exception exception)
    {
        return exception.Message.Contains("text to speech engine", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("tts engine", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("Failed to initialize", StringComparison.OrdinalIgnoreCase);
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

    private int BeginPlaybackRequest()
    {
        CancelPlaybackRequests();
        _playbackCancellationTokenSource = new CancellationTokenSource();
        return _playbackRequestVersion;
    }

    private bool IsPlaybackRequestCurrent(int playbackRequestVersion)
    {
        return playbackRequestVersion == _playbackRequestVersion
            && !(_playbackCancellationTokenSource?.IsCancellationRequested ?? true);
    }

    private void CancelPlaybackRequests()
    {
        _playbackRequestVersion++;

        if (_playbackCancellationTokenSource is null)
        {
            return;
        }

        if (!_playbackCancellationTokenSource.IsCancellationRequested)
        {
            _playbackCancellationTokenSource.Cancel();
        }

        _playbackCancellationTokenSource.Dispose();
        _playbackCancellationTokenSource = null;
    }

    private void StopPlayback()
    {
        CancelPlaybackRequests();
        HidePlaybackUi();
        RequestStopNativeAudioPlayback();
    }

    private void HidePlaybackUi()
    {
        AudioWebView.IsVisible = false;
        AudioWebView.Source = null;
        AudioPlayerCard.IsVisible = false;
    }

    private void RequestStopNativeAudioPlayback()
    {
#if ANDROID
        if (MainThread.IsMainThread)
        {
            StopNativeAudioPlayback();
            return;
        }

        MainThread.BeginInvokeOnMainThread(StopNativeAudioPlayback);
#endif
    }

    private void StopNativeAudioPlayback()
    {
#if ANDROID
        MediaPlayer? player;
        lock (_androidMediaPlayerGate)
        {
            player = _androidMediaPlayer;
            _androidMediaPlayer = null;
        }

        if (player is null)
        {
            return;
        }

        try
        {
            if (player.IsPlaying)
            {
                player.Stop();
            }
        }
        catch
        {
            // best effort
        }

        try
        {
            player.Reset();
        }
        catch
        {
            // best effort
        }

        try
        {
            player.Release();
        }
        catch
        {
            // best effort
        }

        player.Dispose();
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
            RequestStopNativeAudioPlayback();

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
                    RequestStopNativeAudioPlayback();
                    MainThread.BeginInvokeOnMainThread(HidePlaybackUi);
                }
            };
            player.Error += (_, args) =>
            {
                if (_isGpsTestRunning)
                {
                    AddGpsTestLog(_languageService.Format("GpsTestLogPlaybackError", item?.TieuDe ?? item?.MaNoiDung.ToString() ?? audioUrl, $"MediaPlayer {args.What}/{args.Extra}"));
                }

                RequestStopNativeAudioPlayback();
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
            lock (_androidMediaPlayerGate)
            {
                _androidMediaPlayer = player;
            }
            return true;
        }
        catch (Exception ex)
        {
            RequestStopNativeAudioPlayback();
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
        ToolbarItems.Add(new ToolbarItem
        {
            Text = "⚙️",
            Order = ToolbarItemOrder.Primary,
            Priority = 2,
            Command = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(SettingsPage));
            })
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
            "Lich su su dung",
            T("LogoutAction"));

        if (action == "Lich su su dung")
        {
            await Shell.Current.GoToAsync(nameof(UsageHistoryPage));
            return;
        }

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
        GpsTestHintLabel.Text = T("GpsTestHint");
        StartGpsRouteTestButton.Text = T("GpsTestRouteButton");
        StartPoiSweepTestButton.Text = T("GpsTestPoiSweepButton");
        StopGpsTestButton.Text = T("GpsTestStopButton");
        GpsTestProgressTitleLabel.Text = T("GpsTestProgressTitle");
        GpsTestLogTitleLabel.Text = T("GpsTestLogTitle");
        GpsTestLogEmptyLabel.Text = T("GpsTestLogEmpty");
        DisplayLanguageTitleLabel.Text = T("LanguagePreferenceLabel");
        DisplayLanguagePicker.Title = T("LanguagePreferencePickerTitle");
        QrSectionTitleLabel.Text = T("QrSectionTitle");
        QrSectionBodyLabel.Text = T("QrSectionBody");
        ScanQrButton.Text = T("ScanQr");
        PoiSectionTitleLabel.Text = T("PoiListTitle");
        PoiSearchBar.Placeholder = T("PoiSearchPlaceholder");
        PoiEmptyStateLabel.Text = T("PoiSearchEmpty");
        PoiGalleryTitleLabel.Text = T("PoiGalleryTitle");
        PoiGalleryEmptyLabel.Text = T("PoiGalleryEmpty");
        AudioPlayerTitleLabel.Text = T("AudioPlayerTitle");
        StopAudioButton.Text = T("StopAudioButton");
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

        if (_selectedPoi is not DiemThamQuanItem selectedPoi)
        {
            SelectedPoiLabel.Text = T("SelectedPoiDefault");
            SelectedPoiMetaLabel.Text = T("SelectedPoiMetaDefault");
            PoiGalleryCard.IsVisible = _selectedPoiImages.Count > 0;
        }
        else
        {
            SelectedPoiLabel.Text = _languageService.Format("SelectedPoiFormat", selectedPoi.TenDiem, selectedPoi.MaDinhDanh);
            UpdateSelectedPoiMeta(selectedPoi);
        }

        UpdateNearestPoi();
        UpdateLanguageStateLabel();
        UpdateFallbackLanguageLabel();
        UpdateGpsTestStatusLabel();
        UpdateGpsTestUiState();
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

        ApplyPoiSearchFilter();
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

    private async Task LoadPoiImagesAsync(int maDiem)
{
    var images = await _apiClient.GetHinhAnhByDiemAsync(maDiem);
    _selectedPoiImages.Clear();

    var poi = _diemThamQuan.FirstOrDefault(x => x.MaDiem == maDiem);
    var version = poi?.NgayCapNhat.Ticks ?? 0;

    foreach (var img in images)
    {
        var url = img.DuongDanHinhAnh;

        if (string.IsNullOrWhiteSpace(url))
            continue;

        // 🔥 FIX URL
        url = url.Replace("file:///", "")
                 .Replace("file:/", "")
                 .Replace("\\", "/");

        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            url = _baseUrl.TrimEnd('/') + "/" + url.TrimStart('/');

        var fileName = $"{maDiem}_{version}_{Path.GetFileName(url)}";
        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        string finalPath;

        // ✅ ƯU TIÊN LOCAL
        if (File.Exists(localPath))
        {
            finalPath = "file://" + localPath;
        }
        else
        {
            finalPath = url;
        }

        _selectedPoiImages.Add(new HinhAnhDiemThamQuanItem
        {
            DuongDanHinhAnh = finalPath,
            LaAnhDaiDien = img.LaAnhDaiDien
        });
    }

    PoiGalleryCard.IsVisible = _selectedPoiImages.Count > 0;
    PoiGalleryEmptyLabel.IsVisible = _selectedPoiImages.Count == 0;
    PoiGalleryCollection.IsVisible = _selectedPoiImages.Count > 1;

    var heroImage = _selectedPoiImages.FirstOrDefault(x => x.LaAnhDaiDien)
                    ?? _selectedPoiImages.FirstOrDefault();

    SetHeroImage(heroImage?.DuongDanHinhAnh);
    PoiGalleryCollection.SelectedItem = heroImage;
}
    private static readonly HttpClient _http = new();

private async Task<string> DownloadImageAsync(string url, string fileName)
{
    try
    {
        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        if (File.Exists(localPath))
            return localPath;

        var response = await _http.GetAsync(url);
        _http.Timeout = TimeSpan.FromSeconds(10);
        if (!response.IsSuccessStatusCode)
            return "";

        var bytes = await response.Content.ReadAsByteArrayAsync();

        if (bytes.Length == 0)
            return "";

        File.WriteAllBytes(localPath, bytes);

        return localPath;
    }
    catch
    {
        return "";
    }
}
    private void SetHeroImage(string? imageUrl)
    {
        SelectedPoiHeroImage.Source = GetFullUrl(imageUrl);
        PoiGalleryEmptyLabel.IsVisible = string.IsNullOrWhiteSpace(imageUrl);
    }

    private void UpdateSelectedPoiMeta(DiemThamQuanItem? poi)
    {
        if (poi is null)
        {
            SelectedPoiMetaLabel.Text = T("SelectedPoiMetaDefault");
            return;
        }

        SelectedPoiMetaLabel.Text = string.IsNullOrWhiteSpace(poi.MoTaNgan)
            ? (poi.DiaChi ?? T("SelectedPoiMetaDefault"))
            : $"{poi.MoTaNgan}{(string.IsNullOrWhiteSpace(poi.DiaChi) ? string.Empty : $" | {poi.DiaChi}")}";
    }

    private void OnPoiImageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not HinhAnhDiemThamQuanItem image)
        {
            return;
        }

        SetHeroImage(image.DuongDanHinhAnh);
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
