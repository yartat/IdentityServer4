// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Clients;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

namespace ConsoleMTLSClient
{
    public class Program
    {
        public static async Task Main()
        {
            Console.Title = "Console mTLS Client";

            var response = await RequestTokenAsync();
            response.Show();

            Console.ReadLine();
            await CallServiceAsync(response.AccessToken);
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var client = new HttpClient(GetHandler());

            var disco = await client.GetDiscoveryDocumentAsync("https://identityserver.local");
            if (disco.IsError) throw new Exception(disco.Error);

            var endpoint = disco.MtlsEndpointAliases.TokenEndpoint;
            
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = endpoint,

                ClientId = "mtls",
                Scope = "resource1.scope1"
            });

            if (response.IsError) throw new Exception(response.Error);
            return response;
        }

        static async Task CallServiceAsync(string token)
        {
            var client = new HttpClient(GetHandler())
            {
                BaseAddress = new Uri(Constants.SampleApi)
            };

            client.SetBearerToken(token);
            var response = await client.GetStringAsync("identity");

            "\n\nService claims:".ConsoleGreen();
            Console.WriteLine(JsonNode.Parse(response));
        }

        static SocketsHttpHandler GetHandler()
        {
            var handler = new SocketsHttpHandler();
            
            var cert = X509CertificateLoader.LoadPkcs12FromFile("client.p12", "changeit");
            handler.SslOptions.ClientCertificates = new X509CertificateCollection { cert };

            return handler;
        }
    }
}