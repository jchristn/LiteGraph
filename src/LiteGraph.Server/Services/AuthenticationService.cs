namespace LiteGraph.Server.Services
{
    using LiteGraph.GraphRepositories;
    using LiteGraph.Serialization;
    using LiteGraph.Server.Classes;
    using SyslogLogging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class AuthenticationService
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private string _Header = "[AuthService] ";
        private Settings _Settings = null;
        private LoggingModule _Logging = null;
        private SerializationHelper _Serializer = new SerializationHelper();
        private GraphRepositoryBase _Repository = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Authentication service.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="logging">Logging module.</param>
        /// <param name="serializer">Serializer.</param>
        /// <param name="repo">Graph repository driver.</param>
        public AuthenticationService(
            Settings settings, 
            LoggingModule logging, 
            SerializationHelper serializer,
            GraphRepositoryBase repo)
        {
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _Logging = logging ?? throw new ArgumentNullException(nameof(logging));
            _Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _Repository = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        #endregion

        #region Internal-Methods

        /// <summary>
        /// Authenticate and authorize.
        /// </summary>
        /// <param name="req">Request context.</param>
        /// <returns>Request context.</returns>
        internal void AuthenticateAndAuthorize(RequestContext req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            
            if (_Settings.Debug.Authentication)
                _Logging.Debug(_Header + "authentication request:" + Environment.NewLine + _Serializer.SerializeJson(req, false));

            Authenticate(req);
            Authorize(req);

            if (_Settings.Debug.Authentication)
                _Logging.Debug(_Header + "authentication result:" + Environment.NewLine + _Serializer.SerializeJson(req, false));
        }

        #endregion

        #region Private-Methods

        private void Authenticate(RequestContext req)
        {
            if (String.IsNullOrEmpty(req.Authentication.BearerToken))
            {
                _Logging.Warn(_Header + "no authentication material supplied from " + req.Http.Request.Source.IpAddress);
                req.Authentication.Result = AuthenticationResultEnum.NotFound;
                return;
            }

            if (req.Authentication.BearerToken.Equals(_Settings.LiteGraph.AdminBearerToken))
            {
                _Logging.Info(_Header + "admin bearer token in use from " + req.Ip);
                req.Authentication.IsAdmin = true;
            }
            else
            {
                req.Authentication.Credential = _Repository.ReadCredentialByBearerToken(req.Authentication.BearerToken);
                if (req.Authentication.Credential == null)
                {
                    _Logging.Warn(_Header + "unable to find bearer token " + req.Authentication.BearerToken);
                    req.Authentication.Result = AuthenticationResultEnum.NotFound;
                    return;
                }
                else
                {
                    req.Authentication.CredentialGUID = req.Authentication.Credential.GUID;
                }

                if (!req.Authentication.Credential.Active)
                {
                    _Logging.Warn(_Header + "credential " + req.Authentication.Credential.GUID + " is inactive");
                    req.Authentication.Result = AuthenticationResultEnum.Inactive;
                    return;
                }

                req.Authentication.Tenant = _Repository.ReadTenant(req.Authentication.Credential.TenantGUID);
                if (req.Authentication.Tenant == null)
                {
                    _Logging.Warn(_Header + "tenant " + req.Authentication.Credential.TenantGUID + " referenced in credential " + req.Authentication.Credential.GUID + " not found");
                    req.Authentication.Result = AuthenticationResultEnum.NotFound;
                    return;
                }
                else
                {
                    req.Authentication.TenantGUID = req.Authentication.Tenant.GUID;
                }

                if (!req.Authentication.Tenant.Active)
                {
                    _Logging.Warn(_Header + "tenant " + req.Authentication.Credential.TenantGUID + " referenced in credential " + req.Authentication.Credential.GUID + " is inactive");
                    req.Authentication.Result = AuthenticationResultEnum.Inactive;
                    return;
                }

                req.Authentication.User = _Repository.ReadUser(req.Authentication.Credential.TenantGUID, req.Authentication.Credential.UserGUID);
                if (req.Authentication.User == null)
                {
                    _Logging.Warn(_Header + "user " + req.Authentication.Credential.UserGUID + " referenced in credential " + req.Authentication.Credential.GUID + " not found");
                    req.Authentication.Result = AuthenticationResultEnum.NotFound;
                    return;
                }
                else
                {
                    req.Authentication.UserGUID = req.Authentication.User.GUID;
                }

                if (!req.Authentication.User.Active)
                {
                    _Logging.Warn(_Header + "user " + req.Authentication.Credential.UserGUID + " referenced in credential " + req.Authentication.Credential.GUID + " is inactive");
                    req.Authentication.Result = AuthenticationResultEnum.Inactive;
                    return;
                }
            }

            req.Authentication.Result = AuthenticationResultEnum.Success;
            return;
        }

        private void Authorize(RequestContext req)
        {
            if (!req.Authentication.IsAdmin)
            {
                if (!req.TenantGUID.Equals(req.Authentication.TenantGUID))
                {
                    _Logging.Warn(_Header + "attempt to access tenant " + req.TenantGUID + " from tenant " + req.Authentication.TenantGUID);
                    req.Authorization.Result = AuthorizationResultEnum.Denied;
                    return;
                }
            }

            req.Authorization.Result = AuthorizationResultEnum.Permitted;
            return;
        }

        #endregion
    }
}
