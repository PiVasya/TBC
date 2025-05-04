# (1) Базовый образ: ASP.NET Runtime + Docker CLI
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Устанавливаем docker.io (CLI)
RUN apt-get update && apt-get install -y docker.io
# Остаёмся под root (НЕ используем USER $APP_UID)
WORKDIR /app
EXPOSE 80

# (2) Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TBC/TBC.csproj", "TBC/"]
RUN dotnet restore "TBC/TBC.csproj"
COPY . .
WORKDIR "/src/TBC"
RUN dotnet build "TBC.csproj" -c $BUILD_CONFIGURATION -o /app/build

# (3) Этап публикации
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TBC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# (4) Финальный образ = base + опубликованный код
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80

# Запуск приложения
ENTRYPOINT ["dotnet", "TBC.dll"]
