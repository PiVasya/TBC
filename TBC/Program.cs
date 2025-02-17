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
    string telegramToken = form["userData"].ToString();

    try
    {
        // Запускаем сборку и запуск нового контейнера
        string containerId = await DockerBotBuilder.CreateAndRunBotAsync(telegramToken);
        Console.WriteLine($"Создан контейнер: {containerId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при создании бота: {ex.Message}");
    }

    // Перенаправляем что-то
    context.Response.Redirect("/index.html");
});


app.Run();
