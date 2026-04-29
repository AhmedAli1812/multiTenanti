import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // ── Dev proxy ──────────────────────────────────────────────────────────────
    // Proxying /api → backend eliminates CORS issues in development entirely.
    // The browser only ever sees http://localhost:5173, so no preflight needed.
    proxy: {
      '/api': {
        target: 'https://localhost:7154',
        changeOrigin: true,
        // Accept the self-signed dev cert from the .NET dev server
        secure: false,
      },
      '/hubs': {
        target: 'https://localhost:7154',
        changeOrigin: true,
        secure: false,
        ws: true,   // WebSocket (SignalR DashboardHub)
      },
    },
    headers: {
      'Content-Security-Policy':
        "script-src 'self' 'unsafe-eval' 'unsafe-inline'; worker-src 'self' blob:",
    },
  },
})