using crud_park_back.DTOs;
using FluentValidation;

namespace crud_park_back.Validators
{
    public class UpdateMensualidadValidator : AbstractValidator<UpdateMensualidadDTO>
    {
        public UpdateMensualidadValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras y espacios");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del email no es válido")
                .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Placa)
                .NotEmpty().WithMessage("La placa es requerida")
                .MaximumLength(10).WithMessage("La placa no puede exceder 10 caracteres")
                .Matches(@"^[A-Za-z0-9]+$").WithMessage("La placa solo puede contener letras y números");

            RuleFor(x => x.FechaInicio)
                .NotEmpty().WithMessage("La fecha de inicio es requerida");

            RuleFor(x => x.FechaFin)
                .NotEmpty().WithMessage("La fecha de fin es requerida")
                .GreaterThan(x => x.FechaInicio).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio");

        }
    }
}
