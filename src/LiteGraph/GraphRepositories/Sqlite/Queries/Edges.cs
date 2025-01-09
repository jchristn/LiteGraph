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

    internal static class Edges
    {
        internal static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static SerializationHelper Serializer = new SerializationHelper();

        #region Edges

        internal static string InsertEdgeQuery(Edge edge)
        {
            string ret =
                "INSERT INTO 'edges' "
                + "VALUES ("
                + "'" + edge.GUID + "',"
                + "'" + edge.TenantGUID + "',"
                + "'" + edge.GraphGUID + "',"
                + "'" + Sanitizer.Sanitize(edge.Name) + "',"
                + "'" + edge.From.ToString() + "',"
                + "'" + edge.To.ToString() + "',"
                + "'" + edge.Cost.ToString() + "',";

            if (edge.Data == null) ret += "null,";
            else ret += "'" + Sanitizer.Sanitize(Serializer.SerializeJson(edge.Data, false)) + "',";

            ret +=
                "'" + Sanitizer.Sanitize(edge.CreatedUtc.ToString(TimestampFormat)) + "',"
                + "'" + Sanitizer.Sanitize(edge.LastUpdateUtc.ToString(TimestampFormat)) + "'"
                + ") "
                + "RETURNING *;";

            return ret;
        }

        internal static string InsertMultipleEdgesQuery(Guid tenantGuid, List<Edge> edges)
        {
            string ret =
                "INSERT INTO 'edges' "
                + "VALUES ";

            for (int i = 0; i < edges.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "("
                    + "'" + edges[i].GUID + "',"
                    + "'" + tenantGuid + ","
                    + "'" + edges[i].GraphGUID + "',"
                    + "'" + Sanitizer.Sanitize(edges[i].Name) + "',"
                    + "'" + edges[i].From.ToString() + "',"
                    + "'" + edges[i].To.ToString() + "',"
                    + "'" + edges[i].Cost.ToString() + "',";

                if (edges[i].Data == null) ret += "null,";
                else ret += "'" + Sanitizer.Sanitize(Serializer.SerializeJson(edges[i].Data, false)) + "',";

                ret +=
                    "'" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                    + "'" + DateTime.UtcNow.ToString(TimestampFormat) + "'"
                    + ")";
            }

            ret += ";";
            return ret;
        }

        internal static string SelectMultipleEdgesQuery(Guid tenantGuid, List<Guid> guids)
        {
            string ret = "SELECT * FROM 'edges' WHERE tenantguid = '" + tenantGuid + "' AND guid IN (";

            for (int i = 0; i < guids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitizer.Sanitize(guids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        internal static string SelectEdgeQuery(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            return
                "SELECT * FROM 'edges' WHERE "
                + "graphguid = '" + graphGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND guid = '" + edgeGuid + "';";
        }

        internal static string SelectEdgesQuery(
            Guid tenantGuid,
            Guid graphGuid,
            List<string> labels,
            NameValueCollection tags,
            Expr edgeFilter = null,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' ";

            if (labels != null && labels.Count > 0)
                ret += "INNER JOIN 'labels' "
                    + "ON edges.guid = labels.edgeguid "
                    + "AND edges.graphguid = labels.graphguid "
                    + "AND edges.tenantguid = labels.tenantguid ";

            if (tags != null && tags.Count > 0)
            {
                int added = 1;
                foreach (string key in tags.AllKeys)
                {
                    ret +=
                        "INNER JOIN 'tags' t" + added.ToString() + " " +
                        "ON edges.guid = t" + added.ToString() + ".edgeguid " +
                        "AND edges.graphguid = t" + added.ToString() + ".graphguid " +
                        "AND edges.tenantguid = t" + added.ToString() + ".tenantguid ";
                    added++;
                }
            }

            ret += "WHERE "
                + "edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "' ";

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

            if (edgeFilter != null) ret += "AND " + Converters.ExpressionToWhereClause("edges", edgeFilter);

            if (labels != null && labels.Count > 0)
            {
                ret += "GROUP BY edges.guid ";

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

        internal static string SelectConnectedEdgesQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            List<string> labels,
            NameValueCollection tags,
            Expr edgeFilter = null,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret = "SELECT * FROM 'edges' ";

            if (labels != null && labels.Count > 0)
                ret += "INNER JOIN 'labels' "
                    + "ON edges.guid = labels.edgeguid "
                    + "AND edges.graphguid = labels.graphguid "
                    + "AND edges.tenantguid = labels.tenantguid ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.tenantguid = '" + tenantGuid + "' AND "
                + "edges.graphguid = '" + graphGuid + "' AND "
                + "("
                + "edges.fromguid = '" + nodeGuid + "' "
                + "OR edges.toguid = '" + nodeGuid + "'"
                + ") ";

            if (labels != null && labels.Count > 0)
            {
                foreach (string label in labels)
                {
                    ret += "AND labels.label = '" + Sanitizer.Sanitize(label) + "' ";
                }
            }

            if (tags != null && tags.Count > 0)
            {
                foreach (string key in tags.AllKeys)
                {
                    string val = tags.Get(key);
                    ret += "AND tags.tagkey = '" + Sanitizer.Sanitize(key) + "' ";
                    if (!String.IsNullOrEmpty(val)) ret += "AND tags.tagvalue = '" + Sanitizer.Sanitize(val) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + Converters.ExpressionToWhereClause("edges", edgeFilter);

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectEdgesFromQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            List<string> labels,
            NameValueCollection tags,
            Expr edgeFilter = null,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' ";

            if (labels != null && labels.Count > 0)
                ret += "INNER JOIN 'labels' "
                    + "ON edges.guid = labels.edgeguid "
                    + "AND edges.graphguid = labels.graphguid "
                    + "AND edges.tenantguid = labels.tenantguid ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "' "
                + "AND edges.fromguid = '" + nodeGuid + "' ";

            if (labels != null && labels.Count > 0)
            {
                foreach (string label in labels)
                {
                    ret += "AND labels.label = '" + Sanitizer.Sanitize(label) + "' ";
                }
            }

            if (tags != null && tags.Count > 0)
            {
                foreach (string key in tags.AllKeys)
                {
                    string val = tags.Get(key);
                    ret += "AND tags.tagkey = '" + Sanitizer.Sanitize(key) + "' ";
                    if (!String.IsNullOrEmpty(val)) ret += "AND tags.tagvalue = '" + Sanitizer.Sanitize(val) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + Converters.ExpressionToWhereClause("edges", edgeFilter) + " ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectEdgesToQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid nodeGuid,
            List<string> labels,
            NameValueCollection tags,
            Expr edgeFilter = null,
            int batchSize = 100,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' ";

            if (labels != null && labels.Count > 0)
                ret += "INNER JOIN 'labels' "
                    + "ON edges.guid = labels.edgeguid "
                    + "AND edges.graphguid = labels.graphguid "
                    + "AND edges.tenantguid = labels.tenantguid ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "' "
                + "AND edges.toguid = '" + nodeGuid + "' ";

            if (labels != null && labels.Count > 0)
            {
                foreach (string label in labels)
                {
                    ret += "AND labels.label = '" + Sanitizer.Sanitize(label) + "' ";
                }
            }

            if (tags != null && tags.Count > 0)
            {
                foreach (string key in tags)
                {
                    string val = tags.Get(key);
                    ret += "AND tags.tagkey = '" + Sanitizer.Sanitize(key) + "' ";
                    if (!String.IsNullOrEmpty(val)) ret += "AND tags.tagvalue = '" + Sanitizer.Sanitize(val) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + Converters.ExpressionToWhereClause("edges", edgeFilter) + " ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string SelectEdgesBetweenQuery(
            Guid tenantGuid,
            Guid graphGuid,
            Guid from,
            Guid to,
            List<string> labels,
            NameValueCollection tags,
            Expr edgeFilter = null,
            int batchSize = 0,
            int skip = 0,
            EnumerationOrderEnum order = EnumerationOrderEnum.CreatedDescending)
        {
            string ret =
                "SELECT * FROM 'edges' ";

            if (labels != null && labels.Count > 0)
                ret += "INNER JOIN 'labels' "
                    + "ON edges.guid = labels.edgeguid "
                    + "AND edges.graphguid = labels.graphguid "
                    + "AND edges.tenantguid = labels.tenantguid ";

            if (tags != null && tags.Count > 0)
                ret += "INNER JOIN 'tags' "
                    + "ON edges.guid = tags.edgeguid "
                    + "AND edges.graphguid = tags.graphguid "
                    + "AND edges.tenantguid = tags.tenantguid ";

            ret += "WHERE "
                + "edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "' "
                + "AND edges.fromguid = '" + from + "' "
                + "AND edges.toguid = '" + to + "' ";

            if (labels != null && labels.Count > 0)
            {
                foreach (string label in labels)
                {
                    ret += "AND labels.label = '" + Sanitizer.Sanitize(label) + "' ";
                }
            }

            if (tags != null && tags.Count > 0)
            {
                foreach (string key in tags)
                {
                    string val = tags.Get(key);
                    ret += "AND tags.tagkey = '" + Sanitizer.Sanitize(key) + "' ";
                    if (!String.IsNullOrEmpty(val)) ret += "AND tags.tagvalue = '" + Sanitizer.Sanitize(val) + "' ";
                    else ret += "AND tags.tagvalue IS NULL ";
                }
            }

            if (edgeFilter != null) ret += "AND " + Converters.ExpressionToWhereClause("edges", edgeFilter) + " ";

            ret +=
                "ORDER BY " + Converters.EnumerationOrderToClause(order) + " "
                + "LIMIT " + batchSize + " OFFSET " + skip + ";";

            return ret;
        }

        internal static string UpdateEdgeQuery(Edge edge)
        {
            string ret =
                "UPDATE 'edges' SET "
                + "lastupdateutc = '" + DateTime.UtcNow.ToString(TimestampFormat) + "',"
                + "name = '" + Sanitizer.Sanitize(edge.Name) + "',"
                + "cost = '" + edge.Cost.ToString() + "',";

            if (edge.Data == null) ret += "data = null ";
            else ret += "data = '" + Sanitizer.Sanitize(Serializer.SerializeJson(edge.Data, false)) + "' ";

            ret +=
                "WHERE guid = '" + edge.GUID + "' "
                + "RETURNING *;";

            return ret;
        }

        internal static string DeleteEdgeQuery(Guid tenantGuid, Guid graphGuid, Guid edgeGuid)
        {
            return
                "DELETE FROM 'edges' WHERE "
                + "graphguid = '" + graphGuid + "' "
                + "AND tenantguid = '" + tenantGuid + "' "
                + "AND guid = '" + edgeGuid + "';";
        }

        internal static string DeleteEdgesQuery(Guid tenantGuid, Guid graphGuid)
        {
            return
                "DELETE FROM 'edges' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + graphGuid + "';";
        }

        internal static string DeleteEdgesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> edgeGuids)
        {
            string ret =
                "DELETE FROM 'edges' WHERE tenantguid = '" + tenantGuid + "' AND graphguid = '" + graphGuid + "' "
                + "AND guid IN (";

            for (int i = 0; i < edgeGuids.Count; i++)
            {
                if (i > 0) ret += ",";
                ret += "'" + Sanitizer.Sanitize(edgeGuids[i].ToString()) + "'";
            }

            ret += ");";
            return ret;
        }

        internal static string BatchExistsEdgesQuery(Guid tenantGuid, Guid graphGuid, List<Guid> edgeGuids)
        {
            string query = "WITH temp(guid) AS (VALUES ";

            for (int i = 0; i < edgeGuids.Count; i++)
            {
                if (i > 0) query += ",";
                query += "('" + edgeGuids[i].ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.guid, CASE WHEN edges.guid IS NOT NULL THEN 1 ELSE 0 END as \"exists\" "
                + "FROM temp "
                + "LEFT JOIN edges ON temp.guid = edges.guid "
                + "AND edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "';";

            return query;
        }

        internal static string BatchExistsEdgesBetweenQuery(Guid tenantGuid, Guid graphGuid, List<EdgeBetween> edgesBetween)
        {
            string query = "WITH temp(fromguid, toguid) AS (VALUES ";

            for (int i = 0; i < edgesBetween.Count; i++)
            {
                EdgeBetween curr = edgesBetween[i];
                if (i > 0) query += ",";
                query += "('" + curr.From.ToString() + "','" + curr.To.ToString() + "')";
            }

            query +=
                ") "
                + "SELECT temp.fromguid, temp.toguid, CASE WHEN edges.fromguid IS NOT NULL THEN 1 ELSE 0 END AS \"exists\" "
                + "FROM temp "
                + "LEFT JOIN edges ON temp.fromguid = edges.fromguid "
                + "AND temp.toguid = edges.toguid "
                + "AND edges.graphguid = '" + graphGuid + "' "
                + "AND edges.tenantguid = '" + tenantGuid + "';";

            return query;
        }

        #endregion
    }
}
