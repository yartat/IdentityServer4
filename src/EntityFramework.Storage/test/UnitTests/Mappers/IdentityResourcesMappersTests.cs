// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework.Mappers;
using Mapster;
using IdentityServer4.Models;
using Xunit;

namespace IdentityServer4.EntityFramework.UnitTests.Mappers
{
    public class IdentityResourcesMappersTests
    {
        [Fact]
        public void IdentityResourceMapsterConfigurationIsValid()
        {
            var config = new TypeAdapterConfig { RequireDestinationMemberSource = true };
            new IdentityResourceMapperRegister().Register(config);
            config.Compile();
        }

        [Fact]
        public void CanMapIdentityResources()
        {
            var model = new IdentityResource();
            var mappedEntity = model.ToEntity();
            var mappedModel = mappedEntity.ToModel();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
        }
    }
}