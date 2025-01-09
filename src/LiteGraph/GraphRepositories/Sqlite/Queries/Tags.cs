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

    internal static class Tags
    {
        internal static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static SerializationHelper Serializer = new SerializationHelper();

        #region Tags

        internal static string InsertTagQuery(TagMetadata tag)
        {
            string ret =
                "INSERT INTO 'tags' "
                + "VALUES ("
                + "'" + tag.GUID + "',"
                + "'" + tag.TenantGUID + "',"
                + (tag.GraphGUID != null ? "'" + tag.GraphGUID.Value + "'" : "NULL") + ","
                + (tag.NodeGUID != null ? "'" + tag.NodeGUID.Value + "'" : "NULL") + ","
                + (tag.EdgeGUID != null ? "'" + tag.EdgeGUID.Value + "'" : "NULL") + ","
                + "'" + Sanitizer.Sanitize(tag.Key) + "',"
                + (!String.IsNullOrEmpty(tag.Value) ? ("'" + Sanitizer.Sanitize(tag.Value) + "'") : "NULL") + ","
                + "'" + Sanitizer.Sanitize(tag.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitizer.Sanitize(tag.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        internal static string InsertMultipleTagsQuery(Guid tenantGuid, Guid graphGuid, List<TagMetadata> tags)
        {
            string ret =
                "INSERT INTO 'tags' "
                + "VALUES ";

            for (int i = 0; i < tags.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "(";
                ret += "'" + tags[i].GUID + "',"
                    + "'" + tenantGuid + "',"
                    + "'" + tags[i].GraphGUID + "',"
                    + (tags[i].NodeGUID != null ? "'" + tags[i].NodeGUID.Value + "'," : "NULL,")
                    + (tags[i].EdgeGUID != null ? "'" + tags[i].EdgeGUID.Value + "'," : "NULL,")
                    + "'" + Sanitizer.Sanitize(tags[i].Key) + "',"
                    + (!String.IsNullOrEmpty(tags[i].Value) ? "'" + Sanitizer.Sanitize(tags[i].Value) + "'," : "NULL,")
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "'";
                ret += ")";
            }

            ret += ";";
            return ret;
        }

        internal static string SelectMultipleTagsQuery(Guid tenantGuid, List<Guid> guids)
        {
            string ret = "SELECT * FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' AND guid IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitizer.Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        internal static string SelectTagQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string SelectTenantTagsQuery(
            Guid tenantGuid,
            string key,
            string val,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'tags' WHERE guid IS NOT NULL " +
                "AND tenantguid = '" + tenantGuid.ToString() + "' ";

            if (!String.IsNullOrEmpty(key))
                ret += "AND tagkey = '" + Sanitizer.Sanitize(key) + "' ";

            if (!String.IsNullOrEmpty(val))
                ret += "AND tagvalue = '" + Sanitizer.Sanitize(val) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectGraphTagsQuery(
            Guid tenantGuid,
            Guid graphGuid,
            string key,
            string val,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'tags' WHERE guid IS NOT NULL " +
                "AND tenantguid = '" + tenantGuid.ToString() + "' " +
                "AND graphguid = '" + graphGuid.ToString() + "' " +
                "AND nodeguid IS NULL " +
                "AND edgeguid IS NULL ";

            if (!String.IsNullOrEmpty(key))
                ret += "AND tagkey = '" + Sanitizer.Sanitize(key) + "' ";

            if (!String.IsNullOrEmpty(val))
                ret += "AND tagvalue = '" + Sanitizer.Sanitize(val) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectNodeTagsQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            string key,
            string val,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'tags' WHERE guid IS NOT NULL " +
                "AND tenantguid = '" + tenantGuid.ToString() + "' " +
                "AND graphguid = '" + graphGuid.ToString() + "' " + 
                "AND nodeguid = '" + nodeGuid.ToString() + "' " + 
                "AND edgeguid IS NULL ";

            if (!String.IsNullOrEmpty(key))
                ret += "AND tagkey = '" + Sanitizer.Sanitize(key) + "' ";

            if (!String.IsNullOrEmpty(val))
                ret += "AND tagvalue = '" + Sanitizer.Sanitize(val) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectEdgeTagsQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid edgeGuid,
            string key,
            string val,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'tags' WHERE guid IS NOT NULL " +
                "AND tenantguid = '" + tenantGuid.ToString() + "' " + 
                "AND graphguid = '" + graphGuid.ToString() + "' " + 
                "AND nodeguid IS NULL " + 
                "AND edgeguid = '" + edgeGuid.ToString() + "' ";

            if (!String.IsNullOrEmpty(key))
                ret += "AND tagkey = '" + Sanitizer.Sanitize(key) + "' ";

            if (!String.IsNullOrEmpty(val))
                ret += "AND tagvalue = '" + Sanitizer.Sanitize(val) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string UpdateTagQuery(TagMetadata tag)
        {
            return
                "UPDATE 'tags' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "nodeguid = " + (tag.NodeGUID != null ? ("'" + tag.NodeGUID.Value + "'") : "NULL") + ","
                + "edgeguid = " + (tag.EdgeGUID != null ? ("'" + tag.EdgeGUID.Value + "'") : "NULL") + ","
                + "tagkey = '" + Sanitizer.Sanitize(tag.Key) + "',"
                + "tagvalue = " + (!String.IsNullOrEmpty(tag.Value) ? ("'" + Sanitizer.Sanitize(tag.Value) + "'") : "NULL") + " "
                + "WHERE guid = '" + tag.GUID + "' "
                + "RETURNING *;";
        }

        internal static string DeleteTagQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string DeleteTagsQuery(Guid tenantGuid, Guid? graphGuid, List<Guid> nodeGuids, List<Guid> edgeGuids)
        {
            string ret = "DELETE FROM 'tags' WHERE tenantguid = '" + tenantGuid + "' ";

            if (graphGuid != null) ret += "AND graphguid = '" + graphGuid + "' ";

            if (nodeGuids != null && nodeGuids.Count > 0)
            {
                string nodeGuidsStr = string.Join(",", nodeGuids.Select(g => $"'{g}'"));
                ret += "AND nodeguid IN (" + nodeGuidsStr + ") ";
            }

            if (edgeGuids != null && edgeGuids.Count > 0)
            {
                string edgeGuidsStr = string.Join(",", edgeGuids.Select(g => $"'{g}'"));
                ret += "AND edgeguid IN (" + edgeGuidsStr + ") ";
            }

            return ret;
        }

        internal static string DeleteAllTenantTagsQuery(Guid tenantGuid)
        {
            string ret =
                "DELETE FROM 'tags' WHERE " +
                "tenantguid = '" + tenantGuid + "';";
            return ret;
        }

        internal static string DeleteAllGraphTagsQuery(Guid tenantGuid, Guid graphGuid)
        {
            string ret =
                "DELETE FROM 'tags' WHERE " +
                "tenantguid = '" + tenantGuid + "' " +
                "AND graphguid = '" + graphGuid + "';";
            return ret;
        }

        internal static string DeleteGraphTagsQuery(Guid tenantGuid, Guid graphGuid)
        {
            string ret =
                "DELETE FROM 'tags' WHERE " +
                "tenantguid = '" + tenantGuid + "' " +
                "AND graphguid = '" + graphGuid + "' " +
                "AND nodeguid IS NULL " +
                "AND edgeguid IS NULL;";
            return ret;
        }

        internal static string DeleteNodeTagsQuery(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            string ret =
                "DELETE FROM 'tags' WHERE " +
                "tenantguid = '" + tenantGuid + "' " +
                "AND graphguid = '" + graphGuid + "' " +
                "AND nodeguid = '" + nodeGuid + "' " +
                "AND edgeguid IS NULL;";
            return ret;
        }

        internal static string DeleteEdgeTagsQuery(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            string ret =
                "DELETE FROM 'tags' WHERE " +
                "tenantguid = '" + tenantGuid + "' " +
                "AND graphguid = '" + graphGuid + "' " +
                "AND nodeguid IS NULL " +
                "AND edgeguid = '" + edgeGuid + "';";
            return ret;
        }

        #endregion
    }
}
