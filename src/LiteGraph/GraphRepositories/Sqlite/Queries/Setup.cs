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

    internal static class Setup
    {
        internal static List<string> CreateTablesAndIndices()
        {
            List<string> queries = new List<string>();

            #region Tenants

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'tenants' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "name VARCHAR(128), "
                + "active INT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tenants_guid' ON 'tenants' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tenants_name' ON 'tenants' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tenants_createdutc' ON 'tenants' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tenants_lastupdateutc' ON 'tenants' ('lastupdateutc' ASC);");

            #endregion

            #region Users

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'users' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "firstname VARCHAR(64), "
                + "lastname VARCHAR(64), "
                + "email VARCHAR(128), "
                + "password VARCHAR(128), "
                + "active INT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_guid' ON 'users' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_tenantguid' ON 'users' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_email' ON 'users' (email ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_password' ON 'users' (password ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_createdutc' ON 'users' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_users_lastupdateutc' ON 'users' ('lastupdateutc' ASC);");

            #endregion

            #region Credentials

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'creds' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "userguid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(64), "
                + "bearertoken VARCHAR(64), "
                + "active INT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_guid' ON 'creds' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_tenantguid' ON 'creds' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_userguid' ON 'creds' (userguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_bearertoken' ON 'creds' (bearertoken ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_createdutc' ON 'creds' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_creds_lastupdateutc' ON 'creds' ('lastupdateutc' ASC);");

            #endregion

            #region Tags

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'tags' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "graphguid VARCHAR(64), "
                + "nodeguid VARCHAR(64), "
                + "edgeguid VARCHAR(64), "
                + "tagkey VARCHAR(256), "
                + "tagvalue TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_guid' ON 'tags' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_tenantguid' ON 'tags' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_graphguid' ON 'tags' (graphguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_nodeguid' ON 'tags' (nodeguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_edgeguid' ON 'tags' (edgeguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_tagkey' ON 'tags' (tagkey ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_tagvalue' ON 'tags' (tagvalue ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_createdutc' ON 'tags' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_tags_lastupdateutc' ON 'tags' ('lastupdateutc' ASC);");

            #endregion

            #region Labels

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'labels' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "graphguid VARCHAR(64), "
                + "nodeguid VARCHAR(64), "
                + "edgeguid VARCHAR(64), "
                + "label VARCHAR(256), "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_labels_guid' ON 'labels' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_labels_tenantguid' ON 'labels' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_labels_graphguid' ON 'labels' (graphguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_labels_nodeguid' ON 'labels' (nodeguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_labels_edgeguid' ON 'labels' (edgeguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_labels_label' ON 'labels' (label ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_labels_createdutc' ON 'labels' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_labels_lastupdateutc' ON 'labels' ('lastupdateutc' ASC);");

            #endregion

            #region Vectors

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'vectors' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "graphguid VARCHAR(64), "
                + "nodeguid VARCHAR(64), "
                + "edgeguid VARCHAR(64), "
                + "model VARCHAR(256), "
                + "dimensionality INT, "
                + "content TEXT, "
                + "embeddings TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_guid' ON 'vectors' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_tenantguid' ON 'vectors' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_graphguid' ON 'vectors' (graphguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_nodeguid' ON 'vectors' (nodeguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_edgeguid' ON 'vectors' (edgeguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_model' ON 'vectors' (model ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_dimensionality' ON 'vectors' (dimensionality ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_createdutc' ON 'vectors' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_vectors_lastupdateutc' ON 'vectors' ('lastupdateutc' ASC);");

            #endregion

            #region Graphs

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'graphs' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(128), "
                + "data TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_guid' ON 'graphs' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_tenantguid' ON 'graphs' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_name' ON 'graphs' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_createdutc' ON 'graphs' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_lastupdateutc' ON 'graphs' ('lastupdateutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_graphs_data' ON 'graphs' ('data' ASC);");

            #endregion

            #region Nodes

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'nodes' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "graphguid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(128), "
                + "data TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_guid' ON 'nodes' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_tenantguid' ON 'nodes' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_graphguid' ON 'nodes' (graphguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_name' ON 'nodes' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_createdutc' ON 'nodes' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_lastupdateutc' ON 'nodes' ('lastupdateutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_nodes_data' ON 'nodes' (data ASC);");

            #endregion

            #region Edges

            queries.Add(
                "CREATE TABLE IF NOT EXISTS 'edges' ("
                + "guid VARCHAR(64) NOT NULL UNIQUE, "
                + "tenantguid VARCHAR(64) NOT NULL, "
                + "graphguid VARCHAR(64) NOT NULL, "
                + "name VARCHAR(128), "
                + "fromguid VARCHAR(64) NOT NULL, "
                + "toguid VARCHAR(64) NOT NULL, "
                + "cost INT NOT NULL, "
                + "data TEXT, "
                + "createdutc VARCHAR(64), "
                + "lastupdateutc VARCHAR(64) "
                + ");");

            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_guid' ON 'edges' (guid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_tenantguid' ON 'edges' (tenantguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_graphguid' ON 'edges' (graphguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_name' ON 'edges' (name ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_fromguid' ON 'edges' (fromguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_toguid' ON 'edges' (toguid ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_createdutc' ON 'edges' ('createdutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_lastupdateutc' ON 'edges' ('lastupdateutc' ASC);");
            queries.Add("CREATE INDEX IF NOT EXISTS 'idx_edges_data' ON 'edges' (data ASC);");

            #endregion

            return queries;
        }
    }
}
