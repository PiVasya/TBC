namespace TBC.controllers
{
    public class ProjCs
    {
        public static readonly string StdProj = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Telegram.Bot"" Version=""22.4.2"" />
  </ItemGroup>
</Project>";
    }
}
