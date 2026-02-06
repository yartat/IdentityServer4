// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;

namespace IdentityServer4.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IPrincipal"/> and <see cref="IIdentity"/> .
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Gets the authentication time.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DateTime GetAuthenticationTime(this IPrincipal principal) =>
            DateTimeOffset.FromUnixTimeSeconds(principal.GetAuthenticationTimeEpoch()).UtcDateTime;

        /// <summary>
        /// Gets the authentication epoch time.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IPrincipal principal) =>
            principal?.Identity?.GetAuthenticationTimeEpoch() ?? 0;

        /// <summary>
        /// Gets the authentication epoch time.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IIdentity? identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id?.FindFirst(JwtClaimTypes.AuthenticationTime);

            if (claim == null) throw new InvalidOperationException("auth_time is missing.");
           
            return long.Parse(claim.Value);
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(principal))]
        public static string? GetSubjectId(this IPrincipal? principal) =>
            principal?.Identity.GetSubjectId();

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(principal))]
        public static string? TryGetSubjectId(this IPrincipal? principal) =>
            principal?.Identity.TryGetSubjectId();

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">sub claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IIdentity? identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = (id?.FindFirst(JwtClaimTypes.Subject)) ?? throw new InvalidOperationException("sub claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">sub claim is missing</exception>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(identity))]
        public static string? TryGetSubjectId(this IIdentity? identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id?.FindFirst(JwtClaimTypes.Subject);
            return claim?.Value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [Obsolete("This method will be removed in a future version. Use GetDisplayName instead.")]
        [return: NotNullIfNotNull(nameof(principal))]
        public static string? GetName(this IPrincipal? principal) =>
            principal?.Identity?.GetName();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetDisplayName(this ClaimsPrincipal? principal)
        {
            var name = principal?.Identity?.Name;
            if (name.IsPresent())
            {
                return name;
            }

            var sub = principal?.FindFirst(JwtClaimTypes.Subject);
            return sub is not null ? sub.Value : string.Empty;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">name claim is missing</exception>
        [DebuggerStepThrough]
        [Obsolete("This method will be removed in a future version. Use GetDisplayName instead.")]
        public static string GetName(this IIdentity? identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = (id?.FindFirst(JwtClaimTypes.Name)) ?? throw new InvalidOperationException("name claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(principal))]
        public static string? GetAuthenticationMethod(this IPrincipal? principal) =>
            principal?.Identity?.GetAuthenticationMethod();

        /// <summary>
        /// Gets the authentication method claims.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(principal))]
        public static IEnumerable<Claim>? GetAuthenticationMethods(this IPrincipal? principal) =>
            principal?.Identity?.GetAuthenticationMethods();

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">amr claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IIdentity? identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = (id?.FindFirst(JwtClaimTypes.AuthenticationMethod)) ?? throw new InvalidOperationException("amr claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the authentication method claims.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(identity))]
        public static IEnumerable<Claim>? GetAuthenticationMethods(this IIdentity? identity)
        {
            var id = identity as ClaimsIdentity;
            return id?.FindAll(JwtClaimTypes.AuthenticationMethod);
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(principal))]
        public static string? GetIdentityProvider(this IPrincipal? principal) =>
            principal?.Identity?.GetIdentityProvider();

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">idp claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IIdentity? identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = (id?.FindFirst(JwtClaimTypes.IdentityProvider)) ?? throw new InvalidOperationException("idp claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the session id.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns>Returns session id</returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(identity))]
        public static string? GetSessionId(this IIdentity? identity)
        {
            var id = identity as ClaimsIdentity;
            return id?.FindFirst(JwtClaimTypes.SessionId)?.Value;
        }

        /// <summary>
        /// Gets the session id.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns>Returns session id</returns>
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(principal))]
        public static string? GetSessionId(this IPrincipal? principal) =>
            principal?.Identity?.GetSessionId();

        /// <summary>
        /// Determines whether this instance is authenticated.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns>
        ///   <c>true</c> if the specified principal is authenticated; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsAuthenticated([NotNullWhen(true)] this IPrincipal? principal) =>
            principal?.Identity?.IsAuthenticated == true;
    }
}