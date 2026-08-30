# Membr

Membr is a membership management system built for [pubOS](https://github.com/pubos) — it handles members, memberships, and the admin workflows around running a membership-based organisation (clubs, societies, associations, and similar).

## Features

- **Member management** — create, search, and view members, with custom (user-defined) fields for anything the built-in schema doesn't cover.
- **Membership types & renewals** — define membership types as either *rolling* (N months from whenever they start/renew) or *fixed-term* (everyone expires on the same calendar date each year), and renew existing memberships against them.
- **User-defined fields (UDF)** — admins can add custom fields (date, date/time, boolean, text, multi-select) to the member record, set defaults, and bulk-apply a default to every existing member.
- **Admin users & roles** — role-based access (`Admin` / `User`) for the back office, with an admin-only user management screen.
- **Dashboard** — summary stats for the organisation at a glance.
- **Settings** — global membership settings configurable per organisation.
- **Authentication** — JWT access tokens with rotating, hashed refresh tokens (delivered via an `HttpOnly` cookie) and automatic reuse/theft detection. Standalone (built-in) auth today, with room for an external OIDC provider.

## Technology stack

**Backend** — .NET 10 / ASP.NET Core minimal APIs, organised as modules:

- `Membr.API` — the host project; wires up the modules and serves the HTTP API (OpenAPI docs via Scalar in development).
- `Membr.Module.Identity` — authentication, users, roles, JWT issuing/refresh, ASP.NET Core Identity.
- `Membr.Module.Member` — the core domain: members, memberships, membership types, UDF fields, settings, dashboard stats.
- `Membr.Shared` — cross-module primitives (e.g. paging).
- **EF Core** with **Npgsql** (PostgreSQL) and snake_case naming conventions, one migration history table per module.

**Frontend** — `Membr.Web`, an Angular 21 single-page app:

- **Tailwind CSS 4** for styling, with [ZardUI](https://zardui.com) components.
- **Lucide** icons via `@ng-icons`.
- **ngx-charts** for the dashboard.
- API types generated from the backend's OpenAPI schema (`npm run api:generate`), so the frontend stays in sync with the API contract.

**Data** — PostgreSQL.

## Repository layout

```
Membr.API/              Host project — API entry point
Membr.Module.Identity/   Auth, users, roles
Membr.Module.Member/     Members, memberships, UDF fields, settings, dashboard
Membr.Shared/            Shared building blocks
Membr.Web/               Angular frontend
Membr.Module.Member.Tests/  Backend tests
```

## More documentation

- [Local development](./README.Development.md) — running the API and frontend locally.
- [Deployment / hosting](./README.Hosting.md) — running Membr via the published Docker image.
