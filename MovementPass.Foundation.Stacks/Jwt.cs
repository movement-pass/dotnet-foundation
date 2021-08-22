﻿namespace MovementPass.Foundation.Stacks
{
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
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(random);

            var secret = Convert.ToBase64String(random);

            this.PutParameterStoreValue("jwt/secret", secret);
        }
    }
}