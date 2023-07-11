using Serilog;
using Serilog.Sinks.InMemory;

var builder = WebApplication.CreateBuilder(args);

// Add Logger
builder.Logging.ClearProviders();
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.InMemory()
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.AddSerilog(logger);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
