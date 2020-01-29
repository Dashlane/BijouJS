using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUWPChakraNative.Utils;

namespace TestUWPChakraNative.Projected
{
    [TestClass]
    public class Test_Pbkdf2
    {
        class Pbkdf2Params
        {
            public int Iterations { get; set; } = 0;
            public string HashMethod { get; set; } = null;
            public const uint HashLength = 32;
        }

        private static readonly Dictionary<string, string> HashMethodDictionary = new Dictionary<string, string>{
            {
                "sha1", "SHA-1"
            },
            {
                "sha256", "SHA-256"
            }
        };

        private void ProcessTest(FlexibleMarkerTest test)
        {
            var par = ParsePayload(test.payload);
            if (par != null) {
                Debug.WriteLine($"Executing test with {par.Iterations} iterations, {par.HashMethod} hash method");
                var hash = Frameworks.JsExecutor.UWP.Chakra.Native.Projected.Pbkdf2.hashPbkdf2(
                    Convert.FromBase64String(test.key),
                    Convert.FromBase64String(test.salt),
                    par.Iterations,
                    HashMethodDictionary[par.HashMethod],
                    32);
                Assert.AreEqual(Pbkdf2Params.HashLength, (uint)hash.Length, $"Bad hash length for test {test.description}");
                CollectionAssert.AreEqual(Convert.FromBase64String(test.derivedKey), hash, $"Wrong hash for test {test.description}");
            }
        }

        private const int PAYLOAD_INDEX_ITERATIONS = 4;
        private const int PAYLOAD_INDEX_HASH_METHOD = 5;
        private Pbkdf2Params ParsePayload(string payload)
        {
            Pbkdf2Params result = null;

            var splitPayload = payload.Split('$');

            result = new Pbkdf2Params {
                Iterations = int.Parse(splitPayload[PAYLOAD_INDEX_ITERATIONS]),
                HashMethod = splitPayload[PAYLOAD_INDEX_HASH_METHOD]
            };

            return result;
        }

        [TestMethod]
        public void RunCommonCryptoTests()
        {
            CryptoTestUtils.ReadTestsFromFile("ms-appx:///Resources/FlexibleMarkerTests.json").Result
                .Where(x => x.CryptoAlgo == enCryptoAlgo.Pbkdf2)
                .ToList()
                .ForEach(ProcessTest);
        }
    }
}
