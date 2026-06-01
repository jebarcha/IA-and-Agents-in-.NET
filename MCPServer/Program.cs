using MCPServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddMcpServer()
     .WithHttpTransport(options =>
     {
         options.Stateless = true;
     })
     .WithToolsFromAssembly()
     .WithPromptsFromAssembly()
     .WithResourcesFromAssembly();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IPersonsRepository, InMemoryPersonsRepository>();

var app = builder.Build();

app.UseCors();

//app.MapGet("/", () => "Hello World!");
app.MapMcp("/mcp");

app.MapControllers();

app.Run();
