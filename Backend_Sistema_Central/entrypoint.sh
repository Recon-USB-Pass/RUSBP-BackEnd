#!/usr/bin/env bash
set -e

echo "⏳ Esperando a que la base de datos esté lista..."
until pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER"; do
  sleep 2
done

echo "✅ Base de datos lista; ejecutando migraciones EF Core..."
dotnet Backend_Sistema_Central.dll --migrate-and-run
echo "✅ Migraciones completadas; ejecutando la aplicación..."