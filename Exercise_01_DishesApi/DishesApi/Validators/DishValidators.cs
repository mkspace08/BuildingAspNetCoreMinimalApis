using FluentValidation;
using DishesApi.Models;

namespace DishesApi.Validators;

public class CreateDishDtoValidator : AbstractValidator<CreateDishDto>
{
    public CreateDishDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");
        // Add rules for other properties if needed
    }
}

public class UpdateDishDtoValidator : AbstractValidator<UpdateDishDto>
{
    public UpdateDishDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");
        // Add rules for other properties if needed
    }
}
