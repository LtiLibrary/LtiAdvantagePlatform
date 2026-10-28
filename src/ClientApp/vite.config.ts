import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// The platform (ASP.NET Core) runs on http://localhost:5099.
const platform = "http://localhost:5099";

export default defineConfig({
  plugins: [react()],

  // Served by ASP.NET Core at /app (see Spa/SpaApi.cs).
  base: "/app/",
  build: {
    outDir: "../wwwroot/app",
    emptyOutDir: true,
  },

  // `npm run dev`: open http://localhost:5173/app/. The dev server forwards API calls,
  // LTI launches and the sign-in pages to the platform, so the Identity cookie and the
  // launch URLs work as if React were served by ASP.NET Core.
  server: {
    port: 5173,
    strictPort: true,
    // changeOrigin sends Host: localhost:5099, so the platform still puts its own
    // address in the launch's "iss" parameter.
    proxy: Object.fromEntries(
      ["/api", "/OidcLaunch", "/Identity", "/lib", "/css", "/js"].map((path) => [
        path,
        { target: platform, changeOrigin: true },
      ]),
    ),
  },
});
