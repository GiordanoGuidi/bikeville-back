using Microsoft.Data.SqlClient;
using System.Data;
using System.Configuration;
using BikeVille.Models;



public  class DbManager
{
    private readonly string _connectionString;

    // Costruttore che riceve la stringa di connessione
    public DbManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Method to get the user's passwordhash and passwordsalt
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public UserCredentials GetPasswordDetails(string email)
    {
        //Creo istanza di UserPasswordDetails
        var result = new UserCredentials();

        //Il blocco using chiude la connessione automaticamente
        using (var connection = new SqlConnection(_connectionString))
        {
            //Creo istanza sqlCommand  per eseguire la stored procedure
            using (var command = new SqlCommand("GetUserPasswordDetails", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                //Aggiungo il parametro email
                command.Parameters.AddWithValue("@Email", email);

                connection.Open();
                //Esegue la stored procedure e ritorna un SqlDataReader
                using (var reader = command.ExecuteReader())
                {
                    //Controllo se ci sono dati da leggere
                    if (reader.Read())
                    {
                        result.PasswordHash = reader["PasswordHash"]?.ToString();
                        result.PasswordSalt = reader["PasswordSalt"]?.ToString();
                    }
                }
            }
        }

        return result;
    }
}
