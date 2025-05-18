using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using tbc.Data;
using tbc.Services;

var builder = WebApplication.CreateBuilder(args);

// 1) Регистрируем сервис компрессии и исключаем из него text/html
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes
            .Except(new[] { "text/html" });
    options.EnableForHttps = true;
});

// 2) Добавляем DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// 3) Контроллеры и ваши сервисы
builder.Services.AddControllers();
builder.Services.AddSingleton<IDockerBotBuilder, DockerBotBuilder>();
builder.Services.AddSingleton<IContainerService, ContainerService>();
builder.Services.AddScoped<IBotService, BotService>();

builder.WebHost.UseUrls("http://0.0.0.0:80");

// 4) SPA
builder.Services.AddSpaStaticFiles(options =>
{
    options.RootPath = "clientapp/build";
});

var app = builder.Build();

// ——— Здесь, сразу после Build(), но до любых middleware, применяем миграции ———
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
// ——————————————————————————————————————————————————————————————————————————————

app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseRouting();

// (Если нужен) app.UseCors();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "clientapp";

    if (app.Environment.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

app.Run();
