# NexaFlow Frontend

React + TypeScript + Vite shell for the NexaFlow dashboard. This is Phase 1 scaffolding
only — the app shell renders a placeholder page; the auth/workflows/tasks features under
`src/features` are folder stubs for Phase 2, not yet implemented. See the root
[README](../README.md) for the overall project roadmap.

## Structure

- `src/app` — app shell, routing, providers
- `src/features/{auth,workflows,tasks}` — feature-sliced modules (Phase 2)
- `src/shared` — shared UI components, hooks, utils (Phase 2)
- `src/services` — API client (`apiClient.ts` is a minimal fetch wrapper today; Phase 2
  can replace it with a client generated from the backend's OpenAPI document)
- `src/types` — TypeScript interfaces mirroring the backend's `NexaFlow.Application` DTOs

## Getting started

```bash
cp .env.example .env
npm install
npm run dev
```

Requires the backend API running at the URL in `VITE_API_BASE_URL` (defaults to
`http://localhost:5080`; see `backend/src/NexaFlow.Api`).

## Scripts

- `npm run dev` — start the Vite dev server
- `npm run build` — type-check (`tsc -b`) and produce a production build
- `npm run lint` — run Oxlint
- `npm run preview` — preview the production build locally
