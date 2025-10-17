namespace AnimalCrossingTracker.Models
{
    public class UserCollectible
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public int CollectibleId { get; set; }
        public bool HasItem { get; set; }
    }
}
