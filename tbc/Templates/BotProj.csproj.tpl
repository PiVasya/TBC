<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Telegram.Bot" Version="22.4.2" />
  <!-- Базовый пакет EF Core, содержит PooledDbContextFactory -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.4" />

  <!-- Провайдер для PostgreSQL -->
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.4" />
  </ItemGroup>
</Project>