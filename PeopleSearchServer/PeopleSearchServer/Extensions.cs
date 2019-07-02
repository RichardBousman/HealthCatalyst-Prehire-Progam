using Microsoft.Extensions.Configuration;

namespace PeopleSearchServer
{
    /// <summary>
    /// Extension functions that Expand what .NET provides
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Remove the leading pattern of characters from the start of a string
        /// </summary>
        /// <param name="source">String to remove pattern from</param>
        /// <param name="pattern">pattern to remove</param>
        /// <returns>Source string without the pattern at the beginning.</returns>
        public static string RemoveLeading ( this string source, string pattern )
        {
            if ( source.StartsWith ( pattern ) )
            {
                return source.Substring ( pattern.Length );
            }
            else
            {
                return source;
            }
        }

        public static bool RunningOnAzure
        {
            get
            {
                string trueOrFalse = (string) Startup.Configuration.GetValue<string> ("RunningOnAzure");

                return trueOrFalse.ToLower () == "true";
            }
        }
    }
}
