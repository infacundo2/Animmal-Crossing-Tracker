using System.Net.Http;
using System.Text.Json;
using AnimalCrossingTracker.Models;
using AnimalCrossingTracker.Models.Nookipedia;

namespace AnimalCrossingTracker.Services
{
    public class NookipediaService
    {
        private readonly HttpClient _httpClient;

        public NookipediaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(120);

            var apiKey = configuration["Nookipedia:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            
            _httpClient.DefaultRequestHeaders.Add("Accept-Version", "1.0.0");
        }



        // 🔹 Método genérico para obtener y deserializar JSON
        
        // Método genérico existente
        private async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al llamar a {endpoint}: {ex.Message}");
                return default;
            }
        }

        public async Task<string?> GetRawJsonAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al llamar a {endpoint}: {ex.Message}");
                return null;
            }
        }


        // 🔹 Método genérico para listas (poblar base de datos)
        private async Task<List<Collectible>> GetCollectiblesFromEndpoint(string endpoint, string category)
        {
            var result = new List<Collectible>();

            try
            {
                Console.WriteLine($"🌍 Consultando endpoint: {endpoint}");

                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // 🔹 Log del tamaño de la respuesta (por si la API corta)
                Console.WriteLine($"📦 Respuesta recibida ({category}): {json.Length} caracteres");

                var list = JsonSerializer.Deserialize<List<JsonElement>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<JsonElement>();

                Console.WriteLine($"✅ {list.Count} elementos encontrados en {category}");

                foreach (var item in list)
                {
                    string name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    string desc = item.TryGetProperty("location", out var d) ? d.GetString() ?? "" : "";
                    string img = item.TryGetProperty("image_url", out var i) ? i.GetString() ?? "" : "";

                    // 🔹 Guardar el JSON completo de este item (para no perder disponibilidad ni horarios)
                    string rawJson = JsonSerializer.Serialize(item);

                    result.Add(new Collectible
                    {
                        Name = name,
                        Category = category,
                        Description = desc,
                        ImageUrl = img,
                        JsonData = rawJson
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"🌐 Error HTTP al obtener {category}: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"🧩 InnerException: {ex.InnerException.Message}");
            }
            catch (JsonException jex)
            {
                Console.WriteLine($"⚠️ Error al deserializar JSON de {category}: {jex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error general en {category}: {ex.Message}");
            }

            Console.WriteLine($"🧮 Total {result.Count} coleccionables procesados en {category}");
            return result;
        }


        // 🔹 Endpoints generales (listas)
        public Task<List<Collectible>> GetFishAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/fish?lan=es", "fish");
        public Task<List<Collectible>> GetBugsAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/bugs", "bugs");
        public Task<List<Collectible>> GetSeaCreaturesAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/sea", "sea");
        public Task<List<Collectible>> GetEventsAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/events", "events");
        public Task<List<Collectible>> GetArtAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/art", "art");
        public Task<List<Collectible>> GetFurnitureAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/furniture", "furniture");
        public Task<List<Collectible>> GetClothingAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/clothing", "clothing");
        public Task<List<Collectible>> GetInteriorAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/interior", "interior");
        public Task<List<Collectible>> GetToolsAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/tools", "tools");
        public Task<List<Collectible>> GetPhotosAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/photos", "photos");
        public Task<List<Collectible>> GetItemsAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/items", "items");
        public Task<List<Collectible>> GetRecipesAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/recipes", "recipes");
        public Task<List<Collectible>> GetFossilsIndividualsAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/fossils/individuals", "fossils_individuals");
        public Task<List<Collectible>> GetFossilsGroupsAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/fossils/groups", "fossils_groups");
        public Task<List<Collectible>> GetGyroidsAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/nh/gyroids", "gyroids");
        public Task<List<Collectible>> GetVillagersAsync() => GetCollectiblesFromEndpoint("https://api.nookipedia.com/villagers", "villagers");

        // 🔹 Métodos de detalle (individuales)
        public Task<Fish?> GetFishDetailAsync(string name) => GetAsync<Fish>($"https://api.nookipedia.com/nh/fish/{Uri.EscapeDataString(name)}");
        public Task<Bug?> GetBugDetailAsync(string name) => GetAsync<Bug>($"https://api.nookipedia.com/nh/bugs/{Uri.EscapeDataString(name)}");
        public Task<SeaCreature?> GetSeaCreatureDetailAsync(string name) => GetAsync<SeaCreature>($"https://api.nookipedia.com/nh/sea/{Uri.EscapeDataString(name)}");
        public Task<Art?> GetArtDetailAsync(string name) => GetAsync<Art>($"https://api.nookipedia.com/nh/art/{Uri.EscapeDataString(name)}");
        public Task<Villager?> GetVillagerDetailAsync(string name) => GetAsync<Villager>($"https://api.nookipedia.com/villagers/{Uri.EscapeDataString(name)}");
        public Task<Fossil?> GetFossilDetailAsync(string name) => GetAsync<Fossil>($"https://api.nookipedia.com/nh/fossils/individuals/{Uri.EscapeDataString(name)}");
        public Task<Item?> GetItemDetailAsync(string name) => GetAsync<Item>($"https://api.nookipedia.com/nh/items/{Uri.EscapeDataString(name)}");
        public Task<Furniture?> GetFurnitureDetailAsync(string name) => GetAsync<Furniture>($"https://api.nookipedia.com/nh/furniture/{Uri.EscapeDataString(name)}");
        public Task<Interior?> GetInteriorDetailsAsync(string name) => GetAsync<Interior>($"https://api.nookipedia.com/nh/interior/{Uri.EscapeDataString(name)}");
        public Task<Clothing?> GetClothingDetailAsync(string name) => GetAsync<Clothing>($"https://api.nookipedia.com/nh/clothing/{Uri.EscapeDataString(name)}");
        public Task<Tool?> GetToolDetailAsync(string name) => GetAsync<Tool>($"https://api.nookipedia.com/nh/tools/{Uri.EscapeDataString(name)}");
        public Task<Gyroid?> GetGyroidDetailAsync(string name) => GetAsync<Gyroid>($"https://api.nookipedia.com/nh/gyroids/{Uri.EscapeDataString(name)}");
        public Task<Recipe?> GetRecipeDetailAsync(string name) => GetAsync<Recipe>($"https://api.nookipedia.com/nh/recipes/{Uri.EscapeDataString(name)}");

        // 🔹 Combina todos los endpoints de Nookipedia y devuelve una lista de coleccionables base
public async Task<List<Collectible>> GetAllCollectiblesAsync()
{
    var all = new List<Collectible>();

    try
    {
        var fish = await GetFishAsync();
        var bugs = await GetBugsAsync();
        var sea = await GetSeaCreaturesAsync();
        var art = await GetArtAsync();
        var furniture = await GetFurnitureAsync();
        var clothing = await GetClothingAsync();
        var interior = await GetInteriorAsync();
        var tools = await GetToolsAsync();
        var photos = await GetPhotosAsync();
        var items = await GetItemsAsync();
        var recipes = await GetRecipesAsync();
        var fossilsInd = await GetFossilsIndividualsAsync();
        var fossilsGrp = await GetFossilsGroupsAsync();
        var gyroids = await GetGyroidsAsync();
        var villagers = await GetVillagersAsync();

        // Agrupa todos los resultados en una sola lista
        all.AddRange(fish);
        all.AddRange(bugs);
        all.AddRange(sea);
        all.AddRange(art);
        all.AddRange(furniture);
        all.AddRange(clothing);
        all.AddRange(interior);
        all.AddRange(tools);
        all.AddRange(photos);
        all.AddRange(items);
        all.AddRange(recipes);
        all.AddRange(fossilsInd);
        all.AddRange(fossilsGrp);
        all.AddRange(gyroids);
        all.AddRange(villagers);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error general en GetAllCollectiblesAsync: {ex.Message}");
    }

    return all;
}

    }
}
