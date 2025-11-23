//api response model for recipe

namespace RandomMenuLambda.Models;

public class RecipeResponse
{
    public string? FoodName { get; set; }
    public Recipe? Recipe { get; set; }
    public string? GeneratedAt { get; set; }
    public string? GeneratedBy { get; set; }

}