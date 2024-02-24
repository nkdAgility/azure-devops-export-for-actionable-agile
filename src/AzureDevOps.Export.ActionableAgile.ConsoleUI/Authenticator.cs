using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI
{
    class Authenticator
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The Tenant is the name or Id of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        internal static string aadInstance = "https://login.microsoftonline.com/{0}/v2.0";
        internal static string tenant = "686c55d4-ab81-4a17-9eef-6472a5633fab";
        internal static string clientId = "3c0fb0ea-116c-4972-82ce-c8f310865aed";
        internal static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        internal static string[] scopes = new string[] { "499b84ac-1321-427f-aa17-267ca6975798/user_impersonation" }; //Constant value to target Azure DevOps. Do not change
                                                                                                                      
        // MSAL Public client app
        private static IPublicClientApplication application;


        public async Task<string> AuthenticationCommand(string? token)
        {
            if (token != null)
            {
              string auth =  Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", token)));
                return $"Basic {auth}";
            }

            try
            {
                var authResult = await SignInUserAndGetTokenUsingMSAL(scopes);

                // Create authorization header of the form "Bearer {AccessToken}"
                return authResult.CreateAuthorizationHeader();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Something went wrong.");
                Console.WriteLine("Message: " + ex.Message + "\n");
                return string.Empty;
            }
        }

        /// <summary>
        /// Sign-in user using MSAL and obtain an access token for Azure DevOps
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns>AuthenticationResult</returns>
        private static async Task<AuthenticationResult> SignInUserAndGetTokenUsingMSAL(string[] scopes)
        {
            // Initialize the MSAL library by building a public client application
            application = PublicClientApplicationBuilder.Create(clientId)
                                       .WithAuthority(authority)
                                       .WithDefaultRedirectUri()
                                       .Build();
            AuthenticationResult result;

            try
            {
                var accounts = await application.GetAccountsAsync();
                result = await application.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // If the token has expired, prompt the user with a login prompt
                result = await application.AcquireTokenInteractive(scopes)
                        .WithClaims(ex.Claims)
                        .ExecuteAsync();
            }
            return result;
        }

    }
}
