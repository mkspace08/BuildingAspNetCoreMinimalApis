namespace DishesApi.Models
{
    public class UpdateDishDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        // Add other updatable properties as needed
    }
}