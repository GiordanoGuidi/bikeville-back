namespace BikeVille.Utilities
{
    public static class StringHelper
    {
        /// <summary>
        /// Function to make the first letter of the string uppercase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}
