using AutoMapper;
using DishesApi.Entities;
using DishesApi.Models;

namespace DishesApi.Profiles;

public class IngredientProfile : Profile
{
    public IngredientProfile()
    {
        CreateMap<Ingredient, IngredientDto>()
            .ForMember(dest => dest.DishId, opt => opt.MapFrom(src => src.Dishes != null && src.Dishes.Any() ? src.Dishes.First().Id : Guid.Empty));
        CreateMap<IngredientDto, Ingredient>();
    }
}
