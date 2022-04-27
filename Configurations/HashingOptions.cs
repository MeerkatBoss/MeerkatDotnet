using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MeerkatDotnet.Configurations;

public sealed class HashingOptions
{
    public string Salt { get; set; } = null!;
    public int IterationCount { get; set; }

    public byte[] SaltBytes
    {
        get => Encoding.UTF8.GetBytes(Salt);
    }
}