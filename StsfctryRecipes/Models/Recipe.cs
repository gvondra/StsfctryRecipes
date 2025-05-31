using Newtonsoft.Json;
using System.Collections.Generic;

namespace StsfctryRecipes.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double ProductionRate { get; set; } // per minute from 1 production unit
        public bool IsEnabled { get; set; } = true;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete]
        public List<RecipeItem> Items { get; set; }
        public List<RecipeItem> ConsumedItems { get; init; } = new List<RecipeItem>();
        public List<RecipeItem> ProducedItems { get; init; } = new List<RecipeItem>();
    }
}
