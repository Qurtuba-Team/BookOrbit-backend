#!/bin/sh
set -e

CERT_DIR="/https"
CERT_KEY_PATH="$CERT_DIR/aspnetapp.key"
CERT_CRT_PATH="$CERT_DIR/aspnetapp.crt"
CERT_PFX_PATH="$CERT_DIR/aspnetapp.pfx"
CERT_PASSWORD="${ASPNETCORE_Kestrel__Certificates__Default__Password:-BookOrbitDevCert123!}"

if [ ! -f "$CERT_PFX_PATH" ]; then
    mkdir -p "$CERT_DIR"

    openssl req \
        -x509 \
        -nodes \
        -newkey rsa:2048 \
        -keyout "$CERT_KEY_PATH" \
        -out "$CERT_CRT_PATH" \
        -days 365 \
        -subj "/CN=localhost" \
        -addext "subjectAltName=DNS:localhost,DNS:bookorbit-api,IP:127.0.0.1" \
        -addext "basicConstraints=critical,CA:FALSE" \
        -addext "keyUsage=critical,digitalSignature,keyEncipherment" \
        -addext "extendedKeyUsage=serverAuth"

    openssl pkcs12 \
        -export \
        -out "$CERT_PFX_PATH" \
        -inkey "$CERT_KEY_PATH" \
        -in "$CERT_CRT_PATH" \
        -passout "pass:$CERT_PASSWORD"

    rm -f "$CERT_KEY_PATH" "$CERT_CRT_PATH"
fi

exec dotnet BookOrbit.Api.dll
