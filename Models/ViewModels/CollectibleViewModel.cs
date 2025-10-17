namespace AnimalCrossingTracker.Models.ViewModels
{
    public class CollectibleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool HasItem { get; set; }
    }
}
