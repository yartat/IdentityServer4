// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using IdentityServer4.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Services.KeyManagement
{
    /// <summary>
    /// Creates keys and converts them to and from a private JSON Web Key (RFC 7517/7518).
    /// </summary>
    internal static class KeyMaterial
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static AsymmetricSecurityKey Create(string algorithm, string id, int rsaKeySize)
        {
            if (IsECDsa(algorithm))
            {
                var ecdsa = ECDsa.Create(CryptoHelper.GetCurveFromCrvValue(GetCrv(algorithm)));
                return new ECDsaSecurityKey(ecdsa) { KeyId = id };
            }

            var rsa = RSA.Create(rsaKeySize);
            return new RsaSecurityKey(rsa) { KeyId = id };
        }

        public static string Serialize(AsymmetricSecurityKey key, string algorithm)
        {
            Jwk jwk;
            switch (key)
            {
                case RsaSecurityKey rsaKey:
                    var rsa = rsaKey.Rsa?.ExportParameters(true) ?? rsaKey.Parameters;
                    jwk = new Jwk
                    {
                        Kty = JsonWebAlgorithmsKeyTypes.RSA,
                        N = Encode(rsa.Modulus), E = Encode(rsa.Exponent), D = Encode(rsa.D),
                        P = Encode(rsa.P), Q = Encode(rsa.Q), DP = Encode(rsa.DP), DQ = Encode(rsa.DQ), QI = Encode(rsa.InverseQ)
                    };
                    break;
                case ECDsaSecurityKey ecKey:
                    var ec = ecKey.ECDsa.ExportParameters(true);
                    jwk = new Jwk
                    {
                        Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
                        Crv = GetCrv(algorithm), X = Encode(ec.Q.X), Y = Encode(ec.Q.Y), D = Encode(ec.D)
                    };
                    break;
                default:
                    throw new InvalidOperationException($"Key type {key.GetType().Name} is not supported by automatic key management.");
            }

            if (jwk.D == null)
            {
                throw new InvalidOperationException("The key has no private key material.");
            }

            jwk.Kid = key.KeyId;
            jwk.Alg = algorithm;
            return JsonSerializer.Serialize(jwk, SerializerOptions);
        }

        public static AsymmetricSecurityKey Deserialize(string json, string id)
        {
            var jwk = JsonSerializer.Deserialize<Jwk>(json) ?? throw new InvalidOperationException("Invalid key data.");

            if (jwk.Kty == JsonWebAlgorithmsKeyTypes.RSA)
            {
                var rsa = RSA.Create();
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = Decode(jwk.N), Exponent = Decode(jwk.E), D = Decode(jwk.D),
                    P = Decode(jwk.P), Q = Decode(jwk.Q), DP = Decode(jwk.DP), DQ = Decode(jwk.DQ), InverseQ = Decode(jwk.QI)
                });
                return new RsaSecurityKey(rsa) { KeyId = id };
            }

            if (jwk.Kty == JsonWebAlgorithmsKeyTypes.EllipticCurve)
            {
                var ecdsa = ECDsa.Create(new ECParameters
                {
                    Curve = CryptoHelper.GetCurveFromCrvValue(jwk.Crv),
                    Q = new ECPoint { X = Decode(jwk.X), Y = Decode(jwk.Y) },
                    D = Decode(jwk.D)
                });
                return new ECDsaSecurityKey(ecdsa) { KeyId = id };
            }

            throw new InvalidOperationException($"Key type {jwk.Kty} is not supported by automatic key management.");
        }

        private static bool IsECDsa(string algorithm) => algorithm.StartsWith("ES", StringComparison.Ordinal);

        private static string GetCrv(string algorithm) => algorithm switch
        {
            SecurityAlgorithms.EcdsaSha256 => JsonWebKeyECTypes.P256,
            SecurityAlgorithms.EcdsaSha384 => JsonWebKeyECTypes.P384,
            SecurityAlgorithms.EcdsaSha512 => JsonWebKeyECTypes.P521,
            _ => throw new InvalidOperationException($"{algorithm} is not an ECDsa algorithm.")
        };

        private static string Encode(byte[] value) => value == null ? null : Base64Url.Encode(value);

        private static byte[] Decode(string value) => value == null ? null : Base64Url.Decode(value);

        private sealed class Jwk
        {
            [JsonPropertyName("kty")] public string Kty { get; set; }
            [JsonPropertyName("kid")] public string Kid { get; set; }
            [JsonPropertyName("alg")] public string Alg { get; set; }
            [JsonPropertyName("n")] public string N { get; set; }
            [JsonPropertyName("e")] public string E { get; set; }
            [JsonPropertyName("d")] public string D { get; set; }
            [JsonPropertyName("p")] public string P { get; set; }
            [JsonPropertyName("q")] public string Q { get; set; }
            [JsonPropertyName("dp")] public string DP { get; set; }
            [JsonPropertyName("dq")] public string DQ { get; set; }
            [JsonPropertyName("qi")] public string QI { get; set; }
            [JsonPropertyName("crv")] public string Crv { get; set; }
            [JsonPropertyName("x")] public string X { get; set; }
            [JsonPropertyName("y")] public string Y { get; set; }
        }
    }
}
