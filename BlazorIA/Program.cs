using BlazorIA.Components;
using BlazorIA.Data;
using BlazorIA.Services;
using BlazorIA.Services.Chatbots;
using BlazorIA.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=midb.db"));


builder.Services.AddScoped<IPersonService, PersonService>();

builder.Services.AddScoped<IChatbot, RealChatbot>();


builder.Services.AddTransient<IWeatherService, OpenWeatherService>();
builder.Services.AddTransient<ConditionEvaluatorService>();
builder.Services.AddTransient<FakeSendEmailService>();
builder.Services.AddTransient<FakeGetEmailService>();
builder.Services.AddHttpClient();

builder.Services.AddTransient<IChatClientFactory, ChatClientFactory>();

//var provider = "claude"; // temporarily
//var model = "claude-haiku-4-5";  // temporarily

//builder.Services.AddSingleton<IChatClient>(sp =>
//{
//    var configuration = sp.GetRequiredService<IConfiguration>();
//    var OpenAIKey = configuration.GetValue<string>("OPENAI_KEY");
//    var AnthropicKey = configuration.GetValue<string>("ANTHROPIC_KEY");

//    var client = provider switch
//    {
//        "openai" => new OpenAI.Chat.ChatClient(model ?? "gpt-5.4-nano", AnthropicKey).AsIChatClient(),
//        "claude" => new AnthropicClient()
//        {
//            ApiKey = AnthropicKey
//        }.AsIChatClient().AsBuilder().ConfigureOptions(c => c.ModelId = model ?? "claude-haiku-4-5").Build(),
//        _ => throw new ArgumentException($"Unknown provider {provider}")
//    };

//    return client.AsBuilder()
//    .UseFunctionInvocation(null, c =>
//    {
//        c.IncludeDetailedErrors = true;
//    })
//    .Build(sp);
//});

builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions
{
    Tools = [.. Tools.GetTools(sp)],
    Temperature = 0.7f,
    MaxOutputTokens = 2000
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
