namespace StsfctryRecipes
{
    public class RecipeNotFoundException : ApplicationException
    {
        public RecipeNotFoundException(string identifier)
            : base($"Recipe {identifier} not found")
        { }
    }
}
