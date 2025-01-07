using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Models;
using BikeVille.Exceptions;
using BikeVille.Services;

namespace BikeVille.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        private readonly ErrorHandlingService _errorHandlingService;

        public ProductsController(AdventureWorksLt2019Context context, ErrorHandlingService errorHandlingService)
        {
            _context = context;
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
                    throw new GenericException($"Prodotto con ID {id} non trovato.",404);
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

            // PUT: api/Products1/5
            // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
            [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> AdvancedSearch(
            string? name = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            try
            {
                // Lancia un'eccezione se il prezzo minimo è maggiore del prezzo massimo
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                {
                    throw new GenericException("Il prezzo minimo non può essere maggiore del prezzo massimo.", 400);
                }

                // Inizia con la query di base
                IQueryable<Product> query = _context.Products;

                // Filtra per nome, se specificato
                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(p => p.Name.Contains(name));
                }

                // Filtra per prezzo minimo, se specificato
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.ListPrice >= minPrice.Value);
                }

                // Filtra per prezzo massimo, se specificato
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.ListPrice <= maxPrice.Value);
                }

                // Esegui la query
                var products = await query.ToListAsync();

                // Verifica se ci sono risultati
                if (!products.Any())
                {
                    throw new GenericException("Nessun prodotto trovato con i criteri forniti.", 404);
                }

                // Restituisci i risultati
                return Ok(products);
            }
            catch (GenericException ex)
            {
                // Registra l'errore personalizzato
                await _errorHandlingService.LogErrorAsync(ex);

                // Restituisci un errore personalizzato
                return StatusCode(ex.ErrorCode, new
                {
                    Message = ex.Message,
                    ErrorCode = ex.ErrorCode
                });
            }
            catch (Exception ex)
            {
                // Gestisci errori generici
                await _errorHandlingService.LogErrorAsync(ex);

                // Restituisci un errore 500
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore durante la ricerca avanzata.");
            }
        }

    }
}