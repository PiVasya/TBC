var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Включаем поддержку статических файлов (чтобы отдавать index.html)
app.UseStaticFiles();

// Если у вас в index.html форма `<form method="post" action="/submitt">...</form>`,
// то ловим POST здесь:
app.MapPost("/submitt", async context =>
{
    var form = await context.Request.ReadFormAsync();
    // Считаем токен, если нужно (или возьмём из конфига)
    string telegramToken = form["BotToken"].ToString();
    string BotCode = form["BotCode"].ToString();
    string BotProj = form["BotProj"].ToString();
    string BotDocker = form["BotDocker"].ToString();
    try
    {
        // Запускаем сборку и запуск нового контейнера
        string containerId = await DockerBotBuilder.CreateAndRunBot(telegramToken,BotCode,BotProj,BotDocker);
        Console.WriteLine($"Создан контейнер: {containerId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при создании бота: {ex.Message}");
    }

    // Перенаправляем
    context.Response.Redirect("/index.html");
});


app.Run();
