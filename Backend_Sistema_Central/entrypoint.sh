#!/usr/bin/env bash
set -e

echo "⏳ Esperando a la base de datos ($DB_HOST:$DB_PORT)…"
until pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" >/dev/null 2>&1; do
  sleep 2
done
echo "✅ Base de datos lista."

exec dotnet Backend_Sistema_Central.dll --migrate-and-run
