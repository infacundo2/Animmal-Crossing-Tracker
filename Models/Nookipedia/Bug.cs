namespace AnimalCrossingTracker.Models.Nookipedia
{
    public class Bug
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Image_Url { get; set; } = "";
        public string Render_Url { get; set; } = "";
        public string Location { get; set; } = "";
        public string Rarity { get; set; } = "";
        public int Sell_Nook { get; set; }
        public int Sell_Flick { get; set; }
        public string Weather { get; set; } = "";
        public Availability North { get; set; } = new();
        public Availability South { get; set; } = new();
        public string Catch_Phrase { get; set; } = "";
        public string Museum_Phrase { get; set; } = "";
        public string Version_Added { get; set; } = "";
    }
}
