using System.Security.Cryptography;
using System;
using System.Text;


namespace BikeVille.Models.PasswordUtils
{
    public static  class PasswordHelper
    {
        // Funzione per generare un salt casuale
        private static  (byte[] saltBytes, string saltBase64) GenerateSalt()
        {
            //l'oggetto rng verrà eliminato dopo l'utilizzo
            using (var rng = RandomNumberGenerator.Create())
            {
                //Array di 6 byte
                var saltBytes = new byte[6];
                //Aggiungo numeri casuali all'array
                rng.GetBytes(saltBytes);
                //Trasformo l'array di byte in stringa Base64
                var saltBase64 = Convert.ToBase64String(saltBytes);
                return (saltBytes,saltBase64);
            }
        }

        //Funzione per creare una passwordhash restituisce una tupla
        public static(string passwordHash,string saltBase64) HashPassword(string password)
        {
            // Genero il salt
            var (saltBytes, saltBase64) = GenerateSalt();

            //Creo un'istanza di HMACSHA512 e passo saltBytes come chiave segreta per eseguire la crittografia
            using (var hmac = new System.Security.Cryptography.HMACSHA256(saltBytes))
            {
                //trasformo la password dell'utente in array di byte ed eseguo l'hashing su di essa
                byte[] hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var hashString = Convert.ToBase64String(hashBytes);
                return (hashString,saltBase64);
            }
        }
    }
}
