using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace TestBijou.NativeCrypto.Utils
{
    enum enCryptoAlgo
    {
        Argon2,
        Pbkdf2,
        Unknown
    }

    // FlexibleMarkerTest data model
    // used to parse json
    internal class FlexibleMarkerTest
    {
        public string description = null;
        public string payload = null;
        public string key = null;
        public string salt = null;
        public string iv = null;
        public string plaintextContent = null;
        public string encryptedContent = null;
        public string derivedKey = null;


        private static Dictionary<string, enCryptoAlgo> cryptoAlgos = new Dictionary<string, enCryptoAlgo> {
            {"argon2d", enCryptoAlgo.Argon2 },
            {"pbkdf2", enCryptoAlgo.Pbkdf2 }
        };

        public enCryptoAlgo CryptoAlgo
        {
            get
            {
                var splitPayload = string.IsNullOrEmpty(payload) ? null : payload.Split('$');
                if (splitPayload == null || splitPayload.Length < 3) {
                    return enCryptoAlgo.Unknown;
                }

                if (!cryptoAlgos.ContainsKey(splitPayload[2])) {
                    return enCryptoAlgo.Unknown;
                }

                return cryptoAlgos[splitPayload[2]];
            }
        }
    }

    internal class CryptoTestUtils
    {
        internal static async Task<List<FlexibleMarkerTest>> ReadTestsFromFile(string uri)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
            var fileText = await FileIO.ReadTextAsync(file);
            var parsedTests = JsonConvert.DeserializeObject<Dictionary<string, List<FlexibleMarkerTest>>>(fileText);
            return parsedTests["tests"];
        }
    }
}
