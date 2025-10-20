using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using AnimalCrossingTracker.Data;
using AnimalCrossingTracker.Models;
using AnimalCrossingTracker.Services;
using System.Text.Json;
using System.Text.Json.Nodes;
using AnimalCrossingTracker.Models.ViewModels;

namespace AnimalCrossingTracker.Controllers
{
    [Authorize]
    public class CollectiblesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NookipediaService _nookipediaService;

        public CollectiblesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, NookipediaService nookipediaService)
        {
            _context = context;
            _userManager = userManager;
            _nookipediaService = nookipediaService;
        }

        public class CollectibleJsonData
        {
            public string Name { get; set; }
            public List<string> N_availability_array { get; set; }
            public List<string> S_availability_array { get; set; }
            public Dictionary<string, string> Times_by_month_north { get; set; }
            public Dictionary<string, string> Times_by_month_south { get; set; }
        }

        
         private string GetImageUrl(Collectible c)
        {
            if (string.IsNullOrEmpty(c.JsonData))
                return "/images/default-placeholder.png";

            try
            {
                var root = JsonNode.Parse(c.JsonData);

                JsonObject? obj = null;

                // Si el JSON es un array, toma el primer elemento
                if (root is JsonArray arr && arr.Count > 0)
                    obj = arr[0]?.AsObject();
                else if (root is JsonObject o)
                    obj = o;

                if (obj == null)
                    return "/images/default-placeholder.png";

                // 1️⃣ Intentar image_url directo
                var directUrl = obj["image_url"]?.ToString();
                if (!string.IsNullOrEmpty(directUrl))
                    return directUrl;

                // 2️⃣ Intentar variaciones
                var variations = obj["variations"]?.AsArray();
                if (variations != null && variations.Count > 0)
                {
                    var firstVar = variations[0];
                    var varImage = firstVar?["image_url"]?.ToString();
                    if (!string.IsNullOrEmpty(varImage))
                        return varImage;
                }

                // 3️⃣ Nada encontrado
                return "/images/default-placeholder.png";
            }
            catch
            {
                return "/images/default-placeholder.png";
            }
        }


        
        // GET: /Collectibles?category=fish&search=salmon&month=12&page=1
        public async Task<IActionResult> Index(string? category, string? search, int? month, int page = 1)
        {
            const int pageSize = 20;

            var user = await _userManager.GetUserAsync(User);

            // 🔹 Consulta base
            var query = _context.Collectibles.AsQueryable();

            // 🔹 Filtro por categoría
            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.Category == category);

            // 🔹 Filtro por texto (nombre o descripción)
            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Name.Contains(search) || c.Description.Contains(search));

            // 🗓️ Filtro por mes (basado en número de mes exacto)
            if (month.HasValue)
            {
                string monthStr = month.Value.ToString();
                
                query = query.Where(c => !string.IsNullOrEmpty(c.JsonData) && (
                    
                    // Buscar en s_availability_array  
                    (c.JsonData.Contains($"\"s_availability_array\":") &&
                    (c.JsonData.Contains($"\"s_availability_array\":[\"{monthStr}\"]") ||
                    c.JsonData.Contains($"\"s_availability_array\":[\"{monthStr}\",") ||
                    c.JsonData.Contains($",\"{monthStr}\"]"))) ||
                    
                    
                    // Buscar en times_by_month_south (no "NA")
                    (c.JsonData.Contains($"\"times_by_month_south\"") &&
                    c.JsonData.Contains($"\"{monthStr}\":") &&
                    !c.JsonData.Contains($"\"{monthStr}\":\"NA\"") &&
                    !c.JsonData.Contains($"\"{monthStr}\": \"NA\""))
                ));
            }





            // 🔹 Paginación
            var totalItems = await query.CountAsync();
            var collectibles = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🔹 Coleccionables del usuario
            var userItems = await _context.UserCollectibles
                                        .Where(uc => uc.UserId == user.Id && uc.HasItem)
                                        .Select(uc => uc.CollectibleId)
                                        .ToListAsync();

            // 🔹 Proyección al ViewModel
            var model = collectibles.Select(c => new CollectibleViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Category = c.Category,
                Description = c.Description,
                ImageUrl = GetImageUrl(c), // 👈 función auxiliar
                HasItem = userItems.Contains(c.Id)
            }).ToList();

            




            // 🔹 Enviar datos a la vista
            ViewBag.SelectedCategory = category;
            ViewBag.SearchQuery = search;
            ViewBag.SelectedMonth = month;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(model);


        }




        // POST: /Collectibles/Toggle/5
        [HttpPost]
        public async Task<IActionResult> Toggle(int id, string? category, string? search, int? month, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            var userCollectible = await _context.UserCollectibles
                                        .FirstOrDefaultAsync(uc => uc.UserId == user.Id && uc.CollectibleId == id);

            if (userCollectible != null)
            {
                userCollectible.HasItem = !userCollectible.HasItem;
            }
            else
            {
                userCollectible = new UserCollectible
                {
                    UserId = user.Id,
                    CollectibleId = id,
                    HasItem = true
                };
                _context.UserCollectibles.Add(userCollectible);
            }

            await _context.SaveChangesAsync();

            // Redirigir preservando filtros
            return RedirectToAction(nameof(Index), new { category, search, month, page });
        }


        // GET: /Collectibles/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var collectible = await _context.Collectibles.FindAsync(id);
            if (collectible == null)
                return NotFound();

            // Si aún no hay JsonData guardado, lo descargamos desde la API
            if (string.IsNullOrEmpty(collectible.JsonData))
            {
                string? endpoint = collectible.Category switch
                {
                    "fish" => $"https://api.nookipedia.com/nh/fish/{Uri.EscapeDataString(collectible.Name)}",
                    "bugs" => $"https://api.nookipedia.com/nh/bugs/{Uri.EscapeDataString(collectible.Name)}",
                    "sea" => $"https://api.nookipedia.com/nh/sea/{Uri.EscapeDataString(collectible.Name)}",
                    "art" => $"https://api.nookipedia.com/nh/art/{Uri.EscapeDataString(collectible.Name)}",
                    "villagers" => $"https://api.nookipedia.com/villagers/{Uri.EscapeDataString(collectible.Name)}",
                    "items" => $"https://api.nookipedia.com/nh/items/{Uri.EscapeDataString(collectible.Name)}",
                    "furniture" => $"https://api.nookipedia.com/nh/furniture/{Uri.EscapeDataString(collectible.Name)}",
                    "clothing" => $"https://api.nookipedia.com/nh/clothing/{Uri.EscapeDataString(collectible.Name)}",
                    "tools" => $"https://api.nookipedia.com/nh/tools/{Uri.EscapeDataString(collectible.Name)}",
                    "gyroids" => $"https://api.nookipedia.com/nh/gyroids/{Uri.EscapeDataString(collectible.Name)}",
                    "recipes" => $"https://api.nookipedia.com/nh/recipes/{Uri.EscapeDataString(collectible.Name)}",
                    "fossils_individuals" => $"https://api.nookipedia.com/nh/fossils/individuals/{Uri.EscapeDataString(collectible.Name)}",
                    "interior" => $"https://api.nookipedia.com/nh/interior/{Uri.EscapeDataString(collectible.Name)}",
                    _ => null
                };

                if (endpoint != null)
                {
                    // ⚠️ Aquí usamos el nuevo método público
                    var rawJson = await _nookipediaService.GetRawJsonAsync(endpoint);

                    if (!string.IsNullOrEmpty(rawJson))
                    {
                        collectible.JsonData = rawJson;
                        await _context.SaveChangesAsync();

                        Console.WriteLine($"✅ JSON guardado para {collectible.Name}:\n{rawJson}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ No se recibió JSON para {collectible.Name}");
                    }
                }
            }

            return View(collectible);
        }



        // 🔹 Método auxiliar para mostrar por categoría directamente
        [HttpGet("/Collectibles/Category/{category}")]
        public async Task<IActionResult> Category(string category)
        {
            return await Index(category, search: null, month: null, page: 1);
        }

        // GET: /Collectibles/Populate
        [AllowAnonymous]
        public async Task<IActionResult> Populate()
        {
            // 🔹 Llamamos al método del servicio que trae TODOS los coleccionables
            var collectibles = await _nookipediaService.GetAllCollectiblesAsync();

            if (collectibles == null || !collectibles.Any())
            {
                return Content("⚠️ No se pudieron obtener coleccionables desde la API de Nookipedia.");
            }

            int nuevos = 0;

            foreach (var item in collectibles)
            {
                // Evitar duplicados por nombre
                if (!await _context.Collectibles.AnyAsync(c => c.Name == item.Name))
                {
                    await _context.Collectibles.AddAsync(item);
                    nuevos++;
                }
            }

            if (nuevos > 0)
                await _context.SaveChangesAsync();

            return Content($"✅ {nuevos} ítems nuevos importados desde Nookipedia");
        }
    [AllowAnonymous]
