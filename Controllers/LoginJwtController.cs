using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BikeVille.Models;
using BikeVille.Models.Mongodb;
using MongoDB.Driver;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BikeVille.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginJwtController : ControllerBase
    {
        private JwtSettings jwtSettings;
        private readonly MongoPasswordService _passwordService;

        public LoginJwtController(JwtSettings jwtSettings, MongoPasswordService passwordService)
        {
            this.jwtSettings = jwtSettings;
            _passwordService = passwordService;
        }

        // POST api/<LoginJwtController>
        [HttpPost]
        public IActionResult Post([FromBody] Credentials credentials)
        {
            // Recupera i dettagli dell'utente dal database (utilizzando il servizio MongoPasswordService)
            var user = _passwordService.GetUserByEmail(credentials.Email);
            //Controllo se l'email esiste
            if (user == null)
            {
                return Unauthorized("Email o password non corretti");
            }

            // Verifica la password con hash e salt
            bool isValidPassword = VerifyPassword(credentials.Password, user.PasswordHash, user.PasswordSalt);

            if (!isValidPassword)
            {
                return Unauthorized("Email o password non corretti");
            }

            // Genera il token JWT
            var token = GenerateJwtToken(credentials.Email);

            // Restituisce il token come risposta
            return Ok(new { token });
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
        private string GenerateJwtToken(string email)
        {
            var secretKey = jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            //Converto la chiave segreta in un array di byte
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim (ClaimTypes.Email, email)
                }),
                Expires = DateTime.Now.AddMinutes(jwtSettings.TokenExpirationMinutes),
                Issuer=jwtSettings.Issuer,
                Audience=jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        [HttpPost("admin/{email}")]
        public bool AdminCheck(string email)
        {
            return _passwordService.isAdmin(email);
        }
    }
}
