using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using MiPrimeraApi.Services;
using MiPrimeraApi.Data;
using MiPrimeraApi.Mappings; // ✅ Nuevo using
using MiPrimeraApi.Middlewares;
using Microsoft.AspNetCore.RateLimiting; // ✅ Nuevo using
using System.Threading.RateLimiting;    // ✅ Nuevo using
using Microsoft.Extensions.Caching.Distributed; // ✅ Nuevo using
using Asp.Versioning; // ✅ Nuevo using
using Asp.Versioning.ApiExplorer; // ✅ Nuevo using
using MiPrimeraApi.Swagger; // ✅ ConfigureSwaggerOptions (un SwaggerDoc por versión de la API)
//using Microsoft.Extensions.Diagnostics.HealthChecks; // Nuevo using
using Microsoft.AspNetCore.Diagnostics.HealthChecks; // ✅ Aquí vive HealthCheckOptions (para MapHealthChecks)

// Program.cs


// 1. CONFIGURACIÓN TEMPRANA DE SERILOG (Antes de WebApplication.CreateBuilder)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Nivel mínimo de log
    .WriteTo.Console()          // Escribe en la terminal
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day) // Escribe en archivo diario
    .CreateLogger();

try
{
  var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "PostgreSQL")
    .AddRedis(builder.Configuration["Redis:Configuration"] ?? "localhost:6379", name: "Redis");

// 1. CONFIGURAR VERSIONADO DE API
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Versión por defecto
    options.AssumeDefaultVersionWhenUnspecified = true; // Si no se especifica, usa la v1
    options.ReportApiVersions = true; // Devuelve headers HTTP indicando qué versiones están disponibles
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Lee la versión desde la URL: /api/v1/...
})

// 2. CONFIGURAR API EXPLORER (Para que Swagger funcione con versiones)
    .AddApiExplorer(options => // ✅ Correct way to add ApiExplorer in newer versions if using the combined package
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. CONFIGURAR SWAGGER PARA MÚLTIPLES VERSIONES
builder.Services.AddSwaggerGen();
// ConfigureSwaggerOptions (Swagger/ConfigureSwaggerOptions.cs) crea un SwaggerDoc por versión.
// Al registrarla como IConfigureOptions, el contenedor le inyecta IApiVersionDescriptionProvider.
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();


// 1. CONFIGURAR POLÍTICAS DE RATE LIMITING
builder.Services.AddRateLimiter(options =>
{
options.AddFixedWindowLimiter("LimiteAdmin", limiterOptions =>
{
    limiterOptions.PermitLimit = 1000; // Mucho más alto para admins
    limiterOptions.Window = TimeSpan.FromSeconds(10);
});

    // Política: Ventana Fija (Fixed Window)
    options.AddFixedWindowLimiter("LimitePorIP", limiterOptions =>
    {
        limiterOptions.PermitLimit = 2; // Máximo 5 peticiones
        limiterOptions.Window = TimeSpan.FromSeconds(10); // Cada 10 segundos
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // No poner en cola, rechazar inmediatamente si se excede
    });

    // Opcional: Personalizar la respuesta cuando se alcanza el límite (HTTP 429)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        
        var respuesta = new 
        {
            StatusCode = 429,
            Message = "Demasiadas peticiones. Por favor, espera unos segundos antes de intentar de nuevo.",
            RetryAfterSeconds = 10 // Le decimos al cliente cuánto debe esperar
        };

        await context.HttpContext.Response.WriteAsJsonAsync(respuesta, token);
    };
});

// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Lee de la variable de entorno Redis__Configuration, o usa localhost como fallback
    options.Configuration = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    options.InstanceName = "MiApi_";
});

// 1. CONFIGURAR AUTENTICACIÓN JWT
var claveSecreta = "EstaEsUnaClaveSuperSecretaYMuyLargaParaElEjemplo123!"; // Mínimo 16 caracteres
var key = Encoding.ASCII.GetBytes(claveSecreta);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Poner en true en producción
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Para simplificar el ejemplo
        ValidateAudience = false // Para simplificar el ejemplo
    };
});

// Cadena de conexión a PostgreSQL.
// En el contenedor la inyecta docker-compose (ConnectionStrings__DefaultConnection, Host=db).
// Para 'dotnet run' local cae en el fallback (Host=localhost, el Postgres expuesto en el 5432).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=supersecretpassword";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// CORS: registrar la política que luego usamos con app.UseCors("PermitirTodo")
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});
builder.Services.AddScoped<IProductoService, ProductoService>();

var app = builder.Build();

// ✅ AUTO-MIGRACIONES: Aplica las migraciones pendientes al iniciar la app.
// Crea la base y las tablas si no existen (clave dentro del contenedor, que arranca con una BD vacía).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Log.Information("Aplicando migraciones de base de datos...");
        dbContext.Database.Migrate(); // Esto crea las tablas si no existen
        Log.Information("Migraciones aplicadas exitosamente.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Error al aplicar las migraciones de la base de datos.");
        throw;
    }
}

app.UseRateLimiter();

// Configure the HTTP request pipeline.
// Swagger habilitado en TODOS los entornos (el contenedor corre en Production).
// En una app real solo lo dejarías en Development.
app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
        // Crear un dropdown en Swagger para elegir la versión
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });

app.UseHttpsRedirection();
// ✅ REGISTRA EL MIDDLEWARE DE ERRORES AQUÍ (Temprano en el pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();
// El orden importa: CORS va antes de Auth y Controllers
app.UseCors("PermitirTodo"); 
app.UseAuthentication(); // ✅ Primero autenticamos
app.UseAuthorization();  // ✅ Luego autorizamos
app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();
  
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

