

namespace RandomMenuLambda.Models;

public class FoodDiscovery
{
    public string SuggestedFood { get; set; } = string.Empty; // new food name suggested by AI
    public string Reason { get; set; } = string.Empty; // reason for the suggested food name
    public Recipe? Recipe { get; set; } //shared recipe object
    public List<string> BasedOnFavorites { get; set; } = new List<string>(); // list of user's favorite foods (from dynamo db)
}