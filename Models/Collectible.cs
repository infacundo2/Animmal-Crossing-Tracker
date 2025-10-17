using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimalCrossingTracker.Models
{
    public class Collectible
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [MaxLength(100)]
        public string? Category { get; set; } // Ej: fish, bugs, furniture, etc.

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(300)]
        public string? UnlockMethod { get; set; } // Opcional, para indicar cómo se consigue

        // 🔹 Aquí guardamos el JSON completo del detalle
        public string? JsonData { get; set; }

        // 🔹 Relación con usuarios (opcional)
        public ICollection<UserCollectible>? UserCollectibles { get; set; }
    }
}
