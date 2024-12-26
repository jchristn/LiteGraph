﻿namespace LiteGraph.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using LiteGraph.GraphRepositories;
    using LiteGraph.Serialization;
    using LiteGraph.Server.API.Agnostic;
    using LiteGraph.Server.API.REST;
    using LiteGraph.Server.Classes;
    using LiteGraph.Server.Services;
    using SyslogLogging;
    using WatsonWebserver;

    /// <summary>
    /// Orchestrator server.
    /// </summary>
    public static class LiteGraphServer
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private static string _Header = "[LiteGraphServer] ";
        private static int _ProcessId = Environment.ProcessId;
        private static bool _CreateDefaultRecords = false;

        private static Settings _Settings = new Settings();
        private static LoggingModule _Logging = null;
        private static SerializationHelper _Serializer = new SerializationHelper();

        private static GraphRepositoryBase _Repository = null;
        private static LiteGraphClient _LiteGraph = null;

        private static ServiceHandler _ServiceHandler = null;
        private static AuthenticationService _AuthenticationService = null;
        private static RestServiceHandler _RestService = null;

        private static CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private static CancellationToken _Token;

        #endregion

        #region Entrypoint

        public static void Main(string[] args)
        {
            Welcome();
            ParseArguments(args);
            InitializeSettings();
            InitializeGlobals();

            _Logging.Info(_Header + "starting at " + DateTime.UtcNow + " using process ID " + _ProcessId);

            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            bool waitHandleSignal = false;
            do
            {
                waitHandleSignal = waitHandle.WaitOne(1000);
            }
            while (!waitHandleSignal);
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        private static void Welcome()
        {
            Console.WriteLine(
                Environment.NewLine +
                Constants.Logo +
                Environment.NewLine +
                Constants.ProductName +
                Environment.NewLine +
                Constants.Copyright +
                Environment.NewLine);
        }

        private static void ParseArguments(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("--config="))
                    {
                        Constants.SettingsFile = arg.Substring(9);
                    }
                }
            }
        }

        private static void InitializeSettings()
        {
            Console.WriteLine("Using settings file '" + Constants.SettingsFile + "'");

            if (!File.Exists(Constants.SettingsFile))
            {
                Console.WriteLine("Settings file '" + Constants.SettingsFile + "' does not exist, creating");
                File.WriteAllBytes(Constants.SettingsFile, Encoding.UTF8.GetBytes(_Serializer.SerializeJson(_Settings, true)));
                _CreateDefaultRecords = true;
            }
            else
            {
                _Settings = _Serializer.DeserializeJson<Settings>(File.ReadAllText(Constants.SettingsFile));
            }
        }

        private static void InitializeGlobals()
        {
            #region General

            _Token = _TokenSource.Token;

            #endregion

            #region Environment

            string webserverPortStr = Environment.GetEnvironmentVariable(Constants.WebserverPortEnvironmentVariable);
            if (Int32.TryParse(webserverPortStr, out int webserverPort))
            {
                if (webserverPort >= 0 && webserverPort <= 65535)
                {
                    _Settings.Rest.Port = webserverPort;
                }
                else
                {
                    Console.WriteLine("Invalid webserver port detected in environment variable " + Constants.WebserverPortEnvironmentVariable);
                }
            }

            string dbFilename = Environment.GetEnvironmentVariable(Constants.DatabaseFilenameEnvironmentVariable);
            if (!String.IsNullOrEmpty(dbFilename)) _Settings.LiteGraph.GraphRepositoryFilename = dbFilename;

            #endregion

            #region Logging

            Console.WriteLine("Initializing logging");

            List<SyslogServer> syslogServers = new List<SyslogServer>();
            if (_Settings.Logging.Servers != null && _Settings.Logging.Servers.Count > 0)
            {
                foreach (LiteGraph.SyslogServer server in _Settings.Logging.Servers)
                {
                    syslogServers.Add(
                        new SyslogServer
                        {
                            Hostname = server.Hostname,
                            Port = server.Port
                        }
                    );

                    Console.WriteLine("| syslog://" + server.Hostname + ":" + server.Port);
                }
            }

            _Logging = new LoggingModule(syslogServers);
            _Logging.Settings.EnableConsole = _Settings.Logging.ConsoleLogging;
            _Logging.Settings.EnableColors = _Settings.Logging.EnableColors;

            if (!String.IsNullOrEmpty(_Settings.Logging.LogDirectory))
            {
                if (!Directory.Exists(_Settings.Logging.LogDirectory))
                    Directory.CreateDirectory(_Settings.Logging.LogDirectory);

                _Settings.Logging.LogFilename = _Settings.Logging.LogDirectory + _Settings.Logging.LogFilename;
            }

            if (!String.IsNullOrEmpty(_Settings.Logging.LogFilename))
            {
                _Logging.Settings.FileLogging = FileLoggingMode.FileWithDate;
                _Logging.Settings.LogFilename = _Settings.Logging.LogFilename;
            }

            _Logging.Debug(_Header + "logging initialized");

            #endregion

            #region Repositories

            _Repository = new SqliteGraphRepository(_Settings.LiteGraph.GraphRepositoryFilename);

            #endregion

            #region Create-Default-Records

            if (_CreateDefaultRecords) CreateDefaultRecords();

            #endregion

            #region LiteGraph-Client

            _LiteGraph = new LiteGraphClient(_Repository, _Settings.Logging);
            _LiteGraph.Logging.Enable = true;
            _LiteGraph.Logging.Logger = LiteGraphLogger;

            _LiteGraph.InitializeRepository();

            #endregion

            #region Services

            _AuthenticationService = new AuthenticationService(
                _Settings,
                _Logging,
                _Serializer,
                _Repository);

            _ServiceHandler = new ServiceHandler(
                _Settings, 
                _Logging, 
                _LiteGraph, 
                _Serializer, 
                _AuthenticationService);

            _RestService = new RestServiceHandler(
                _Settings,
                _Logging,
                _LiteGraph,
                _Serializer,
                _AuthenticationService,
                _ServiceHandler);

            #endregion

            Console.WriteLine("");
        }

        private static void LiteGraphLogger(SeverityEnum sev, string msg)
        {
            _Logging.Debug(msg);
        }

        private static void CreateDefaultRecords()
        {
            #region Metadata-Records

            Console.WriteLine("Creating default records in database " + _Settings.LiteGraph.GraphRepositoryFilename);

            TenantMetadata tenant = new TenantMetadata
            {
                GUID = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Name = "Default tenant",
                Active = true,
                CreatedUtc = DateTime.UtcNow
            };

            if (!_Repository.ExistsTenant(tenant.GUID))
            {
                tenant = _Repository.CreateTenant(tenant);
                Console.WriteLine("| Created tenant     : " + tenant.GUID);
            }

            UserMaster user = new UserMaster
            {
                GUID = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                TenantGUID = tenant.GUID,
                FirstName = "Default",
                LastName = "User",
                Email = "default@user.com",
                Password = "password",
                Active = true,
                CreatedUtc = DateTime.UtcNow
            };

            if (!_Repository.ExistsUser(tenant.GUID, user.GUID))
            {
                user = _Repository.CreateUser(user);
                Console.WriteLine("| Created user       : " + user.GUID + " email: " + user.Email + " pass: " + user.Password);
            }

            Credential cred = new Credential
            {
                GUID = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                TenantGUID = tenant.GUID,
                UserGUID = user.GUID,
                Name = "Default credential",
                BearerToken = "default",
                Active = true,
                CreatedUtc = DateTime.UtcNow
            };

            if (!_Repository.ExistsCredential(cred.TenantGUID, cred.GUID))
            {
                cred = _Repository.CreateCredential(cred);
                Console.WriteLine("| Created credential : " + cred.GUID + " bearer token: " + cred.BearerToken);
            }

            Graph graph = new Graph
            {
                GUID = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                TenantGUID = tenant.GUID,
                Name = "Default graph",
                CreatedUtc = DateTime.UtcNow
            };

            if (!_Repository.ExistsGraph(graph.TenantGUID, graph.GUID))
            {
                graph = _Repository.CreateGraph(graph);
                Console.WriteLine("| Created graph      : " + graph.GUID + " " + graph.Name);
            }

            #endregion

            Console.WriteLine("Finished creating default records");
        }

        #endregion
    }
}