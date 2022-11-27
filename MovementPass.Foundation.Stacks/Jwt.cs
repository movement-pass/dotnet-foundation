namespace MovementPass.Foundation.Stacks;

using Constructs;
using System;
using System.Security.Cryptography;

using Amazon.CDK;

public class Jwt : BaseStack
{
    public Jwt(Construct scope, string id, IStackProps props = null) :
        base(scope, id, props)
    {
        this.PutParameterStoreValue("jwt/audience", this.Domain);
        this.PutParameterStoreValue("jwt/issuer", this.Domain);
        this.PutParameterStoreValue("jwt/expiration",
            this.GetContextValue<string>("jwtExpire"));

        var random = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);

        var secret = BitConverter.ToString(random)
            .Replace(
                "-",
                string.Empty,
                StringComparison.Ordinal)
            .ToLowerInvariant();

        this.PutParameterStoreValue("jwt/secret", secret);
    }
}