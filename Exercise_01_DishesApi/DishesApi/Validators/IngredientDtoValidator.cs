using FluentValidation;
using DishesApi.Models;

namespace DishesApi.Validators;

public class IngredientDtoValidator : AbstractValidator<IngredientDto>
{
    public IngredientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
