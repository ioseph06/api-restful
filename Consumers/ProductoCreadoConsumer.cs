// Consumers/ProductoCreadoConsumer.cs
using MassTransit;
using Microsoft.Extensions.Logging;
using MiPrimeraApi.Events;
using System.Threading.Tasks;

namespace MiPrimeraApi.Consumers
{
    // Implementamos IConsumer<T> donde T es nuestro evento
    public class ProductoCreadoConsumer : IConsumer<ProductoCreadoEvent>
    {
        private readonly ILogger<ProductoCreadoConsumer> _logger;

        public ProductoCreadoConsumer(ILogger<ProductoCreadoConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductoCreadoEvent> context)
        {
            var evento = context.Message;

            _logger.LogInformation("📩 [CONSUMER] Recibido evento: ProductoCreadoEvent");
            _logger.LogInformation("📩 [CONSUMER] Procesando notificación para el producto: {Nombre} (ID: {ProductoId})", evento.Nombre, evento.ProductoId);

            // SIMULACIÓN DE TRABAJO PESADO (Ej: Enviar email, generar PDF, llamar a API externa)
            // Esto tarda 3 segundos, pero NO bloqueará la petición HTTP del usuario.
            await Task.Delay(3000); 

            _logger.LogInformation("✅ [CONSUMER] Procesamiento del producto {ProductoId} completado exitosamente.", evento.ProductoId);
        }
    }
}