namespace LiteGraph.GraphRepositories.Sqlite.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ExpressionTree;
    using LiteGraph.Serialization;

    internal static class Credentials
    {
        internal static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static SerializationHelper Serializer = new SerializationHelper();

        #region Credentials

        internal static string InsertCredentialQuery(Credential cred)
        {
            string ret =
                "INSERT INTO 'creds' "
                + "VALUES ("
                + "'" + cred.GUID + "',"
                + "'" + cred.TenantGUID + "',"
                + "'" + cred.UserGUID + "',"
                + "'" + Sanitizer.Sanitize(cred.Name) + "',"
                + "'" + Sanitizer.Sanitize(cred.BearerToken) + "',"
                + (cred.Active ? "1" : "0") + ","
                + "'" + Sanitizer.Sanitize(cred.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitizer.Sanitize(cred.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        internal static string SelectCredentialQuery(string bearerToken)
        {
            return "SELECT * FROM 'creds' WHERE bearertoken = '" + Sanitizer.Sanitize(bearerToken) + "';";
        }

        internal static string SelectCredentialQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'creds' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string SelectCredentialsQuery(
            Guid? tenantGuid,
            Guid? userGuid,
            string bearerToken,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'creds' WHERE guid IS NOT NULL ";

            if (tenantGuid != null)
                ret += "AND tenantguid = '" + tenantGuid.Value.ToString() + "' ";

            if (userGuid != null)
                ret += "AND userGuid = '" + userGuid.Value.ToString() + "' ";

            if (!String.IsNullOrEmpty(bearerToken))
                ret += "AND bearertoken = '" + Sanitizer.Sanitize(bearerToken) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string UpdateCredentialQuery(Credential cred)
        {
            return
                "UPDATE 'creds' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitizer.Sanitize(cred.Name) + "',"
                + "active = " + (cred.Active ? "1" : "0") + " "
                + "WHERE guid = '" + cred.GUID + "' "
                + "RETURNING *;";
        }

        internal static string DeleteCredentialQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'creds' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        #endregion
    }
}
