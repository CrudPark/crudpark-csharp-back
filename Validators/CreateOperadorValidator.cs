using crud_park_back.DTOs;
using FluentValidation;

namespace crud_park_back.Validators
{
    public class CreateOperadorValidator : AbstractValidator<CreateOperadorDTO>
    {
        public CreateOperadorValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras y espacios");


            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del email no es válido")
                .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
