using AutoMapper;
using DishesApi.Entities;
using DishesApi.Models;

namespace DishesApi.Profiles
{
    public class CreateDishProfile : Profile
    {
        public CreateDishProfile()
        {
            CreateMap<CreateDishDto, Dish>();
        }
    }
}