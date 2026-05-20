/**
 * Message generation service - produces simulated customer support messages.
 * Each message retains its "ground truth" sentiment from the pool it was drawn from.
 */

const FIRST_NAMES = [
    'Emma', 'Liam', 'Olivia', 'Noah', 'Ava', 'James', 'Sophia', 'William',
    'Isabella', 'Oliver', 'Mia', 'Benjamin', 'Charlotte', 'Elijah', 'Amelia',
    'Lucas', 'Harper', 'Mason', 'Evelyn', 'Logan'
];

const LAST_NAMES = [
    'Smith', 'Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller',
    'Davis', 'Rodriguez', 'Martinez', 'Hernandez', 'Lopez', 'Gonzalez',
    'Wilson', 'Anderson', 'Thomas', 'Taylor', 'Moore', 'Jackson', 'Martin'
];

const NEGATIVE_MESSAGES = [
    "I've been waiting over 30 minutes for a response. This is unacceptable!",
    "Your product crashed again and I lost all my work. I'm furious.",
    "The billing department charged me twice. Fix this immediately!",
    "I can't believe how terrible the quality of your service has become.",
    "This is the worst customer experience I've ever had.",
    "Your app is completely broken. Nothing works!",
    "I want a full refund. This product is garbage.",
    "How is it possible that your servers are down AGAIN?",
    "I've been a customer for 5 years and I've never been treated this poorly.",
    "Your support team is useless. Nobody can solve my problem.",
    "I'm switching to a competitor. You've lost a loyal customer.",
    "The update broke everything. Who tested this?",
    "I've called three times and nobody has helped me.",
    "This is fraud. You charged me for features that don't work.",
    "Your product is a complete waste of money.",
    "I'm filing a complaint. This level of service is unacceptable.",
    "The delivery was late and the product was damaged.",
    "Your website is impossible to navigate. It's 2025, not 2005.",
    "I've been on hold for an hour. Do you even care about customers?",
    "The premium plan is a scam. The free version was better."
];

const NEUTRAL_MESSAGES = [
    "Can you tell me how to change my account settings?",
    "I'd like to know the status of my recent order.",
    "What are your business hours for phone support?",
    "I need to update my billing information.",
    "Can you send me a copy of my latest invoice?",
    "How do I reset my password?",
    "I'm looking for information about your enterprise plan.",
    "What's the difference between the basic and premium plans?",
    "Can I schedule a demo of your product?",
    "I need to transfer my license to a new device.",
    "What file formats does your product support?",
    "Is there a way to export my data?",
    "I'm trying to integrate with your API. Where's the documentation?",
    "Can you confirm my subscription renewal date?",
    "I'd like to add another user to my account.",
    "What's your policy on data retention?",
    "Do you offer educational discounts?",
    "I need an update on my support ticket #4521.",
    "How do I configure the notification settings?",
    "Can you walk me through the setup process?"
];

const POSITIVE_MESSAGES = [
    "Just wanted to say the new update is fantastic! Great work!",
    "Your support team resolved my issue in minutes. Impressive!",
    "I love the new features you added. Keep it up!",
    "Thank you for the quick response. Very helpful!",
    "Your product has completely transformed our workflow. Amazing!",
    "The customer service I received today was outstanding.",
    "I've recommended your product to all my colleagues.",
    "The new dashboard design is beautiful and intuitive.",
    "Your team went above and beyond to help me. Thank you!",
    "I'm really impressed with the performance improvements.",
    "Best purchase decision our company has made this year!",
    "The onboarding experience was smooth and well-designed.",
    "Your documentation is excellent. Easy to follow!",
    "I appreciate how responsive your support team is.",
    "The mobile app works perfectly. Great job!",
    "Five stars! This product exceeds all my expectations.",
    "Thank you for listening to customer feedback and improving.",
    "The integration was seamless. Took less than an hour!",
    "Your product reliability is the best in the industry.",
    "I'm amazed at how fast and helpful your chat support is!"
];

const ALL_MESSAGES = [
    ...NEGATIVE_MESSAGES,
    ...NEUTRAL_MESSAGES,
    ...POSITIVE_MESSAGES
];

function randomItem(arr) {
    return arr[Math.floor(Math.random() * arr.length)];
}

/**
 * Generate a random support message.
 * @returns {{ customerName: string, message: string, sentiment: string, sentimentScore: number, timestamp: Date, isAnalyzed: boolean }}
 */
export function generateMessage() {
    return {
        customerName: `${randomItem(FIRST_NAMES)} ${randomItem(LAST_NAMES)}`,
        message: randomItem(ALL_MESSAGES),
        sentiment: 'pending',
        sentimentScore: 0,
        timestamp: new Date(),
        isAnalyzed: false
    };
}
