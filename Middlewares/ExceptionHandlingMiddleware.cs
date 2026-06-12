// Middlewares/ExceptionHandlingMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiPrimeraApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        // Usamos el logger de Serilog (o el ILogger genérico, Serilog lo intercepta)
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 🌟 REGISTRO ESTRUCTURADO: Pasamos la excepción como primer argumento
                // y usamos propiedades con nombre ({Path}, {Method})
                _logger.LogError(ex, "Excepción no controlada en la petición {Method} {Path}", 
                    context.Request.Method, 
                    context.Request.Path);
                
                await HandleExceptionAsync(context);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Ocurrió un error interno en el servidor. El incidente ha sido registrado.",
                Timestamp = DateTime.UtcNow
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}