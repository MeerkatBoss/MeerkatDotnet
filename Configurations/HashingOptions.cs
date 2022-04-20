using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MeerkatDotnet.Configurations;

public sealed class HashingOptions
{
    public byte[] Salt { get; set; } = null!;
    public int IterationCount { get; set; }
}