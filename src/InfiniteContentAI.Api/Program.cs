using InfiniteContentAI.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Environment);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

public partial class Program;
