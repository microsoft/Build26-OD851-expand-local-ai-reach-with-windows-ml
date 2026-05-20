using System;

namespace SentimentAnalysis.Models;

public class SentimentSnapshot
{
    public DateTime Timestamp { get; set; }
    public int TotalMessages { get; set; }
    public int PositiveCount { get; set; }
    public int NeutralCount { get; set; }
    public int NegativeCount { get; set; }
    public double AverageScore { get; set; }
}
