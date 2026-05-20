using System.Collections.ObjectModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using SentimentAnalysis.Models;
using WinRT.Interop;

namespace SentimentAnalysis;

public sealed partial class PopoutChartWindow : Window
{
    public PopoutChartWindow(ObservableCollection<SentimentSnapshot> snapshots)
    {
        InitializeComponent();
        Chart.Snapshots = snapshots;

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.Resize(new Windows.Graphics.SizeInt32(960, 640));

        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
        var workArea = displayArea.WorkArea;
        appWindow.Move(new Windows.Graphics.PointInt32(
            workArea.X + workArea.Width - 960,
            workArea.Y + (workArea.Height - 640) / 2));

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = true;
        }
    }
}
