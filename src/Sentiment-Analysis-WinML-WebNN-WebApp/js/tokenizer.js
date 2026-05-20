/**
 * Byte-level BPE Tokenizer for RoBERTa / GPT-2 style models.
 * Loads vocab.json and merges.txt to tokenize text into token IDs.
 */

// Byte-to-unicode mapping (GPT-2 style)
function bytesToUnicode() {
    const bs = [];
    for (let i = 33; i <= 126; i++) bs.push(i);
    for (let i = 161; i <= 172; i++) bs.push(i);
    for (let i = 174; i <= 255; i++) bs.push(i);

    const cs = [...bs];
    let n = 0;
    for (let b = 0; b < 256; b++) {
        if (!bs.includes(b)) {
            bs.push(b);
            cs.push(256 + n);
            n++;
        }
    }

    const result = {};
    for (let i = 0; i < bs.length; i++) {
        result[bs[i]] = String.fromCharCode(cs[i]);
    }
    return result;
}

const BYTE_ENCODER = bytesToUnicode();

// Pre-tokenization regex (GPT-2 pattern)
const PAT = /'s|'t|'re|'ve|'m|'ll|'d| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+/gu;

export class BpeTokenizer {
    constructor(vocab, merges) {
        this.encoder = vocab;
        this.decoder = {};
        for (const [k, v] of Object.entries(vocab)) {
            this.decoder[v] = k;
        }

        this.bpeRanks = new Map();
        for (let i = 0; i < merges.length; i++) {
            this.bpeRanks.set(merges[i][0] + ' ' + merges[i][1], i);
        }

        this.cache = new Map();
    }

    static async load(vocabUrl, mergesUrl) {
        const [vocab, mergesText] = await Promise.all([
            fetch(vocabUrl).then(r => r.json()),
            fetch(mergesUrl).then(r => r.text())
        ]);

        const merges = mergesText
            .split('\n')
            .map(line => line.replace(/\r$/, ''))
            .filter(line => line && !line.startsWith('#version'))
            .map(line => line.split(' '));

        return new BpeTokenizer(vocab, merges);
    }

    bpe(token) {
        if (this.cache.has(token)) return this.cache.get(token);

        let word = token.split('');
        if (word.length <= 1) {
            this.cache.set(token, word);
            return word;
        }

        while (true) {
            let minRank = Infinity;
            let minPair = null;

            for (let i = 0; i < word.length - 1; i++) {
                const pair = word[i] + ' ' + word[i + 1];
                const rank = this.bpeRanks.get(pair);
                if (rank !== undefined && rank < minRank) {
                    minRank = rank;
                    minPair = [word[i], word[i + 1]];
                }
            }

            if (minPair === null) break;

            const newWord = [];
            let i = 0;
            while (i < word.length) {
                if (i < word.length - 1 && word[i] === minPair[0] && word[i + 1] === minPair[1]) {
                    newWord.push(minPair[0] + minPair[1]);
                    i += 2;
                } else {
                    newWord.push(word[i]);
                    i++;
                }
            }
            word = newWord;
            if (word.length === 1) break;
        }

        this.cache.set(token, word);
        return word;
    }

    encode(text, maxLength) {
        const BOS_TOKEN_ID = 0; // <s>
        const EOS_TOKEN_ID = 2; // </s>

        // Reserve 2 slots for BOS and EOS special tokens
        const tokenLimit = maxLength ? maxLength - 2 : undefined;

        const ids = [];
        const matches = text.match(PAT) || [];

        for (const match of matches) {
            const encoded = new TextEncoder().encode(match);
            const byteStr = Array.from(encoded).map(b => BYTE_ENCODER[b]).join('');
            const bpeTokens = this.bpe(byteStr);

            for (const tok of bpeTokens) {
                const id = this.encoder[tok];
                ids.push(id !== undefined ? id : 3); // 3 = <unk>
            }

            if (tokenLimit && ids.length >= tokenLimit) {
                ids.length = tokenLimit;
                break;
            }
        }

        // Wrap with RoBERTa special tokens: <s> ... </s>
        return [BOS_TOKEN_ID, ...ids, EOS_TOKEN_ID];
    }
}
