# Deploying Membr

Membr ships as a single Docker image: the ASP.NET Core API with the built Angular SPA served as static files from the same origin (`wwwroot`), listening on port `8080`. There's no separate frontend container or reverse-proxy config required to get the app itself running — just the image plus a PostgreSQL database.

On startup the app applies any pending EF Core migrations for both modules automatically, creates the `Admin`/`User` roles if they don't exist, and creates an initial admin account from `Auth__Standalone__InitialAdmin__*` if one isn't already there. It is safe to restart/redeploy the container against the same database.

## Building the image

```bash
docker build -t membr:latest .
```

If you publish it to a registry (e.g. as part of pubOS's image builds), tag and push it from there — swap `membr:latest` below for that image reference (e.g. `ghcr.io/pubos/membr:latest`) once published.

## Configuration

Configuration is layered the standard ASP.NET Core way — `appsettings.json` defaults, overridden by environment variables using double-underscore section separators.

| Variable | Required | Description |
|---|---|---|
| `ConnectionStrings__Default` | yes | PostgreSQL connection string. |
| `Auth__Standalone__Jwt__Issuer` | yes | JWT issuer, e.g. `Membr`. |
| `Auth__Standalone__Jwt__Audience` | yes | JWT audience, e.g. `Membr.Web`. |
| `Auth__Standalone__Jwt__SigningKey` | yes | Secret used to sign access tokens. Generate a long random value per deployment — **do not** reuse the value from `appsettings.json`. |
| `Auth__Standalone__Jwt__AccessTokenMinutes` | no | Access token lifetime (default `15`). |
| `Auth__Standalone__Jwt__RefreshTokenDays` | no | Refresh token lifetime (default `14`). |
| `Auth__Standalone__InitialAdmin__Email` | no | Bootstraps an admin user with this email if it doesn't already exist. |
| `Auth__Standalone__InitialAdmin__Password` | no | Password for the bootstrapped admin user. |
| `ASPNETCORE_ENVIRONMENT` | no | Leave unset or `Production` for a normal deployment. |

Since the frontend is served from the same origin as the API in this image, you generally don't need to touch CORS (`Cors__AllowedOrigins`) — it only matters if you're calling the API from a different origin.

The container listens on plain HTTP (`8080`); put it behind a reverse proxy/load balancer that terminates TLS.

## Example docker-compose

```yaml
services:
  membr:
    image: membr:latest # or your published image, e.g. ghcr.io/pubos/membr:latest
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      ConnectionStrings__Default: "Host=postgres;Port=5432;Database=membr;Username=membr;Password=${DB_PASSWORD}"
      Auth__Standalone__Jwt__Issuer: "Membr"
      Auth__Standalone__Jwt__Audience: "Membr.Web"
      Auth__Standalone__Jwt__SigningKey: "${JWT_SIGNING_KEY}"
      Auth__Standalone__InitialAdmin__Email: "admin@example.org"
      Auth__Standalone__InitialAdmin__Password: "${INITIAL_ADMIN_PASSWORD}"
    depends_on:
      - postgres

  postgres:
    image: postgres:17
    restart: unless-stopped
    environment:
      POSTGRES_USER: membr
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: membr
    volumes:
      - membr-pgdata:/var/lib/postgresql/data

volumes:
  membr-pgdata:
```

Set `DB_PASSWORD`, `JWT_SIGNING_KEY`, and `INITIAL_ADMIN_PASSWORD` (e.g. via an `.env` file next to the compose file, or your platform's secret store) — don't commit real values for these.

Once it's up, Membr is available at `http://localhost:8080` (or wherever your reverse proxy exposes it).
