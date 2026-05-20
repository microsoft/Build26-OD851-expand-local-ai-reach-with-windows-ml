"""Dev server with COOP/COEP headers (required for SharedArrayBuffer/WASM threads),
no-cache headers, and multi-threaded request handling."""
import http.server
import socketserver
import sys

class DevHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        # Required for SharedArrayBuffer (multi-threaded WASM)
        self.send_header('Cross-Origin-Opener-Policy', 'same-origin')
        self.send_header('Cross-Origin-Embedder-Policy', 'require-corp')
        # No caching during development
        self.send_header('Cache-Control', 'no-store')
        super().end_headers()

    extensions_map = {
        **http.server.SimpleHTTPRequestHandler.extensions_map,
        '.mjs': 'application/javascript',
        '.wasm': 'application/wasm',
        '.onnx': 'application/octet-stream',
    }

class ThreadedHTTPServer(socketserver.ThreadingMixIn, http.server.HTTPServer):
    daemon_threads = True

port = int(sys.argv[1]) if len(sys.argv) > 1 else 8888
print(f'Serving on http://localhost:{port}')
ThreadedHTTPServer(('', port), DevHandler).serve_forever()
