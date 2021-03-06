﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiteGraph
{
    internal static class DatabaseHelper
    {
        // Helpful references for Sqlite JSON:
        // https://stackoverflow.com/questions/33432421/sqlite-json1-example-for-json-extract-set
        // https://www.sqlite.org/json1.html

        internal static string _TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff";

        internal static class Nodes
        {
            internal static string TableName = "nodes";
            internal static string IdField = "id";
            internal static string GuidField = "guid";
            internal static string NameField = "name";
            internal static string TypeField = "type";
            internal static string PropsField = "props";
            internal static string CreatedUtcField = "createdutc";

            internal static Node FromDataRow(DataRow row)
            {
                if (row == null) throw new ArgumentNullException(nameof(row));

                Node ret = new Node();

                if (row.Table.Columns.Contains(IdField) && row[IdField] != null && row[IdField] != DBNull.Value)
                {
                    ret.Id = Convert.ToInt32(row[IdField]);
                }

                if (row.Table.Columns.Contains(GuidField) && row[GuidField] != null && row[GuidField] != DBNull.Value)
                {
                    ret.GUID = row[GuidField].ToString();
                }

                if (row.Table.Columns.Contains(NameField) && row[NameField] != null && row[NameField] != DBNull.Value)
                {
                    ret.Name = row[NameField].ToString();
                }

                if (row.Table.Columns.Contains(TypeField) && row[TypeField] != null && row[TypeField] != DBNull.Value)
                {
                    ret.NodeType = row[TypeField].ToString();
                }

                if (row.Table.Columns.Contains(CreatedUtcField) && row[CreatedUtcField] != null && row[CreatedUtcField] != DBNull.Value)
                {
                    ret.CreatedUtc = Convert.ToDateTime(row[CreatedUtcField].ToString());
                }

                if (row.Table.Columns.Contains(PropsField) && row[PropsField] != null && row[PropsField] != DBNull.Value)
                {
                    if (!String.IsNullOrEmpty(row[PropsField].ToString()))
                    {
                        ret.Properties = JObject.Parse(row[PropsField].ToString());
                    }
                }

                return ret;
            }

            internal static List<Node> FromDataTable(DataTable table)
            {
                if (table == null) throw new ArgumentNullException(nameof(table));

                List<Node> nodes = new List<Node>();

                foreach (DataRow row in table.Rows)
                {
                    nodes.Add(FromDataRow(row));
                }

                return nodes;
            }

            internal static string CreateTableQuery
            {
                get
                {
                    return
                       "BEGIN TRANSACTION; " +
                       "CREATE TABLE IF NOT EXISTS " + TableName + " (" +
                       "  " + IdField +         " INTEGER     PRIMARY KEY AUTOINCREMENT, " +
                       "  " + GuidField       + " VARCHAR(64) COLLATE NOCASE, " +
                       "  " + NameField       + " VARCHAR(64) COLLATE NOCASE, " +
                       "  " + TypeField       + " VARCHAR(64) COLLATE NOCASE, " +
                       "  " + CreatedUtcField + " TEXT, " +
                       "  " + PropsField +      " TEXT" +
                       "); " +
                       "COMMIT; ";
                }
            } 

            internal static string InsertQuery(Node node)
            {
                // json('{"guid":"joel","first":"joel","last":"christner"}'))
                return
                    "BEGIN TRANSACTION; " +
                    "INSERT INTO " + TableName + " (" +
                    "  " + GuidField + ", " +
                    "  " + NameField + ", " +
                    "  " + TypeField + ", " +
                    "  " + CreatedUtcField + ", " +
                    "  " + PropsField +
                    ") " +
                    "VALUES (" +
                    "  '" + Sanitize(node.GUID) + "', " +
                    "  '" + Sanitize(node.Name) + "', " +
                    "  '" + Sanitize(node.NodeType) + "', " +
                    "  '" + Sanitize(node.CreatedUtc.ToString(_TimestampFormat)) + "', " +
                    "  " + (node.Properties != null ? "json('" + node.Properties.ToJson(false) + "')" : "null" ) + " " +
                    "); " +
                    "COMMIT; "; 
            }

            internal static string UpdateQuery(Node node)
            {
                return
                    "BEGIN TRANSACTION;" +
                    "UPDATE " + TableName + " " +
                    "SET " +
                    "  " + TypeField + " = '" + Sanitize(node.NodeType) + "', " +
                    "  " + PropsField + " = " + (node.Properties != null ? "json('" + node.Properties.ToJson(false) + "')" : "null") + " " +
                    "WHERE " + 
                    "  " + GuidField + " = '" + Sanitize(node.GUID) + "';" +
                    "COMMIT;";
            }

            internal static string ExistsQuery(string guid)
            {
                return "SELECT " + GuidField + " FROM " + TableName + " WHERE " + GuidField + " = '" + Sanitize(guid) + "'";
            }

            internal static string DeleteQuery(string guid)
            {
                return "DELETE FROM " + TableName + " WHERE " + GuidField + " = '" + Sanitize(guid) + "'";
            }

            internal static string SelectAllQuery
            {
                get
                {
                    return "SELECT * FROM " + TableName;
                }
            }

            internal static string SelectByGuid(string guid)
            {
                return "SELECT * FROM " + TableName + " WHERE " + GuidField + " = '" + Sanitize(guid) + "'";
            }

            internal static string SelectByFilter(List<string> guids, List<string> types, List<SearchFilter> filters, int indexStart, int maxResults)
            {
                guids = Sanitize(guids);
                filters = Sanitize(filters);

                string ret =
                    "SELECT * FROM " + TableName + " " +
                    "WHERE " + IdField + " > 0 ";

                if (filters != null && filters.Count > 0)
                {
                    foreach (SearchFilter filter in filters)
                    {
                        if (String.IsNullOrEmpty(filter.Field)) continue;
                        ret += "AND " + SearchFilterToQueryCondition(filter, PropsField) + " ";
                    }
                }

                if (guids != null && guids.Count > 0)
                {
                    ret += "AND (" + GuidField + " IN (";

                    int guidsAdded = 0;
                    foreach (string guid in guids)
                    {
                        if (String.IsNullOrEmpty(guid)) continue;
                        if (guidsAdded > 0) ret += ",";
                        ret += "'" + guid + "'";
                        guidsAdded++;
                    }

                    ret += "))";
                }

                if (types != null && types.Count > 0)
                {
                    ret += "AND (" + TypeField + " IN (";

                    int typesAdded = 0;
                    foreach (string type in types)
                    {
                        if (String.IsNullOrEmpty(type)) continue;
                        if (typesAdded > 0) ret += ",";
                        ret += "'" + type + "'";
                        typesAdded++;
                    }

                    ret += "))";
                }

                if (indexStart >= 0 && maxResults > 0)
                {
                    ret += "LIMIT " + indexStart + ", " + maxResults;
                }
                else if (maxResults > 0)
                {
                    ret += "LIMIT " + maxResults;
                }

                return ret;
            } 
        }

        internal static class Edges
        {
            internal static string TableName = "edges";
            internal static string IdField = "id";
            internal static string GuidField = "guid";
            internal static string TypeField = "type";
            internal static string CostField = "cost";
            internal static string FromGuidField = "fromguid";
            internal static string ToGuidField = "toguid";
            internal static string PropsField = "props";
            internal static string CreatedUtcField = "createdutc";

            internal static Edge FromDataRow(DataRow row)
            {
                if (row == null) throw new ArgumentNullException(nameof(row));

                Edge ret = new Edge();

                if (row.Table.Columns.Contains(IdField) && row[IdField] != null && row[IdField] != DBNull.Value)
                {
                    ret.Id = Convert.ToInt32(row[IdField]);
                }

                if (row.Table.Columns.Contains(GuidField) && row[GuidField] != null && row[GuidField] != DBNull.Value)
                {
                    ret.GUID = row[GuidField].ToString();
                }

                if (row.Table.Columns.Contains(CostField) && row[CostField] != null && row[CostField] != DBNull.Value)
                {
                    ret.Cost = Convert.ToInt32(row[CostField]);
                }

                if (row.Table.Columns.Contains(TypeField) && row[TypeField] != null && row[TypeField] != DBNull.Value)
                {
                    ret.EdgeType = row[TypeField].ToString();
                }

                if (row.Table.Columns.Contains(FromGuidField) && row[FromGuidField] != null && row[FromGuidField] != DBNull.Value)
                {
                    ret.FromGUID = row[FromGuidField].ToString();
                }

                if (row.Table.Columns.Contains(ToGuidField) && row[ToGuidField] != null && row[ToGuidField] != DBNull.Value)
                {
                    ret.ToGUID = row[ToGuidField].ToString();
                }

                if (row.Table.Columns.Contains(CreatedUtcField) && row[CreatedUtcField] != null && row[CreatedUtcField] != DBNull.Value)
                {
                    ret.CreatedUtc = Convert.ToDateTime(row[CreatedUtcField].ToString());
                }

                if (row.Table.Columns.Contains(PropsField) && row[PropsField] != null && row[PropsField] != DBNull.Value)
                {
                    if (!String.IsNullOrEmpty(row[PropsField].ToString()))
                    {
                        ret.Properties = JObject.Parse(row[PropsField].ToString());
                    }
                }

                return ret;
            }

            internal static List<Edge> FromDataTable(DataTable table)
            {
                if (table == null) throw new ArgumentNullException(nameof(table));

                List<Edge> nodes = new List<Edge>();

                foreach (DataRow row in table.Rows)
                {
                    nodes.Add(FromDataRow(row));
                }

                return nodes;
            }

            internal static string CreateTableQuery
            {
                get
                {
                    return
                       "BEGIN TRANSACTION; " +
                       "CREATE TABLE IF NOT EXISTS " + TableName + " (" +
                       "  " + IdField +         " INTEGER PRIMARY KEY AUTOINCREMENT, " +
                       "  " + GuidField       + " VARCHAR(64) COLLATE NOCASE, " +
                       "  " + TypeField       + " VARCHAR(64) COLLATE NOCASE, " +
                       "  " + CostField       + " INTEGER, " +
                       "  " + FromGuidField +   " VARCHAR(64) COLLATE NOCASE, " +
                       "  " + ToGuidField +     " VARCHAR(64) COLLATE NOCASE, " +
                       "  " + CreatedUtcField + " TEXT, " +
                       "  " + PropsField +      " TEXT" +
                       "); " +
                       "COMMIT; ";
                }
            }

            internal static string InsertQuery(Edge edge)
            {
                return
                    "BEGIN TRANSACTION; " +
                    "INSERT INTO " + TableName + " (" +
                    "  " + GuidField + ", " +
                    "  " + TypeField + ", " +
                    "  " + CostField + ", " +
                    "  " + FromGuidField + ", " +
                    "  " + ToGuidField + ", " +
                    "  " + CreatedUtcField + ", " +
                    "  " + PropsField +
                    ") " +
                    "VALUES (" +
                    "  '" + Sanitize(edge.GUID) + "', " +
                    "  '" + Sanitize(edge.EdgeType) + "', " +
                    "  " + (edge.Cost != null ? edge.Cost.ToString() : "null") + ", " +
                    "  '" + Sanitize(edge.FromGUID) + "', " +
                    "  '" + Sanitize(edge.ToGUID) + "', " +
                    "  '" + Sanitize(edge.CreatedUtc.ToString(_TimestampFormat)) + "', " +
                    "  " + (edge.Properties != null ? "json('" + edge.Properties.ToJson(false) + "')" : "null") + " " +
                    "); " +
                    "COMMIT; ";
            }

            internal static string UpdateQuery(Edge edge)
            {
                return
                    "BEGIN TRANSACTION;" +
                    "UPDATE " + TableName + " " +
                    "SET " +
                    "  " + TypeField + " = '" + Sanitize(edge.EdgeType) + "', " +
                    "  " + FromGuidField + " = '" + Sanitize(edge.FromGUID) + "', " +
                    "  " + ToGuidField + " = '" + Sanitize(edge.ToGUID) + "', " +
                    "  " + CostField + " = " + (edge.Cost != null ? edge.Cost.ToString() : "null") + ", " +
                    "  " + PropsField + " = " + (edge.Properties != null ? "json('" + edge.Properties.ToJson(false) + "')" : "null") + " " +
                    "WHERE " + 
                    "  " + GuidField + " = '" + Sanitize(edge.GUID) + "';" +
                    "COMMIT;";
            }

            internal static string ExistsQuery(string guid)
            {
                return "SELECT " + GuidField + " FROM " + TableName + " WHERE " + GuidField + " = '" + Sanitize(guid) + "'";
            }

            internal static string DeleteQuery(string guid)
            {
                return "DELETE FROM " + TableName + " WHERE " + GuidField + " = '" + Sanitize(guid) + "'";
            }

            internal static string DeleteNodeEdgesQuery(string guid)
            {
                return "DELETE FROM " + TableName + " WHERE " + FromGuidField + " = '" + Sanitize(guid) + "' OR " + ToGuidField + " = '" + Sanitize(guid) + "'";
            }

            internal static string SelectAllQuery
            {
                get
                {
                    return "SELECT * FROM " + TableName;
                }
            }

            internal static string SelectByGuid(string guid)
            {
                return "SELECT * FROM " + TableName + " WHERE " + GuidField + " = '" + Sanitize(guid) + "'";
            }

            internal static string SelectByFilter(List<string> guids, List<string> types, List<SearchFilter> filters, int indexStart, int maxResults, int? costMin = null, int? costMax = null)
            {
                guids = Sanitize(guids);
                filters = Sanitize(filters);

                string ret =
                    "SELECT * FROM " + TableName + " " +
                    "WHERE " + IdField + " > 0 ";

                if (costMin != null && costMax != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " >= " + costMin.ToString() + " AND " + CostField + " <= " + costMax.ToString() + " ";
                else if (costMin != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " >= " + costMin.ToString() + " ";
                else if (costMax != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " <= " + costMax.ToString() + " ";

                if (filters != null && filters.Count > 0)
                { 
                    foreach (SearchFilter filter in filters)
                    {
                        if (String.IsNullOrEmpty(filter.Field)) continue;
                        ret += "AND " + SearchFilterToQueryCondition(filter, PropsField) + " ";
                    } 
                }

                if (types != null && types.Count > 0)
                {
                    ret += "AND (" + TypeField + " IN (";

                    int typesAdded = 0;
                    foreach (string type in types)
                    {
                        if (String.IsNullOrEmpty(type)) continue;
                        if (typesAdded > 0) ret += ",";
                        ret += "'" + type + "'";
                        typesAdded++;
                    }

                    ret += "))";
                }

                if (guids != null && guids.Count > 0)
                {
                    ret += "AND (";

                    #region GUID

                    ret += "(";
                    int guidsAdded = 0;
                    ret += GuidField + " IN (";
                    foreach (string guid in guids)
                    {
                        if (String.IsNullOrEmpty(guid)) continue;
                        if (guidsAdded > 0) ret += ",";
                        ret += "'" + guid + "'";
                        guidsAdded++;
                    }
                    ret += "))";

                    #endregion

                    ret += " OR ";

                    #region From-GUID

                    ret += "(";
                    int fromGuidsAdded = 0;
                    ret += FromGuidField + " IN (";
                    foreach (string guid in guids)
                    {
                        if (String.IsNullOrEmpty(guid)) continue;
                        if (fromGuidsAdded > 0) ret += ",";
                        ret += "'" + guid + "'";
                        fromGuidsAdded++;
                    }
                    ret += "))";

                    #endregion

                    ret += " OR ";

                    #region To-GUID

                    ret += "(";
                    int toGuidsAdded = 0;
                    ret += ToGuidField + " IN (";
                    foreach (string guid in guids)
                    {
                        if (String.IsNullOrEmpty(guid)) continue;
                        if (toGuidsAdded > 0) ret += ",";
                        ret += "'" + guid + "'";
                        toGuidsAdded++;
                    }
                    ret += "))";

                    #endregion

                    ret += ")";    
                }

                if (indexStart >= 0 && maxResults > 0)
                {
                    ret += "LIMIT " + indexStart + ", " + maxResults;
                }
                else if (maxResults > 0)
                {
                    ret += "LIMIT " + maxResults;
                }

                return ret;
            }

            internal static string SelectEdgesFrom(string guid, List<string> types, List<SearchFilter> filters, int indexStart, int maxResults, int? costMin = null, int? costMax = null)
            {
                guid = Sanitize(guid);
                types = Sanitize(types);

                string ret =
                    "SELECT * FROM " + TableName + " " +
                    "WHERE " + IdField + " > 0 " +
                    "AND " + FromGuidField + " = '" + guid + "' ";

                if (costMin != null && costMax != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " >= " + costMin.ToString() + " AND " + CostField + " <= " + costMax.ToString() + " ";
                else if (costMin != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " >= " + costMin.ToString() + " ";
                else if (costMax != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " <= " + costMax.ToString() + " ";

                if (filters != null && filters.Count > 0)
                {
                    foreach (SearchFilter filter in filters)
                    {
                        if (String.IsNullOrEmpty(filter.Field)) continue;
                        ret += "AND " + SearchFilterToQueryCondition(filter, PropsField) + " ";
                    }
                }

                if (types != null && types.Count > 0)
                {
                    ret += "AND " + TypeField + " IN (";

                    int typesAdded = 0;
                    foreach (string type in types)
                    {
                        if (typesAdded > 0) ret += ",";
                        ret += "'" + type + "'";
                    }
                    ret += ") ";
                }

                if (indexStart >= 0 && maxResults > 0)
                {
                    ret += "LIMIT " + indexStart + ", " + maxResults;
                }
                else if (maxResults > 0)
                {
                    ret += "LIMIT " + maxResults;
                }

                return ret;
            }

            internal static string SelectEdgesTo(string guid, List<string> types, List<SearchFilter> filters, int indexStart, int maxResults, int? costMin = null, int? costMax = null)
            {
                guid = Sanitize(guid);
                types = Sanitize(types);

                string ret =
                    "SELECT * FROM " + TableName + " " +
                    "WHERE " + IdField + " > 0 " +
                    "AND " + ToGuidField + " = '" + guid + "' ";

                if (costMin != null && costMax != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " >= " + costMin.ToString() + " AND " + CostField + " <= " + costMax.ToString() + " ";
                else if (costMin != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " >= " + costMin.ToString() + " ";
                else if (costMax != null)
                    ret += "AND " + CostField + " != null AND " + CostField + " <= " + costMax.ToString() + " ";

                if (filters != null && filters.Count > 0)
                {
                    foreach (SearchFilter filter in filters)
                    {
                        if (String.IsNullOrEmpty(filter.Field)) continue;
                        ret += "AND " + SearchFilterToQueryCondition(filter, PropsField) + " ";
                    }
                }

                if (types != null && types.Count > 0)
                {
                    ret += "AND " + TypeField + " IN (";

                    int typesAdded = 0;
                    foreach (string type in types)
                    {
                        if (typesAdded > 0) ret += ",";
                        ret += "'" + type + "'";
                    }
                    ret += ") ";
                }

                if (indexStart >= 0 && maxResults > 0)
                {
                    ret += "LIMIT " + indexStart + ", " + maxResults;
                }
                else if (maxResults > 0)
                {
                    ret += "LIMIT " + maxResults;
                }

                return ret;
            }
        }

        internal static string Sanitize(string val)
        {
            string ret = "";

            //
            // null, below ASCII range, above ASCII range
            //
            for (int i = 0; i < val.Length; i++)
            {
                if (((int)(val[i]) == 10) ||      // Preserve carriage return
                    ((int)(val[i]) == 13))        // and line feed
                {
                    ret += val[i];
                }
                else if ((int)(val[i]) < 32)
                {
                    continue;
                }
                else
                {
                    ret += val[i];
                }
            }

            //
            // double dash
            //
            int doubleDash = 0;
            while (true)
            {
                doubleDash = ret.IndexOf("--");
                if (doubleDash < 0)
                {
                    break;
                }
                else
                {
                    ret = ret.Remove(doubleDash, 2);
                }
            }

            //
            // open comment
            // 
            int openComment = 0;
            while (true)
            {
                openComment = ret.IndexOf("/*");
                if (openComment < 0) break;
                else
                {
                    ret = ret.Remove(openComment, 2);
                }
            }

            //
            // close comment
            //
            int closeComment = 0;
            while (true)
            {
                closeComment = ret.IndexOf("*/");
                if (closeComment < 0) break;
                else
                {
                    ret = ret.Remove(closeComment, 2);
                }
            }

            //
            // in-string replacement
            //
            ret = ret.Replace("'", "''");
            return ret;
        }

        internal static List<string> Sanitize(List<string> vals)
        {
            List<string> ret = new List<string>();

            if (vals != null && vals.Count > 0)
            {
                foreach (string str in vals)
                {
                    string sanitized = Sanitize(str);
                    if (!String.IsNullOrEmpty(sanitized)) ret.Add(sanitized);
                }
            }

            return ret; 
        }

        internal static SearchFilter Sanitize(SearchFilter sf)
        {
            if (sf == null) return null;

            SearchFilter sanitized = null;

            if (sf.Condition == SearchCondition.In)
            {
                if (!SearchFilter.IsStringList(sf.Value))
                    throw new ArgumentException("Value must be convertible to List<string> when using In.");

                sanitized = new SearchFilter(
                    Sanitize(sf.Field),
                    sf.Condition,
                    Sanitize((List<string>)(sf.Value)));
            }
            else
            {
                if (sf.Value != null)
                {
                    sanitized = new SearchFilter(
                        Sanitize(sf.Field),
                        sf.Condition,
                        Sanitize(sf.Value.ToString()));
                }
                else
                {
                    sanitized = new SearchFilter(
                        Sanitize(sf.Field),
                        sf.Condition,
                        null);
                }
            }
            return sanitized;
        }

        internal static List<SearchFilter> Sanitize(List<SearchFilter> filters)
        {
            List<SearchFilter> ret = new List<SearchFilter>();
            if (filters == null || filters.Count < 1) return ret;

            foreach (SearchFilter sf in filters)
            {
                ret.Add(Sanitize(sf));
            }

            return ret;
        }

        internal static string SearchFilterToQueryCondition(SearchFilter filter, string propsField)
        {
            // select * from nodes WHERE json_extract(props, '$.first') = 'joel'
            string ret = "json_extract(" + propsField + ", '$." + Sanitize(filter.Field) + "') ";

            switch (filter.Condition)
            {
                case SearchCondition.Contains:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "LIKE '%" + Sanitize(filter.Value.ToString()) + "%'";
                    break;
                case SearchCondition.ContainsNot:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "NOT LIKE '%" + Sanitize(filter.Value.ToString()) + "%'";
                    break;
                case SearchCondition.EndsWith:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "LIKE '%" + Sanitize(filter.Value.ToString()) + "'";
                    break;
                case SearchCondition.Equals:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "= '" + Sanitize(filter.Value.ToString()) + "'";
                    break;
                case SearchCondition.GreaterThan:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "> '" + Sanitize(filter.Value.ToString()) + "'";
                    break;
                case SearchCondition.GreaterThanOrEqualTo:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += ">= '" + Sanitize(filter.Value.ToString()) + "'";
                    break;
                case SearchCondition.IsNotNull: 
                    ret += "IS NOT NULL";
                    break;
                case SearchCondition.IsNull: 
                    ret += "IS NULL";
                    break;
                case SearchCondition.LessThan:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "< '" + Sanitize(filter.Value.ToString()) + "'";
                    break;
                case SearchCondition.LessThanOrEqualTo:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "<= '" + Sanitize(filter.Value.ToString()) + "'";
                    break;
                case SearchCondition.NotEquals:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "!= '" + Sanitize(filter.Value.ToString()) + "'";
                    break;
                case SearchCondition.StartsWith:
                    if (filter.Value == null || String.IsNullOrEmpty(filter.Value.ToString()))
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    ret += "LIKE '" + Sanitize(filter.Value.ToString()) + "%'";
                    break;
                case SearchCondition.In:
                    if (filter.Value == null)
                        throw new ArgumentNullException(nameof(SearchFilter.Value));
                    if (!SearchFilter.IsStringList(filter.Value))
                        throw new ArgumentException("Value must be convertible to List<string> when using In.");
                    ret += "IN (";
                    int filtersAdded = 0;
                    List<string> vals = (List<string>)(filter.Value);
                    foreach (string val in vals)
                    {
                        if (filtersAdded > 0) ret += ",";
                        ret += "'" + Sanitize(val) + "'";
                        filtersAdded++;
                    }
                    ret += ")";
                    break;
                default:
                    throw new ArgumentException("Unsupported filter condition '" + filter.Condition.ToString() + "'.");
            }

            return ret;
        }
    }
}
