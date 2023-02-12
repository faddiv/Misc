import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import ssr from 'vite-plugin-ssr/plugin'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    ssr({
      prerender: true
    })],
  css: {
    modules: {
      localsConvention: "camelCase",
      generateScopedName: "[name]__[local]__[hash:base64:5]",
    }
  },
  resolve: {
    alias: [
      {
        find: /^~.+/,
        replacement: ((val: string) => {
          return val.replace(/^~/, "");
        }) as unknown as string,
      },
    ],
  },
  build: {
    minify: false
  }
})
