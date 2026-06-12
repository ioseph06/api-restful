using MiPrimeraApi.DTOs;
using MiPrimeraApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiPrimeraApi.Services
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoDto>> ObtenerTodosAsync();
        Task<ProductoDto?> ObtenerPorIdAsync(int id);
        Task<ProductoDto> CrearAsync(CrearProductoDto dto);
        Task<bool> ActualizarAsync(int id, ActualizarProductoDto dto);
        Task<bool> EliminarAsync(int id);
    }
}