// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using IdentityServer4.Configuration.DependencyInjection;
using IdentityServer4.Extensions;
using System;
using IdentityModel;
using System.Linq;
using IdentityServer4.Configuration;

namespace IdentityServer4.Hosting
{
    // this decorates the real authentication service to detect when the 
    // user is being signed in. this allows us to ensure the user has
    // the claims needed for identity server to do its job. it also allows
    // us to track signin/signout so we can issue/remove the session id
    // cookie used for check session iframe for session management spec.
    // finally, we track if signout is called to collaborate with the 
    // FederatedSignoutAuthenticationHandlerProvider for federated signout.
    internal class IdentityServerAuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationService _inner;
        private readonly IAuthenticationSchemeProvider _schemes;
        private readonly ISystemClock _clock;
        private readonly IUserSession _session;
        private readonly ILogger<IdentityServerAuthenticationService> _logger;

        public IdentityServerAuthenticationService(
            Decorator<IAuthenticationService> decorator,
            IAuthenticationSchemeProvider schemes,
            ISystemClock clock,
            IUserSession session,
            ILogger<IdentityServerAuthenticationService> logger)
        {
            _inner = decorator.Instance;
            
            _schemes = schemes;
            _clock = clock;
            _session = session;
            _logger = logger;
        }

        public async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            var defaultScheme = await _schemes.GetDefaultSignInSchemeAsync();
            var cookieScheme = await context.GetCookieAuthenticationSchemeAsync();

            if ((scheme == null && defaultScheme?.Name == cookieScheme) || scheme == cookieScheme)
            {
                AugmentPrincipal(principal);

                if (properties == null) properties = new AuthenticationProperties();
                properties.Items[IdentityConstants.AuthenticationProperties.Ip] = context.GetRequestIp();
                var userDevice = context.GetHeaderValueAs<string>("User-Agent").GetDevice();
                if (userDevice != null)
                {
                    properties.Items[IdentityConstants.AuthenticationProperties.Device] = userDevice.GetDeviceName();
                }

                await _session.CreateSessionIdAsync(principal, properties);
            }

            await _inner.SignInAsync(context, scheme, principal, properties);
        }

        private void AugmentPrincipal(ClaimsPrincipal principal)
        {
            _logger.LogDebug("Augmenting SignInContext");

            AssertRequiredClaims(principal);
            AugmentMissingClaims(principal, _clock.UtcNow.UtcDateTime);
        }

        public async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            var defaultScheme = await _schemes.GetDefaultSignOutSchemeAsync();
            var cookieScheme = await context.GetCookieAuthenticationSchemeAsync();

            if ((scheme == null && defaultScheme?.Name == cookieScheme) || scheme == cookieScheme)
            {
                // this sets a flag used by the FederatedSignoutAuthenticationHandlerProvider
                context.SetSignOutCalled();
                
                // this clears our session id cookie so JS clients can detect the user has signed out
                await _session.RemoveSessionIdCookieAsync();
            }

            await _inner.SignOutAsync(context, scheme, properties);
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
        {
            return _inner.AuthenticateAsync(context, scheme);
        }

        public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return _inner.ChallengeAsync(context, scheme, properties);
        }

        public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return _inner.ForbidAsync(context, scheme, properties);
        }

        private void AssertRequiredClaims(ClaimsPrincipal principal)
        {
            // for now, we don't allow more than one identity in the principal/cookie
            if (principal.Identities.Count() != 1) throw new InvalidOperationException("only a single identity supported");
            SetClaimByExistName(principal, JwtClaimTypes.Subject, ClaimTypes.Email);
            SetClaimByExistName(principal, JwtClaimTypes.Subject, ClaimTypes.Name);
            SetClaimByExistName(principal, JwtClaimTypes.Subject, ClaimTypes.GivenName);
            SetClaimByExistName(principal, JwtClaimTypes.Subject, ClaimTypes.NameIdentifier);
            if (principal.FindFirst(JwtClaimTypes.Subject) == null)
            {
                throw new InvalidOperationException("sub claim is missing");
            }
        }

        private void SetClaimByExistName(ClaimsPrincipal principal, string claimName, params string[] existsClaimNames)
        {
            if (principal.FindFirst(claimName) == null)
            {
                var resultClaim = principal.Claims.Join(existsClaimNames, x => x.Type, x => x, (claim, _) => claim).FirstOrDefault();
                if (resultClaim != null)
                {
                    var identity = principal.Identities.First();
                    identity.AddClaim(new Claim(claimName, resultClaim.Value));
                }
            }
        }

        private void AugmentMissingClaims(ClaimsPrincipal principal, DateTime authTime)
        {
            var identity = principal.Identities.First();

            // ASP.NET Identity issues this claim type and uses the authentication middleware name
            // such as "Google" for the value. this code is trying to correct/convert that for
            // our scenario. IOW, we take their old AuthenticationMethod value of "Google"
            // and issue it as the idp claim. we then also issue a amr with "external"
            var amr = identity.FindFirst(ClaimTypes.AuthenticationMethod);
            if (amr != null &&
                identity.FindFirst(JwtClaimTypes.IdentityProvider) == null &&
                identity.FindFirst(JwtClaimTypes.AuthenticationMethod) == null)
            {
                _logger.LogDebug("Removing amr claim with value: {value}", amr.Value);
                identity.RemoveClaim(amr);

                _logger.LogDebug("Adding idp claim with value: {value}", amr.Value);
                identity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, amr.Value));

                _logger.LogDebug("Adding amr claim with value: {value}", Constants.ExternalAuthenticationMethod);
                identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, Constants.ExternalAuthenticationMethod));
            }

            if (identity.FindFirst(JwtClaimTypes.IdentityProvider) == null)
            {
                _logger.LogDebug("Adding idp claim with value: {value}", IdentityServerConstants.LocalIdentityProvider);
                identity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, IdentityServerConstants.LocalIdentityProvider));
            }

            if (identity.FindFirst(JwtClaimTypes.AuthenticationMethod) == null)
            {
                if (identity.FindFirst(JwtClaimTypes.IdentityProvider).Value == IdentityServerConstants.LocalIdentityProvider)
                {
                    _logger.LogDebug("Adding amr claim with value: {value}", OidcConstants.AuthenticationMethods.Password);
                    identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, OidcConstants.AuthenticationMethods.Password));
                }
                else
                {
                    _logger.LogDebug("Adding amr claim with value: {value}", Constants.ExternalAuthenticationMethod);
                    identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, Constants.ExternalAuthenticationMethod));
                }
            }

            if (identity.FindFirst(JwtClaimTypes.AuthenticationTime) == null)
            {
                var time = new DateTimeOffset(authTime).ToUnixTimeSeconds().ToString();

                _logger.LogDebug("Adding auth_time claim with value: {value}", time);
                identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationTime, time, ClaimValueTypes.Integer64));
            }
        }
    }
}