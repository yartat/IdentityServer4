// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Hosting.FederatedSignOut
{
    // this intercepts IAuthenticationRequestHandler authentication handlers
    // to detect when they are handling federated signout. when they are invoked,
    // call signout on the default authentication scheme, and return 200 then 
    // we assume they are handling the federated signout in an iframe. 
    // based on this assumption, we then render our federated signout iframes 
    // to any current clients.
    internal class FederatedSignoutAuthenticationHandlerProvider : IAuthenticationHandlerProvider
    {
        private readonly IAuthenticationHandlerProvider _provider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, IAuthenticationHandler> _handlerMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="FederatedSignoutAuthenticationHandlerProvider"/> class.
        /// </summary>
        /// <param name="decorator">The authentication handler provider decorator.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <exception cref="ArgumentNullException">httpContextAccessor</exception>
        public FederatedSignoutAuthenticationHandlerProvider(
            Decorator<IAuthenticationHandlerProvider> decorator,
            IHttpContextAccessor httpContextAccessor)
        {
            _provider = decorator.Instance;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _handlerMap = new Dictionary<string, IAuthenticationHandler>();
        }

        /// <inheritdoc/>
        public async virtual Task<IAuthenticationHandler> GetHandlerAsync(HttpContext context, string authenticationScheme)
        {
            if (_handlerMap.TryGetValue(authenticationScheme, out var handler))
            {
                return handler;
            }

            handler = await _provider.GetHandlerAsync(context, authenticationScheme);
            if (handler is IAuthenticationRequestHandler requestHandler)
            {
                IAuthenticationHandler wrapper;

                if (requestHandler is IAuthenticationSignInHandler signinHandler)
                {
                    wrapper = new AuthenticationRequestSignInHandlerWrapper(signinHandler, _httpContextAccessor);
                    _handlerMap[authenticationScheme] = wrapper;
                    return wrapper;
                }

                if (requestHandler is IAuthenticationSignOutHandler signoutHandler)
                {
                    wrapper = new AuthenticationRequestSignOutHandlerWrapper(signoutHandler, _httpContextAccessor);
                    _handlerMap[authenticationScheme] = wrapper;
                    return wrapper;
                }

                wrapper = new AuthenticationRequestHandlerWrapper(requestHandler, _httpContextAccessor);
                _handlerMap[authenticationScheme] = wrapper;
                return wrapper;
            }

            _handlerMap[authenticationScheme] = handler;
            return handler;
        }
    }
}
