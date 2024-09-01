﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StsfctryRecipes.Models;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace StsfctryRecipes
{
    internal class Program
    {
        private const string _fileName = "stsfctry-recipes.json";

        public static void Main(string[] args)
        {
            try
            {
                ProcessCommand(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void ProcessCommand(string[] args)
        {
            RootCommand rootCommand = new RootCommand("Satisfactory Recipe Calculator");

            Command recipeCommand = new Command("recipe", "Recipe options");
            rootCommand.AddCommand(recipeCommand);

            recipeCommand.AddCommand(CreateListRecipeCommand());
            recipeCommand.AddCommand(CreateUpdateRecipeCommand());
            recipeCommand.AddCommand(CreateAddRecipeCommand());

            rootCommand.Invoke(args);
        }

        private static Command CreateAddRecipeCommand()
        {
            Command addRecipeCommand = new Command("add", "Add new recipe");

            Argument<string> title = new Argument<string>("title", "Recipe title");
            addRecipeCommand.AddArgument(title);

            Argument<double> productionRate = new Argument<double>("production-rate", () => 1.0, "Items produced per minute from 1 production unit");
            addRecipeCommand.AddArgument(productionRate);

            addRecipeCommand.SetHandler(
                (t, pR) =>
                {
                    EditRecipes((l) => RecipeEdit.Add(l, t, pR));
                    ListRecipes();
                },
                title,
                productionRate);

            return addRecipeCommand;
        }

        private static Command CreateUpdateRecipeCommand()
        {
            Command updateComand = new Command("update", "Update existing recipe");

            Argument<int> id = new Argument<int>("id", "Recipe id");
            updateComand.AddArgument(id);

            Option<string> title = new Option<string>("--title", () => string.Empty, "Recipe title");
            updateComand.AddOption(title);

            Option<double?> productionRate = new Option<double?>("--production-rate", () => default(double?), "Recipe production rate per minute");
            updateComand.AddOption(productionRate);

            updateComand.SetHandler(
                (i, t, pR) =>
                {
                    EditRecipes((l) => RecipeEdit.Update(l, i, t, pR));
                    Console.WriteLine("Recipe updated");
                },
                id,
                title,
                productionRate);

            return updateComand;
        }

        private static Command CreateListRecipeCommand()
        {
            Command recipeListCommand = new Command("list", "List recipes");

            Argument<int?> id = new Argument<int?>("id", () => default(int?), "Recipe id");
            recipeListCommand.AddArgument(id);

            recipeListCommand.SetHandler(
                (i) => ListRecipes(i),
                id);
            return recipeListCommand;
        }

        private static void ListRecipes(int? id = null)
        {
            Console.WriteLine("Satisfactory Recipes");
            List<Recipe> recipes = LoadRecipes();
            Recipe targetRecipe = recipes.SingleOrDefault(r => r.Id == id);
            if (targetRecipe != null)
            {
                ListRecipe(targetRecipe);
            }
            else
            {
                foreach (Recipe recipe in recipes)
                {
                    Console.WriteLine($"{recipe.Id:000} {recipe.Title}");
                }
            }
        }

        private static void ListRecipe(Recipe recipe)
        {
            Console.WriteLine($"Recipe {recipe.Id}: {recipe.Title}");
            Console.WriteLine($"Production Rate: {recipe.ProductionRate:###,##0} per minute");
        }

        public delegate IEnumerable<Recipe> EditRecipesDelegate(List<Recipe> recipes);

        private static void EditRecipes(EditRecipesDelegate editRecipesDelegate)
        {
            try
            {
                SaveRecipes(editRecipesDelegate.Invoke(LoadRecipes()));
            }
            catch (DuplicateRecipeException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static List<Recipe> LoadRecipes()
        {
            List<Recipe> result = null;
            if (File.Exists(_fileName))
            {
                using FileStream fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                using StreamReader streamReader = new StreamReader(fileStream);
                using JsonTextReader jsonReader = new JsonTextReader(streamReader);
                JsonSerializer serializer = new JsonSerializer()
                {
                    ContractResolver = new DefaultContractResolver()
                };
                result = serializer.Deserialize<List<Recipe>>(jsonReader);
            }
            return result ?? new List<Recipe>();
        }

        private static void SaveRecipes(IEnumerable<Recipe> recipes)
        {
            ArgumentNullException.ThrowIfNull(recipes);
            using FileStream fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter streamWriter = new StreamWriter(fileStream);
            using JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter);
            JsonSerializer serializer = new JsonSerializer()
            {
                ContractResolver = new DefaultContractResolver(),
                Formatting = Formatting.Indented,
            };
            serializer.Serialize(jsonWriter, recipes);
        }
    }
}
