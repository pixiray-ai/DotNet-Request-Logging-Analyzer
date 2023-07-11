using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Sinks.InMemory;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add Logger
builder.Logging.ClearProviders();
//TODO: Add Serilog     .WriteTo.InMemory()
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.AddSerilog(logger);

string[] ips = { };

var envProxies = Environment.GetEnvironmentVariable("KNOWN_PROXIES");
logger.Information("KNOWN_PROXIES: {KNOWN_PROXIES}", envProxies);
if (!string.IsNullOrEmpty(envProxies))
{
    ips = envProxies.Split(",");
}


if (ips.Length > 0)
{
    logger.Information("Adding Forwarded Headers");
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        foreach (var ip in ips)
        {
            logger.Information("Adding Forwarded Header: {ip}", ip);
            options.KnownProxies.Add(IPAddress.Parse(ip));
        }
    });
}

//TODO: Add ViewEngine
//builder.Services.AddRazorPages();

var app = builder.Build();

if (ips.Length > 0)
{
    app.UseForwardedHeaders();
    logger.Information("Using Forwarded Headers enabled");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{

    // Log/Print all Headers
    foreach (var header in context.Request.Headers)
    {
        logger.Information("Header: {Key}: {Value}", header.Key, header.Value);
    }

    logger.Information("Request Method: {Method}", context.Request.Method);
    logger.Information("Request Scheme: {Scheme}", context.Request.Scheme);
    logger.Information("Request Path: {Path}", context.Request.Path);
    logger.Information("Request PathBase: {PathBase}", context.Request.PathBase);
    logger.Information("Request Protocol: {Protocol}", context.Request.Protocol);
    logger.Information("Request Origin: {RemoteIpAddress}", context.Connection.RemoteIpAddress);

    await next();
});

app.MapGet("/", async (HttpContext context) =>
{
    Log.Information("{DateTime.UtcNow} ><> {context}");
    return Results.Ok("hello world again");
});

//app.MapGet("/test/", () => "Hello World Test!");
app.MapGet("/test/{id}", async (string id, HttpContext context) =>
{
    Log.Information("{DateTime.UtcNow} ><> {context} ID {id}");
    return Results.Ok("hello world" + id);
});

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapRazorPages();

app.Run();
