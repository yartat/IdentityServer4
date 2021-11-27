// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace IdentityServer4.Storage.Exceptions
{
    /// <summary>
    /// Exception for signaling invalid persist grant version errors.
    /// </summary>
    /// <seealso cref="Exception"/>
    public class InvalidPersistGrantVersionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPersistGrantVersionException"/> class.
        /// </summary>
        public InvalidPersistGrantVersionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPersistGrantVersionException"/> class with message.
        /// </summary>
        /// <param name="message">The message.</param>
        public InvalidPersistGrantVersionException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPersistGrantVersionException"/> class with message and internal exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidPersistGrantVersionException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}