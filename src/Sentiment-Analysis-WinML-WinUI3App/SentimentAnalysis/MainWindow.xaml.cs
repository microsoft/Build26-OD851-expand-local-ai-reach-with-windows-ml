using Microsoft.UI.Xaml;
using SentimentAnalysis.ViewModels;

namespace SentimentAnalysis;

public sealed partial class MainWindow : Window
{
    public DashboardViewModel ViewModel { get; }
    private PopoutChartWindow? _popoutWindow;

    public MainWindow()
    {
        ViewModel = new DashboardViewModel();
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        Closed += OnMainWindowClosed;
    }

    private void OnPopoutClick(object sender, RoutedEventArgs e)
    {
        if (_popoutWindow != null) return;

        _popoutWindow = new PopoutChartWindow(ViewModel.Snapshots);
        _popoutWindow.Closed += OnPopoutWindowClosed;
        _popoutWindow.Activate();

        PopoutButton.IsEnabled = false;
    }

    private void OnPopoutWindowClosed(object sender, WindowEventArgs e)
    {
        _popoutWindow = null;
        PopoutButton.IsEnabled = true;
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs e)
    {
        _popoutWindow?.Close();
        ViewModel.Dispose();
    }
}
