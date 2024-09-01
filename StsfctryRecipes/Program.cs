using Newtonsoft.Json;
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
            recipeCommand.AddCommand(CreateAddRecipeCommand());
            recipeCommand.AddCommand(CreateUpdateRecipeCommand());
            recipeCommand.AddCommand(CreateAddRecipeDependencyCommand());
            recipeCommand.AddCommand(CreateRemoveRecipeDependencyCommand());

            rootCommand.AddCommand(CreateCalculateComand());

            rootCommand.Invoke(args);
        }

        private static Command CreateCalculateComand()
        {
            Command command = new Command("calc", "Calculate consuption rates and production units");

            Argument<int> id = new Argument<int>("id", "Id of recipe to calculate");
            command.Add(id);

            Argument<double?> consuptionRate = new Argument<double?>("consuption-rate", "Items per minute to be produced");
            command.Add(consuptionRate);

            command.SetHandler(
                (i, cR) =>
                {
                    Calculator calculator = new Calculator();
                    calculator.Calculate(LoadRecipes(), i, cR);
                },
                id,
                consuptionRate);

            return command;
        }
        
        private static Command CreateAddRecipeDependencyCommand()
        {
            Command command = new Command("add-dependency", "Add an inpute recepe dependency");

            Argument<int> id = new Argument<int>("id", "Recipe id");
            command.AddArgument(id);

            Argument<int> dependencyId = new Argument<int>("dependency-id", "Target recipe id");
            command.AddArgument(dependencyId);

            Argument<double> consuptionRate = new Argument<double>("consuption-rate", "Items per minute consumed from the dependency");
            command.AddArgument(consuptionRate);

            command.SetHandler(
                (i, dId, cR) =>
                {
                    EditRecipes((l) => RecipeEdit.AddDependency(l, i, dId, cR));
                    ListRecipes(i);
                },
                id,
                dependencyId,
                consuptionRate);
            return command;
        }

        private static Command CreateRemoveRecipeDependencyCommand()
        {
            Command command = new Command("remove-dependency", "Add an inpute recepe dependency");

            Argument<int> id = new Argument<int>("id", "Recipe id");
            command.AddArgument(id);

            Argument<int> dependencyId = new Argument<int>("dependency-id", "Target recipe id");
            command.AddArgument(dependencyId);

            command.SetHandler(
                (i, dId) =>
                {
                    EditRecipes((l) => RecipeEdit.RemoveDependency(l, i, dId));
                    ListRecipes(i);
                },
                id,
                dependencyId);
            return command;
        }

        private static Command CreateAddRecipeCommand()
        {
            Command command = new Command("add", "Add new recipe");

            Argument<string> title = new Argument<string>("title", "Recipe title");
            command.AddArgument(title);

            Argument<double> productionRate = new Argument<double>("production-rate", () => 1.0, "Items produced per minute from 1 production unit");
            command.AddArgument(productionRate);

            command.SetHandler(
                (t, pR) =>
                {
                    EditRecipes((l) => RecipeEdit.Add(l, t, pR));
                    ListRecipes();
                },
                title,
                productionRate);

            return command;
        }

        private static Command CreateUpdateRecipeCommand()
        {
            Command command = new Command("update", "Update existing recipe");

            Argument<int> id = new Argument<int>("id", "Recipe id");
            command.AddArgument(id);

            Option<string> title = new Option<string>("--title", () => string.Empty, "Recipe title");
            command.AddOption(title);

            Option<double?> productionRate = new Option<double?>("--production-rate", () => default(double?), "Recipe production rate per minute");
            command.AddOption(productionRate);

            command.SetHandler(
                (i, t, pR) =>
                {
                    EditRecipes((l) => RecipeEdit.Update(l, i, t, pR));
                    Console.WriteLine("Recipe updated");
                },
                id,
                title,
                productionRate);

            return command;
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
            List<Recipe> recipes = LoadRecipes();
            Recipe targetRecipe = recipes.SingleOrDefault(r => r.Id == id);
            if (targetRecipe != null)
            {
                ListRecipe(recipes, targetRecipe);
            }
            else
            {
                Console.WriteLine("Satisfactory Recipes");
                foreach (Recipe recipe in recipes)
                {
                    Console.WriteLine($"{recipe.Id:000} {recipe.Title}");
                }
            }
        }

        private static void ListRecipe(List<Recipe> recipes, Recipe recipe)
        {
            Console.WriteLine($"Recipe {recipe.Id}: {recipe.Title}");
            Console.WriteLine($"Production Rate: {recipe.ProductionRate:###,##0} per minute");
            ListRecipeChildren(recipes, recipe);
        }

        private static void ListRecipeChildren(List<Recipe> recipes, Recipe recipe, string padding = "")
        {
            if (recipe.Items.Count > 0)
            {
                for (int i = 0; i < recipe.Items.Count; i += 1)
                {
                    Recipe child = recipes.Find(r => r.Id == recipe.Items[i].RecipeId);
                    bool isLast = recipe.Items.Count - 1 == i;
                    if (isLast)
                    {
                        Console.Write(padding + "└ ");
                    }   
                    else
                    {
                        Console.Write(padding + "├ ");
                    }                        
                    Console.Write($"{recipe.Items[i].ConsuptionRate:###,##0} per min ");                    
                    if (child != null)
                    {
                        Console.WriteLine(child.Title);
                        string childPadding = padding;
                        if (isLast)
                            childPadding += "  ";
                        else
                            childPadding += "│ ";
                        ListRecipeChildren(recipes, child, childPadding);
                    }
                    else
                    {
                        Console.WriteLine("not found");
                    }
                }
            }
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
            catch (RecipeNotFoundException ex)
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
