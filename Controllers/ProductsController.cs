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



        // PUT: api/Products1/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, CreateProductDTO product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products1
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products1/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}