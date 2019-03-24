// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace IdentityServer
{
    public class Config
    {
        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId()
            };
        }

        public IEnumerable<ApiResource> GetApis()
        {
            return new[]
            {
                new ApiResource("PhotoLakeAPI", "The API for upload photo to Photo Lake APP")
            };
        }

        public IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "TestClient",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        // This needs to comes from azure vault.

                        new Secret("testclientphotolake".Sha256())
                    },
                    AllowedScopes = { "PhotoLakeAPI" }
                }
            };
        }
    }
}