using System.Collections.ObjectModel;
using HeThongThuyetMinhDuLich.Mobile.Models;
using HeThongThuyetMinhDuLich.Mobile.Services;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class UsageHistoryPage : ContentPage
{
    private readonly MobileApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly ObservableCollection<UsageHistoryViewItem> _items = [];

    public UsageHistoryPage(MobileApiClient apiClient, AuthSession authSession)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _authSession = authSession;
        HistoryCollection.ItemsSource = _items;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        ReloadButton.IsEnabled = false;
        try
        {
            if (!_authSession.IsAuthenticated)
            {
                _items.Clear();
                await DisplayAlertAsync("Thong bao", "Ban can dang nhap de xem lich su su dung.", "Dong");
                return;
            }

            var history = await _apiClient.GetLichSuSuDungCuaToiAsync();
            _items.Clear();
            foreach (var item in history)
            {
                _items.Add(new UsageHistoryViewItem(item));
            }
        }
        finally
        {
            ReloadButton.IsEnabled = true;
        }
    }

    private async void OnReloadClicked(object? sender, EventArgs e)
    {
        await LoadAsync();
    }

    private sealed class UsageHistoryViewItem
    {
        public UsageHistoryViewItem(LichSuSuDungItem source)
        {
            TieuDeNoiDung = string.IsNullOrWhiteSpace(source.TieuDeNoiDung)
                ? $"Noi dung #{source.MaNoiDung}"
                : source.TieuDeNoiDung;
            TenDiem = string.IsNullOrWhiteSpace(source.TenDiem)
                ? $"Ma diem {source.MaDiem}"
                : source.TenDiem;
            CachKichHoat = string.IsNullOrWhiteSpace(source.CachKichHoat) ? "manual" : source.CachKichHoat;
            ThoiGianBatDauDisplay = $"Bat dau: {source.ThoiGianBatDau:dd/MM/yyyy HH:mm}";
            ThoiLuongDisplay = $"Da nghe: {FormatDuration(source.ThoiLuongDaNghe ?? 0)}";
        }

        public string TieuDeNoiDung { get; }
        public string TenDiem { get; }
        public string CachKichHoat { get; }
        public string ThoiGianBatDauDisplay { get; }
        public string ThoiLuongDisplay { get; }

        private static string FormatDuration(int totalSeconds)
        {
            var seconds = Math.Max(0, totalSeconds);
            var span = TimeSpan.FromSeconds(seconds);
            if (span.TotalHours >= 1)
            {
                return $"{(int)span.TotalHours}h {span.Minutes}m {span.Seconds}s";
            }

            if (span.TotalMinutes >= 1)
            {
                return $"{span.Minutes}m {span.Seconds}s";
            }

            return $"{span.Seconds}s";
        }
    }
}
