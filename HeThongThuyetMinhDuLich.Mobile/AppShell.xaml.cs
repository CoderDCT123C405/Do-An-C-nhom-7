namespace HeThongThuyetMinhDuLich.Mobile;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage)
	{
		InitializeComponent();
		HomeShellContent.Content = mainPage;
	}
}
