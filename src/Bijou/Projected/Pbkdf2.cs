using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography.Core;

namespace Bijou.Projected
{
    /// <summary>
    /// Class projected to JS context, implementing Pbkdf2 cryptographic hash.
    /// Methods name are lowerCase as projection force them to lowerCase.
    /// Inspired by: https://gist.github.com/charlesportwoodii/a571e1a3541b708df18881f086e31002
    /// </summary>
    public sealed class Pbkdf2
    {
        private static readonly Dictionary<string, string> _keyDerivationAlgorithmNames = new Dictionary<string, string> {
            { "SHA-1", KeyDerivationAlgorithmNames.Pbkdf2Sha1 },
            { "SHA-256", KeyDerivationAlgorithmNames.Pbkdf2Sha256 }
        };

        /// <summary>
        /// Generates a PBDFK hash.
        /// </summary>
        /// <param name="pwd">password</param>
        /// <param name="salt"></param>
        /// <param name="iterations">iterationCountIn</param>
        /// <param name="hashMethod"></param>
        /// <param name="hashLength"></param>
        /// <returns></returns>
        public static byte[] hashPbkdf2([ReadOnlyArray]byte[] pwd, [ReadOnlyArray]byte[] salt, int iterations, string hashMethod, int hashLength)
        {
            var algorithm = KeyDerivationAlgorithmNames.Pbkdf2Sha256;
            if (_keyDerivationAlgorithmNames.ContainsKey(hashMethod)) {
                algorithm = _keyDerivationAlgorithmNames[hashMethod];
            }

            var provider = KeyDerivationAlgorithmProvider.OpenAlgorithm(algorithm);

            // Create the derivation parameters.
            var pbkdf2Params = KeyDerivationParameters.BuildForPbkdf2(salt.AsBuffer(), (uint)iterations);

            // Create a key from the secret value.
            var keyOriginal = provider.CreateKey(pwd.AsBuffer());

            // Derive a key based on the original key and the derivation parameters.
            var keyDerived = CryptographicEngine.DeriveKeyMaterial(
                keyOriginal,
                pbkdf2Params,
                (uint)hashLength
            );

            return keyDerived.ToArray();
        }
    }
}
