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
using BikeVille.Models.Services;
using BikeVille.Models.Bike;

namespace BikeVille.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly FilterService _filterService;

        public ProductsController(AdventureWorksLt2019Context context, FilterService filterService)
        {
            _context = context;
            _filterService = filterService;
        }

        // GET: api/Products1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products1/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET: api/Products/Categories
        [HttpGet("parent-categories")]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetTopCategories()
        {
            var topCategories = await _context.ProductCategories
                .Take(4) 
                .ToListAsync();

            return Ok(topCategories);
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

        //Funzione per recuperare i filtri specifici della categoria (Bike)
        [HttpGet("bike-filters")]
        public async Task<ActionResult<BikeFilters>> GetFilters()
        {
            // Recuper entrambi i filtri
            var bikeFilters = await _filterService.GetBikeFiltersAsync();

            return Ok(bikeFilters);
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
