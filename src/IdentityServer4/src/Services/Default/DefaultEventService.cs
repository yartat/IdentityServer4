// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Configuration;
using System.Threading.Tasks;
using IdentityServer4.Services;
#if NET7_0_OR_GREATER
#else
using Microsoft.AspNetCore.Authentication;
#endif

namespace IdentityServer4.Events
{
    /// <summary>
    /// The default event service
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.IEventService" />
    public class DefaultEventService : IEventService
    {
        /// <summary>
        /// The options
        /// </summary>
        protected readonly IdentityServerOptions Options;

        /// <summary>
        /// The context
        /// </summary>
        protected readonly IHttpContextAccessor Context;

        /// <summary>
        /// The sink
        /// </summary>
        protected readonly IEventSink Sink;

        /// <summary>
        /// The clock
        /// </summary>
#if NET7_0_OR_GREATER
        protected readonly TimeProvider Clock;
#else
        protected readonly ISystemClock Clock;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEventService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="context">The context.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="clock">The clock.</param>
        public DefaultEventService(
            IdentityServerOptions options,
            IHttpContextAccessor context,
            IEventSink sink,
#if NET7_0_OR_GREATER
            TimeProvider clock)
#else
            ISystemClock clock)
#endif
        {
            Options = options;
            Context = context;
            Sink = sink;
            Clock = clock;
        }

        /// <summary>
        /// Raises the specified event.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">evt</exception>
        public async Task RaiseAsync(Event? evt)
        {
            ArgumentNullException.ThrowIfNull(evt);

            if (CanRaiseEvent(evt))
            {
                await PrepareEventAsync(evt);
                await Sink.PersistAsync(evt);
            }
        }

        /// <summary>
        /// Indicates if the type of event will be persisted.
        /// </summary>
        /// <param name="evtType"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public bool CanRaiseEventType(EventTypes evtType) => 
            evtType switch
            {
                EventTypes.Failure => Options.Events.RaiseFailureEvents,
                EventTypes.Information => Options.Events.RaiseInformationEvents,
                EventTypes.Success => Options.Events.RaiseSuccessEvents,
                EventTypes.Error => Options.Events.RaiseErrorEvents,
                _ => throw new ArgumentOutOfRangeException(nameof(evtType)),
            };

        /// <summary>
        /// Determines whether this event would be persisted.
        /// </summary>
        /// <param name="evt">The evt.</param>
        /// <returns>
        ///   <c>true</c> if this event would be persisted; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanRaiseEvent(Event evt) =>
            CanRaiseEventType(evt.EventType);

        /// <summary>
        /// Prepares the event.
        /// </summary>
        /// <param name="evt">The evt.</param>
        /// <returns></returns>
        protected virtual async Task PrepareEventAsync(Event evt)
        {
            evt.ActivityId = Context.HttpContext?.TraceIdentifier;
#if NET7_0_OR_GREATER
            evt.TimeStamp = Clock.GetUtcNow().UtcDateTime;
#else
            evt.TimeStamp = Clock.UtcNow.UtcDateTime;
#endif
            evt.ProcessId = Process.GetCurrentProcess().Id;

            if (Context.HttpContext?.Connection.LocalIpAddress is not null)
            {
                evt.LocalIpAddress = Context.HttpContext.Connection.LocalIpAddress.ToString() + ":" + Context.HttpContext.Connection.LocalPort;
            }
            else
            {
                evt.LocalIpAddress = "unknown";
            }

            if (Context.HttpContext?.Connection.RemoteIpAddress is not null)
            {
                evt.RemoteIpAddress = Context.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            else
            {
                evt.RemoteIpAddress = "unknown";
            }

            await evt.PrepareAsync();
        }
    }
}