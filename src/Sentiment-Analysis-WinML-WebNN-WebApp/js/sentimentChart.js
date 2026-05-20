/**
 * Canvas-based sentiment trend chart.
 * Draws three polylines (positive, neutral, negative) as percentages over time.
 */

const COLORS = {
    positive: '#10B981',
    neutral:  '#6366F1',
    negative: '#EF4444',
    grid:     'rgba(255, 255, 255, 0.06)',
    gridText: 'rgba(255, 255, 255, 0.25)'
};

const PADDING = { top: 10, right: 10, bottom: 24, left: 36 };

export class SentimentChart {
    /** @param {HTMLCanvasElement} canvas */
    constructor(canvas) {
        this._canvas = canvas;
        this._ctx = canvas.getContext('2d');
        /** @type {Array<import('./app.js').Snapshot>} */
        this._snapshots = [];

        this._resizeObserver = new ResizeObserver(() => this._handleResize());
        this._resizeObserver.observe(canvas);
        this._handleResize();
    }

    /** @param {Array} snapshots */
    update(snapshots) {
        this._snapshots = snapshots;
        this._draw();
    }

    destroy() {
        this._resizeObserver.disconnect();
    }

    _handleResize() {
        const rect = this._canvas.getBoundingClientRect();
        const dpr = window.devicePixelRatio || 1;
        this._canvas.width = rect.width * dpr;
        this._canvas.height = rect.height * dpr;
        this._ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        this._draw();
    }

    _draw() {
        const ctx = this._ctx;
        const rect = this._canvas.getBoundingClientRect();
        const w = rect.width;
        const h = rect.height;

        ctx.clearRect(0, 0, w, h);

        const chartX = PADDING.left;
        const chartY = PADDING.top;
        const chartW = w - PADDING.left - PADDING.right;
        const chartH = h - PADDING.top - PADDING.bottom;

        if (chartW <= 0 || chartH <= 0) return;

        // Grid lines (5 horizontal)
        ctx.strokeStyle = COLORS.grid;
        ctx.lineWidth = 1;
        ctx.font = '10px system-ui';
        ctx.fillStyle = COLORS.gridText;
        ctx.textAlign = 'right';
        ctx.textBaseline = 'middle';

        for (let i = 0; i <= 4; i++) {
            const y = chartY + (chartH * i) / 4;
            const pct = 100 - (i * 25);

            ctx.beginPath();
            ctx.moveTo(chartX, y);
            ctx.lineTo(chartX + chartW, y);
            ctx.stroke();

            ctx.fillText(`${pct}%`, chartX - 6, y);
        }

        const data = this._snapshots;
        if (data.length < 2) return;

        const stepX = chartW / (data.length - 1);

        // Draw each sentiment line
        this._drawLine(ctx, data, 'positiveCount', COLORS.positive, chartX, chartY, chartH, stepX);
        this._drawLine(ctx, data, 'neutralCount',  COLORS.neutral,  chartX, chartY, chartH, stepX);
        this._drawLine(ctx, data, 'negativeCount', COLORS.negative, chartX, chartY, chartH, stepX);
    }

    _drawLine(ctx, data, key, color, chartX, chartY, chartH, stepX) {
        ctx.beginPath();
        ctx.strokeStyle = color;
        ctx.lineWidth = 2;
        ctx.lineJoin = 'round';
        ctx.lineCap = 'round';

        let started = false;
        for (let i = 0; i < data.length; i++) {
            const snap = data[i];
            if (snap.totalMessages === 0) continue;

            const pct = (snap[key] / snap.totalMessages) * 100;
            const x = chartX + i * stepX;
            const y = chartY + chartH - (pct / 100) * chartH;

            if (!started) {
                ctx.moveTo(x, y);
                started = true;
            } else {
                ctx.lineTo(x, y);
            }
        }

        ctx.stroke();
    }
}
