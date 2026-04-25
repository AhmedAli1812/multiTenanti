using FluentValidation;

public class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.MedicalNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty();

        RuleFor(x => x.Gender)
            .NotEmpty();

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow);
    }
}