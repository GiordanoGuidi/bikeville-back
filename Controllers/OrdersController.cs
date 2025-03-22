using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BikeVille.Exceptions;
using BikeVille.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeVille.Models.Services;
using Microsoft.EntityFrameworkCore;
using BikeVille.Services;
using BikeVille.Models.DTO;





namespace BikeVille.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly ErrorHandlingService _errorHandlingService;


        public OrdersController(AdventureWorksLt2019Context context,ErrorHandlingService errorHandlingService)
        {
            _context = context;
            _errorHandlingService = errorHandlingService;
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserCart>>> GetUserCart(int id)
        {
            return await _context.UserCart.Where( cart => cart.CustomerId == id).ToListAsync();
        }
    
        [HttpPost]
        [Authorize]//Controllo che il jwt sia valido
        public async Task<ActionResult<UserCart>> PostUserCart(UserCartDTO userCartDto)
        {
            if (User==null) {
                return Unauthorized(new { Message = "Session is expired." });
            }
            
            //verify if the data entered are valid
            if(!ModelState.IsValid)return BadRequest();
            try{
                // Get the current time in italy (CET/CEST time zone)
                TimeZoneInfo italyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
                DateTime currentTimeInItaly = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,italyTimeZone);

                var userCart = new UserCart
                {
                    Name = userCartDto.Name,
                    CustomerId = userCartDto.CustomerId,
                    OrderQty = userCartDto.OrderQty,
                    ProductId = userCartDto.ProductId,
                    UnitPrice = userCartDto.UnitPrice,
                    AddedAt = currentTimeInItaly,
                };
                Console.WriteLine(userCart);
                _context.UserCart.Add(userCart);
                await _context.SaveChangesAsync();
                return Ok(new{Message = "Product created seccessfully"});
            }
            catch(GenericException ex)
            {
                await _errorHandlingService.LogErrorAsync(ex);
                return Conflict(new{Messge = ex.Message});

            }
            catch(Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during product creation");
            }
            
        }

        [HttpPut("increase/{productId}")]
        [Authorize] //Controllo che il jwt sia valido
        public async Task<IActionResult> IncreaseCartItem(int productId)
        {
            // Recupero l'id dell'utente dal tokenJwt
            var userIdString = User.FindFirst("Id")?.Value;
            // Convert il valore in int
            if(int.TryParse(userIdString, out int userId))
            {
                Console.WriteLine($"User ID: {userId}");
            }
            else
            {
                Console.WriteLine("Errore: ID non valido");
            }
            //Trovo il prodotto
            var existingItem = await _context.UserCart.FirstOrDefaultAsync(
                c=> c.ProductId == productId && c.CustomerId == userId);

            if(existingItem == null) return NotFound();
            existingItem.OrderQty += 1;
            await _context.SaveChangesAsync();
            return Ok(existingItem);
        }

        [HttpPut("decrease/{productId}")]
        [Authorize] //Controllo che il jwt sia valido
        public async Task<IActionResult> DecreaseCartItem(int productId)
        {
            // Recupero l'id dell'utente dal tokenJwt
            var userIdString = User.FindFirst("Id")?.Value;
            // Convert il valore in int
            if(int.TryParse(userIdString, out int userId))
            {
                Console.WriteLine($"User ID: {userId}");
            }
            else
            {
                Console.WriteLine("Errore: ID non valido");
            }
            //Trovo il prodotto
            var existingItem = await _context.UserCart.FirstOrDefaultAsync(c =>c.ProductId == productId && c.CustomerId == userId);
            if(existingItem == null) return NotFound();

            //Controllo se la quantità è uno rimuovo il prodotto
            if(existingItem.OrderQty == 1){
                _context.UserCart.Remove(existingItem);
                await _context.SaveChangesAsync();
                return Ok(new { removed = true, productId = productId });
            }
            existingItem.OrderQty -= 1;
            await _context.SaveChangesAsync();
            return Ok(existingItem);
        }

        [HttpDelete("delete/{productId}")]
        public async Task<IActionResult> DeleteCartItem(int productId)
        {
            //Trovo l'id dell'utente
            var userIdString = User.FindFirst("Id")?.Value;
            if(!int.TryParse(userIdString,out int userId)){
                return Unauthorized(new{Message = "ID utente non valido"});
            }
            //Trovo il prodotto
            var existingItem = await _context.UserCart.FirstOrDefaultAsync(
                c=> c.ProductId == productId && c.CustomerId == userId);

            if(existingItem == null) return NotFound();

            _context.UserCart.Remove(existingItem);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product removed successfully" });
        }


    }

}