using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestBijou.Projected.Utils;

namespace TestBijou.Projected
{
    [TestClass]
    public class TestArgon2
    {
        class Argon2Params
        {
            public int tCost;
            public int mCost;
            public int parallelism;
            public readonly int hashlength = 32;
        }

        private void ProcessTest(FlexibleMarkerTest test)
        {
            var par = ParsePayload(test.payload);
            if (par == null) return;

            var hash = Bijou.Projected.Argon2.hashArgon2d(par.tCost, 
                par.mCost, 
                par.parallelism, 
                Convert.FromBase64String(test.key), 
                Convert.FromBase64String(test.salt),
                par.hashlength);
            Assert.AreEqual(par.hashlength, hash.Length, "Bad hash length for test " + test.description);
            CollectionAssert.AreEqual(Convert.FromBase64String(test.derivedKey), hash, "Wrong hash for test " + test.description);
        }

        private const int PayloadIndexTCost = 4;
        private const int PayloadIndexMCost = 5;
        private const int PayloadIndexParallelism = 6;
        private Argon2Params ParsePayload(string payload)
        {
            var splitPayload = payload.Split('$');

            var result = new Argon2Params
            {
                tCost = int.Parse(splitPayload[PayloadIndexTCost]),
                mCost = int.Parse(splitPayload[PayloadIndexMCost]),
                parallelism = int.Parse(splitPayload[PayloadIndexParallelism])
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
