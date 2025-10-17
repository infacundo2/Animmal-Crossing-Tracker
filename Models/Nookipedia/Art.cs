namespace AnimalCrossingTracker.Models.Nookipedia
{
    public class Art
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Image_Url { get; set; } = "";
        public string Real_Image_Url { get; set; } = "";
        public string Fake_Image_Url { get; set; } = "";
        public bool Has_Fake { get; set; }
        public string Authenticity { get; set; } = "";
        public string Buy_Price { get; set; } = "";
        public string Sell_Price { get; set; } = "";
        public string Museum_Phrase { get; set; } = "";
    }
}
