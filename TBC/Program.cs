var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// �������� ��������� ����������� ������ (����� �������� index.html)
app.UseStaticFiles();

// ���� � ��� � index.html ����� `<form method="post" action="/submitt">...</form>`,
// �� ����� POST �����:
app.MapPost("/submitt", async context =>
{
    var form = await context.Request.ReadFormAsync();
    // ������� �����, ���� ����� (��� ������ �� �������)
    string telegramToken = form["BotToken"].ToString();
    string BotCode = form["BotCode"].ToString();
    string BotProj = form["BotProj"].ToString();
    string BotDocker = form["BotDocker"].ToString();
    try
    {
        // ��������� ������ � ������ ������ ����������
        string containerId = await DockerBotBuilder.CreateAndRunBot(telegramToken,BotCode,BotProj,BotDocker);
        Console.WriteLine($"������ ���������: {containerId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"������ ��� �������� ����: {ex.Message}");
    }

    // ��������������
    context.Response.Redirect("/index.html");
});


app.Run();
