﻿namespace LiteGraph.Server.Services
{
    using LiteGraph.GraphRepositories;
    using LiteGraph.Serialization;
    using LiteGraph.Server.Classes;
    using SyslogLogging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
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

        /// <summary>
        /// Generate a token for a user.
        /// </summary>
        /// <param name="tenantGuid">Tenant GUID.</param>
        /// <param name="userGuid">User GUID.</param>
        /// <returns>Authentication token.</returns>
        internal AuthenticationToken GenerateToken(Guid tenantGuid, Guid userGuid)
        {
            AuthenticationToken token = new AuthenticationToken
            {
                TenantGUID = tenantGuid,
                UserGUID = userGuid
            };

            token.Token = GenerateSecurityTokenString(token);
            token.Random = null;
            return token;
        }

        /// <summary>
        /// Read a token's details.
        /// </summary>
        /// <param name="token">Authentication token string.</param>
        /// <returns>Authentication token.</returns>
        internal AuthenticationToken ReadToken(string token)
        {
            if (String.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            AuthenticationToken authToken = ParseSecurityTokenString(token);
            authToken.Random = null;
            return authToken;
        }


        #endregion

        #region Private-Methods

        private void Authenticate(RequestContext req)
        {
            if (!String.IsNullOrEmpty(req.Authentication.Email)
                && !String.IsNullOrEmpty(req.Authentication.Password)
                && req.Authentication.TenantGUID != null)
            {
                #region Credential-Authentication

                req.Authentication.User = _Repository.ReadUserByEmail(req.Authentication.TenantGUID.Value, req.Authentication.Email);
                if (req.Authentication.User == null)
                {
                    _Logging.Warn(_Header + "user with email " + req.Authentication.Email + " not found");
                    req.Authentication.Result = AuthenticationResultEnum.NotFound;
                    return;
                }
                else
                {
                    req.Authentication.UserGUID = req.Authentication.User.GUID;
                    req.Authentication.TenantGUID = req.Authentication.User.TenantGUID;
                }

                if (!req.Authentication.User.Active)
                {
                    _Logging.Warn(_Header + "user " + req.Authentication.UserGUID + " is inactive");
                    req.Authentication.Result = AuthenticationResultEnum.Inactive;
                    return;
                }

                if (!req.Authentication.Password.Equals(req.Authentication.User.Password))
                {
                    _Logging.Warn(_Header + "invalid password supplied for user " + req.Authentication.UserGUID);
                    req.Authentication.Result = AuthenticationResultEnum.Invalid;
                    return;
                }

                req.Authentication.Tenant = _Repository.ReadTenant(req.Authentication.TenantGUID.Value);
                if (req.Authentication.Tenant == null)
                {
                    _Logging.Warn(_Header + "tenant " + req.Authentication.TenantGUID + " referenced by user " + req.Authentication.UserGUID + " not found");
                    req.Authentication.Result = AuthenticationResultEnum.NotFound;
                    return;
                }
                else
                {
                    req.Authentication.TenantGUID = req.Authentication.Tenant.GUID;
                }

                if (!req.Authentication.Tenant.Active)
                {
                    _Logging.Warn(_Header + "tenant " + req.Authentication.TenantGUID + " referenced by user " + req.Authentication.UserGUID + " is inactive");
                    req.Authentication.Result = AuthenticationResultEnum.Inactive;
                    return;
                }

                req.Authentication.Result = AuthenticationResultEnum.Success;
                return;

                #endregion
            }
            else if (!String.IsNullOrEmpty(req.Authentication.BearerToken))
            {
                #region Bearer-Token-Authentication

                if (req.Authentication.BearerToken.Equals(_Settings.LiteGraph.AdminBearerToken))
                {
                    #region LiteGraph-Admin

                    _Logging.Info(_Header + "admin bearer token in use from " + req.Ip);
                    req.Authentication.IsAdmin = true;
                    req.Authentication.Result = AuthenticationResultEnum.Success;
                    return;

                    #endregion
                }
                else
                {
                    #region User

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

                    req.Authentication.Result = AuthenticationResultEnum.Success;
                    return;

                    #endregion
                }

                #endregion
            }
            else if (!String.IsNullOrEmpty(req.Authentication.SecurityToken))
            {
                #region Security-Token-Authentication

                AuthenticationToken token = null;

                try
                {
                    token = ParseSecurityTokenString(req.Authentication.SecurityToken);
                }
                catch (Exception)
                {
                    _Logging.Warn(_Header + "malformed security token received");
                    req.Authentication.Result = AuthenticationResultEnum.Invalid;
                    return;
                }

                if (token.IsExpired)
                {
                    _Logging.Warn(_Header + "expired security token received");
                    req.Authentication.Result = AuthenticationResultEnum.Invalid;
                    return;
                }

                req.Authentication.User = _Repository.ReadUser(token.TenantGUID.Value, token.UserGUID.Value);
                if (req.Authentication.User == null)
                {
                    _Logging.Warn(_Header + "user " + token.UserGUID + " not found");
                    req.Authentication.Result = AuthenticationResultEnum.NotFound;
                    return;
                }
                else
                {
                    req.Authentication.UserGUID = req.Authentication.User.GUID;
                }

                if (!req.Authentication.User.Active)
                {
                    _Logging.Warn(_Header + "user " + token.UserGUID + " is inactive");
                    req.Authentication.Result = AuthenticationResultEnum.Inactive;
                    return;
                }

                req.Authentication.Tenant = _Repository.ReadTenant(req.Authentication.User.TenantGUID);
                if (req.Authentication.Tenant == null)
                {
                    _Logging.Warn(_Header + "tenant " + req.Authentication.User.TenantGUID + " not found");
                    req.Authentication.Result = AuthenticationResultEnum.NotFound;
                    return;
                }
                else
                {
                    req.Authentication.TenantGUID = req.Authentication.Tenant.GUID;
                }

                if (!req.Authentication.Tenant.Active)
                {
                    _Logging.Warn(_Header + "tenant " + req.Authentication.Tenant.GUID + " is inactive");
                    req.Authentication.Result = AuthenticationResultEnum.Inactive;
                    return;
                }

                req.Authentication.Result = AuthenticationResultEnum.Success;
                return;

                #endregion
            }
            else
            {
                _Logging.Warn(_Header + "no authentication material supplied from " + req.Http.Request.Source.IpAddress);
                req.Authentication.Result = AuthenticationResultEnum.NotFound;
                return;
            }
        }

        private void Authorize(RequestContext req)
        {
            if (!req.Authentication.IsAdmin)
            {
                if (req.TenantGUID != null)
                {
                    if (!req.TenantGUID.Equals(req.Authentication.TenantGUID))
                    {
                        _Logging.Warn(_Header + "attempt to access tenant " + req.TenantGUID + " from tenant " + req.Authentication.TenantGUID);
                        req.Authorization.Result = AuthorizationResultEnum.Denied;
                        return;
                    }
                }
            }

            req.Authorization.Result = AuthorizationResultEnum.Permitted;
            return;
        }

        private string GenerateSecurityTokenString(AuthenticationToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            string json = _Serializer.SerializeJson(token, false);
            byte[] clear = Encoding.UTF8.GetBytes(json);
            byte[] cipher = Encrypt(clear);
            return Convert.ToBase64String(cipher);
        }

        private AuthenticationToken ParseSecurityTokenString(string token)
        {
            if (String.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            byte[] cipher = Convert.FromBase64String(token);
            byte[] clear = Decrypt(cipher);
            string json = Encoding.UTF8.GetString(clear);
            return _Serializer.DeserializeJson<AuthenticationToken>(json);
        }

        private byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data));

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromHexString(_Settings.Encryption.Key);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = Convert.FromHexString(_Settings.Encryption.Iv);

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(data, 0, data.Length);
                            csEncrypt.FlushFinalBlock();
                        }

                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        private byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length == 0)
                throw new ArgumentNullException(nameof(encryptedData));

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromHexString(_Settings.Encryption.Key);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = Convert.FromHexString(_Settings.Encryption.Iv);

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    using (MemoryStream msDecrypt = new MemoryStream())
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(
                            new MemoryStream(encryptedData),
                            decryptor,
                            CryptoStreamMode.Read))
                        {
                            byte[] buffer = new byte[1024];
                            int count;
                            while ((count = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msDecrypt.Write(buffer, 0, count);
                            }
                        }

                        return msDecrypt.ToArray();
                    }
                }
            }
        }

        #endregion
    }
}
