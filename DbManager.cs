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

    /// <summary>
    /// Logs an error in the database using the uspLogError stored procedure.
    /// </summary>
    /// <returns>The ID of the logged error.</returns>
    public async Task<int> LogErrorAsync(ErrorLog errorLog)
    {
        int errorLogId = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand command = new SqlCommand("uspLogError", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters for the stored procedure
                command.Parameters.AddWithValue("@ErrorTime", errorLog.ErrorTime);
                command.Parameters.AddWithValue("@UserName", errorLog.UserName);
                command.Parameters.AddWithValue("@ErrorNumber", errorLog.ErrorNumber);
                command.Parameters.AddWithValue("@ErrorSeverity", (object)errorLog.ErrorSeverity ?? DBNull.Value);
                command.Parameters.AddWithValue("@ErrorState", (object)errorLog.ErrorState ?? DBNull.Value);
                command.Parameters.AddWithValue("@ErrorProcedure", (object)errorLog.ErrorProcedure ?? DBNull.Value);
                command.Parameters.AddWithValue("@ErrorLine", (object)errorLog.ErrorLine ?? DBNull.Value);
                command.Parameters.AddWithValue("@ErrorMessage", errorLog.ErrorMessage);

                // Define the output parameter for the ErrorLogId
                SqlParameter outputParam = new SqlParameter
                {
                    ParameterName = "@ErrorLogID",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                await connection.OpenAsync();

                // Execute the stored procedure
                await command.ExecuteNonQueryAsync();

                // Retrieve the value of the output parameter
                if (outputParam.Value != DBNull.Value)
                {
                    errorLogId = Convert.ToInt32(outputParam.Value);
                }
            }
        }

        return errorLogId;
    }
}
