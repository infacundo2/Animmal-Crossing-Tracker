namespace AnimalCrossingTracker.Models.Nookipedia
{
    public class Interior
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Image_Url { get; set; } = "";
        public string Category { get; set; } = "";          // Ej: Wallpaper, Flooring, Rug
        public string Series { get; set; } = "";
        public string Style { get; set; } = "";
        public string Tag { get; set; } = "";
        public string Buy { get; set; } = "";
        public string Sell { get; set; } = "";
        public string Source { get; set; } = "";            // Dónde se obtiene
        public string Version_Added { get; set; } = "";
    }
}
