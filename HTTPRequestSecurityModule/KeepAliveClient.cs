// <copyright file="RequestKeepAliveValidation.cs" company="">
//     Copyright ©  2017
// </copyright>
namespace HTTPRequestSecurity
{
    using System;

    /// <summary>
    /// The Keep Alive Client.
    /// </summary>
    class KeepAliveClient
    {
        /// <summary>
        /// The timeout timespan.
        /// </summary>
        private static TimeSpan TIMEOUT = new TimeSpan(1, 0, 0);

        /// <summary>
        /// The last accessed date time.
        /// </summary>
        private DateTime lastAccessedDateTime;

        /// <summary>
        /// The request counter.
        /// </summary>
        private int requestCounter;

        /// <summary>
        /// The dictionary key.
        /// </summary>
        private string key;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeepAliveClient"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public KeepAliveClient(string key)
        {
            this.key = key;
            lastAccessedDateTime = DateTime.Now;
            requestCounter = 1;
        }

        /// <summary>
        /// Gets the request counter.
        /// </summary>
        /// <value>
        /// The request counter.
        /// </value>
        public int RequestCounter
        {
            get { return requestCounter; }
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key
        {
            get { return key; }
        }

        /// <summary>
        /// Touches this instance.
        /// </summary>
        /// <returns></returns>
        public int Touch()
        {
            requestCounter++;
            lastAccessedDateTime = DateTime.Now;
            return requestCounter;
        }

        /// <summary>
        /// Determines whether this instance is expired.
        /// </summary>
        /// <returns>
        ///   <c>True</c> if this instance is expired; otherwise, <c>false</c>.
        /// </returns>
        public bool IsExpired
        {
            get
            {
                return lastAccessedDateTime + TIMEOUT < DateTime.Now;
            }
        }
    }
}