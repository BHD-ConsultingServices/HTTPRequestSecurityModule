// <copyright file="RequestKeepAliveValidation.cs" company="">
//     Copyright ©  2017
// </copyright>
namespace HTTPRequestSecurity
{
    using NLog;
    using System;

    /// <summary>
    /// The NLog logging class.
    /// </summary>
    public class Logging
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static Logging instance;

        /// <summary>
        /// The lock object.
        /// </summary>
        private static Object lockObject = new object();

        /// <summary>
        /// The is verbose logging enabled flag.
        /// </summary>
        private bool IsVerboseLoggingEnabled = false;

        /// <summary>
        /// Gets an instance.
        /// </summary>
        /// <value>
        /// The Logging instance.
        /// </value>
        public static Logging Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logging();
                }
                return instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Logging"/> class from being created.
        /// </summary>
        private Logging()
        {
            _logger = LogManager.GetCurrentClassLogger();
            if (!bool.TryParse(System.Web.Configuration.WebConfigurationManager.AppSettings["IsVerboseLoggingEnabled"], out IsVerboseLoggingEnabled))
            {
                IsVerboseLoggingEnabled = false;
            }
        }

        /// <summary>
        /// Errors the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public void Error(string msg, params object[] messageArgs)
        {
            _logger.Error(string.Format(msg, messageArgs));
        }

        /// <summary>
        /// Informations the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public void Info(string msg, params object[] messageArgs)
        {
            if (IsVerboseLoggingEnabled)
            {
                _logger.Info(string.Format(msg, messageArgs));
            }
        }
    }
}