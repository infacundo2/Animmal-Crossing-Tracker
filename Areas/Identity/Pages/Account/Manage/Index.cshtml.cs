using System.IO;
using System.Threading.Tasks;
using AnimalCrossingTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AnimalCrossingTracker.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public string? ProfileImageUrl { get; set; } // 👈 Añade esto

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("No se pudo cargar el usuario.");

            ProfileImageUrl = user.ProfileImageUrl; // 👈 Carga actual

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("No se pudo cargar el usuario.");

            user.ProfileImageUrl = ProfileImageUrl; // 👈 Guarda cambios
            await _userManager.UpdateAsync(user);

            StatusMessage = "✅ Tu perfil ha sido actualizado.";
            return RedirectToPage();
        }
    }
}
