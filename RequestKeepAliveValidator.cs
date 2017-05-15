// <copyright file="RequestKeepAliveValidation.cs" company="">
//     Copyright ©  2017
// </copyright>
namespace HTTPRequestSecurity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Collections.Specialized;
    using System.Timers;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Web.IHttpModule" />
    public class HTTPRequestSecurityModule : IHttpModule
    {
        /// <summary>
        /// The module name.
        /// </summary>
        private const string MODULE_NAME = "HTTPRequestSecurityModule";

        /// <summary>
        /// The cleanup minutes.
        /// </summary>
        private const int CLEANUP_MINUTES = 15;

        /// <summary>
        /// The maximum requests (default value = 1).
        /// </summary>
        private int maxRequests = 1;

        /// <summary>
        /// The list lock object.
        /// </summary>
        private static object listLock = new object();

        /// <summary>
        /// The session list.
        /// </summary>
        private Dictionary<string, KeepAliveClient> sessionList = new Dictionary<string, KeepAliveClient>();

        /// <summary>
        /// The cleanup timer.
        /// </summary>
        private Timer cleanupTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="HTTPRequestSecurityModule"/> class.
        /// </summary>
        public HTTPRequestSecurityModule()
        {
            cleanupTimer = new Timer(CLEANUP_MINUTES * 60 * 1000);
            cleanupTimer.Elapsed += CleanupTimer_Elapsed;
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <value>
        /// The name of the module.
        /// </value>
        public string ModuleName
        {
            get { return MODULE_NAME; }
        }

        /// <summary>
        /// Gets or sets the maximum requests.
        /// </summary>
        /// <value>
        /// The maximum requests.
        /// </value>
        public int MaxRequests
        {
            get { return maxRequests; }
            set { maxRequests = value; }
        }


        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication" /> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            Logging.Instance.Info("Initialising HTTPRequestSecurityModule");
            maxRequests = 1;
            string maxKeepAliveRequests = System.Web.Configuration.WebConfigurationManager.AppSettings["MaxKeepAliveRequests"];
            if (!int.TryParse(maxKeepAliveRequests, out maxRequests))
            {
                var msg = $"Validate MaxKeepAliveRequests Configuration Value invalid.  MaxKeepAliveRequests='{maxKeepAliveRequests}'";
                Logging.Instance.Error(msg);
                throw new ArgumentException(msg);
            }
            if (maxRequests < 1)
            {
                var msg = $"appSettings.MaxKeepAliveRequests must be greater than zero: {maxRequests}";
                Logging.Instance.Error(msg);
                throw new ArgumentException(msg);
            }
            Logging.Instance.Info($"MaxRequests is configured as {maxRequests}");
            context.EndRequest += new EventHandler(OnEndRequest);
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule" />.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Called on the Http End Request event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void OnEndRequest(Object source, EventArgs e)
        {
            HttpApplication httpApplication = (HttpApplication)source;
            HttpRequest httpRequest = httpApplication.Context.Request;
            HttpResponse httpResponse = httpApplication.Context.Response;

            NameValueCollection serverVariables = httpRequest.ServerVariables;
            string key = serverVariables["REMOTE_HOST"] + ":" + serverVariables["REMOTE_PORT"];
            Logging.Instance.Info($"OnEndRequest key='{key}'");

            lock (listLock)
            {
                if (sessionList.ContainsKey(key))
                {
                    KeepAliveClient keepAliveClient = sessionList[key];
                    Logging.Instance.Info($"Found '{key}' in dictionary.");
                    if (keepAliveClient.RequestCounter > maxRequests)
                    {
                        Logging.Instance.Info($"Max requests reached for '{key}' (" + keepAliveClient.RequestCounter + "), force close connection to client");
                        httpResponse.Headers["Connection"] = "close";
                        sessionList.Remove(key);
                        return;
                    }
                    var requestCount = keepAliveClient.Touch();
                    Logging.Instance.Info($"Current request count = {requestCount}");
                }
                else
                {
                    sessionList.Add(key, new KeepAliveClient(key));
                    Logging.Instance.Info($"New session: '{key}' added to dictionary. Current request count = 1");
                }
            }
        }

        /// <summary>
        /// Removes expired records from the list.
        /// </summary>
        private void cleanOldKeepAliveRecords()
        {
            
            lock (listLock)
            {
                if(sessionList.Count==0)
                {
                    return;
                }

                foreach (KeepAliveClient keepAliveClient in sessionList.Values.ToList().Where(x => x.IsExpired))
                {
                    Logging.Instance.Info($"HTTPRequestSecurityModule.cleanOldKeepAliveRecords: key='{keepAliveClient.Key}' has expired - removing from dictionary.");
                    sessionList.Remove(keepAliveClient.Key);
                }
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the CleanupTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void CleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            cleanupTimer.Stop();
            lock (listLock)
            {
                cleanOldKeepAliveRecords();
            }
            cleanupTimer.Start();
        }
    }
}