using Org.BouncyCastle.Asn1.Ocsp;
using RestSharp;
using Sodium;
using System.Buffers.Binary;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LoginWWW
{
    internal static class Program
    {
        static RequestRestsharp _request = new RequestRestsharp();
        static void Main(string[] args)
        {
            var password = EncryptPasswordAsync("hiendz").Result;
            Console.WriteLine(password);
            Console.ReadLine();
        }
        public static string Encrypt(string password, string pubkey, int keyId)
        {
            byte[] randomKey = new byte[32];
            byte[] randomIV = new byte[12];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomKey);
                rng.GetBytes(randomIV);
            }
            RSA rsa = ImportPublicKeyFromPEM(pubkey);

            byte[] encryptedRandKey = rsa.Encrypt(randomKey, RSAEncryptionPadding.Pkcs1);

            using (AesGcm aesGcm = new AesGcm(randomKey))
            {
                byte[] cipherText = new byte[password.Length];
                byte[] tag = new byte[16]; 
                aesGcm.Encrypt(randomIV, Encoding.UTF8.GetBytes(password), cipherText, tag);

                long unixTimeSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();

                byte[] encodeData = Encoding.UTF8.GetBytes($"{(byte)1}{keyId}");
                encodeData = CombineByteArrays(encodeData, randomIV);
                encodeData = CombineByteArrays(encodeData, BitConverter.GetBytes((short)encryptedRandKey.Length));
                encodeData = CombineByteArrays(encodeData, encryptedRandKey);
                encodeData = CombineByteArrays(encodeData, tag);
                encodeData = CombineByteArrays(encodeData, cipherText);

                string encode = Convert.ToBase64String(encodeData);

                return $"#PWD_WILDE:2:{unixTimeSeconds}:{encode}";
            }
        }
        private static byte[] CombineByteArrays(byte[] arr1, byte[] arr2)
        {
            byte[] combined = new byte[arr1.Length + arr2.Length];
            Buffer.BlockCopy(arr1, 0, combined, 0, arr1.Length);
            Buffer.BlockCopy(arr2, 0, combined, arr1.Length, arr2.Length);
            return combined;
        }
        public static RSA ImportPublicKeyFromPEM(string pemPublicKey)
        {
            pemPublicKey = pemPublicKey.Replace("-----BEGIN PUBLIC KEY-----", "");
            pemPublicKey = pemPublicKey.Replace("-----END PUBLIC KEY-----", "");
            pemPublicKey = pemPublicKey.Replace("\n", "").Replace("\r", "");

            byte[] derPublicKey = Convert.FromBase64String(pemPublicKey);

            RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(derPublicKey, out _);

            return rsa;
        }
        private static async Task<dynamic> PwdKeyFetchAsync()
        {           
            var options = new RestClientOptions("https://graph.facebook.com")
            {
                MaxTimeout = -1,
                UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/17H35 [FBAN/FBIOS;FBAV/390.1.0.38.101;FBBV/412474635;FBDV/iPhone10,2;FBMD/iPhone;FBSN/iOS;FBSV/13.7;FBSS/3;FBID/phone;FBLC/vi_VN;FBOP/5;FBRV/0]",
            };
            var client = new RestClient(options);
            var request = new RestRequest("/pwd_key_fetch?app_version=412474635&fb_api_caller_class=FBPasswordEncryptionKeyFetchRequest&fb_api_req_friendly_name=FBPasswordEncryptionKeyFetchRequest:networkRequest&flow=controller_initialization&format=json&locale=vi_VN&pretty=0&sdk=ios&sdk_version=3&version=2", Method.Get);
            request.AddHeader("x-fb-privacy-context", "0xf0000000b659eb4a");
            request.AddHeader("x-fb-connection-type", "wifi.CTRadioAccessTechnologyLTE");
            request.AddHeader("x-fb-sim-hni", "45202");
            request.AddHeader("authorization", "OAuth 6628568379|c1e620fa708a1d5696fb991c1bde5662");
            request.AddHeader("x-tigon-is-retry", "False");
            request.AddHeader("x-fb-friendly-name", "FBPasswordEncryptionKeyFetchRequest:networkRequest");
            request.AddHeader("x-fb-http-engine", "Liger");
            request.AddHeader("x-fb-client-ip", "True");
            request.AddHeader("x-fb-server-cluster", "True");
            request.AddHeader("Cookie", "fr=0BJkjk3SPPdlX82RE..BkLbon..AAA.0.0.BmCtn4.AWV7sNHh1cc; ps_l=0; ps_n=0; sb=L42rZYXiUQJLPCtrixzjRuSw");
            RestResponse response = await client.ExecuteAsync(request);

            if (string.IsNullOrEmpty(response.Content))
                return null;

            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
            return data;
        }
        public static async Task<string> EncryptPasswordAsync(string text)
        {
            try
            {
                dynamic pwdKeyFetch = await PwdKeyFetchAsync();
                if (pwdKeyFetch != null)
                {
                    string public_key = pwdKeyFetch.public_key;
                    int key_id = pwdKeyFetch.key_id;
                    string pwd = Encrypt(text, public_key, key_id);
                    return pwd;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encrypting password: {ex.Message}");
                return null;
            }
        }
    }
}