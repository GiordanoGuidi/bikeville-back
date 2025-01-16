using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BikeVille.Models;
using BikeVille.Models.Mongodb;
using MongoDB.Driver;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using BikeVille.Services;
using BikeVille.Exceptions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BikeVille.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginJwtController : ControllerBase
    {
        private JwtSettings jwtSettings;
        private readonly MongoPasswordService _passwordService;
        private readonly ErrorHandlingService _errorHandlingService;
        private readonly AdventureWorksLt2019Context _context;


        public LoginJwtController(JwtSettings jwtSettings, MongoPasswordService passwordService, ErrorHandlingService errorHandlingService, AdventureWorksLt2019Context context)
        {
            this.jwtSettings = jwtSettings;
            _passwordService = passwordService;
            _errorHandlingService = errorHandlingService;
            _context = context;

        }


        // POST api/<LoginJwtController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Credentials credentials)
        {
            try
            {
                // Validazione dei parametri in input
                if (credentials == null || string.IsNullOrWhiteSpace(credentials.Email) || string.IsNullOrWhiteSpace(credentials.Password))
                {
                    throw new GenericException("Input non valido: Email o Password mancante.", 400);

                }

                // Recupera i dettagli dell'utente dal database
                var user = _passwordService.GetUserByEmail(credentials.Email);

                // Controllo se l'email esiste
                if (user == null)
                {
                    // Crea e logga un'eccezione personalizzata
                    throw new GenericException($"L'email '{credentials.Email}' non è presente nel database.", 404);

                }

                // Verifica la password con hash e salt
                bool isValidPassword = VerifyPassword(credentials.Password, user.PasswordHash, user.PasswordSalt);

                if (!isValidPassword)
                {
                    throw new GenericException("Email o password non corretti.", 401);

                }

                // Genera il token JWT
                var result = await GenerateJwtToken(user);

                if (result is OkObjectResult okResult && okResult.Value is string token)
                {
                    return Ok(new { token });
                }
                return BadRequest("Errore nella generazione del token.");
            }
            catch (GenericException genEx)
            {
               
                // Log specifico per GenericException
                await _errorHandlingService.LogErrorAsync(genEx);
                return StatusCode(500, new { error = genEx.Message, code = genEx.ErrorCode, number = genEx.HResult });

            }
            catch (Exception ex)
            {
                // Logga eventuali altri errori
                await _errorHandlingService.LogErrorAsync(ex);
                return StatusCode(500, new { error = "Si è verificato un errore interno." });
            }
        }




        /// <summary>
        /// method that checks that the entered password is equal to the passwordhash
        /// </summary>
        /// <param name="enteredPassword"></param>
        /// <param name="storedHash"></param>
        /// <param name="storedSalt"></param>
        /// <returns></returns>
        private bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            //Creo un'istanza di HMACSHA512 e passo saltBytes come chiave segreta per eseguire la crittografia
            using (var hmac = new System.Security.Cryptography.HMACSHA256(saltBytes))
            {
                //trasformo la password dell'utente in array di byte ed eseguo l'hashing su di essa
                byte[] computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(enteredPassword));
                //Confronto i due array di byte
                return computedHash.SequenceEqual(Convert.FromBase64String(storedHash));
            }
        }


        //Metodo per creare un JWT
        private async Task<ActionResult> GenerateJwtToken(UserCredentials user)
        {
            var secretKey = jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            //Converto la chiave segreta in un array di byte
            var key = Encoding.ASCII.GetBytes(secretKey);
            var customer = await _context.Customers.FindAsync(user.CustomerID);

            // Verifico che l'utente esista
            if (customer == null)
            {
                return BadRequest("Utente non trovato.");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim (ClaimTypes.Email, user.EmailAddress),
                    new Claim("FirstName", customer.FirstName),   
                    new Claim("LastName", customer.LastName),
                    new Claim("Id",customer.CustomerId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);
            Console.WriteLine($"Generated Token: {tokenString}");
            // Restituisco  il token
            return Ok( tokenString );
        }

        [HttpPost("admin/{email}")]
        public bool AdminCheck(string email)
        {
            return _passwordService.isAdmin(email);
        }
    }
}
