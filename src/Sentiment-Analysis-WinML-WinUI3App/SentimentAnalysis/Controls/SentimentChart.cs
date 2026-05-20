using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using SentimentAnalysis.Models;
using Windows.Foundation;

namespace SentimentAnalysis.Controls;

public sealed class SentimentChart : UserControl
{
    private readonly Canvas _canvas = new();

    public static readonly DependencyProperty SnapshotsProperty =
        DependencyProperty.Register(
            nameof(Snapshots),
            typeof(ObservableCollection<SentimentSnapshot>),
            typeof(SentimentChart),
            new PropertyMetadata(null, OnSnapshotsChanged));

    public ObservableCollection<SentimentSnapshot>? Snapshots
    {
        get => (ObservableCollection<SentimentSnapshot>?)GetValue(SnapshotsProperty);
        set => SetValue(SnapshotsProperty, value);
    }

    public SentimentChart()
    {
        Content = _canvas;
        _canvas.SizeChanged += (_, _) => Redraw();
    }

    private static void OnSnapshotsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chart = (SentimentChart)d;

        if (e.OldValue is ObservableCollection<SentimentSnapshot> oldCol)
        {
            oldCol.CollectionChanged -= chart.OnCollectionChanged;
        }

        if (e.NewValue is ObservableCollection<SentimentSnapshot> newCol)
        {
            newCol.CollectionChanged += chart.OnCollectionChanged;
        }

        chart.Redraw();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Redraw();

    private void Redraw()
    {
        _canvas.Children.Clear();
        var snapshots = Snapshots;
        if (snapshots == null || snapshots.Count < 2) return;

        double w = _canvas.ActualWidth;
        double h = _canvas.ActualHeight;
        if (w <= 0 || h <= 0) return;

        double padding = 4;
        double chartW = w - padding * 2;
        double chartH = h - padding * 2;

        // Draw horizontal grid lines and labels
        for (int i = 0; i <= 4; i++)
        {
            double y = padding + chartH * (1.0 - i / 4.0);
            var gridLine = new Line
            {
                X1 = padding,
                Y1 = y,
                X2 = w - padding,
                Y2 = y,
                Stroke = new SolidColorBrush(ColorHelper.FromArgb(30, 255, 255, 255)),
                StrokeThickness = 1
            };
            _canvas.Children.Add(gridLine);
        }

        // Three lines: positive %, neutral %, negative %
        DrawLine(snapshots, s => snapshots[snapshots.IndexOf(s)].TotalMessages > 0
            ? (double)s.PositiveCount / s.TotalMessages * 100 : 0,
            ColorHelper.FromArgb(255, 16, 185, 129), chartW, chartH, padding);

        DrawLine(snapshots, s => snapshots[snapshots.IndexOf(s)].TotalMessages > 0
            ? (double)s.NeutralCount / s.TotalMessages * 100 : 0,
            ColorHelper.FromArgb(255, 99, 102, 241), chartW, chartH, padding);

        DrawLine(snapshots, s => snapshots[snapshots.IndexOf(s)].TotalMessages > 0
            ? (double)s.NegativeCount / s.TotalMessages * 100 : 0,
            ColorHelper.FromArgb(255, 239, 68, 68), chartW, chartH, padding);
    }

    private void DrawLine(
        ObservableCollection<SentimentSnapshot> snapshots,
        Func<SentimentSnapshot, double> valueSelector,
        Windows.UI.Color color,
        double chartW, double chartH, double padding)
    {
        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round
        };

        int count = snapshots.Count;
        double stepX = count > 1 ? chartW / (count - 1) : 0;

        for (int i = 0; i < count; i++)
        {
            double val = valueSelector(snapshots[i]);
            double x = padding + i * stepX;
            // y: 0% at bottom, 100% at top
            double y = padding + chartH * (1.0 - val / 100.0);
            polyline.Points.Add(new Point(x, y));
        }

        _canvas.Children.Add(polyline);
    }
}
