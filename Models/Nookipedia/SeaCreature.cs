namespace AnimalCrossingTracker.Models.Nookipedia
{
    public class SeaCreature
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Image_Url { get; set; } = "";
        public string Render_Url { get; set; } = "";
        public int Sell_Nook { get; set; }
        public string Shadow_Size { get; set; } = "";
        public string Movement_Speed { get; set; } = "";
        public string Catch_Phrase { get; set; } = "";
        public string Museum_Phrase { get; set; } = "";
        public string Version_Added { get; set; } = "";
        public Availability North { get; set; } = new();
        public Availability South { get; set; } = new();
    }
}

