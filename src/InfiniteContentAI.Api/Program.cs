using InfiniteContentAI.Api;
using InfiniteContentAI.Api.Pipelines;
using InfiniteContentAI.Api.Projects;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapProjectEndpoints();
app.MapPipelineEndpoints();

app.Run();

public partial class Program;
