using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BikeVille.Models;
using BikeVille.Models.DTO;
using Microsoft.Data.SqlClient;
using BikeVille.Models.Services;
using BikeVille.Models.DTO.filters;
using BikeVille.Exceptions;
using BikeVille.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BikeVille.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly FilterService _filterService;
        private readonly ErrorHandlingService _errorHandlingService;

        public ProductsController(AdventureWorksLt2019Context context, FilterService filterService, ErrorHandlingService errorHandlingService)
        {
            _context = context;
            _filterService = filterService;
            _errorHandlingService = errorHandlingService;
        }

        // GET: api/Products1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                // Trova il prodotto
                var product = await _context.Products.FindAsync(id);

                // Se il prodotto non esiste, lancia un'eccezione
                if (product == null)
                {
                    throw new GenericException($"Prodotto con ID {id} non trovato.", 404);
                }

                // Restituisci il prodotto
                return product;
            }
            catch (GenericException ex)
            {
                // Registra l'errore nel database
                await _errorHandlingService.LogErrorAsync(ex);
                // Restituisci un errore 404
                return NotFound(new
                {
                    Message = ex.Message,
                    ProductId = id
                });
            }
            catch (Exception ex)
            {
                // Gestisci errori generici
                await _errorHandlingService.LogErrorAsync(ex);

                // Restituisci un errore 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore durante il recupero del prodotto.");
            }

        }

        //Funzione per recuperare i prodotti in base alla parent-categories
        [HttpGet("by-parent-category")]
        public async Task<ActionResult<Product>> GetProductByParentCategory(int parentCategoryId)
        {
            var products = await _context.Products
                                                .Join(
                                                    _context.ProductCategories,
                                                    // Chiave primaria della tabella Products
                                                    product => product.ProductCategoryId,
                                                    // Chiave esterna della tabella ProductCategories
                                                    category => category.ProductCategoryId,
                                                    // Risultato della join
                                                    (product, category) => new { Product = product, Category = category }
                                                )
                                                // Filtro sul ParentProductCategoryId
                                                .Where(joined => joined.Category.ParentProductCategoryId == parentCategoryId)
                                                // Seleziona solo i prodotti
                                                .Select(joined => joined.Product)
                                                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("getDescByModelId/{modelId}")]
        public async Task<ActionResult<ProductDescriptionDTO>> GetDescription(int modelId)
        {
            var description = await _context.ProductDescriptions
                .FromSqlInterpolated($"SELECT Description FROM [SalesLT].[ProductDescription] INNER JOIN [SalesLT].[ProductModelProductDescription] ON [ProductDescription].[ProductDescriptionID] = [ProductModelProductDescription].[ProductDescriptionID] INNER JOIN [SalesLT].[ProductModel] ON [ProductModelProductDescription].[ProductModelID] = [ProductModel].[ProductModelID] WHERE [ProductModel].[ProductModelID] = {modelId} AND [ProductModelProductDescription].[Culture] = 'en'")
                .Select(d => new ProductDescriptionDTO
                {
                    Description = d.Description
                })
                .ToListAsync();

            return Ok(description);
        }

        //Funzione per filtrare le biciclette in base ai filtri selezionati
        [HttpGet("get-filtered-bikes")]
        public async Task<ActionResult<List<Product>>> GetProductsByFilter([FromQuery] string? color, [FromQuery] int parentCategoryId, [FromQuery] int? typeId, [FromQuery] string? size, [FromQuery] int? price)
        {
            var query = _context.Products.Join(
                _context.ProductCategories,
                product => product.ProductCategoryId,
                category => category.ProductCategoryId,
                (product, category) => new { Product = product, Category = category }
            )
            .Where(joined => joined.Category.ParentProductCategoryId == parentCategoryId);

            // Filtro per colore se presente
            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(joined => joined.Product.Color == color);
            }

            // Filtro per tipologia se presente
            if (typeId != null)
            {
                query = query.Where(joined => joined.Product.ProductCategoryId == typeId);
            }
            // Filtro per taglia se presente
            if (size != null)
            {
                query = query.Where(joined => joined.Product.Size == size);
            }
            // Filtro per prezzo se presente
            if (price != null)
            {
                switch (parentCategoryId)
                {
                    //Biciclette
                    case 1:
                        switch (price)
                        {
                            case 1:
                                price = 700;
                                query = query.Where(joined => joined.Product.ListPrice <= price);
                                break;
                            case 2:
                                price = 700;
                                query = query.Where(joined => joined.Product.ListPrice >= price && joined.Product.ListPrice <= 1500);
                                break;
                            case 3:
                                price = 1500;
                                query = query.Where(joined => joined.Product.ListPrice >= price && joined.Product.ListPrice <= 2500);
                                break;
                            case 4:
                                query = query.Where(joined => joined.Product.ListPrice >= 2500);
                                break;

                            default:
                                Console.WriteLine("Scelta non valida.");
                                break;
                        }
                        break;
                    //Componenti 
                    case 2:
                        switch (price)
                        {
                            case 1:
                                price = 100;
                                query = query.Where(joined => joined.Product.ListPrice <= price);
                                break;
                            case 2:
                                price = 100;
                                query = query.Where(joined => joined.Product.ListPrice >= price && joined.Product.ListPrice <= 500);
                                break;
                            case 3:
                                price = 500;
                                query = query.Where(joined => joined.Product.ListPrice >= price && joined.Product.ListPrice <= 1000);
                                break;
                            case 4:
                                query = query.Where(joined => joined.Product.ListPrice >= 1000);
                                break;

                            default:
                                Console.WriteLine("Scelta non valida.");
                                break;
                        }
                        break;
                    //Vestiti
                    case 3:
                        switch (price)
                        {
                            case 1:
                                price = 10;
                                query = query.Where(joined => joined.Product.ListPrice <= price);
                                break;
                            case 2:
                                price = 10;
                                query = query.Where(joined => joined.Product.ListPrice >= price && joined.Product.ListPrice <= 30);
                                break;
                            case 3:
                                price = 30;
                                query = query.Where(joined => joined.Product.ListPrice >= price && joined.Product.ListPrice <= 50);
                                break;
                            case 4:
                                query = query.Where(joined => joined.Product.ListPrice >= 50);
                                break;

                            default:
                                Console.WriteLine("Scelta non valida.");
                                break;
                        }
                        break;
                    //Accessori
                    case 4:
                        switch (price)
                        {
                            case 1:
                                price = 10;
                                query = query.Where(joined => joined.Product.ListPrice <= price);
                                break;
                            case 2:
                                price = 10;
                                query = query.Where(joined => joined.Product.ListPrice >= price && joined.Product.ListPrice <= 30);
                                break;
                            case 3:
                                price = 30;
                                query = query.Where(joined => joined.Product.ListPrice >= price && joined.Product.ListPrice <= 50);
                                break;
                            case 4:
                                query = query.Where(joined => joined.Product.ListPrice >= 50);
                                break;

                            default:
                                Console.WriteLine("Scelta non valida.");
                                break;
                        }
                        break;

                }
            }

            var filteredProducts = await query
                .Select(joined => joined.Product)
                .ToListAsync();
            return Ok(filteredProducts);
        }


        // Funzione per recuperare le Parent-categories
        [HttpGet("parent-categories")]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetTopCategories()
        {
            var topCategories = await _context.ProductCategories
                .Take(4)
                .ToListAsync();

            return Ok(topCategories);
        }

        //Funzione per recuperare i filtri specifici della categoria (Bike)
        [HttpGet("product-filters")]
        public async Task<ActionResult<Filters>> GetFilters(int parentCategoryId)
        {
            // Recupero i filtri
            var filters = await _filterService.GetFiltersAsync(parentCategoryId);

            return Ok(filters);
        }

        [HttpGet("categoryId/{categoryId}")]
        public async Task<string> GetCategory(int categoryId)
        {
            var category = await _context.ProductCategories.FindAsync(categoryId);
            if (category == null)
            {
                return "";
            }

            return category.Name;
        }

        [HttpGet("modelId/{modelId}")]
        public async Task<string> GetModel(int modelId)
        {
            var model = await _context.ProductModels.FindAsync(modelId);
            if (model == null)
            {
                return "";
            }

            return model.Name;
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<int>> GetCategoryId(string category)
        {
            var productCategory = await _context.ProductCategories.FirstOrDefaultAsync(pc => pc.Name == category);
            if (productCategory == null)
            {
                return NotFound($"Categoria con nome '{category}' non trovata.");
            }

            return Ok(productCategory.ProductCategoryId);
        }

        [HttpGet("model/{model}")]
        public async Task<ActionResult<int>> GetModelId(string model)
        {
            var productModel = await _context.ProductModels.FirstOrDefaultAsync(pc => pc.Name == model);
            if (productModel == null)
            {
                return NotFound($"Categoria con nome '{model}' non trovata.");
            }

            return Ok(productModel.ProductModelId);
        }

        // POST: api/Products1
        [HttpPost]
        [Authorize]//Controllo che il jwt sia valido
        public async Task<ActionResult<Product>> PostProduct(CreateProductDTO createProductDTO)
        {
            if (User==null) {
                return Unauthorized(new { Message = "Session is expired." });
            }
            // Verifica se il token è valido(altrimenti User sarebbe null) e controllo che il ruolo sia admin
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim != "Admin")
            {
                return Unauthorized(new { Message = "You are not authorized to perform this action." });
            }

            //verify if the data entered are valid
            if (!ModelState.IsValid)
                return BadRequest();

            //verify if the product already exists in the sql database
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == createProductDTO.ProductId);

            if (existingProduct != null)
            {
                //if the product exists already
                return Conflict(new { Message = "Product number already exists in the system." });
            }
            else
            {
                Console.WriteLine("Product not found in the database");
            }

            // Ottieni l'ora corrente in Italia (fuso orario CET/CEST)
            TimeZoneInfo italyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
            DateTime currentTimeInItaly = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, italyTimeZone);

            /*if the product doesn't exist, a new Product object is created by mapping the fields from CreateProductDTO*/
            // Map CreateProductDTO to Product entity
            var product = new Product
            {
                
                Name = createProductDTO.Name,
                ProductNumber = createProductDTO.ProductNumber,
                Color = createProductDTO.Color,
                StandardCost = createProductDTO.StandardCost.HasValue ? (decimal)createProductDTO.StandardCost : 0M,
                ListPrice = createProductDTO.ListPrice.HasValue ? (decimal)createProductDTO.ListPrice : 0M,
                Size = createProductDTO.Size,
                Weight = createProductDTO.Weight.HasValue ? (decimal)createProductDTO.Weight : 0M,
                ProductCategoryId = createProductDTO.ProductCategoryId,
                ProductModelId = createProductDTO.ProductModelId,
                SellStartDate = createProductDTO.SellStartDate,
                SellEndDate = createProductDTO.SellEndDate,
                DiscontinuedDate = createProductDTO.DiscontinuedDate,
                ThumbnailPhotoFileName = createProductDTO.ThumbnailPhotoFileName,
                ThumbNailPhoto = createProductDTO.ThumbnailPhoto != null
            ? Convert.FromBase64String(createProductDTO.ThumbnailPhoto)
            : null,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = currentTimeInItaly,
               
            };

            

            //save the new product in sql database
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            //if it's successful return a 200(ok)
            return Ok(new { Message = "Product created successfully" });
        }


        // PUT: api/Products1/{id}
          [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, UpdateProductDTO updateProductDTO)
        {
            // Verifica se i dati inviati sono validi
            if (!ModelState.IsValid)
                return BadRequest();

            // Cerca il prodotto esistente nel database
            var productCategoryId = await _context.ProductCategories
                .Where(pc => pc.Name == updateProductDTO.ProductCategory)
                .Select(pc => pc.ProductCategoryId)
                .FirstOrDefaultAsync();

            // Cerca il modello esistente nel database
            var productModelId = await _context.ProductModels
                .Where(pm => pm.Name == updateProductDTO.ProductModel)
                .Select(pm => pm.ProductModelId)
                .FirstOrDefaultAsync();


            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null)
            {
                // Se il prodotto non esiste, restituisci un errore 404 (Not Found)
                return NotFound(new { Message = "Product not found in the system." });
            }

            // Ottieni l'ora corrente in Italia (fuso orario CET/CEST)
            TimeZoneInfo italyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
            DateTime currentTimeInItaly = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, italyTimeZone);

            // Aggiorna le proprietà del prodotto esistente basandoti sui dati ricevuti
            existingProduct.Name = updateProductDTO.Name ?? existingProduct.Name;
            existingProduct.ProductNumber = updateProductDTO.ProductNumber ?? existingProduct.ProductNumber;
            existingProduct.Color = updateProductDTO.Color ?? existingProduct.Color;
            existingProduct.StandardCost = updateProductDTO.StandardCost.HasValue ? (decimal)updateProductDTO.StandardCost : existingProduct.StandardCost;
            existingProduct.ListPrice = updateProductDTO.ListPrice.HasValue ? (decimal)updateProductDTO.ListPrice : existingProduct.ListPrice;
            existingProduct.Size = updateProductDTO.Size ?? existingProduct.Size;
            existingProduct.Weight = updateProductDTO.Weight.HasValue ? (decimal)updateProductDTO.Weight : existingProduct.Weight;
            existingProduct.ProductCategoryId =productCategoryId == 0 ? existingProduct.ProductCategoryId : productCategoryId;
            existingProduct.ProductModelId = productModelId == 0 ? existingProduct.ProductModelId : productModelId;
            existingProduct.SellStartDate = updateProductDTO.SellStartDate ?? existingProduct.SellStartDate;
            existingProduct.SellEndDate = updateProductDTO.SellEndDate ?? existingProduct.SellEndDate;
            existingProduct.DiscontinuedDate = updateProductDTO.DiscontinuedDate ?? existingProduct.DiscontinuedDate;
            existingProduct.ThumbnailPhotoFileName = updateProductDTO.ThumbnailPhotoFileName ?? existingProduct.ThumbnailPhotoFileName;
            existingProduct.ThumbNailPhoto = updateProductDTO.ThumbnailPhoto != null
                ? Convert.FromBase64String(updateProductDTO.ThumbnailPhoto)
                : existingProduct.ThumbNailPhoto;
            existingProduct.ModifiedDate = currentTimeInItaly;

            // Salva le modifiche nel database
            _context.Entry(existingProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!_context.Products.Any(p => p.ProductId == id))
                {
                    return NotFound(ex.Message);
                }
                else
                {
                    throw;
                }
            }

            // Restituisci una risposta 204 (No Content) per indicare che l'aggiornamento è avvenuto con successo
            return NoContent();
        }

        
        // DELETE: api/Products1/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            //Find the product
            var product = await _context.Products.FindAsync(id);
            //if the product doesn't exist throw an exception
            if (product == null)
            {
                throw new GenericException($"Prodotto con ID {id} non trovato.", 404);
            }

            //remove the product
            _context.Products.Remove(product);
            //save the changes in the database
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Product removed successfully" });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}