using StsfctryRecipes.Models;
using System.Collections.Generic;

namespace StsfctryRecipes
{
    public class Calculator
    {
        private readonly Dictionary<int, double> _recipeConsumptionTotals = new Dictionary<int, double>();

        public void Calculate(List<Recipe> recipes, int id, double? consuptionRate)
        {
            Recipe recipe = recipes.Find(r => r.Id == id);
            if (recipe == null)
                throw new RecipeNotFoundException(id.ToString());
            if (!consuptionRate.HasValue)
                consuptionRate = recipe.ProductionRate;
            Console.WriteLine($"Calculating production of {consuptionRate} {recipe.Title} per minute.");
            Console.WriteLine($"Requires {consuptionRate.Value / recipe.ProductionRate} production unit(s)");
            _recipeConsumptionTotals.Add(recipe.Id, consuptionRate.Value);
            double scale = consuptionRate.Value / recipe.ProductionRate;
            foreach (RecipeItem item in recipe.Items)
            {
                Calculate(recipes, recipe, item, scale * item.ConsuptionRate, string.Empty);
            } 
            PrintTotals(recipes);
        }

        private void Calculate(List<Recipe> recipes, Recipe recipe, RecipeItem recipeItem, double consuptionRate, string padding)
        {
            Recipe child = recipes.Find(r => r.Id == recipeItem.RecipeId);
            double scale = consuptionRate / child.ProductionRate;
            bool isLast = recipe.Items.Count - 1 == recipe.Items.IndexOf(recipeItem);
            if (isLast)
                Console.Write(padding + "└ ");
            else
                Console.Write(padding + "├ ");
            Console.WriteLine($"{child.Title} x {scale} = {consuptionRate} per minute ");
            if (!_recipeConsumptionTotals.TryAdd(child.Id, consuptionRate))
            {
                _recipeConsumptionTotals[child.Id] += consuptionRate;
            }
            string childPadding;
            if (isLast)
                childPadding = padding + "  ";
            else
                childPadding = padding + "│ ";
            foreach (RecipeItem item in child.Items)
            {
                Calculate(recipes, child, item, scale * item.ConsuptionRate, childPadding);
            }
        }

        private void PrintTotals(List<Recipe> recipes)
        {
            foreach (KeyValuePair<int, double> keyValuePair in _recipeConsumptionTotals)
            {
                Recipe recipe = recipes.Find(r => r.Id == keyValuePair.Key);
                Console.WriteLine($"{recipe.Title} x {keyValuePair.Value / recipe.ProductionRate} total consumption {keyValuePair.Value} per minute");
            }
        }
    }
}
