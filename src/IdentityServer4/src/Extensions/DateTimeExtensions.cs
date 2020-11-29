// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Diagnostics;

namespace IdentityServer4.Extensions
{
    /// <summary>
    /// Defines a date and time extension methods.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Determines whether the specified seconds has exceeded.
        /// </summary>
        /// <param name="creationTime">The creation time.</param>
        /// <param name="seconds">The seconds.</param>
        /// <param name="now">The now.</param>
        /// <returns><c>true</c> if the specified seconds has exceeded; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        public static bool HasExceeded(this DateTime creationTime, int seconds, DateTime now) =>
            now > creationTime.AddSeconds(seconds);

        /// <summary>
        /// Gets the lifetime in seconds.
        /// </summary>
        /// <param name="creationTime">The creation time.</param>
        /// <param name="now">The now.</param>
        /// <returns>System.Int32.</returns>
        [DebuggerStepThrough]
        public static int GetLifetimeInSeconds(this DateTime creationTime, DateTime now) =>
            (int)(now - creationTime).TotalSeconds;

        /// <summary>
        /// Determines whether the specified now has expired.
        /// </summary>
        /// <param name="expirationTime">The expiration time.</param>
        /// <param name="now">The now.</param>
        /// <returns><c>true</c> if the specified now has expired; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        public static bool HasExpired(this DateTime? expirationTime, DateTime now) =>
            expirationTime.HasValue && expirationTime.Value.HasExpired(now);

        /// <summary>
        /// Determines whether the specified now has expired.
        /// </summary>
        /// <param name="expirationTime">The expiration time.</param>
        /// <param name="now">The now.</param>
        /// <returns><c>true</c> if the specified now has expired; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        public static bool HasExpired(this DateTime expirationTime, DateTime now) =>
            now > expirationTime;
    }
}