using TBC.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавляем папку Controllers (HTTP API)
builder.Services.AddControllers();

// 2. Регистрируем наш DockerBotBuilder как сервис
builder.Services
       .AddSingleton<IDockerBotBuilder, DockerBotBuilder>();

var app = builder.Build();

// 3. Статика из wwwroot
app.UseStaticFiles();

// 4. Маршрутизация к контроллерам
app.MapControllers();

app.Run();
