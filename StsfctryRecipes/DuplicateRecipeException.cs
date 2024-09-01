namespace StsfctryRecipes
{
    public class DuplicateRecipeException : ApplicationException
    {
        public DuplicateRecipeException(string title)
            : base($"Duplicate recipe: {title}")
        { }
    }
}
