using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Interface for the login URL processor
    /// </summary>
    public interface ILoginUrlProcessor
    {
        /// <summary>
        /// Processes a login URL.
        /// </summary>
        /// <param name="url">The login URL.</param>
        /// <param name="data">The login data.</param>
        /// <returns>The processed login URL.</returns>
        string Process(string url, IDictionary<string, string[]> data);
    }
}
