import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  use: {
    baseURL: 'http://127.0.0.1:5173',
    trace: 'retain-on-failure',
  },
  projects: [
    {
      name: 'desktop',
      use: { viewport: { width: 1440, height: 900 } },
    },
    {
      name: 'mobile',
      use: { isMobile: true, viewport: { width: 390, height: 844 } },
    },
  ],
  webServer: [
    {
      command: 'ASPNETCORE_ENVIRONMENT=Development dotnet run --project Convocado.Api/Convocado.Api.csproj --no-build --no-launch-profile --urls http://127.0.0.1:5081',
      cwd: '../backend',
      reuseExistingServer: !process.env.CI,
      url: 'http://127.0.0.1:5081/health',
    },
    {
      command: 'VITE_API_BASE_URL=http://127.0.0.1:5081 npm run dev -- --host 127.0.0.1 --port 5173',
      reuseExistingServer: !process.env.CI,
      url: 'http://127.0.0.1:5173',
    },
  ],
})
