using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUWPChakraNative.Utils;

namespace TestUWPChakraNative.Cpp
{
    class Argon2Params
    {
        public uint tCost;
        public uint mCost;
        public uint parallelism;
        public readonly uint hashlength = 32;
    }

    [TestClass]
    public class TestArgon2
    {
        private void ProcessTest(FlexibleMarkerTest test)
        {
            var par = ParsePayload(test.payload);
            if (par == null) return;

            var hash = Bijou.NativeCrypto.UWPArgon2Helper.argon2d_hash(par.tCost, par.mCost, par.parallelism,
                par.hashlength, Convert.FromBase64String(test.key), Convert.FromBase64String(test.salt));
            Assert.AreEqual(par.hashlength, (uint)hash.Length, "Bad hash length for test " + test.description);
            CollectionAssert.AreEqual(Convert.FromBase64String(test.derivedKey), hash, "Wrong hash for test " + test.description);
        }

        private const int PayloadIndexTCost = 4;
        private const int PayloadIndexMCost = 5;
        private const int PayloadIndexParallelism = 6;
        private Argon2Params ParsePayload(string payload)
        {
            var splitPayload = payload.Split('$');

            var result = new Argon2Params {
                tCost = uint.Parse(splitPayload[PayloadIndexTCost]),
                mCost = uint.Parse(splitPayload[PayloadIndexMCost]),
                parallelism = uint.Parse(splitPayload[PayloadIndexParallelism])
            };

            return result;
        }

        [TestMethod]
        public void RunCommonCryptoTests()
        {
            CryptoTestUtils.ReadTestsFromFile("ms-appx:///Resources/FlexibleMarkerTests.json").Result
                .Where(x => x.CryptoAlgo == enCryptoAlgo.Argon2)
                .ToList()
                .ForEach(ProcessTest);
        }
    }
}
