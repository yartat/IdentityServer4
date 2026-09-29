// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.EntityFramework.Mappers;
using Mapster;
using IdentityServer4.Models;
using Xunit;

namespace IdentityServer4.EntityFramework.UnitTests.Mappers
{
    public class PersistedGrantMappersTests
    {
        [Fact]
        public void PersistedGrantMapsterConfigurationIsValid()
        {
            var config = new TypeAdapterConfig { RequireDestinationMemberSource = true };
            config.NewConfig<IdentityServer4.EntityFramework.Entities.PersistedGrant, PersistedGrant>();
            config.NewConfig<PersistedGrant, IdentityServer4.EntityFramework.Entities.PersistedGrant>();
            config.Compile();
        }

        [Fact]
        public void CanMap()
        {
            var model = new PersistedGrant()
            {
                ConsumedTime = new System.DateTime(2020, 02, 03, 4, 5, 6)
            };
            
            var mappedEntity = model.ToEntity();
            mappedEntity.ConsumedTime.Value.Should().Be(new System.DateTime(2020, 02, 03, 4, 5, 6));
            
            var mappedModel = mappedEntity.ToModel();
            mappedModel.ConsumedTime.Value.Should().Be(new System.DateTime(2020, 02, 03, 4, 5, 6));

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
        }
    }
}