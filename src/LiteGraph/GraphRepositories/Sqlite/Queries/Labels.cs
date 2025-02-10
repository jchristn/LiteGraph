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

    internal static class Labels
    {
        internal static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static SerializationHelper Serializer = new SerializationHelper();

        #region Labels

        internal static string InsertLabelQuery(LabelMetadata label)
        {
            string ret =
                "INSERT INTO 'labels' "
                + "VALUES ("
                + "'" + label.GUID + "',"
                + "'" + label.TenantGUID + "',"
                + "'" + label.GraphGUID + "',"
                + (label.NodeGUID != null ? "'" + label.NodeGUID.Value + "'" : "NULL") + ","
                + (label.EdgeGUID != null ? "'" + label.EdgeGUID.Value + "'" : "NULL") + ","
                + "'" + Sanitizer.Sanitize(label.Label) + "',"
                + "'" + Sanitizer.Sanitize(label.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitizer.Sanitize(label.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        internal static string InsertMultipleLabelsQuery(Guid tenantGuid, Guid graphGuid, List<LabelMetadata> labels)
        {
            string ret =
                "INSERT INTO 'labels' "
                + "VALUES ";

            for (int i = 0; i < labels.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "(";
                ret += "'" + labels[i].GUID + "',"
                    + "'" + tenantGuid + "',"
                    + "'" + labels[i].GraphGUID + "',"
                    + (labels[i].NodeGUID != null ? "'" + labels[i].NodeGUID.Value + "'," : "NULL,")
                    + (labels[i].EdgeGUID != null ? "'" + labels[i].EdgeGUID.Value + "'," : "NULL,")
                    + "'" + Sanitizer.Sanitize(labels[i].Label) + "',"
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "'";
                ret += ")";
            }

            ret += ";";
            return ret;
        }

        internal static string SelectMultipleLabelsQuery(Guid tenantGuid, List<Guid> guids)
        {
            string ret = "SELECT * FROM 'labels' WHERE tenantguid = '" + tenantGuid + "' AND guid IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitizer.Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        internal static string SelectLabelQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'labels' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string SelectTenantLabelsQuery(
            Guid tenantGuid,
            string label,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'labels' WHERE guid IS NOT NULL " +
                "AND tenantguid = '" + tenantGuid.ToString() + "' ";

            if (!String.IsNullOrEmpty(label))
                ret += "AND label = '" + Sanitizer.Sanitize(label) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectGraphLabelsQuery(
            Guid tenantGuid,
            Guid graphGuid,
            string label,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'labels' WHERE guid IS NOT NULL " +
                "AND tenantguid = '" + tenantGuid.ToString() + "' " +
                "AND graphguid = '" + graphGuid.ToString() + "' " +
                "AND nodeguid IS NULL " +
                "AND edgeguid IS NULL ";

            if (!String.IsNullOrEmpty(label))
                ret += "AND label = '" + Sanitizer.Sanitize(label) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectNodeLabelsQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            string label,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'labels' WHERE guid IS NOT NULL " +
                "AND tenantguid = '" + tenantGuid.ToString() + "' " +
                "AND graphguid = '" + graphGuid.ToString() + "' " +
                "AND nodeguid = '" + nodeGuid.ToString() + "' " +
                "AND edgeguid IS NULL ";

            if (!String.IsNullOrEmpty(label))
                ret += "AND label = '" + Sanitizer.Sanitize(label) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectEdgeLabelsQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid edgeGuid,
            string label,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'labels' WHERE guid IS NOT NULL " +
                "AND tenantguid = '" + tenantGuid.ToString() + "' " +
                "AND graphguid = '" + graphGuid.ToString() + "' " +
                "AND edgeguid = '" + edgeGuid.ToString() + "' " +
                "AND nodeguid IS NULL ";

            if (!String.IsNullOrEmpty(label))
                ret += "AND label = '" + Sanitizer.Sanitize(label) + "' ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string UpdateLabelQuery(LabelMetadata label)
        {
            return
                "UPDATE 'labels' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "nodeguid = " + (label.NodeGUID != null ? ("'" + label.NodeGUID.Value + "'") : "NULL") + ","
                + "edgeguid = " + (label.EdgeGUID != null ? ("'" + label.EdgeGUID.Value + "'") : "NULL") + ","
                + "label = '" + Sanitizer.Sanitize(label.Label) + "',"
                + "WHERE guid = '" + label.GUID + "' "
                + "RETURNING *;";
        }

        internal static string DeleteLabelQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'labels' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string DeleteLabelsQuery(Guid tenantGuid, Guid? graphGuid, List<Guid> nodeGuids, List<Guid> edgeGuids)
        {
            string ret = "DELETE FROM 'labels' WHERE tenantguid = '" + tenantGuid + "' ";

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

        internal static string DeleteAllTenantLabelsQuery(Guid tenantGuid)
        {
            string ret =
                "DELETE FROM 'labels' WHERE " +
                "tenantguid = '" + tenantGuid + "';";
            return ret;
        }

        internal static string DeleteAllGraphLabelsQuery(Guid tenantGuid, Guid graphGuid)
        {
            string ret =
                "DELETE FROM 'labels' WHERE " +
                "tenantguid = '" + tenantGuid + "' " +
                "AND graphguid = '" + graphGuid + "';";
            return ret;
        }

        internal static string DeleteGraphLabelsQuery(Guid tenantGuid, Guid graphGuid)
        {
            string ret =
                "DELETE FROM 'labels' WHERE " +
                "tenantguid = '" + tenantGuid + "' " +
                "AND graphguid = '" + graphGuid + "' " +
                "AND nodeguid IS NULL " +
                "AND edgeguid IS NULL;";
            return ret;
        }

        internal static string DeleteNodeLabelsQuery(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            string ret =
                "DELETE FROM 'labels' WHERE " +
                "tenantguid = '" + tenantGuid + "' " +
                "AND graphguid = '" + graphGuid + "' " +
                "AND nodeguid = '" + nodeGuid + "' " +
                "AND edgeguid IS NULL;";
            return ret;
        }

        internal static string DeleteEdgeLabelsQuery(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            string ret =
                "DELETE FROM 'labels' WHERE " +
                "tenantguid = '" + tenantGuid + "' " +
                "AND graphguid = '" + graphGuid + "' " +
                "AND nodeguid IS NULL " +
                "AND edgeguid = '" + edgeGuid + "';";
            return ret;
        }

        #endregion
    }
}
