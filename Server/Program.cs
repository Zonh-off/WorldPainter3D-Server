using Server.Configs;
using Server.Hubs;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

var configPath = Path.Combine(AppContext.BaseDirectory, "config.yml");
var serverConfig = YamlConfig.LoadOrCreate(configPath);

builder.Services.AddSingleton(serverConfig);
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
           .AllowAnyMethod()
           .AllowAnyOrigin();
    });
});

builder.Services.AddSingleton<WorldService>();
builder.Services.AddSingleton<PlayerService>();
builder.Services.AddSingleton<GameHub>();

var app = builder.Build();

app.UseCors();
app.UseRouting();
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<GameHub>("/gamehub");
});
app.Run();