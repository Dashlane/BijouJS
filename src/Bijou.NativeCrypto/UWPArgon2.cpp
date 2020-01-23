#include "pch.h"

#include "UWPArgon2.h"

#include <cstdlib>
#include <windows.h>
#include "argon2/argon2.h"

using namespace Bijou::NativeCrypto;
using namespace Platform;
using namespace Platform::Runtime::InteropServices;
using namespace std;

Array<byte>^ UWPArgon2Helper::argon2d_hash(
    unsigned int tCost,
    unsigned int mCost,
    unsigned int parallelism,
    unsigned int hashLength,
    const Array<byte>^ password,
    const Array<byte>^ salt)
{
    Array<byte>^ result = ref new Array<byte>(hashLength);

    char* hash = new char[hashLength];

    if (argon2d_hash_raw(tCost, mCost, parallelism, password->Data,
        password->Length, salt->Data,
        salt->Length, hash, hashLength) == ARGON2_OK)
    {
        // copy the hash to the result
        for (unsigned int i = 0; i < hashLength; i++)
        {
            result[i] = hash[i];
        }
    }

    // clean
    delete[] hash;

    return result;
}