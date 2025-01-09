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

    internal static class Nodes
    {
        internal static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static SerializationHelper Serializer = new SerializationHelper();

        #region Nodes

        internal static string InsertNodeQuery(Node node)
        {
            string ret =
                "INSERT INTO 'nodes' "
                + "VALUES ("
                + "'" + node.GUID + "',"
                + "'" + node.TenantGUID + "',"
                + "'" + node.GraphGUID + "',"
                + "'" + Sanitizer.Sanitize(node.Name) + "',";

            if (node.Data == null) ret += "null,";
            else ret += "'" + Sanitizer.Sanitize(Serializer.SerializeJson(node.Data, false)) + "',";

            ret +=
                "'" + Sanitizer.Sanitize(node.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitizer.Sanitize(node.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        internal static string InsertMultipleNodesQuery(Guid tenantGuid, List<Node> nodes)
        {
            string ret =
                "INSERT INTO 'nodes' "
                + "VALUES ";

            for (int i = 0; i < nodes.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "(";
                ret += "'" + nodes[i].GUID + "',"
                    + "'" + tenantGuid + "',"
                    + "'" + nodes[i].GraphGUID + "',"
                    + "'" + Sanitizer.Sanitize(nodes[i].Name) + "',";

                if (nodes[i].Data == null) ret += "null,";
                else ret += "'" + Sanitizer.Sanitize(Serializer.SerializeJson(nodes[i].Data, false)) + "',";
                ret +=
                    "'" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "'"
                    + ")";
            }

            ret += ";";
            return ret;
        }

        internal static string SelectMultipleNodesQuery(Guid tenantGuid, List<Guid> guids)
        {
            string ret = "SELECT * FROM 'nodes' WHERE tenantguid = '" + tenantGuid + "' AND guid IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitizer.Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        internal static string SelectNodeQuery(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            return "SELECT * FROM 'nodes' WHERE "
                + "guid = '" + nodeGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND graphguid = '" + graphGuid + "';";
        }

        internal static string SelectNodesQuery(
            Guid tenantGuid,
            Guid graphGuid,
            List<string> labels,
            NameValueCollection tags,
            Expr nodeFilter = null,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'nodes' ";

            if (labels != null && labels.Count > 0)
                ret += "INNER JOIN 'labels' "
                    + "ON nodes.guid = labels.nodeguid "
                    + "AND nodes.graphguid = labels.graphguid "
                    + "AND nodes.tenantguid = labels.tenantguid ";

            if (tags != null && tags.Count > 0)
            {
                int added = 1;
                foreach (string key in tags.AllKeys)
                {
                    ret +=
                        "INNER JOIN 'tags' t" + added.ToString() + " " +
                        "ON nodes.guid = t" + added.ToString() + ".nodeguid " +
                        "AND nodes.graphguid = t" + added.ToString() + ".graphguid " +
                        "AND nodes.tenantguid = t" + added.ToString() + ".tenantguid ";
                    added++;
                }
            }

            ret += "WHERE "
                + "nodes.tenantguid = '" + tenantGuid + "' "
                + "AND nodes.graphguid = '" + graphGuid + "' ";

            if (labels != null && labels.Count > 0)
            {
                string labelList = "(";

                int labelsAdded = 0;
                foreach (string label in labels)
                {
                    if (labelsAdded > 0) labelList += ",";
                    labelList += "'" + Sanitizer.Sanitize(label) + "'";
                    labelsAdded++;
                }

                labelList += ")";

                ret += "AND labels.label IN " + labelList + " ";
            }

            if (tags != null && tags.Count > 0)
            {
                int added = 1;
                foreach (string key in tags.AllKeys)
                {
                    string val = tags.Get(key);
                    ret += "AND t" + added.ToString() + ".tagkey = '" + Sanitizer.Sanitize(key) + "' ";
                    if (!String.IsNullOrEmpty(val)) ret += "AND t" + added.ToString() + ".tagvalue = '" + Sanitizer.Sanitize(val) + "' ";
                    else ret += "AND t" + added.ToString() + ".tagvalue IS NULL ";
                    added++;
                }
            }

            if (nodeFilter != null)
            {
                string filterClause = Converters.ExpressionToWhereClause("nodes", nodeFilter);
                if (!String.IsNullOrEmpty(filterClause)) ret += "AND " + filterClause;
            }

            if (labels != null && labels.Count > 0)
            {
                ret += "GROUP BY nodes.guid ";

                int labelsAdded = 0;
                ret += "HAVING ";
                foreach (string label in labels)
                {
                    if (labelsAdded > 0) ret += "AND ";
                    ret += "SUM(CASE WHEN labels.label = '" + Sanitizer.Sanitize(label) + "' THEN 1 ELSE 0 END) > 0 ";
                    labelsAdded++;
                }
            }

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string UpdateNodeQuery(Node node)
        {
            string ret =
                "UPDATE 'nodes' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitizer.Sanitize(node.Name) + "',";

            if (node.Data == null) ret += "data = null ";
            else ret += "data = '" + Sanitizer.Sanitize(Serializer.SerializeJson(node.Data, false)) + "' ";

            ret +=
                "WHERE guid = '" + node.GUID + "' "
                + "RETURNING *;";

            return ret;
        }

        internal static string DeleteNodeQuery(Guid tenantGuid, Guid graphGuid, Guid nodeGuid)
        {
            return
                "DELETE FROM 'nodes' WHERE "
                + "guid = '" + nodeGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND graphguid = '" + graphGuid + "';";
        }

        internal static string DeleteNodesQuery(Guid tenantGuid, Guid graphGuid)
        {
            return
                "DELETE FROM 'nodes' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + graphGuid + "';";
        }

        internal static string DeleteNodesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids)
        {
            string ret =
                "DELETE FROM 'nodes' WHERE graphguid = '" + graphGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND guid IN (";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitizer.Sanitize(nodeGuids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        internal static string DeleteNodeEdgesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids)
        {
            string ret =
                "DELETE FROM 'edges' WHERE graphguid = '" + graphGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND (";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) ret += "OR ";
                ret += "(fromguid = '" + Sanitizer.Sanitize(nodeGuids[i].ToString()) + "' OR toguid = '" + Sanitizer.Sanitize(nodeGuids[i].ToString()) + "')";
            }

            ret += ")";
            return ret;
        }

        internal static string BatchExistsNodesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> nodeGuids)
        {
            string query = "WITH temp(guid) AS (VALUES ";

            for (int i = 0; i < nodeGuids.Count; i++)
            {
                if (i > 0) query += ",";
                query += "('" + nodeGuids[i].ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.guid, CASE WHEN nodes.guid IS NOT NULL THEN 1 ELSE 0 END as \"exists\" "
                + "FROM temp "
                + "LEFT JOIN nodes ON temp.guid = nodes.guid "
                + "AND nodes.graphguid = '" + graphGuid + "' "
                + "AND nodes.tenantguid = '" + tenantGuid + "';";

            return query;
        }

        #endregion
    }
}
