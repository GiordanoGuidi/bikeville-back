using BikeVille.Exceptions;
using BikeVille.Models;  

namespace BikeVille.Services
{
    public class ErrorHandlingService
    {
        private readonly AdventureWorksLt2019Context _context;

        // Aggiunge AdventureWorksLt2019Context come dipendenza
        public ErrorHandlingService(AdventureWorksLt2019Context context)
        {
            _context = context;  // Inizializzazione del contesto
        }

        // Metodo per loggare gli errori
        public async Task LogErrorAsync(Exception exception)
        {
            try
            {
                // Ottiengo il fuso orario italiano
                var italianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                var italianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, italianTimeZone);

                // Determina la gravità dell'errore in base al tipo di eccezione
                var severity = exception switch
                {
                    GenericException => SeverityLevel.Medium, // Default per le eccezioni personalizzate
                    NullReferenceException => SeverityLevel.Critical,
                    InvalidOperationException => SeverityLevel.High,
                    ArgumentException => SeverityLevel.Medium,
                    _ => SeverityLevel.Low
                };

                // Se l'eccezione è una GenericException, usa il suo ErrorCode
                int? errorCode = null;
                string? errorLine = null;  // Variabile per il numero della riga

                if (exception is GenericException genEx)
                {
                    errorCode = genEx.ErrorCode;

                    // Inserisco una gravità personalizzata
                    severity = genEx.ErrorCode switch
                    {
                        100 => SeverityLevel.Low,
                        200 => SeverityLevel.Medium,
                        300 => SeverityLevel.High,
                        400 => SeverityLevel.Critical,
                        401 => SeverityLevel.Critical,
                        404 => SeverityLevel.Critical,
                        409 => SeverityLevel.Critical,  
                       
                        _ => severity // Mantieni il valore di default
                    };
                }

                if (exception.StackTrace != null)
                {
                    var stackTraceLines = exception.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var line in stackTraceLines)
                    {
                        if (line.Contains("at") && line.Contains(":line"))
                        {
                            // Analizza la riga che contiene "at" e ":line"
                            var parts = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 1)
                            {
                                var lineInfo = parts[parts.Length - 1].Trim(); // Es. "line 46"
                                var lineNumberPart = lineInfo.Replace("line", "").Trim(); // Rimuove "line"

                                if (int.TryParse(lineNumberPart, out var lineNumber))
                                {
                                    errorLine = lineNumber.ToString(); // Converte l'int in stringa
                                    break;  // Esci appena trovato il numero di riga
                                }
                            }
                        }
                    }
                }



                var errorLog = new ErrorLog
                {
                    ErrorTime = italianTime,  // Salvo l'ora italiana
                    UserName = Environment.UserName,
                    ErrorNumber = exception.HResult,
                    ErrorMessage = exception.Message,
                    ErrorProcedure = exception.TargetSite?.Name,
                    ErrorLine = !string.IsNullOrEmpty(errorLine) ? (int?)int.Parse(errorLine) : null,  // Salvo la riga di errore, se disponibile
                    ErrorSeverity = (int)severity,  // Salvo il valore numerico dell'enum
                    ErrorState = errorCode  // Usa il codice d'errore personalizzato se presente
                };

                await _context.ErrorLogs.AddAsync(errorLog);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Errore registrato nella tabella ErrorLog con gravità {(int)severity} e ID: {errorLog.ErrorLogId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossibile registrare l'errore: {ex.Message}");
            }
        }
    }
}
