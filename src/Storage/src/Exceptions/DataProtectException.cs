// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace IdentityServer4.Storage.Exceptions
{
    /// <summary>
    /// Exception for signaling data protect errors.
    /// </summary>
    /// <seealso cref="Exception"/>
    public class DataProtectException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectException"/> class.
        /// </summary>
        public DataProtectException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectException"/> class with message.
        /// </summary>
        /// <param name="message">The message.</param>
        public DataProtectException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectException"/> class with message and internal exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataProtectException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}