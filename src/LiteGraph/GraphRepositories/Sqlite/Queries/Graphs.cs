namespace LiteGraph.GraphRepositories.Sqlite.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading.Tasks;
    using ExpressionTree;
    using LiteGraph.Serialization;

    internal static class Graphs
    {
        internal static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static SerializationHelper Serializer = new SerializationHelper();

        #region Graphs

        internal static string InsertGraphQuery(Graph graph)
        {
            string ret =
                "INSERT INTO 'graphs' "
                + "VALUES ("
                + "'" + graph.GUID + "',"
                + "'" + graph.TenantGUID + "',"
                + "'" + Sanitizer.Sanitize(graph.Name) + "',";

            if (graph.Data == null) ret += "null,";
            else ret += "'" + Sanitizer.Sanitize(Serializer.SerializeJson(graph.Data, false)) + "',";

            ret +=
                "'" + Sanitizer.Sanitize(graph.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitizer.Sanitize(graph.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        internal static string SelectGraphQuery(Guid tenantGuid, Guid guid)
        {
            return "SELECT * FROM 'graphs' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string SelectGraphsQuery(
            Guid tenantGuid,
            List<string> labels,
            NameValueCollection tags,
            Expr graphFilter = null,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'graphs' ";

            if (labels != null && labels.Count > 0)
                ret += "INNER JOIN 'labels' "
                    + "ON graphs.guid = labels.graphguid "
                    + "AND graphs.tenantguid = labels.tenantguid ";

            if (tags != null && tags.Count > 0)
            {
                int added = 1;
                foreach (string key in tags.AllKeys)
                {
                    ret +=
                        "INNER JOIN 'tags' t" + added.ToString() + " " +
                        "ON graphs.guid = t" + added.ToString() + ".graphguid " +
                        "AND graphs.tenantguid = t" + added.ToString() + ".tenantguid ";
                    added++;
                }
            }

            ret += "WHERE graphs.tenantguid = '" + tenantGuid + "' ";

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

            if (graphFilter != null)
            {
                string filterClause = Converters.ExpressionToWhereClause("graphs", graphFilter);
                if (!String.IsNullOrEmpty(filterClause)) ret += "AND " + filterClause;
            }

            if (labels != null && labels.Count > 0)
            {
                ret += "GROUP BY graphs.guid ";

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

        internal static string UpdateGraphQuery(Graph graph)
        {
            string ret =
                "UPDATE 'graphs' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitizer.Sanitize(graph.Name) + "',";

            if (graph.Data == null) ret += "data = null ";
            else ret += "data = '" + Sanitizer.Sanitize(Serializer.SerializeJson(graph.Data, false)) + "' ";

            ret +=
                "WHERE guid = '" + graph.GUID + "' "
                + "AND tenantguid = '" + graph.TenantGUID + "' "
                + "RETURNING *;";

            return ret;
        }

        internal static string DeleteGraphQuery(Guid tenantGuid, string name)
        {
            return "DELETE FROM 'graphs' WHERE tenantguid = '" + tenantGuid + "' AND name = '" + Sanitizer.Sanitize(name) + "';";
        }

        internal static string DeleteGraphQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'graphs' WHERE tenantguid = '" + tenantGuid + "' AND guid = '" + guid + "';";
        }

        internal static string DeleteGraphEdgesQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'edges' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + guid + "';";
        }

        internal static string DeleteGraphNodesQuery(Guid tenantGuid, Guid guid)
        {
            return "DELETE FROM 'nodes' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + guid + "';";
        }

        #endregion
    }
}
