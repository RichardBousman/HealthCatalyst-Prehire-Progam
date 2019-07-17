using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

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

        /// <summary>
        /// Is the application running on Azure (as defined in the appsettings.json file)
        /// </summary>
        public static bool RunningOnAzure
        {
            get
            {
                string trueOrFalse = (string) Startup.Configuration.GetValue<string> ("RunningOnAzure");

                return trueOrFalse.ToLower () == "true";
            }
        }

        /// <summary>
        /// Get the value from the key vault for the secret specified.
        /// For this to run the application needs to be run by the owner of the key vault (using Windows Login) or
        /// if called from an Azure App, the application must have Identity turned on (in Azure) and the key vault
        /// access policy must have the application on the list of entities that can access the vault.
        /// </summary>
        /// <param name="whichsecret">Which secret from the vault do you want</param>
        /// <returns>Value for the secret</returns>
        public static string GetSecretFromKeyVault ( string whichsecret )
        {
            SecretBundle secret = null;

            try
            {
                string keyVault = (string) Startup.Configuration.GetValue<string> ( "KeyVault" );

                AzureServiceTokenProvider tokenProvider = new AzureServiceTokenProvider ();
                KeyVaultClient keyVaultClient = new KeyVaultClient ( new KeyVaultClient.AuthenticationCallback (tokenProvider.KeyVaultTokenCallback));
                string url = $"{keyVault}/secrets/{whichsecret}";
                Task.Run ( async () => secret = await keyVaultClient.GetSecretAsync ( url ) ).Wait ();

                return secret.Value;
            }
            catch ( Exception exception )
            {
                MessageHandler.Error ( $"Unable to get the value ('{whichsecret}') from the key vault", exception: exception );
                return string.Empty;
            }
        }
    }
}
