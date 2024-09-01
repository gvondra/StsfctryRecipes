using StsfctryRecipes.Models;
using System.Collections.Generic;
using System.Linq;

namespace StsfctryRecipes
{
    public static class RecipeEdit
    {
        public static IEnumerable<Recipe> Add(List<Recipe> recipes, string title, double productionRate)
        {
            if (recipes.Any(r => string.Equals(r.Title, title, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DuplicateRecipeException(title);
            }
            Recipe recipe = new Recipe
            {
                Id = recipes.Count > 0 ? recipes.Max(r => r.Id) + 1 : 1,
                Title = title,
                ProductionRate = productionRate,
            };
            return recipes.Concat(new List<Recipe> { recipe }).OrderBy(r => r.Id);
        }

        public static IEnumerable<Recipe> Update(List<Recipe> recipes, int id, string title, double? productionRate)
        {
            List<Recipe> result = null;
            int index = recipes.FindIndex(r => r.Id == id);
            if (index >= 0)
            {
                Recipe existingRecipe = recipes[index];
                Recipe newRecipe = new Recipe
                {
                    Id = existingRecipe.Id,
                    Title = string.IsNullOrEmpty(title) ? existingRecipe.Title : title,
                    ProductionRate = productionRate.HasValue ? productionRate.Value : existingRecipe.ProductionRate,
                    IsEnabled = existingRecipe.IsEnabled,
                    Items = new List<RecipeItem>(existingRecipe.Items)
                };
                result = new List<Recipe>(recipes.Where(r => r.Id != id))
                {
                    newRecipe
                };
            }
            else
            {
                throw new RecipeNotFoundException(id.ToString());
            }
            return result.OrderBy(r => r.Id).ToList() ?? recipes;
        }

        public static IEnumerable<Recipe> AddDependency(List<Recipe> recipes, int id, int targetId, double consuptionRate)
        {
            List<Recipe> result = null;
            int index = recipes.FindIndex(r => r.Id == id);
            if (index < 0)
            {
                throw new RecipeNotFoundException(id.ToString());
            }
            int targetIndex = recipes.FindIndex(r => r.Id == targetId);
            if (targetIndex < 0)
            {
                throw new RecipeNotFoundException(targetId.ToString());
            }
            Recipe existingRecipe = recipes[index];
            if (!existingRecipe.Items.Exists(r => r.RecipeId == targetId))
            {
                Recipe newRecipe = new Recipe
                {
                    Id = existingRecipe.Id,
                    Title = existingRecipe.Title,
                    ProductionRate = existingRecipe.ProductionRate,
                    IsEnabled = existingRecipe.IsEnabled,
                    Items = new List<RecipeItem>(existingRecipe.Items.Concat(new List<RecipeItem> { new RecipeItem { RecipeId = targetId, ConsuptionRate = consuptionRate} }))
                };
                result = new List<Recipe>(recipes.Where(r => r.Id != id))
                {
                    newRecipe
                };
            }
            return result?.OrderBy(r => r.Id)?.ToList() ?? recipes;
        }

        public static IEnumerable<Recipe> RemoveDependency(List<Recipe> recipes, int id, int targetId)
        {
            List<Recipe> result = null;
            int index = recipes.FindIndex(r => r.Id == id);
            if (index < 0)
            {
                throw new RecipeNotFoundException(id.ToString());
            }
            Recipe existingRecipe = recipes[index];
            Recipe newRecipe = new Recipe
            {
                Id = existingRecipe.Id,
                Title = existingRecipe.Title,
                ProductionRate = existingRecipe.ProductionRate,
                IsEnabled = existingRecipe.IsEnabled,
                Items = new List<RecipeItem>(existingRecipe.Items.Where(i => i.RecipeId != targetId))
            };
            result = new List<Recipe>(recipes.Where(r => r.Id != id))
            {
                newRecipe
            };
            return result?.OrderBy(r => r.Id)?.ToList() ?? recipes;
        }
    }
}
