// Swagger/ConfigureSwaggerOptions.cs
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MiPrimeraApi.Swagger
{
    /// <summary>
    /// Configura SwaggerGen creando un documento por cada versión de la API descubierta.
    /// El framework nos inyecta IApiVersionDescriptionProvider por constructor:
    /// así no hace falta el anti-patrón de llamar a BuildServiceProvider() en Program.cs.
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo
                    {
                        Title = $"MiPrimeraApi {description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                        Description = description.IsDeprecated
                            ? "Esta versión está obsoleta."
                            : "API de Productos activa."
                    });
            }
        }
    }
}
