using crud_park_back.DTOs;
using crud_park_back.Models;
using FluentValidation;

namespace crud_park_back.Validators
{
    public class CreateIngresoValidator : AbstractValidator<CreateIngresoDTO>
    {
        public CreateIngresoValidator()
        {
            RuleFor(x => x.Placa)
                .NotEmpty().WithMessage("La placa es requerida")
                .MaximumLength(10).WithMessage("La placa no puede exceder 10 caracteres")
                .Matches(@"^[A-Z0-9]+$").WithMessage("La placa solo puede contener letras mayúsculas y números");

            RuleFor(x => x.TipoIngreso)
                .IsInEnum().WithMessage("El tipo de ingreso no es válido");

            RuleFor(x => x.OperadorIngresoId)
                .GreaterThan(0).WithMessage("El operador es requerido");

        }
    }
}
