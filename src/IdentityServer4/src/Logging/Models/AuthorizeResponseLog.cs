// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Extensions;
using IdentityServer4.ResponseHandling;

namespace IdentityServer4.Logging.Models
{
    /// <summary>
    /// Defines a class to view authorize response in the log.
    /// </summary>
    public class AuthorizeResponseLog
    {
        /// <summary>
        /// Gets or sets the subject identifier.
        /// </summary>
        /// <value>The subject identifier.</value>
        public string SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>The client identifier.</value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>The redirect URI.</value>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>The scope.</value>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the error description.
        /// </summary>
        /// <value>The error description.</value>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeResponseLog"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        public AuthorizeResponseLog(AuthorizeResponse response)
        {
            ClientId = response.Request?.Client?.ClientId;
            SubjectId = response.Request?.Subject?.GetSubjectId();
            RedirectUri = response.RedirectUri;
            State = response.State;
            Scope = response.Scope;
            Error = response.Error;
            ErrorDescription = response.ErrorDescription;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return LogSerializer.Serialize(this);
        }
    }
}