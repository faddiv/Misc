import { configDefaults, defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react-swc";
import ssr from "vite-plugin-ssr/plugin";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    ssr({
      prerender: false,
    }),
  ],
  define: {
    "process.env": process.env
  },
  css: {
    modules: {
      localsConvention: "camelCase",
      generateScopedName: "[name]__[local]__[hash:base64:5]",
    },
  },
  resolve: {
    alias: [
      {
        // This handles ~node references in scss.
        find: /^~.+/,
        replacement: ((val: string) => {
          return val.replace(/^~/, "");
        }) as unknown as string,
      },
    ],
  },
  build: {
    minify: false,
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/setupTests.ts"],
    exclude: [...configDefaults.exclude],
  },
  server: {
    port: 3000,
    strictPort: true,
    host: true
  },
});
