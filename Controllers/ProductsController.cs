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

namespace BikeVille.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public ProductsController(AdventureWorksLt2019Context context)
        {
            _context = context;
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
        public async Task<ActionResult<Product>> PostProduct(CreateProductDTO createProductDTO)
        {
            //verify if the data entered are valid
            if (!ModelState.IsValid)
                return BadRequest();

            //verify if the product already exists in the sql database
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == createProductDTO.ProductId);

            if (existingProduct != null)
            {
                //if the product exists already
                return Conflict(new { Message = "Product address already exists in the system." });
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

            // Gestisci la decodifica della foto Thumbnail se la stringa è valida
            //if (!string.IsNullOrEmpty(createProductDTO.ThumbnailPhoto) && IsBase64String(createProductDTO.ThumbnailPhoto))
            //{
            //    product.ThumbNailPhoto = Convert.FromBase64String(createProductDTO.ThumbnailPhoto);
            //}
            //else
            //{
            //   // Se la stringa non è Base64 valida, imposta la foto come null
            //    product.ThumbNailPhoto = null;
            //}

            //save the new product in sql database
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            //if it's successful return a 200(ok)
            return Ok(new { Message = "Product created successfully" });
        }

        private bool IsBase64String(string thumbnailPhoto)
        {
            if (string.IsNullOrWhiteSpace(thumbnailPhoto))
                return false;

            Span<byte> buffer = new Span<byte>(new byte[thumbnailPhoto.Length * 3 / 4]);
            return Convert.TryFromBase64String(thumbnailPhoto, buffer, out _);
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
