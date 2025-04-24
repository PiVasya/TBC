using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using TBC.Services;


var builder = WebApplication.CreateBuilder(args);

// 1) Регистрируем сервис компрессии и исключаем из него text/html
builder.Services.AddResponseCompression(options =>
{
    // Убираем HTML, чтобы BrowserLink мог инжектить свой скрипт
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes
            .Except(new[] { "text/html" });
    options.EnableForHttps = true;
});


builder.Services.AddControllers();
builder.Services.AddSingleton<IDockerBotBuilder, DockerBotBuilder>();
builder.Services.AddSingleton<IContainerService, ContainerService>();

builder.Services.AddSpaStaticFiles(options =>
{
    options.RootPath = "clientapp/build";
});

var app = builder.Build();

app.UseStaticFiles();
app.UseSpaStaticFiles();

// ✅ Добавляем маршрутизацию
app.UseRouting();

// ✅ Добавляем CORS, если нужен
// app.UseCors(...); 

// ✅ Контроллеры внутри UseEndpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// SPA Middleware
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "clientapp";

    if (app.Environment.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

app.Run();
