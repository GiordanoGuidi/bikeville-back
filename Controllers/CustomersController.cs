using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeVille.Models;
using BikeVille.Models.DTO;
using System.Security.Cryptography;
using BikeVille.Models.PasswordUtils;
using MongoDB.Bson;
using MongoDB.Driver;
using BikeVille.Utilities;
using Microsoft.CodeAnalysis.Elfie.Model.Strings;
using BikeVille.Utilities;
using BikeVille.Exceptions;
using BikeVille.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace BikeVille.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ErrorHandlingService _errorHandlingService;

        //Costruttore con DI 
        public CustomersController(AdventureWorksLt2019Context context, IMongoDatabase mongoDatabase, ErrorHandlingService errorHandlingService)
        {
            _context = context;
            _mongoDatabase = mongoDatabase;
            _errorHandlingService = errorHandlingService;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        [HttpGet("/user/{email}")]
        public async Task<ActionResult<bool>> GetCustomerByMail(string email)
        {
            //controlla l'esistenza della mail nel database
            var collection = _mongoDatabase.GetCollection<UserCredentials>("BikeVille");
            var existingUser = await collection.Find(u => u.EmailAddress == email).FirstOrDefaultAsync();

            if (existingUser == null)
            {
                return false;
            }

            return true;
        }

        [HttpGet("/getMail/{id}")]
        public async Task<IActionResult> GetCustomerEmailById(int id)
        {
            var collection = _mongoDatabase.GetCollection<UserCredentials>("BikeVille");
            var user = await collection.Find(u => u.CustomerID == id).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"Nessun utente trovato con l'ID {id}.");
            }

            return Ok(user.EmailAddress);
        }
        // GET: api/Customers/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            try
            {
                // Trovo il cliente
                var customer = await _context.Customers.FindAsync(id);

                // Se il cliente non esiste, lancia un'eccezione personalizzata
                if (customer == null)
                {
                    throw new GenericException($"Cliente con ID {id} non trovato.", 404);
                }

                // Restituisce il cliente
                return Ok(customer);
            }
            catch (GenericException ex)
            {
                // Registra l'errore nel database
                await _errorHandlingService.LogErrorAsync(ex);

                // Restituisci un errore 404 con i dettagli
                return NotFound(new
                {
                    Message = ex.Message,
                    CustomerId = id
                });
            }
            catch (Exception ex)
            {
                // Gestione generica degli errori
                await _errorHandlingService.LogErrorAsync(ex);

                // Restituisci un errore 500 per problemi inaspettati
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore durante il recupero del cliente.");
            }
        }

        // PUT: api/Customers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, UpdateCustomerDTO updateCustomerDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                // Cerca il cliente esistente in SQL Server
                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);

                if (existingCustomer == null)
                {
                    return NotFound(new { Message = "Customer not found in the system." });
                }

                // Aggiorna i campi su SQL Server
                existingCustomer.Title = updateCustomerDTO.Title ?? existingCustomer.Title;
                existingCustomer.FirstName = updateCustomerDTO.FirstName != null
                    ? StringHelper.CapitalizeFirstLetter(updateCustomerDTO.FirstName)
                    : existingCustomer.FirstName;
                existingCustomer.LastName = updateCustomerDTO.LastName != null
                    ? StringHelper.CapitalizeFirstLetter(updateCustomerDTO.LastName)
                    : existingCustomer.LastName;
                existingCustomer.CompanyName = updateCustomerDTO.CompanyName != null
                    ? StringHelper.CapitalizeFirstLetter(updateCustomerDTO.CompanyName)
                    : existingCustomer.CompanyName;
                existingCustomer.Phone = updateCustomerDTO.Phone ?? existingCustomer.Phone;
                existingCustomer.ModifiedDate = DateTime.UtcNow;

                _context.Entry(existingCustomer).State = EntityState.Modified;

                // Aggiorna EmailAddress su MongoDB
                var collection = _mongoDatabase.GetCollection<UserCredentials>("BikeVille");
                var userCredentials = await collection.Find(u => u.CustomerID == id).FirstOrDefaultAsync();

                if (userCredentials != null)
                {
                    if (!string.IsNullOrEmpty(updateCustomerDTO.EmailAddress))
                    {
                        // Verifica se l'email esiste già in MongoDB per un altro cliente
                        var existingEmailInMongo = await collection.Find(u => u.EmailAddress == updateCustomerDTO.EmailAddress && u.CustomerID != id).FirstOrDefaultAsync();

                        if (existingEmailInMongo != null)
                        {
                            throw new GenericException("Email già in uso non può essere registrata.", 409);
                        }

                        userCredentials.EmailAddress = updateCustomerDTO.EmailAddress;

                        // Salva le modifiche in MongoDB
                        var filter = Builders<UserCredentials>.Filter.Eq(u => u.CustomerID, id);
                        await collection.ReplaceOneAsync(filter, userCredentials);
                    }
                }

                // Salva le modifiche in SQL Server
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Customer updated successfully" });
            }
            catch (GenericException ex)
            {
                await _errorHandlingService.LogErrorAsync(ex);
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during customer update.");
            }
        }


        // POST: api/Customers
        [HttpPost]
        // Il parametro createCustomerDto sarà popolato con i dati inviati nel body della richiesta
        public async Task<ActionResult> CreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                // Recupera le credenziali dell'amministratore dalle variabili di ambiente
                var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
                var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

                string role = "Customer";
                //Se l'email e la password dell'utente sono uguali a quelle dell'admin assegno il ruolo admin
                if (createCustomerDto.EmailAddress == adminEmail && createCustomerDto.Password == adminPassword)
                {
                    role = "Admin";
                }

                // Verifica se l'email esiste già in MongoDB
                var collection = _mongoDatabase.GetCollection<UserCredentials>("BikeVille");
                var existingUserInMongo = await collection.Find(u => u.EmailAddress == createCustomerDto.EmailAddress).FirstOrDefaultAsync();

                if (existingUserInMongo != null)
                {
                    throw new GenericException("Email già in uso non può essere registrata.", 409);
                }

                // Tupla con password hash e salt
                var result = PasswordHelper.HashPassword(createCustomerDto.Password);

                // Modifico il titolo in base al genere dell'utente
                string title = createCustomerDto.Gender switch
                {
                    "Male" => "Mr.",
                    "Female" => "Ms.",
                    "Other" => "Other",
                    _ => "Mx."
                };

                // Creo istanza Customer
                var customer = new Customer
                {
                    FirstName = StringHelper.CapitalizeFirstLetter(createCustomerDto.FirstName),
                    LastName = StringHelper.CapitalizeFirstLetter(createCustomerDto.LastName),
                    Phone = createCustomerDto.Phone,
                    Title = title,
                    CompanyName = StringHelper.CapitalizeFirstLetter(createCustomerDto.CompanyName),
                    ModifiedDate = DateTime.UtcNow,
                    Rowguid = Guid.NewGuid(),
                    EmailAddress = "",
                    PasswordHash = "",
                    PasswordSalt = "",
                };

                // Salvataggio in SQL Server
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Creazione dell'oggetto UserCredentials per MongoDB
                UserCredentials userCredentials = new UserCredentials
                {
                    Id = ObjectId.GenerateNewId(),
                    CustomerID = customer.CustomerId,
                    EmailAddress = createCustomerDto.EmailAddress,
                    PasswordHash = result.passwordHash,
                    PasswordSalt = result.saltBase64,
                    Role = role,
                };

                // Salvataggio in MongoDB
                await collection.InsertOneAsync(userCredentials);

                return Ok(new { Message = "Customer created successfully" });
            }
            catch (GenericException ex)
            {
                await _errorHandlingService.LogErrorAsync(ex);
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                await _errorHandlingService.LogErrorAsync(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during customer creation.");
            }
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        [HttpGet("ValidateAdminToken")]
        [Authorize]
        public IActionResult ValidateAdminToken()
        {
            // Verifica se il token è valido(altrimenti User sarebbe null) e controllo che il ruolo sia admin
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim != "Admin")
            {
                return Unauthorized(new { Message = "You are not authorized to perform this action." });
            }

            // Se il token è valido, restituisce OK
            return Ok(new { valid = true });
        }
    }
}
