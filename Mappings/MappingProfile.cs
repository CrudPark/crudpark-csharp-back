using AutoMapper;
using crud_park_back.DTOs;
using crud_park_back.Models;

namespace crud_park_back.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Operador mappings
            CreateMap<Operador, OperadorDTO>();
            CreateMap<CreateOperadorDTO, Operador>();
            CreateMap<UpdateOperadorDTO, Operador>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Mensualidad mappings
            CreateMap<Mensualidad, MensualidadDTO>()
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombrePropietario));
            CreateMap<CreateMensualidadDTO, Mensualidad>()
                .ForMember(dest => dest.NombrePropietario, opt => opt.MapFrom(src => src.Nombre));
            CreateMap<UpdateMensualidadDTO, Mensualidad>()
                .ForMember(dest => dest.NombrePropietario, opt => opt.MapFrom(src => src.Nombre))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Tarifa mappings
            CreateMap<Tarifa, TarifaDTO>();
            CreateMap<CreateTarifaDTO, Tarifa>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<UpdateTarifaDTO, Tarifa>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            
            // Ingreso mappings
            CreateMap<Ingreso, IngresoDTO>()
                .ForMember(dest => dest.OperadorIngresoNombre, opt => opt.MapFrom(src => src.OperadorIngreso != null ? src.OperadorIngreso.Nombre : null))
                .ForMember(dest => dest.OperadorSalidaNombre, opt => opt.MapFrom(src => src.OperadorSalida != null ? src.OperadorSalida.Nombre : null));
            
            CreateMap<CreateIngresoDTO, Ingreso>()
                .ForMember(dest => dest.FechaIngreso, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.OperadorIngresoId, opt => opt.MapFrom(src => src.OperadorIngresoId))
                .ForMember(dest => dest.NumeroFolio, opt => opt.Ignore()); // Se generará automáticamente
            
            // Turno mappings
            CreateMap<Turno, TurnoDTO>()
                .ForMember(dest => dest.OperadorNombre, opt => opt.MapFrom(src => src.Operador != null ? src.Operador.Nombre : null))
                .ForMember(dest => dest.TotalVehiculos, opt => opt.MapFrom(src => src.TotalIngresos))
                .ForMember(dest => dest.TotalIngresos, opt => opt.MapFrom(src => src.TotalCobros));
            CreateMap<CreateTurnoDTO, Turno>()
                .ForMember(dest => dest.FechaApertura, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}