public async Task<IActionResult> PopulateDetailed()
{
    var allFish = await _nookipediaService.GetFishAsync();
    var added = 0;

    foreach (var fish in allFish)
    {
        if (!await _context.Collectibles.AnyAsync(c => c.Name == fish.Name && c.Category == "fish"))
        {
            // Descargar datos detallados
            var detail = await _nookipediaService.GetFishDetailAsync(fish.Name);
            if (detail != null)
            {
                fish.JsonData = JsonSerializer.Serialize(detail);
            }

            await _context.Collectibles.AddAsync(fish);
            added++;
        }
    }

    await _context.SaveChangesAsync();
    return Content($"✅ {added} peces agregados con detalles completos.");
}

        // =============================
        // 🔹 POPULATE FULL: carga todos los coleccionables detallados
        // =============================
        [AllowAnonymous]
        public async Task<IActionResult> PopulateFull()
        {
            Console.WriteLine("🚀 Iniciando PopulateFull...");

            var categories = new (string Name, Func<Task<List<Collectible>>> FetchList, Func<string, Task<string?>> FetchDetail)[]
            {
        ("fish", _nookipediaService.GetFishAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/fish/{Uri.EscapeDataString(name)}")),
        //("bugs", _nookipediaService.GetBugsAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/bugs/{Uri.EscapeDataString(name)}")),
        //("sea", _nookipediaService.GetSeaCreaturesAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/sea/{Uri.EscapeDataString(name)}")),
        //("art", _nookipediaService.GetArtAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/art/{Uri.EscapeDataString(name)}")),
        //("villagers", _nookipediaService.GetVillagersAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/villagers/{Uri.EscapeDataString(name)}")),
        //("items", _nookipediaService.GetItemsAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/items/{Uri.EscapeDataString(name)}")),
        //("furniture", _nookipediaService.GetFurnitureAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/furniture/{Uri.EscapeDataString(name)}")),
        //("clothing", _nookipediaService.GetClothingAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/clothing/{Uri.EscapeDataString(name)}")),
        //("tools", _nookipediaService.GetToolsAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/tools/{Uri.EscapeDataString(name)}")),
        //("gyroids", _nookipediaService.GetGyroidsAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/gyroids/{Uri.EscapeDataString(name)}")),
        //("recipes", _nookipediaService.GetRecipesAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/recipes/{Uri.EscapeDataString(name)}")),
        //("fossils_individuals", _nookipediaService.GetFossilsIndividualsAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/fossils/individuals/{Uri.EscapeDataString(name)}")),
        //("interior", _nookipediaService.GetInteriorAsync, name => _nookipediaService.GetRawJsonAsync($"https://api.nookipedia.com/nh/interior/{Uri.EscapeDataString(name)}"))
            };

            int total = 0;

            foreach (var cat in categories)
            {
                try
                {
                    Console.WriteLine($"🔹 Procesando categoría: {cat.Name}");
                    var list = await cat.FetchList();

                    if (list == null || list.Count == 0)
                    {
                        Console.WriteLine($"⚠️ No se obtuvieron ítems para {cat.Name}");
                        continue;
                    }

                    foreach (var item in list)
                    {
                        try
                        {
                            // Evita duplicados
                            if (_context.Collectibles.Any(c => c.Name == item.Name && c.Category == cat.Name))
                                continue;

                            // 🔹 Obtener detalle JSON del ítem
                            var json = await cat.FetchDetail(item.Name);

                            if (!string.IsNullOrWhiteSpace(json))
                                item.JsonData = json;

                            _context.Collectibles.Add(item);
                            total++;
                        }
                        catch (Exception exItem)
                        {
                            Console.WriteLine($"❌ Error al procesar {item.Name} ({cat.Name}): {exItem.Message}");
                        }
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Guardados {list.Count} ítems de {cat.Name}");
                }
                catch (Exception exCat)
                {
                    Console.WriteLine($"❌ Error en categoría {cat.Name}: {exCat.Message}");
                }
            }

            Console.WriteLine($"🎉 Proceso terminado. Total importados: {total}");
            return Content($"✅ {total} ítems importados correctamente a la base de datos.");
        }




    }
}
