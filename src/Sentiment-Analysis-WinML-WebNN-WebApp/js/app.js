/**
 * Contoso Support Dashboard — main application controller.
 */

import { generateMessage } from './messageGenerator.js';
import * as analyzer from './sentimentAnalyzer.js';
import { SentimentChart } from './sentimentChart.js';

// --- Configuration ---
const MSG_INTERVAL_MS     = 200;   // generate messages every 200ms
const SNAPSHOT_INTERVAL_MS = 3000; // chart snapshot every 3 seconds
const MAX_DISPLAY_MESSAGES = 200;
const MAX_SNAPSHOTS        = 40;   // ~2 minute window
const MSG_BATCH_MIN        = 1;
const MSG_BATCH_MAX        = 3;

// --- DOM refs ---
const $totalMessages  = document.getElementById('totalMessages');
const $messagesPerSec = document.getElementById('messagesPerSec');
const $positiveCount  = document.getElementById('positiveCount');
const $neutralCount   = document.getElementById('neutralCount');
const $negativeCount  = document.getElementById('negativeCount');
const $modelStatus    = document.getElementById('modelStatus');
const $toggleStream   = document.getElementById('toggleStream');
const $messageFeed    = document.getElementById('messageFeed');
const $chartCanvas    = document.getElementById('sentimentChart');

// --- State ---
let totalMessages = 0;
let positiveCount = 0;
let neutralCount  = 0;
let negativeCount = 0;
let isRunning     = false;

/** @type {Array<{ customerName: string, message: string, sentiment: string, sentimentScore: number, timestamp: Date, isAnalyzed: boolean, element?: HTMLElement }>} */
const analysisQueue = [];
const snapshots = [];
let msgTimerId = null;
let snapshotTimerId = null;
let throughputStartTime = null;
let messagesAtStart = 0;

// --- Chart ---
const chart = new SentimentChart($chartCanvas);

// --- Emoji / badge helpers ---
const EMOJI = { positive: '😊', neutral: '😐', negative: '😠', pending: '❓' };
const BADGE_CLASS = { positive: 'badge-positive', neutral: 'badge-neutral', negative: 'badge-negative', pending: 'badge-pending' };

function formatTime(date) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

// --- UI helpers ---
function updateStats() {
    $totalMessages.textContent  = totalMessages;
    $positiveCount.textContent  = positiveCount;
    $neutralCount.textContent   = neutralCount;
    $negativeCount.textContent  = negativeCount;

    // Messages per second (rolling average over last 5 seconds)
    if (throughputStartTime) {
        const elapsed = (Date.now() - throughputStartTime) / 1000;
        if (elapsed > 0.5) {
            const mps = (totalMessages - messagesAtStart) / elapsed;
            $messagesPerSec.textContent = mps.toFixed(1);
        }
    }
}

function createMessageElement(msg) {
    const el = document.createElement('div');
    el.className = 'message-item';

    el.innerHTML = `
        <div class="message-emoji">${EMOJI[msg.sentiment]}</div>
        <div class="message-content">
            <div class="message-customer">${escapeHtml(msg.customerName)}</div>
            <div class="message-text">${escapeHtml(msg.message)}</div>
        </div>
        <div class="message-meta">
            <span class="sentiment-badge ${BADGE_CLASS[msg.sentiment]}">${capitalize(msg.sentiment)}</span>
            <span class="message-time">${formatTime(msg.timestamp)}</span>
        </div>
    `;

    return el;
}

function updateMessageElement(msg) {
    if (!msg.element) return;
    const emojiEl = msg.element.querySelector('.message-emoji');
    const badgeEl = msg.element.querySelector('.sentiment-badge');
    if (emojiEl) emojiEl.textContent = EMOJI[msg.sentiment];
    if (badgeEl) {
        badgeEl.textContent = capitalize(msg.sentiment);
        badgeEl.className = `sentiment-badge ${BADGE_CLASS[msg.sentiment]}`;
    }
}

function escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

function capitalize(s) {
    return s.charAt(0).toUpperCase() + s.slice(1);
}

// --- Message generation ---
function generateBatch() {
    const count = MSG_BATCH_MIN + Math.floor(Math.random() * (MSG_BATCH_MAX - MSG_BATCH_MIN + 1));
    for (let i = 0; i < count; i++) {
        const msg = generateMessage();
        totalMessages++;

        const el = createMessageElement(msg);
        msg.element = el;
        $messageFeed.prepend(el);

        // Cap displayed messages
        while ($messageFeed.children.length > MAX_DISPLAY_MESSAGES) {
            $messageFeed.lastElementChild.remove();
        }

        analysisQueue.push(msg);
    }
    updateStats();
}

// --- Analysis processing (async queue) ---
let analysisRunning = false;

async function processQueue() {
    if (analysisRunning) return;
    analysisRunning = true;

    while (analysisQueue.length > 0) {
        const msg = analysisQueue.shift();
        try {
            const result = await analyzer.analyze(msg);
            msg.sentiment = result.sentiment;
            msg.sentimentScore = result.score;
            msg.isAnalyzed = true;

            // Update counters
            if (result.sentiment === 'positive') positiveCount++;
            else if (result.sentiment === 'neutral') neutralCount++;
            else if (result.sentiment === 'negative') negativeCount++;

            updateMessageElement(msg);
            updateStats();
        } catch (err) {
            console.error('Analysis failed:', err);
        }
    }

    analysisRunning = false;
}

// Continuously drain queue
setInterval(() => {
    if (analysisQueue.length > 0) processQueue();
}, 50);

// --- Snapshots ---
/** @type {Array<{ timestamp: Date, totalMessages: number, positiveCount: number, neutralCount: number, negativeCount: number, averageScore: number }>} */

function createSnapshot() {
    const snap = {
        timestamp: new Date(),
        totalMessages,
        positiveCount,
        neutralCount,
        negativeCount,
        averageScore: totalMessages > 0
            ? (positiveCount - negativeCount) / totalMessages
            : 0
    };

    snapshots.push(snap);
    while (snapshots.length > MAX_SNAPSHOTS) {
        snapshots.shift();
    }

    chart.update(snapshots);
}

// --- Stream control ---
function startStream() {
    if (isRunning) return;
    isRunning = true;

    throughputStartTime = Date.now();
    messagesAtStart = totalMessages;

    msgTimerId = setInterval(generateBatch, MSG_INTERVAL_MS);
    snapshotTimerId = setInterval(createSnapshot, SNAPSHOT_INTERVAL_MS);

    $toggleStream.textContent = '⏸ Pause';
}

function stopStream() {
    if (!isRunning) return;
    isRunning = false;

    clearInterval(msgTimerId);
    clearInterval(snapshotTimerId);
    msgTimerId = null;
    snapshotTimerId = null;

    $toggleStream.textContent = '▶ Resume';
}

$toggleStream.addEventListener('click', () => {
    if (isRunning) stopStream();
    else startStream();
});

// --- Initialize ---
async function init() {
    analyzer.onStatusChanged(status => {
        $modelStatus.textContent = status;
    });

    await analyzer.initialize();

    // Hide status after a brief display of "Model ready"
    setTimeout(() => {
        $modelStatus.classList.add('hidden');
    }, 1500);

    // Enable controls and auto-start
    $toggleStream.disabled = false;
    startStream();
}

init().catch(err => {
    $modelStatus.textContent = `Error: ${err.message}`;
    $modelStatus.style.color = '#ff6b6b';
    console.error('Initialization failed:', err);
});
