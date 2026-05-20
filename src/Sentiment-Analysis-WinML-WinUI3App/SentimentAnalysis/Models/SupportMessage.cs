using System;

namespace SentimentAnalysis.Models;

public enum Sentiment
{
    Pending,
    Positive,
    Neutral,
    Negative
}

public class SupportMessage
{
    public string CustomerName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Sentiment Sentiment { get; set; }
    public double SentimentScore { get; set; }
    public DateTime Timestamp { get; init; }
    public bool IsAnalyzed { get; set; }
}
