using WebAPI.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder
    .ConfigureBuilder()
    .ConfigureLogging()
    .ConfigureServices();

var app = builder.Build();

app.ConfigureApp();

await app.RunAsync();
