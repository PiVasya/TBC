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
    string telegramToken = form["userData"].ToString();

    try
    {
        // ��������� ������ � ������ ������ ����������
        string containerId = await DockerBotBuilder.CreateAndRunBotAsync(telegramToken);
        Console.WriteLine($"������ ���������: {containerId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"������ ��� �������� ����: {ex.Message}");
    }

    // �������������� ���-��
    context.Response.Redirect("/index.html");
});


app.Run();
