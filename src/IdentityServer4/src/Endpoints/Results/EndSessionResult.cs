// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Endpoints.Results
{
    /// <summary>
    /// Represents the result of an end session endpoint request.
    /// </summary>
    /// <remarks>
    /// This class is responsible for handling the end session (sign-out) flow by processing
    /// the validation result and coordinating the logout message storage and redirect to the
    /// configured logout URL. It implements the IEndpointResult interface to integrate with
    /// the IdentityServer endpoint result execution pipeline.
    /// </remarks>
    /// <param name="result">The end session validation result. Cannot be null.</param>
    /// <seealso cref="IEndpointResult" />
    public class EndSessionResult(EndSessionValidationResult result) : IEndpointResult
    {
        /// <summary>
        /// The end session validation result.
        /// </summary>
        private readonly EndSessionValidationResult _result = result ?? throw new ArgumentNullException(nameof(result));
        
        /// <summary>
        /// The IdentityServer configuration options.
        /// </summary>
        private IdentityServerOptions? _options;
        
        /// <summary>
        /// The system clock for obtaining current time.
        /// </summary>
#if NET7_0_OR_GREATER
        private  TimeProvider? _clock;
#else
        private ISystemClock? _clock;
#endif
        
        /// <summary>
        /// The message store for persisting logout messages.
        /// </summary>
        private IMessageStore<LogoutMessage>? _logoutMessageStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndSessionResult"/> class with all required dependencies.
        /// </summary>
        /// <param name="result">The end session validation result. Cannot be null.</param>
        /// <param name="options">The IdentityServer configuration options.</param>
        /// <param name="clock">The system clock for obtaining current time.</param>
        /// <param name="logoutMessageStore">The message store for persisting logout messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
        internal EndSessionResult(
            EndSessionValidationResult result,
            IdentityServerOptions options,
#if NET7_0_OR_GREATER
            TimeProvider clock,
#else
            ISystemClock clock,
#endif
            IMessageStore<LogoutMessage> logoutMessageStore)
            : this(result)
        {
            _options = options;
            _clock = clock;
            _logoutMessageStore = logoutMessageStore;
        }

        /// <summary>
        /// Initializes the internal dependencies from the HTTP context's service provider if they are not already set.
        /// </summary>
        /// <param name="context">The HTTP context providing access to the dependency injection container.</param>
        /// <remarks>
        /// This method uses lazy initialization to resolve required services from the dependency injection
        /// container only when needed. It allows the class to work with either pre-configured dependencies
        /// (from the internal constructor) or resolve them at execution time.
        /// </remarks>
        [MemberNotNull(nameof(_options), nameof(_clock), nameof(_logoutMessageStore))]
        private void Init(HttpContext context)
        {
            _options ??= context.RequestServices.GetRequiredService<IdentityServerOptions>();
#if NET7_0_OR_GREATER
            _clock ??= context.RequestServices.GetRequiredService<TimeProvider>();
#else
            _clock ??= context.RequestServices.GetRequiredService<ISystemClock>();
#endif
            _logoutMessageStore ??= context.RequestServices.GetRequiredService<IMessageStore<LogoutMessage>>();
        }

        /// <summary>
        /// Executes the end session result by handling logout messages and redirecting to the logout URL.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A completed task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Initializes required dependencies from the service provider
        /// 2. Creates a logout message from the validated request if available
        /// 3. Stores the logout message and obtains a message ID if it contains payload
        /// 4. Constructs the logout redirect URL with the message ID as a query parameter
        /// 5. Redirects the response to the configured logout URL
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            var validatedRequest = _result.IsError ? null : _result.ValidatedRequest;

            string? id = null;

            if (validatedRequest != null)
            {
                var logoutMessage = new LogoutMessage(validatedRequest);
                if (logoutMessage.ContainsPayload)
                {
#if NET7_0_OR_GREATER
                    var msg = new Message<LogoutMessage>(logoutMessage, _clock.GetUtcNow().UtcDateTime);
#else
                    var msg = new Message<LogoutMessage>(logoutMessage, _clock.UtcNow.UtcDateTime);
#endif
                    id = await _logoutMessageStore.WriteAsync(msg);
                }
            }

            var redirect = _options.UserInteraction.LogoutUrl.AsSpan();

            if (redirect.IsLocalUrl())
            {
                redirect = context.GetIdentityServerRelativeUrl(redirect);
            }

            if (id != null)
            {
                redirect = redirect.AddQueryString(_options.UserInteraction.LogoutIdParameter, id);
            }

            context.Response.Redirect(redirect.ToString());
        }
    }
}
