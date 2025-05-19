@echo off
REM Запускаем сервисы и сразу уходим в фон (detached)
docker compose up --build -d
