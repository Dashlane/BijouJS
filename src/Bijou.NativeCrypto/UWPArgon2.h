#pragma once

namespace Bijou {
namespace NativeCrypto {
public ref class UWPArgon2Helper sealed {
private:
    UWPArgon2Helper() {};

public:
    static Platform::Array<byte>^ argon2d_hash(
        unsigned int tCost,
        unsigned int mCost,
        unsigned int parallelism,
        unsigned int hashLength,
        const Platform::Array<byte>^ password,
        const Platform::Array<byte>^ salt);
};
} // namespace NativeCrypto
} // namespace Bijou
