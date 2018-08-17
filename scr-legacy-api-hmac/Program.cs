using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flurl.Http;


namespace scr_legacy_api_hmac {
    class Program {
        
        public static void Main(string[] args) {

            var yourAPIKey = "apiKey";
            var yourSecretKey ="secretKey";
            var apiLocation = "https://apilocation/";
            var projectLocation = "api/1.0/Projects/12345/Departments";
            var timeout = new TimeSpan(1,0,0);
            //We should have provided you these variables

            Console.WriteLine(RunExampleCredentials());
            FlurlClient client = new FlurlClient();
            Console.WriteLine(CallAPI(client.WithTimeout(timeout), yourAPIKey, yourSecretKey, apiLocation, projectLocation).GetAwaiter().GetResult());
        
        }

        public static async Task<string> CallAPI(FlurlClient client, string apiKey, string secret, string host, string requestURI) {
            try {
                var contentType = "application/json";
                var fullUrl = $"{host}{requestURI}";
                var timestampString = GetNowInISOFormat();

                var authorizationHeaderValue = CreateAuthorizationString(apiKey, secret, "GET", contentType, requestURI, timestampString);
                

                var response = await fullUrl
                    .WithHeader("att-api-timestamp", timestampString)
                    .WithHeader("authorization", authorizationHeaderValue)
                    .WithHeader("Content-Type", contentType)
                    .WithHeader("Accept", contentType)
                    .GetAsync();
                

                Console.WriteLine(authorizationHeaderValue);

                response.EnsureSuccessStatusCode();
                string responseBodyAsText = await response.Content.ReadAsStringAsync();
                return responseBodyAsText;
            }
            catch (Exception ex) {
                Console.WriteLine($"'{ex.ToString()}'");
            }

            return null;
        }

        //Test authorization string creation
        public static string RunExampleCredentials() {
            var authString = CreateAuthorizationString("apiKey",
                                                       "secretKey",
                                                       "GET",
                                                       "application/json",
                                                       "api/1.0/Projects/12345/Departments",
                                                       "2018-07-12T14:57:16+02:00");
            return authString;
        }

        public static string GetNowInISOFormat() {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }

        public static string CreateAuthorizationString(string apiKey, string secret, string httpVerb, string contentType, string requestURI, string timestampString) {
            Console.WriteLine($"Creating Authorization String with apiKey:'{apiKey}', secret:'{secret}', httpVerb:'{httpVerb}' contentType:'{contentType}' requestURI:'{requestURI}', timestampString:'{timestampString}'");
            var encoding = new System.Text.ASCIIEncoding();
            byte[] secretBytes = encoding.GetBytes(secret);
            string canonicalString = string.Format("{0}\n{1}\n{2}\n{3}", httpVerb, contentType, requestURI, timestampString);
            Console.WriteLine("Canonicalstring pre convert: \n"+ canonicalString);
            using (var sha = new HMACSHA256(secretBytes)) {
                canonicalString = Convert.ToBase64String(sha.ComputeHash(encoding.GetBytes(canonicalString)));
                Console.WriteLine("CannonicalString: " + canonicalString);
            }

            return string.Format("AttAPI {0}:{1}", apiKey, canonicalString);
        }
    }
}
