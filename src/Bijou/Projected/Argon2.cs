using System.Runtime.InteropServices.WindowsRuntime;
using Bijou.NativeCrypto;

namespace Bijou.Projected
{
    // Class projected to JS context, implementing Argon2d cryptographic hash
    // Methods name are lowerCase as projection force them to lowerCase
    public sealed class Argon2
    {
        public static byte[] hashArgon2d(int tCost, int mCost, int parallelism, [ReadOnlyArray]byte[] pwd, [ReadOnlyArray]byte[] salt, int hashLength)
        {
            return UWPArgon2Helper.argon2d_hash((uint)tCost, (uint)mCost, (uint)parallelism, (uint)hashLength, pwd, salt);
        }
    }
}
