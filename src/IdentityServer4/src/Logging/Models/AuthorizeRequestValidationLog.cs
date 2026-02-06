// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;
using System;

namespace IdentityServer4.Logging.Models
{
    /// <summary>
    /// Defines a class to view authorize request in log.
    /// </summary>
    public class AuthorizeRequestValidationLog
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>The client identifier.</value>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>The name of the client.</value>
        public string? ClientName { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>The redirect URI.</value>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the allowed redirect URIs.
        /// </summary>
        /// <value>The allowed redirect URIs.</value>
        public IEnumerable<Uri>? AllowedRedirectUris { get; set; }

        /// <summary>
        /// Gets or sets the subject identifier.
        /// </summary>
        /// <value>The subject identifier.</value>
        public string? SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the type of the response.
        /// </summary>
        /// <value>The type of the response.</value>
        public string ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the response mode.
        /// </summary>
        /// <value>The response mode.</value>
        public string ResponseMode { get; set; }

        /// <summary>
        /// Gets or sets the type of the grant.
        /// </summary>
        /// <value>The type of the grant.</value>
        public string GrantType { get; set; }

        /// <summary>
        /// Gets or sets the requested scopes.
        /// </summary>
        /// <value>The requested scopes.</value>
        public string RequestedScopes { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the UI locales.
        /// </summary>
        /// <value>The UI locales.</value>
        public string UiLocales { get; set; }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        /// <value>The nonce.</value>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets the authentication context reference classes.
        /// </summary>
        /// <value>The authentication context reference classes.</value>
        public IEnumerable<string>? AuthenticationContextReferenceClasses { get; set; }

        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>The display mode.</value>
        public string DisplayMode { get; set; }

        /// <summary>
        /// Gets or sets the prompt mode.
        /// </summary>
        /// <value>The prompt mode.</value>
        public string PromptMode { get; set; }

        /// <summary>
        /// Gets or sets the maximum age.
        /// </summary>
        /// <value>The maximum age.</value>
        public int? MaxAge { get; set; }

        /// <summary>
        /// Gets or sets the login hint.
        /// </summary>
        /// <value>The login hint.</value>
        public string LoginHint { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>The session identifier.</value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the raw.
        /// </summary>
        /// <value>The raw.</value>
        public Dictionary<string, string?> Raw { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeRequestValidationLog"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="sensitiveValuesFilter"></param>
        public AuthorizeRequestValidationLog(ValidatedAuthorizeRequest request, HashSet<string> sensitiveValuesFilter)
        {
            Raw = request.Raw.ToScrubbedDictionary(sensitiveValuesFilter);

            if (request.Client is not null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;

                AllowedRedirectUris = request.Client.RedirectUris;
            }

            if (request.Subject is not null)
            {
                var subjectClaim = request.Subject.FindFirst(JwtClaimTypes.Subject);
                if (subjectClaim is not null)
                {
                    SubjectId = subjectClaim.Value;
                }
                else
                {
                    SubjectId = "anonymous";
                }
            }

            if (request.AuthenticationContextReferenceClasses.Any())
            {
                AuthenticationContextReferenceClasses = request.AuthenticationContextReferenceClasses;
            }

            RedirectUri = request.RedirectUri;
            ResponseType = request.ResponseType;
            ResponseMode = request.ResponseMode;
            GrantType = request.GrantType;
            RequestedScopes = request.RequestedScopes.ToSpaceSeparatedString();
            State = request.State;
            UiLocales = request.UiLocales;
            Nonce = request.Nonce;

            DisplayMode = request.DisplayMode;
            PromptMode = request.PromptModes.ToSpaceSeparatedString();
            LoginHint = request.LoginHint;
            MaxAge = request.MaxAge;
            SessionId = request.SessionId;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            LogSerializer.Serialize(this);
    }
}