using Newtonsoft.Json;

namespace StsfctryRecipes.Models
{
    public class RecipeItem
    {
        public int RecipeId { get; set; }
        [Obsolete]
        public double? ConsuptionRate
        {
            get => Rate;
            set
            {
                if (value.HasValue)
                    Rate = value.Value;
            }
        }
        public double Rate { get; set; }
    }
}
