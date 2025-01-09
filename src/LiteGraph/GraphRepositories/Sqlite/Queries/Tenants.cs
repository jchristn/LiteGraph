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

    internal static class Tenants
    {
        internal static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static SerializationHelper Serializer = new SerializationHelper();

        #region Tenants

        internal static string InsertTenantQuery(TenantMetadata tenant)
        {
            string ret =
                "INSERT INTO 'tenants' "
                + "VALUES ("
                + "'" + tenant.GUID + "',"
                + "'" + Sanitizer.Sanitize(tenant.Name) + "',"
                + (tenant.Active ? "1" : "0") + ","
                + "'" + Sanitizer.Sanitize(tenant.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitizer.Sanitize(tenant.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        internal static string SelectTenantQuery(string name)
        {
            return "SELECT * FROM 'tenants' WHERE name = '" + Sanitizer.Sanitize(name) + "';";
        }

        internal static string SelectTenantQuery(Guid guid)
        {
            return "SELECT * FROM 'tenants' WHERE guid = '" + guid.ToString() + "';";
        }

        internal static string SelectTenantsQuery(
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'tenants' WHERE guid IS NOT NULL "
                + "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string UpdateTenantQuery(TenantMetadata tenant)
        {
            return
                "UPDATE 'tenants' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitizer.Sanitize(tenant.Name) + "',"
                + "active = " + (tenant.Active ? "1" : "0") + " "
                + "WHERE guid = '" + tenant.GUID + "' "
                + "RETURNING *;";
        }

        internal static string DeleteTenantQuery(Guid guid)
        {
            return "DELETE FROM 'tenants' WHERE guid = '" + guid + "';";
        }

        internal static string DeleteTenantUsersQuery(Guid guid)
        {
            return "DELETE FROM 'users' WHERE tenantguid = '" + guid + "';";
        }

        internal static string DeleteTenantCredentialsQuery(Guid guid)
        {
            return "DELETE FROM 'creds' WHERE tenantguid = '" + guid + "';";
        }

        #endregion
    }
}
