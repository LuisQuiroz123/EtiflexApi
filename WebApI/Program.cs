using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using WebApi.Data;
using WebApi.DTOs.Status;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// CONFIGURACIÓN DE APPSETTINGS POR ENTORNO
// ===============================
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Variables de entorno (Azure)

// ===============================
// CONFIGURACIÓN DE LOGGING
// ===============================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ===============================
// CONFIGURACIÓN DE BASE DE DATOS
// ===============================
builder.Services.AddDbContext<EtiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

// ===============================
// CONTROLADORES Y JSON
// ===============================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ===============================
// SERVICIOS HTTP CLIENT
// ===============================
builder.Services.AddScoped<AccessManagerService>();
builder.Services.AddHttpClient<SalesOrderService>();
builder.Services.AddHttpClient<CermOrderService>(client =>
{
    client.BaseAddress = new Uri("https://secure.cerm.be/hd/");
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

// ===============================
// AUTENTICACIÓN JWT
// ===============================
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"UNAUTHORIZED\",\"message\":\"Token faltante o inválido\"}");
        }
    };
});

// ===============================
// SWAGGER / OPENAPI
// ===============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Print Status API", Version = "v1" });
    c.MapType<StatusUpdateDto>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSchema>
        {
            ["printRequestId"] = new() { Type = "string", Format = "uuid" },
            ["date"] = new() { Type = "string", Format = "date-time" },
            ["status"] = new() { Type = "string" },
            ["code"] = new() { Type = "integer" },
            ["data"] = new()
            {
                Type = "object",
                Properties = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSchema>
                {
                    ["trackingId"] = new() { Type = "string" }
                }
            }
        },
        Required = new HashSet<string> { "printRequestId", "date", "status", "code" }
    });
});

var app = builder.Build();

// ===============================
// MIDDLEWARE DE ERRORES
// ===============================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandlerPathFeature?.Error != null)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "INTERNAL_SERVER_ERROR",
                message = exceptionHandlerPathFeature.Error.Message,
                stackTrace = exceptionHandlerPathFeature.Error.StackTrace,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path,
                traceId = context.TraceIdentifier
            });
        }
    });
});

// ===============================
// SWAGGER SOLO EN DESARROLLO
// ===============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
