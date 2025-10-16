using crud_park_back.DTOs;
using FluentValidation;

namespace crud_park_back.Validators
{
    public class CreateTarifaValidator : AbstractValidator<CreateTarifaDTO>
    {
        public CreateTarifaValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.ValorBaseHora)
                .GreaterThan(0).WithMessage("El valor base por hora debe ser mayor a 0")
                .LessThanOrEqualTo(100000).WithMessage("El valor base por hora no puede exceder $100,000");

            RuleFor(x => x.ValorAdicionalFraccion)
                .GreaterThanOrEqualTo(0).WithMessage("El valor adicional por fracción no puede ser negativo")
                .LessThanOrEqualTo(50000).WithMessage("El valor adicional por fracción no puede exceder $50,000");

            RuleFor(x => x.TopeDiario)
                .GreaterThan(0).WithMessage("El tope diario debe ser mayor a 0")
                .LessThanOrEqualTo(1000000).WithMessage("El tope diario no puede exceder $1,000,000");

            RuleFor(x => x.TiempoGraciaMinutos)
                .GreaterThanOrEqualTo(30).WithMessage("El tiempo de gracia mínimo es 30 minutos")
                .LessThanOrEqualTo(120).WithMessage("El tiempo de gracia no puede exceder 120 minutos");
        }
    }
}
