# (1) Базовый образ: ASP.NET Runtime + Docker CLI
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y docker.io
WORKDIR /app
EXPOSE 80

# (2) Этап сборки и публикации
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 2.1) Сначала копируем только .csproj, чтобы кэшировать dotnet restore
COPY TBC.csproj ./TBC.csproj
RUN dotnet restore "TBC.csproj"

# 2.2) Теперь копируем всю папку TBC целиком: код, Data/, Models/, Services/, Migrations/ и т.д.
COPY ./. ./

# 2.3) Собираем и публикуем приложение
RUN dotnet publish "TBC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# (3) Финальный образ на базе runtime
FROM base AS final
WORKDIR /app

# Копируем опубликованные артефакты
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "TBC.dll"]
