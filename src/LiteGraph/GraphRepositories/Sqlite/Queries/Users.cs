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

    internal static class Users
    {
        internal static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static SerializationHelper Serializer = new SerializationHelper();

        #region Users

        internal static string InsertUserQuery(UserMaster user)
        {
            string ret =
                "INSERT INTO 'users' "
                + "VALUES ("
                + "'" + user.GUID + "',"
                + "'" + user.TenantGUID + "',"
                + "'" + Sanitizer.Sanitize(user.FirstName) + "',"
                + "'" + Sanitizer.Sanitize(user.LastName) + "',"
                + "'" + Sanitizer.Sanitize(user.Email) + "',"
                + "'" + Sanitizer.Sanitize(user.Password) + "',"
                + (user.Active ? "1" : "0") + ","
                + "'" + Sanitizer.Sanitize(user.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitizer.Sanitize(user.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        internal static string SelectUserQuery(Guid tenantGuid, string email)
        {
            return "SELECT * FROM 'users' WHERE tenantguid = '" + tenantGuid + "' AND email = '" + Sanitizer.Sanitize(email) + "';";
        }

        internal static string SelectUserQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'users' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string SelectUsersQuery(
            Guid? tenantGuid,
            string email,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'users' WHERE guid IS NOT NULL ";

            if (tenantGuid != null)
                ret += "AND tenantguid = '" + tenantGuid.Value.ToString() + "' ";

            if (!String.IsNullOrEmpty(email))
                ret += "AND email = '" + Sanitizer.Sanitize(email) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string UpdateUserQuery(UserMaster user)
        {
            return
                "UPDATE 'users' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "firstname = '" + Sanitizer.Sanitize(user.FirstName) + "',"
                + "lastname = '" + Sanitizer.Sanitize(user.LastName) + "',"
                + "email = '" + Sanitizer.Sanitize(user.Email) + "',"
                + "password = '" + Sanitizer.Sanitize(user.Password) + "',"
                + "active = " + (user.Active ? "1" : "0") + " "
                + "WHERE guid = '" + user.GUID + "' "
                + "RETURNING *;";
        }

        internal static string DeleteUserQuery(string name)
        {
            return "DELETE FROM 'users' WHERE name = '" + Sanitizer.Sanitize(name) + "';";
        }

        internal static string DeleteUserQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'users' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string DeleteUserCredentialsQuery(Guid guid)
        {
            return "DELETE FROM 'creds' WHERE userguid = '" + guid + "';";
        }

        #endregion
    }
}
