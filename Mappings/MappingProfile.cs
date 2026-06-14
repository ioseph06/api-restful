using AutoMapper;
using MiPrimeraApi.DTOs;
using MiPrimeraApi.Models;

namespace MiPrimeraApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // De DTO a Entidad (para crear/actualizar)
            CreateMap<CrearProductoDto, Producto>();
            CreateMap<ActualizarProductoDto, Producto>();

            // De Entidad a DTO (para lecturas)
            CreateMap<Producto, ProductoDto>();
        }
    }
}