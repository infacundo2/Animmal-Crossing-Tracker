namespace AnimalCrossingTracker.Models.Nookipedia
{
    public class Item
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Image_Url { get; set; } = "";
        public int Stack { get; set; }
        public int Sell { get; set; }
        public string Material_Type { get; set; } = "";
        public string Material_Seasonality { get; set; } = "";
        public bool Edible { get; set; }
        public string Version_Added { get; set; } = "";
        public bool Unlocked { get; set; }
        public string Notes { get; set; } = "";
    }
}
