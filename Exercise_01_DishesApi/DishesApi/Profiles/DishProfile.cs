using AutoMapper;
using DishesApi.Entities;
using DishesApi.Models;

namespace DishesApi.Profiles;

public class DishProfile : Profile
{
    public DishProfile()
    {
        // Entity <-> DTO
        CreateMap<Dish, DishDto>();
        CreateMap<DishDto, Dish>();
        // Create DTO -> Entity
        CreateMap<CreateDishDto, Dish>();
        // Update DTO -> Entity
        CreateMap<UpdateDishDto, Dish>();
    }
}
