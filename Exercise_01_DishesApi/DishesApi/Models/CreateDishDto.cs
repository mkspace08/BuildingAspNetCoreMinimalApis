namespace DishesApi.Models
{
    public class CreateDishDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        // Add other properties needed for creation, except Id
    }
}