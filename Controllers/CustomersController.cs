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


namespace BikeVille.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly IMongoDatabase _mongoDatabase;

        //Costruttore con DI 
        public CustomersController(AdventureWorksLt2019Context context, IMongoDatabase mongoDatabase)
        {
            _context = context;
            _mongoDatabase = mongoDatabase;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }


        // PUT: api/Customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
        [HttpPost]
        // Il parametro createCustomerDto sarà popolato con i dati inviati nel body della richiesta
        public async Task<ActionResult> CreateCustomer ([FromBody] CreateCustomerDto createCustomerDto)
        {
            if(!ModelState.IsValid)
               return BadRequest();

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
            Console.WriteLine("Verifica email: " + createCustomerDto.EmailAddress);
            var existingUser = await collection.Find(u => u.EmailAddress == createCustomerDto.EmailAddress).FirstOrDefaultAsync();

            if (existingUser != null)
            {
                Console.WriteLine("Email già esistente: " + existingUser.EmailAddress);
                Console.WriteLine("Utente:" + existingUser.ToString);
                return Conflict(new { Message = "Email address already exists in the system." });
            }
            else
            {
                Console.WriteLine("Nessuna email trovata nel database.");
            }

            //Tupla con passwordhash e passwordsalt
            var result = PasswordHelper.HashPassword(createCustomerDto.Password);
            //Modifico il titolo in base al genere dell'utente
            string title = createCustomerDto.Gender switch
            {
                "Male" => "Mr.",
                "Female"=>"Ms.",
                "Other"=>"Other"
            };


            //Creo istanza Customer
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
            // Genera il CustomerID automaticamente
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
    }
}
