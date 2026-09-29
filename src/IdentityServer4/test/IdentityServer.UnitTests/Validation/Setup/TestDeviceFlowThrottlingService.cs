// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServer.UnitTests.Validation.Setup
{
    public class TestDeviceFlowThrottlingService : IDeviceFlowThrottlingService
    {
        private readonly bool shouldSlownDown;

        public TestDeviceFlowThrottlingService(bool shouldSlownDown = false)
        {
            this.shouldSlownDown = shouldSlownDown;
        }

        public Task<bool> ShouldSlowDown(string deviceCode, DeviceCode details) => Task.FromResult(shouldSlownDown);
    }
}