using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace HeThongThuyetMinhDuLich.Mobile;

public class QrScannerPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _resultSource = new();
    private readonly CameraBarcodeReaderView _cameraView;
    private bool _completed;

    private QrScannerPage()
    {
        Title = "Quet QR";
        BackgroundColor = Colors.Black;

        _cameraView = new CameraBarcodeReaderView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            IsDetecting = true,
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.TwoDimensional,
                AutoRotate = true,
                Multiple = false
            }
        };
        _cameraView.BarcodesDetected += OnBarcodesDetected;

        var closeButton = new Button
        {
            Text = "Dong",
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#B00020"),
            CornerRadius = 10,
            HorizontalOptions = LayoutOptions.Fill
        };
        closeButton.Clicked += async (_, _) => await FinishAsync(null);

        var actionPanel = new VerticalStackLayout
        {
            Padding = new Thickness(16, 12),
            Spacing = 10,
            BackgroundColor = Color.FromArgb("#CC111111"),
            Children =
            {
                new Label
                {
                    Text = "Dua ma QR vao khung de quet",
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                closeButton
            }
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        root.Children.Add(_cameraView);
        Grid.SetRow(_cameraView, 0);
        root.Children.Add(actionPanel);
        Grid.SetRow(actionPanel, 1);

        Content = root;
    }

    public static async Task<string?> ScanAsync(INavigation navigation)
    {
        var page = new QrScannerPage();
        await navigation.PushModalAsync(page);
        return await page._resultSource.Task;
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_completed)
        {
            return;
        }

        var value = e.Results.FirstOrDefault()?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(() => FinishAsync(value));
    }

    private async Task FinishAsync(string? result)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _cameraView.IsDetecting = false;
        _cameraView.BarcodesDetected -= OnBarcodesDetected;
        _resultSource.TrySetResult(result);
        await Navigation.PopModalAsync();
    }
}
