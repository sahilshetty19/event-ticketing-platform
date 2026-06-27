#!/bin/sh
# Write the runtime config consumed by the SPA (window.__APP_CONFIG__).
# API_BASE_URL points the browser at the gateway's host-visible URL.
set -e

: "${API_BASE_URL:=http://localhost:8088}"

cat > /usr/share/nginx/html/env.js <<EOF
window.__APP_CONFIG__ = { apiBaseUrl: "${API_BASE_URL}" };
EOF

echo "app-config: apiBaseUrl=${API_BASE_URL}"
