using Microsoft.AspNetCore.Identity;

namespace AnimalCrossingTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Aquí podrías agregar campos personalizados más adelante
        // Ejemplo: public string IslandName { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
