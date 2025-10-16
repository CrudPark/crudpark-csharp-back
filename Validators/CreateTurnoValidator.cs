using crud_park_back.DTOs;
using FluentValidation;

namespace crud_park_back.Validators
{
    public class CreateTurnoValidator : AbstractValidator<CreateTurnoDTO>
    {
        public CreateTurnoValidator()
        {
            RuleFor(x => x.OperadorId)
                .GreaterThan(0).WithMessage("El operador es requerido");
        }
    }
}
