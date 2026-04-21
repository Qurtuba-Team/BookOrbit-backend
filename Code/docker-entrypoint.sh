#!/bin/sh
set -e

echo "Starting container..."

# =========================
# Initialize wwwroot
# =========================
if [ ! -f "/app/wwwroot/.initialized" ]; then
    echo "Initializing wwwroot..."

    cp -r /app/wwwroot_backup/* /app/wwwroot/

    touch /app/wwwroot/.initialized

    echo "wwwroot initialized."
else
    echo "wwwroot already initialized."
fi

# =========================
# Run App
# =========================
exec dotnet BookOrbit.Api.dll