import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  base: '/RetencionJudicial/',
  plugins: [vue()],
  server: {
    port: 5173,
    proxy: {
      '/RetencionJudicial/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/RetencionJudicial/, '')
      }
    }
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true
  }
})
