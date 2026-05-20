using System;
using System.Collections.Generic;

namespace SentimentAnalysis.Services;

public static class MessageGeneratorService
{
    private static readonly string[] FirstNames =
    [
        "Alex", "Jordan", "Taylor", "Morgan", "Casey", "Riley", "Quinn", "Avery",
        "Skyler", "Dakota", "Cameron", "Reese", "Parker", "Hayden", "Emerson",
        "Jamie", "Drew", "Sage", "Rowan", "Finley", "Blake", "Charlie", "Sam",
        "Pat", "Robin", "Lee", "Chris", "Dana", "Kim", "Lynn"
    ];

    private static readonly string[] LastNames =
    [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller",
        "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez",
        "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin"
    ];

    private static readonly string[] NegativeMessages =
    [
        "I've been waiting over 30 minutes for a response. This is unacceptable!",
        "Your product crashed again and I lost all my work. Very frustrated.",
        "I was charged twice for the same order. I need an immediate refund.",
        "The delivery was supposed to arrive yesterday. Still nothing.",
        "Your website is incredibly slow and keeps timing out.",
        "I've called three times about this issue and nobody has helped me.",
        "The quality of the product is terrible compared to what was advertised.",
        "I can't believe how difficult it is to cancel my subscription.",
        "Your update broke everything. My entire workflow is disrupted.",
        "I've been a loyal customer for 5 years and this is how I'm treated?",
        "The software keeps freezing whenever I try to export my data.",
        "I requested a callback two days ago and still haven't heard back.",
        "This is the worst customer service I've ever experienced.",
        "My account was locked for no reason and support is unreachable.",
        "The new UI is horrible. Why did you change something that worked?",
        "I found a security vulnerability and nobody seems to care.",
        "Your billing system charged my expired card and now I have fees.",
        "The feature you removed was the only reason I used your product.",
        "I can't access my files after the latest update. This is critical!",
        "Your chatbot is useless and just goes in circles."
    ];

    private static readonly string[] NeutralMessages =
    [
        "Can you tell me how to change my account settings?",
        "I'd like to know the status of my recent order.",
        "What are your business hours for phone support?",
        "I need to update my billing information.",
        "How do I add another user to my account?",
        "Is there a way to export my data to CSV?",
        "I'm looking for documentation on the API.",
        "Can I schedule a product demo for my team?",
        "What's the difference between the Basic and Pro plans?",
        "I need to change my shipping address for order #4521.",
        "Where can I find the release notes for the latest version?",
        "How do I enable two-factor authentication?",
        "I need a copy of my invoice from last month.",
        "Is there a mobile app available for Android?",
        "Can you confirm my subscription renewal date?",
        "How do I integrate with third-party tools?",
        "I'd like to request access to the beta program.",
        "What file formats are supported for import?",
        "Do you offer educational or nonprofit discounts?",
        "How do I reset my password?"
    ];

    private static readonly string[] PositiveMessages =
    [
        "Just wanted to say the new update is fantastic! Great work!",
        "Your support team resolved my issue in minutes. Thank you!",
        "I love how intuitive the new dashboard is. Well done!",
        "The onboarding experience was smooth and easy. Very impressed!",
        "Your product has saved our team hours of work every week.",
        "Excellent response time from your support team. Keep it up!",
        "The new feature you added is exactly what I needed. Thank you!",
        "I recommended your product to my entire network. It's that good.",
        "Your documentation is thorough and easy to follow. Much appreciated!",
        "The migration tool worked flawlessly. Seamless experience!",
        "I've been using your product for a year and it keeps getting better.",
        "Your team went above and beyond to help me. Truly grateful.",
        "The performance improvements in the latest release are noticeable!",
        "Best customer support I've ever experienced. Five stars!",
        "The collaboration features are a game changer for our remote team.",
        "Setup took less than 5 minutes. That's how it should be!",
        "I'm blown away by the attention to detail in this release.",
        "Your pricing is very fair for the value provided.",
        "The mobile experience is just as good as desktop. Impressive!",
        "Thank you for listening to customer feedback and implementing changes."
    ];

    private static readonly string[][] AllMessagePools = [NegativeMessages, NeutralMessages, PositiveMessages];
    private static readonly Random _random = new();

    public static Models.SupportMessage GenerateMessage()
    {
        var firstName = FirstNames[_random.Next(FirstNames.Length)];
        var lastName = LastNames[_random.Next(LastNames.Length)];
        var customerName = $"{firstName} {lastName}";

        // Pick a random message from any pool
        var pool = AllMessagePools[_random.Next(AllMessagePools.Length)];
        var message = pool[_random.Next(pool.Length)];

        return new Models.SupportMessage
        {
            CustomerName = customerName,
            Message = message,
            Timestamp = DateTime.Now
        };
    }
}
