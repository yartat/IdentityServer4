// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Clients;
using Duende.IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

namespace ConsoleResourceOwnerFlowReference
{
    public class Program
    {
        static async Task Main()
        {
            Console.Title = "Console ResourceOwner Flow Reference";

            var response = await RequestTokenAsync();
            response.Show();

            Console.ReadLine();
            await CallServiceAsync(response.AccessToken);
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "roclient.reference",
                ClientSecret = "secret",

                UserName = "bob",
                Password = "bob",

                Scope = "resource1.scope1 resource2.scope1 scope3"
            });

            if (response.IsError) throw new Exception(response.Error);
            return response;
        }

        static async Task CallServiceAsync(string token)
        {
            var baseAddress = Constants.SampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);

            while (true)
            {
                var response = await client.GetStringAsync("identity");

                "\n\nService claims:".ConsoleGreen();
                Console.WriteLine(JsonNode.Parse(response));

                Console.ReadLine();
            }
        }
    }
}