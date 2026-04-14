using HeThongThuyetMinhDuLich.Mobile.Services;
using System.Reflection;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace HeThongThuyetMinhDuLich.Mobile;

public class QrScannerPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _resultSource = new();
    private readonly CameraBarcodeReaderView _cameraView;
    private readonly LanguageService? _languageService;
    private bool _completed;

    private QrScannerPage(LanguageService? languageService)
    {
        _languageService = languageService;
        Title = T("QrScannerTitle");
        BackgroundColor = Colors.Black;

        _cameraView = new CameraBarcodeReaderView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            IsDetecting = true,
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            }
        };
        ConfigurePreferredRearCamera();
        _cameraView.BarcodesDetected += OnBarcodesDetected;

        var closeButton = new Button
        {
            Text = T("CloseAction"),
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
                    Text = T("QrScannerHint"),
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
        var languageService = Application.Current?.Handler?.MauiContext?.Services.GetService<LanguageService>();
        var page = new QrScannerPage(languageService);
        await navigation.PushModalAsync(page, false);
        return await page._resultSource.Task;
    }

    private string T(string key) => _languageService?.GetText(key) ?? key;

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

        _cameraView.IsDetecting = false;
        _cameraView.BarcodesDetected -= OnBarcodesDetected;

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
        await Navigation.PopModalAsync(false);
    }

    private void ConfigurePreferredRearCamera()
    {
        try
        {
            var cameraLocationProperty = _cameraView.GetType().GetProperty("CameraLocation", BindingFlags.Instance | BindingFlags.Public);
            if (cameraLocationProperty?.CanWrite == true)
            {
                var cameraLocationType = cameraLocationProperty.PropertyType;
                var rearValue = Enum.GetNames(cameraLocationType)
                    .FirstOrDefault(name => string.Equals(name, "Rear", StringComparison.OrdinalIgnoreCase) ||
                                            string.Equals(name, "Back", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(rearValue))
                {
                    cameraLocationProperty.SetValue(_cameraView, Enum.Parse(cameraLocationType, rearValue));
                    return;
                }
            }

            var selectedCameraProperty = _cameraView.GetType().GetProperty("SelectedCamera", BindingFlags.Instance | BindingFlags.Public);
            var camerasProperty = _cameraView.GetType().GetProperty("Cameras", BindingFlags.Instance | BindingFlags.Public);
            var cameras = camerasProperty?.GetValue(_cameraView) as System.Collections.IEnumerable;
            if (selectedCameraProperty?.CanWrite != true || cameras is null)
            {
                return;
            }

            object? preferredRearCamera = null;
            foreach (var camera in cameras)
            {
                if (camera is null)
                {
                    continue;
                }

                var name = camera.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public)?.GetValue(camera)?.ToString();
                var locationValue = camera.GetType().GetProperty("Location", BindingFlags.Instance | BindingFlags.Public)?.GetValue(camera)?.ToString();
                if ((locationValue?.Contains("Rear", StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (locationValue?.Contains("Back", StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (name?.Contains("Rear", StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (name?.Contains("Back", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    preferredRearCamera = camera;
                    break;
                }
            }

            if (preferredRearCamera is not null)
            {
                selectedCameraProperty.SetValue(_cameraView, preferredRearCamera);
            }
        }
        catch
        {
            // best effort: scanner still works if camera selection API differs by platform/package version
        }
    }
}
