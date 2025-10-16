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
            CreateMap<Mensualidad, MensualidadDTO>();
            CreateMap<CreateMensualidadDTO, Mensualidad>();
            CreateMap<UpdateMensualidadDTO, Mensualidad>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Tarifa mappings
            CreateMap<Tarifa, TarifaDTO>();
            CreateMap<CreateTarifaDTO, Tarifa>();
            CreateMap<UpdateTarifaDTO, Tarifa>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Ingreso mappings
            CreateMap<Ingreso, IngresoDTO>()
                .ForMember(dest => dest.OperadorIngresoNombre, opt => opt.MapFrom(src => src.OperadorIngreso != null ? src.OperadorIngreso.Nombre : null))
                .ForMember(dest => dest.OperadorSalidaNombre, opt => opt.MapFrom(src => src.OperadorSalida != null ? src.OperadorSalida.Nombre : null));
            
            CreateMap<CreateIngresoDTO, Ingreso>()
                .ForMember(dest => dest.FechaIngreso, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.OperadorIngresoId, opt => opt.MapFrom(src => src.OperadorIngresoId))
                .ForMember(dest => dest.NumeroFolio, opt => opt.Ignore()); // Se generará automáticamente
            
            // Turno mappings
            CreateMap<Turno, TurnoDTO>();
            CreateMap<CreateTurnoDTO, Turno>()
                .ForMember(dest => dest.FechaApertura, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}

