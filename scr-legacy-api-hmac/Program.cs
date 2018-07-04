using System;
using System.Security.Cryptography;

namespace scr_legacy_api_hmac {
    class Program {
        
        public static void Main(string[] args) {
            Console.WriteLine(RunExampleCredentials());
        }

        public static string RunExampleCredentials() {
            var authString = CreateAuthorizationString("yourAPIKey",
                                                       "yourSecretKey",
                                                       "GET",
                                                       "application/json",
                                                       "api/1.0/Projects/12345/Departments",
                                                       "2018-06-11T17:05:31-0700");
            return authString;
        }

        public static string CreateAuthorizationString(string apiKey, string secret, string httpVerb, string contentType, string requestURI, string timestampString) {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] secretBytes = encoding.GetBytes(secret);
            string canonicalString = string.Format("{0}\n{1}\n{2}\n{3}", httpVerb, contentType, requestURI, timestampString);

            using (var sha = new HMACSHA256(secretBytes)) {
                canonicalString = Convert.ToBase64String(sha.ComputeHash(encoding.GetBytes(canonicalString)));
            }

            return string.Format("AttAPI {0}:{1}", apiKey, canonicalString);
        }
    }
}
