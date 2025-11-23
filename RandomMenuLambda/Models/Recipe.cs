

namespace RandomMenuLambda.Models;

public class Recipe
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Ingredients { get; set; }
    public List<string>? Instructions { get; set; }
    public string? PrepTime { get; set; }
    public string? CookTime { get; set; }
    public int? Servings { get; set; }
    public string? Difficulty { get; set; }
}