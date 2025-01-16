namespace BikeVille.Exceptions
{
    public class GenericException : Exception
    {
        public int ErrorCode { get; }

        // Costruttore
        public GenericException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
            HResult = MapErrorCodeToHResult(errorCode);

            Console.WriteLine($"[DEBUG] HResult impostato a: {HResult:X} per ErrorCode: {errorCode}");
        }
        
        // Metodo privato per mappare ErrorCode a HResult
        private static int MapErrorCodeToHResult(int errorCode)
        {
            // Definisci una mappatura personalizzata tra ErrorCode e HResult
            return errorCode switch
            {
                100 => unchecked((int)0x80131610), // HResult personalizzato per errore 100
                200 => unchecked((int)0x80131620), // HResult personalizzato per errore 200
                300 => unchecked((int)0x80131630), // HResult personalizzato per errore 300
                400 => unchecked((int)0x80131640), // HResult personalizzato per errore 400
                401 => unchecked((int)0x80131641), // HResult personalizzato per errore 401
                404 => unchecked((int)0x80131644), // HResult personalizzato per errore 404
                409 => unchecked((int)0x80131649), // HResult personalizzato per errore 409
                500 => unchecked((int)0x80131650), // HResult personalizzato per errore 500
                _ => unchecked((int)0x80131600)    // HResult di default per altri errori
            };
        }
    }
}
