// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modifications copyright (c) Yaroslav Tatarenko, co-author and maintainer of this fork.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Duende.IdentityModel;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    /// <summary>
    /// The userinfo endpoint
    /// </summary>
    /// <seealso cref="IdentityServer4.Hosting.IEndpointHandler" />
    internal class UserInfoEndpoint : IEndpointHandler
    {
        private readonly BearerTokenUsageValidator _tokenUsageValidator;
        private readonly IUserInfoRequestValidator _requestValidator;
        private readonly IUserInfoResponseGenerator _responseGenerator;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoEndpoint" /> class.
        /// </summary>
        /// <param name="tokenUsageValidator">The token usage validator.</param>
        /// <param name="requestValidator">The request validator.</param>
        /// <param name="responseGenerator">The response generator.</param>
        /// <param name="logger">The logger.</param>
        public UserInfoEndpoint(
            BearerTokenUsageValidator tokenUsageValidator,
            IUserInfoRequestValidator requestValidator,
            IUserInfoResponseGenerator responseGenerator,
            ILogger<UserInfoEndpoint> logger)
        {
            _tokenUsageValidator = tokenUsageValidator ?? throw new ArgumentNullException(nameof(tokenUsageValidator));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("User info request processing");
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsPost(context.Request.Method))
            {
                _logger.LogWarning("Invalid HTTP method for userinfo endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var result = await ProcessUserInfoRequestAsync(context);

            _logger.LogTrace("User info request completed");
            return result;
        }

        private async Task<IEndpointResult> ProcessUserInfoRequestAsync(HttpContext context)
        {
            _logger.LogTrace("Start userinfo request");

            // userinfo requires an access token on the request
            var tokenUsageResult = await _tokenUsageValidator.ValidateAsync(context);
            if (tokenUsageResult.TokenFound == false)
            {
                var error = "No access token found.";

                _logger.LogError(error);
                return Error(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            // validate the request
            _logger.LogTrace("Calling into userinfo request validator: {type}", _requestValidator.GetType().FullName);
            var validationResult = await _requestValidator.ValidateRequestAsync(tokenUsageResult.Token);

            if (validationResult.IsError)
            {
                //_logger.LogError("Error validating  validationResult.Error);
                return Error(validationResult.Error);
            }

            // generate response
            _logger.LogTrace("Calling into userinfo response generator: {type}", _responseGenerator.GetType().FullName);
            var response = await _responseGenerator.ProcessAsync(validationResult);

            _logger.LogDebug("End userinfo request. Return UserInfoResult");
            return new UserInfoResult(response);
        }

        private IEndpointResult Error(string error, string description = null) =>
            new ProtectedResourceErrorResult(error, description);
    }
}