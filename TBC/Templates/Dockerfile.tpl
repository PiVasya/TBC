# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BotCode.csproj .
RUN dotnet restore BotCode.csproj
COPY Program.cs .
RUN dotnet publish BotCode.csproj -c Release -o /app

# Финальный образ на базе ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "BotCode.dll"]