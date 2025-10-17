using Microsoft.AspNetCore.Mvc;
using AnimalCrossingTracker.Services;
using AnimalCrossingTracker.Data;
using AnimalCrossingTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimalCrossingTracker.Controllers
{
    public class PopulateController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly NookipediaService _nookipedia;

        public PopulateController(ApplicationDbContext db, NookipediaService nookipedia)
        {
            _db = db;
            _nookipedia = nookipedia;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var allTasks = new List<Task<List<Collectible>>>
            {
                _nookipedia.GetFishAsync(),
                _nookipedia.GetBugsAsync(),
                _nookipedia.GetSeaCreaturesAsync(),
                _nookipedia.GetEventsAsync(),
                _nookipedia.GetArtAsync(),
                _nookipedia.GetFurnitureAsync(),
                _nookipedia.GetClothingAsync(),
                _nookipedia.GetInteriorAsync(),
                _nookipedia.GetToolsAsync(),
                _nookipedia.GetPhotosAsync(),
                _nookipedia.GetItemsAsync(),
                _nookipedia.GetRecipesAsync(),
                _nookipedia.GetFossilsIndividualsAsync(),
                _nookipedia.GetFossilsGroupsAsync(),
                _nookipedia.GetGyroidsAsync(),
                _nookipedia.GetVillagersAsync()
            };

            var allResults = await Task.WhenAll(allTasks);
            var allCollectibles = allResults.SelectMany(r => r).ToList();

            int newItems = 0;
            foreach (var item in allCollectibles)
            {
                if (!await _db.Collectibles.AnyAsync(c => c.Name == item.Name && c.Category == item.Category))
                {
                    _db.Collectibles.Add(item);
                    newItems++;
                }
            }

            await _db.SaveChangesAsync();
            return Content($"✅ {newItems} ítems nuevos importados de Nookipedia.");
        }
    }
}
