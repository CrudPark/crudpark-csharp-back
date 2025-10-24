using crud_park_back.DTOs;
using FluentValidation;

namespace crud_park_back.Validators
{
    public class FinalizarIngresoValidator : AbstractValidator<FinalizarIngresoDTO>
    {
        public FinalizarIngresoValidator()
        {
            RuleFor(x => x.IngresoId)
                .GreaterThan(0).WithMessage("El ID del ingreso es requerido");

            RuleFor(x => x.OperadorSalidaId)
                .GreaterThan(0).WithMessage("El operador es requerido");
        }
    }
}
