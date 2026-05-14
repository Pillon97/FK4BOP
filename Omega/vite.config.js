import { defineConfig } from 'vite'

export default defineConfig({
  base: './',
  root: '.',
  publicDir: 'public',
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    sourcemap: false,
    minify: 'esbuild',
    target: 'es2020'
  },
  server: {
    port: 5173,
    open: true
  }
})
