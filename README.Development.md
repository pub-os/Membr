# Local development

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) + npm (see `packageManager` in `Membr.Web/package.json` for the version this repo was built with)
- Docker (for PostgreSQL — or point at a Postgres instance of your own)

## 1. Start PostgreSQL

The repo's `docker-compose.yaml` starts a local Postgres instance:

```bash
docker compose up -d
```

This exposes Postgres on `localhost:5432` with the credentials already wired up in `Membr.API/appsettings.Development.json` (database `membr`, user `membr`, password `membr_dev`).

## 2. Run the API

```bash
dotnet run --project Membr.API
```

In the `Development` environment the API automatically applies EF Core migrations for both modules on startup, and creates an initial admin user from `Auth:Standalone:InitialAdmin` in `appsettings.Development.json` (`admin@membr.local` / `ChangeMe123!` by default — change this before it's ever exposed anywhere but your machine).

The API listens on `http://localhost:5219` (see `Membr.API/Properties/launchSettings.json`). In development it also serves an OpenAPI reference UI ([Scalar](https://scalar.com)) at `/scalar`.

### Seeding sample data

To generate sample members for local testing/perf work:

```bash
dotnet run --project Membr.API -- seed [count]
```

`count` defaults to 2000. This runs migrations, seeds, and exits — it does not start the API.

## 3. Run the frontend

```bash
cd Membr.Web
npm install
npm start
```

The Angular dev server runs on `http://localhost:4200` and proxies `/auth` and `/admin` requests to the API on `http://localhost:5219` (see `Membr.Web/proxy.conf.json`).

## Keeping the frontend's API types in sync

The frontend's TypeScript types are generated from the backend's OpenAPI schema. After changing an API contract, regenerate them with the API running:

```bash
cd Membr.Web
npm run api:generate
```

This fetches `http://localhost:5219/openapi/v1.json` and regenerates `src/app/api/schema.d.ts`.

## Running tests

Backend:

```bash
dotnet test
```

Frontend:

```bash
cd Membr.Web
npm test
```
