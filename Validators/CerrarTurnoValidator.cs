using crud_park_back.DTOs;
using FluentValidation;

namespace crud_park_back.Validators
{
    public class CerrarTurnoValidator : AbstractValidator<CerrarTurnoDTO>
    {
        public CerrarTurnoValidator()
        {
            RuleFor(x => x.TurnoId)
                .GreaterThan(0).WithMessage("El ID del turno es requerido");
        }
    }
}
