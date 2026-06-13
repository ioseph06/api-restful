// Events/ProductoCreadoEvent.cs
namespace MiPrimeraApi.Events
{
    // Un record es ideal para eventos de mensajería
    public record ProductoCreadoEvent(
        int ProductoId,
        string Nombre,
        decimal Precio,
        DateTime FechaCreacion
    );
}