using FluentValidation;

namespace CRM.Application.Users.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID gereklidir.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni parola gereklidir.")
            .MinimumLength(6).WithMessage("Parola en az 6 karakter olmalıdır.");
    }
}

