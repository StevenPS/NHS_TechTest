import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The API's https launch profile (src/ClinicSpace.Api/Properties/launchSettings.json).
const apiTarget = process.env.API_URL ?? 'https://localhost:7196'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      // Same-origin in dev: the browser only talks to Vite, which forwards /api to ASP.NET Core.
      '/api': {
        target: apiTarget,
        changeOrigin: true,
        secure: false, // trust the local ASP.NET Core dev certificate
      },
    },
  },
})
