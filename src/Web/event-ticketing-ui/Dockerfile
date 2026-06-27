# syntax=docker/dockerfile:1
# Multi-stage build: compile the Angular app with Node, serve the static output with nginx.

# ---- build ----
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---- runtime ----
FROM nginx:alpine AS final

# SPA-aware nginx config (client-side routing falls back to index.html).
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Static site (Angular application builder emits to dist/<name>/browser).
COPY --from=build /app/dist/event-ticketing-ui/browser /usr/share/nginx/html

# Regenerate env.js from $API_BASE_URL at container start. nginx's official entrypoint
# runs every script in /docker-entrypoint.d/ before launching nginx.
COPY docker-entrypoint.d/40-app-config.sh /docker-entrypoint.d/40-app-config.sh
RUN chmod +x /docker-entrypoint.d/40-app-config.sh

EXPOSE 80
