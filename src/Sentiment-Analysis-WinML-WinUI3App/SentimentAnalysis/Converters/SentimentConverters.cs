using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using SentimentAnalysis.Models;

namespace SentimentAnalysis.Converters;

public class SentimentToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush PositiveBrush = new(ColorHelper.FromArgb(255, 16, 185, 129));
    private static readonly SolidColorBrush NeutralBrush = new(ColorHelper.FromArgb(255, 99, 102, 241));
    private static readonly SolidColorBrush NegativeBrush = new(ColorHelper.FromArgb(255, 239, 68, 68));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Sentiment sentiment)
        {
            return sentiment switch
            {
                Sentiment.Positive => PositiveBrush,
                Sentiment.Neutral => NeutralBrush,
                Sentiment.Negative => NegativeBrush,
                _ => NeutralBrush
            };
        }
        return NeutralBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class SentimentToEmojiConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Sentiment sentiment)
        {
            return sentiment switch
            {
                Sentiment.Positive => "??",
                Sentiment.Neutral => "??",
                Sentiment.Negative => "??",
                _ => "??"
            };
        }
        return "??";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class SentimentToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Sentiment sentiment)
        {
            return sentiment switch
            {
                Sentiment.Positive => "Positive",
                Sentiment.Neutral => "Neutral",
                Sentiment.Negative => "Negative",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
