--
-- LiteGraph Sqlite Database Setup
-- 

--
-- Graphs
--
DROP TABLE IF EXISTS 'graphs';
CREATE TABLE IF NOT EXISTS 'graphs' (
  'id' VARCHAR(64) NOT NULL UNIQUE, 
  'name' VARCHAR(128), 
  'data' TEXT, 
  'createdutc' VARCHAR(64)
  );
CREATE INDEX IF NOT EXISTS 'idx_graphs_id' ON 'graphs' ('id' ASC);
CREATE INDEX IF NOT EXISTS 'idx_graphs_name' ON 'graphs' ('name' ASC);
CREATE INDEX IF NOT EXISTS 'idx_graphs_createdutc' ON 'graphs' ('createdutc' ASC);
CREATE INDEX IF NOT EXISTS 'idx_graphs_data' ON 'graphs' ('data' ASC);

--
-- Nodes
--
DROP TABLE IF EXISTS 'nodes';
CREATE TABLE IF NOT EXISTS 'nodes' (
  'id' VARCHAR(64) NOT NULL UNIQUE,
  'graphid' VARCHAR(64) NOT NULL,
  'name' VARCHAR(128), 
  'data' TEXT, 
  'createdutc' VARCHAR(64)
);
CREATE INDEX IF NOT EXISTS 'idx_nodes_id' ON 'nodes' ('id' ASC);
CREATE INDEX IF NOT EXISTS 'idx_nodes_graphid' ON 'nodes' ('graphid' ASC);
CREATE INDEX IF NOT EXISTS 'idx_nodes_name' ON 'nodes' ('name' ASC);
CREATE INDEX IF NOT EXISTS 'idx_nodes_createdutc' ON 'nodes' ('createdutc' ASC);
CREATE INDEX IF NOT EXISTS 'idx_nodes_data' ON 'nodes' ('data' ASC);

--
-- Edges
--
DROP TABLE IF EXISTS 'edges';
CREATE TABLE IF NOT EXISTS 'edges' (
  'id' VARCHAR(64) NOT NULL UNIQUE, 
  'graphid' VARCHAR(64) NOT NULL, 
  'name' VARCHAR(128), 
  'fromguid' VARCHAR(64) NOT NULL, 
  'toguid' VARCHAR(64) NOT NULL, 
  'cost' INT NOT NULL, 
  'data' TEXT, 
  'createdutc' VARCHAR(64)
  );
CREATE INDEX IF NOT EXISTS 'idx_edges_id' ON 'edges' ('id' ASC);
CREATE INDEX IF NOT EXISTS 'idx_edges_graphid' ON 'edges' ('graphid' ASC);
CREATE INDEX IF NOT EXISTS 'idx_edges_fromguid' ON 'edges' ('fromguid' ASC);
CREATE INDEX IF NOT EXISTS 'idx_edges_toguid' ON 'edges' ('toguid' ASC);
CREATE INDEX IF NOT EXISTS 'idx_edges_name' ON 'edges' ('name' ASC);
CREATE INDEX IF NOT EXISTS 'idx_edges_createdutc' ON 'edges' ('createdutc' ASC);
CREATE INDEX IF NOT EXISTS 'idx_edges_data' ON 'edges' ('data' ASC);

