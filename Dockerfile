# syntax=docker/dockerfile:1

# ---- Frontend build --------------------------------------------------------
FROM node:22-alpine AS web-build
WORKDIR /src/web

COPY Membr.Web/package.json Membr.Web/package-lock.json ./
# --legacy-peer-deps works around a peer-dependency version mismatch between
# @angular/animations and @angular/core in the current lockfile.
RUN npm ci --legacy-peer-deps

COPY Membr.Web/ ./
RUN npm run build

# ---- Backend build ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

COPY Membr.API/Membr.API.csproj Membr.API/
COPY Membr.Module.Identity/Membr.Module.Identity.csproj Membr.Module.Identity/
COPY Membr.Module.Member/Membr.Module.Member.csproj Membr.Module.Member/
COPY Membr.Shared/Membr.Shared.csproj Membr.Shared/
RUN dotnet restore Membr.API/Membr.API.csproj

COPY Membr.API/ Membr.API/
COPY Membr.Module.Identity/ Membr.Module.Identity/
COPY Membr.Module.Member/ Membr.Module.Member/
COPY Membr.Shared/ Membr.Shared/
RUN dotnet publish Membr.API/Membr.API.csproj -c Release -o /app/publish --no-restore

# ---- Runtime ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=api-build /app/publish .
COPY --from=web-build /src/web/dist/Membr.Web/browser ./wwwroot

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Membr.API.dll"]
