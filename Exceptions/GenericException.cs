namespace BikeVille.Exceptions
{
    public class GenericException : Exception
    {
        public int ErrorCode { get; }

        public GenericException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
