//api response model for criteria suggestion

namespace RandomMenuLambda.Models;


public class CriteriaSuggestion{
    public string Criteria { get; set; } = string.Empty; //what user ask for 
    public string SuggestedFood { get; set; } = string.Empty; //new food name suggested by AI

    public Recipe? Recipe { get; set; } 
    public string Reason { get; set; } = string.Empty; //reason for the suggested food name
   
}