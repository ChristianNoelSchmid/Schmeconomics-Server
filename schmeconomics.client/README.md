# schmeconomics-client

Nuxt 4 frontend for Schmeconomics. Built with Nuxt UI (Tailwind CSS) and installable as a PWA.

## Stack

- **Nuxt 4** + Vue 3
- **Nuxt UI** (component library, Tailwind CSS)
- **@vite-pwa/nuxt** — PWA with auto-update
- TypeScript

## Pages

| Route | Description |
|---|---|
| `/login` | Sign-in form |
| `/` | Dashboard — lists accounts and their categories |
| `/accounts` | Account management (admin) |
| `/transactions` | Paginated transaction history, filterable by category |
| `/refill` | Trigger a refill for the current account |
| `/users` | User management (admin) |

## Configuration

The only runtime config value is the API base URL, set via environment variable:

```env
NUXT_PUBLIC_API_BASE=http://localhost:5153
```

The default (in `nuxt.config.ts`) is `http://localhost:5153`.

## Development

```bash
npm install
npm run dev       # http://localhost:3000
```

## Production build

```bash
npm run build
npm run preview   # preview the production build locally
```

In Docker the client is built with `npm run build` and served with `npm run preview`. See the root [`README.md`](../README.md) for the full Docker Compose setup.

## PWA

The app registers a service worker with auto-update and ships with 192×192 and 512×512 icons (`public/pwa-192x192.png`, `public/pwa-512x512.png`). The PWA short name is `Schm`.
