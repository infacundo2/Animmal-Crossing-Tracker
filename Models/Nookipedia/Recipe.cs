namespace AnimalCrossingTracker.Models.Nookipedia
{
    public class Recipe
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Image_Url { get; set; } = "";
        public List<string> Materials { get; set; } = new();
        public string Source { get; set; } = "";
    }
}
