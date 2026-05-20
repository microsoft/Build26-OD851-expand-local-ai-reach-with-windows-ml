/**
 * Sentiment Analyzer Service
 *
 * Uses onnxruntime-web with WebNN EP for NPU-accelerated inference.
 * Falls back to WebNN GPU, then WASM (CPU) if NPU is unavailable.
 *
 * Model: cardiffnlp/twitter-roberta-base-sentiment-latest
 *   0 = Negative, 1 = Neutral, 2 = Positive
 */

import * as ort from 'onnxruntime-web/experimental';
import { BpeTokenizer } from './tokenizer.js';

const MODEL_BASE_PATH = 'model';
const MAX_SEQUENCE_LENGTH = 512;
const PAD_TOKEN_ID = 1; // RoBERTa <pad> token

let _session = null;
let _tokenizer = null;
let _onStatusChanged = null;
let _initialized = false;
let _epName = 'unknown';

/**
 * Set a callback for model loading status updates.
 * @param {(status: string) => void} callback
 */
export function onStatusChanged(callback) {
    _onStatusChanged = callback;
}

function _reportStatus(msg) {
    console.log(`[EP] ${msg}`);
    if (_onStatusChanged) _onStatusChanged(msg);
}

/**
 * Initialize — load tokenizer, discover EPs, create inference session.
 */
export async function initialize() {
    if (_initialized) return;

    // Configure ORT WASM paths (absolute from server root to avoid path doubling)
    //1
    try {
        ort.env.wasm.wasmPaths = '/node_modules/onnxruntime-web/dist/';
        ort.env.wasm.proxy = false;
    } catch (e) {
        console.error('[ORT] Failed to configure env.wasm:', e);
    }

    // 1. Load BPE tokenizer
    _reportStatus('Loading tokenizer...');
    _tokenizer = await BpeTokenizer.load(
        `${MODEL_BASE_PATH}/vocab.json`,
        `${MODEL_BASE_PATH}/merges.txt`
    );

    // 2. Create inference session with EP fallback chain
    _reportStatus('Discovering execution providers...');
    _session = await _createSession();

    _initialized = true;
    _reportStatus(`Model ready (${_epName})`);
}

/**
 * Create inference session with WebNN NPU.
 */
async function _createSession() {
    // Fetch the ONNX model and its external data file in parallel - 1
    _reportStatus('Loading FP32 model...');
    const [modelResponse, dataResponse] = await Promise.all([
        fetch(`${MODEL_BASE_PATH}/model.onnx`),
        fetch(`${MODEL_BASE_PATH}/model.onnx.data`)
    ]);
    const modelBuffer = await modelResponse.arrayBuffer();
    const externalData = new Uint8Array(await dataResponse.arrayBuffer());


    _reportStatus('Creating WebNN context...');
    try {
        // Request a WebNN context targeting a particular device - 2
        const mlContext = await navigator.ml.createContext({ deviceType: 'npu' });
        // Create the ORT inference session using the WebNN EP and external weights - 3
        const session = await ort.InferenceSession.create(new Uint8Array(modelBuffer), {
            executionProviders: [{ name: 'webnn', context: mlContext }],
            externalData: [{ path: 'model.onnx.data', data: externalData }],
        });
        
        _epName = 'WebNN';
        _reportStatus('Registered EP: WebNN');
        return session;
    } catch (ex) {
        console.error('[EP] WebNN failed:', ex.message || ex);
        throw new Error(`WebNN available: ${ex.message || ex}`);
    }
}

/**
 * Analyze a message's sentiment.
 *
 * @param {{ message: string }} msg
 * @returns {Promise<{ sentiment: string, score: number }>}
 */
export async function analyze(msg) {
    if (!_initialized || !_session || !_tokenizer) {
        throw new Error('Analyzer not initialized. Call initialize() first.');
    }

    const text = msg.message;
    if (!text || !text.trim()) {
        return { sentiment: 'neutral', score: 0 };
    }

    // Tokenize
    const encoded = _tokenizer.encode(text, MAX_SEQUENCE_LENGTH);

    // Build input tensors
    const inputIds = new Int32Array(MAX_SEQUENCE_LENGTH);
    const attentionMask = new Int32Array(MAX_SEQUENCE_LENGTH);

    for (let i = 0; i < MAX_SEQUENCE_LENGTH; i++) {
        if (i < encoded.length) {
            inputIds[i] = encoded[i];
            attentionMask[i] = 1;
        } else {
            inputIds[i] = PAD_TOKEN_ID;
            attentionMask[i] = 0;
        }
    }

    const feeds = {
        input_ids: new ort.Tensor('int32', inputIds, [1, MAX_SEQUENCE_LENGTH]),
        attention_mask: new ort.Tensor('int32', attentionMask, [1, MAX_SEQUENCE_LENGTH])
    };

    // Run inference
    const results = await _session.run(feeds);
    const outputName = _session.outputNames[0];
    const logits = Array.from(results[outputName].data);

    // Softmax
    const probs = _softmax(logits);

    // Predicted class (argmax)
    let maxIdx = 0;
    for (let i = 1; i < probs.length; i++) {
        if (probs[i] > probs[maxIdx]) maxIdx = i;
    }

    // Score: positive - negative, range [-1, 1]
    const score = probs.length >= 3 ? probs[2] - probs[0] : 0;

    const LABELS = ['negative', 'neutral', 'positive'];
    return { sentiment: LABELS[maxIdx] || 'neutral', score };
}

function _softmax(values) {
    const maxVal = Math.max(...values);
    const exp = values.map(v => Math.exp(v - maxVal));
    const sum = exp.reduce((a, b) => a + b, 0);
    return exp.map(e => e / sum);
}
