#!/usr/bin/env bash
set -e
until pg_isready -h db -p 5432 -U seguridad; do
  echo "⏳ Esperando a PostgreSQL..."
  sleep 2
done
dotnet Backend_Sistema_Central.dll --migrate-and-run
