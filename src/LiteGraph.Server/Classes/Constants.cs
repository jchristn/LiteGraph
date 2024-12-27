﻿namespace LiteGraph.Server.Classes
{
    using System;

    /// <summary>
    /// Constants.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Logo.
        /// </summary>
        public static string Logo =
            @"  _ _ _                          _     " + Environment.NewLine +
            @" | (_) |_ ___ __ _ _ _ __ _ _ __| |_   " + Environment.NewLine +
            @" | | |  _/ -_) _` | '_/ _` | '_ \ ' \  " + Environment.NewLine +
            @" |_|_|\__\___\__, |_| \__,_| .__/_||_| " + Environment.NewLine +
            @"             |___/         |_|         " + Environment.NewLine;

        /// <summary>
        /// Product name.
        /// </summary>
        public static string ProductName = " LiteGraph Server";

        /// <summary>
        /// Copyright.
        /// </summary>
        public static string Copyright = " (c)2024 Joel Christner";

        /// <summary>
        /// Settings file.
        /// </summary>
        public static string SettingsFile = "./litegraph.json";

        /// <summary>
        /// Log file directory.
        /// </summary>
        public static string LogDirectory = "./logs/";

        /// <summary>
        /// Log filename.
        /// </summary>
        public static string LogFilename = "litegraph.log";

        #region Environment-Variables

        /// <summary>
        /// Webserver port environment variable.
        /// </summary>
        public static string WebserverPortEnvironmentVariable = "LITEGRAPH_PORT";

        /// <summary>
        /// Data database filename environment variable.
        /// </summary>
        public static string DatabaseFilenameEnvironmentVariable = "LITEGRAPH_DB";

        #endregion

        #region Content-Types

        /// <summary>
        /// Content-type value for XML.
        /// </summary>
        public static string XmlContentType = "application/xml";

        /// <summary>
        /// Content-type value for JSON.
        /// </summary>
        public static string JsonContentType = "application/json";

        /// <summary>
        /// Content-type value for HTML.
        /// </summary>
        public static string HtmlContentType = "text/html";

        /// <summary>
        /// Favicon content type.
        /// </summary>
        public static string FaviconContentType = "image/x-icon";

        #endregion

        #region HTML

        /// <summary>
        /// Default homepage contents.
        /// </summary>
        public static string DefaultHomepage =
            "<html>"
            + "<head><title>LiteGraph</title></head>"
            + "<body>"
            + "<div><pre>" + Logo + Environment.NewLine
            + " Your LiteGraph node is operational</p></div>"
            + "</body>"
            + "</html>";

        /// <summary>
        /// Favicon file.
        /// </summary>
        public static string FaviconFile = "./assets/favicon.ico";

        #endregion

        #region Headers

        /// <summary>
        /// Hostname header key.
        /// </summary>
        public static string HostnameHeader = "x-hostname";

        /// <summary>
        /// Authorization header.
        /// </summary>
        public static string AuthorizationHeader = "Authorization";

        #endregion

        #region Querystring

        /// <summary>
        /// Skip querystring key.
        /// </summary>
        public static string SkipQuerystring = "skip";

        /// <summary>
        /// Force querystring key.
        /// </summary>
        public static string ForceQuerystring = "force";

        /// <summary>
        /// Include data querystring key.
        /// </summary>
        public static string IncludeDataQuerystring = "incldata";

        /// <summary>
        /// From GUID querystring key.
        /// </summary>
        public static string FromGuidQuerystring = "from";

        /// <summary>
        /// To GUID querystring key.
        /// </summary>
        public static string ToGuidQuerystring = "to";

        #endregion
    }
}
