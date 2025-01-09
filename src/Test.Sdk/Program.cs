namespace Test.Sdk
{
    using System;
    using System.Collections.Specialized;
    using ExpressionTree;
    using GetSomeInput;
    using LiteGraph.Sdk;

    public static class Program
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8603 // Possible null reference return.

        private static bool _Debug = true;
        private static bool _RunForever = true;
        private static string _Endpoint = "http://localhost:8701";
        private static string _BearerToken = "litegraphadmin";
        private static Guid _Tenant = Guid.Parse("00000000-0000-0000-0000-000000000000");
        private static Guid _Graph = Guid.Parse("00000000-0000-0000-0000-000000000000");
        private static LiteGraphSdk _Sdk = null;

        public static void Main(string[] args)
        {
            _Sdk = new LiteGraphSdk(
                Inputty.GetString("Endpoint     :", _Endpoint, false),
                Inputty.GetString("Bearer token :", _BearerToken, false));

            _Tenant = Inputty.GetGuid("Tenant GUID  :", _Tenant);

            if (_Debug) _Sdk.Logger = Logger;

            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [?/help]:", null, false);

                switch (userInput)
                {
                    case "?":
                        Menu();
                        break;
                    case "q":
                        _RunForever = false;
                        break;
                    case "cls":
                        Console.Clear();
                        break;

                    case "conn":
                        ValidateConnectivity();
                        break;

                    case "tenant":
                        SetTenant();
                        break;
                    case "graph":
                        SetGraph();
                        break;

                    case "tenant exists":
                        TenantExists();
                        break;
                    case "tenant create":
                        TenantCreate();
                        break;
                    case "tenant update":
                        TenantUpdate();
                        break;
                    case "tenant all":
                        TenantAll();
                        break;
                    case "tenant read":
                        TenantRead();
                        break;
                    case "tenant delete":
                        TenantDelete();
                        break;

                    case "user exists":
                        UserExists();
                        break;
                    case "user create":
                        UserCreate();
                        break;
                    case "user update":
                        UserUpdate();
                        break;
                    case "user all":
                        UserAll();
                        break;
                    case "user read":
                        UserRead();
                        break;
                    case "user delete":
                        UserDelete();
                        break;

                    case "cred exists":
                        CredentialExists();
                        break;
                    case "cred create":
                        CredentialCreate();
                        break;
                    case "cred update":
                        CredentialUpdate();
                        break;
                    case "cred all":
                        CredentialAll();
                        break;
                    case "cred read":
                        CredentialRead();
                        break;
                    case "cred delete":
                        CredentialDelete();
                        break;

                    case "tag exists":
                        TagExists();
                        break;
                    case "tag create":
                        TagCreate();
                        break;
                    case "tag update":
                        TagUpdate();
                        break;
                    case "tag all":
                        TagAll();
                        break;
                    case "tag read":
                        TagRead();
                        break;
                    case "tag delete":
                        TagDelete();
                        break;

                    case "graph exists":
                        GraphExists();
                        break;
                    case "graph create":
                        GraphCreate();
                        break;
                    case "graph update":
                        GraphUpdate();
                        break;
                    case "graph all":
                        GraphAll();
                        break;
                    case "graph read":
                        GraphRead();
                        break;
                    case "graph delete":
                        GraphDelete();
                        break;
                    case "graph search":
                        GraphSearch();
                        break;

                    case "node exists":
                        NodeExists();
                        break;
                    case "node create":
                        NodeCreate();
                        break;
                    case "node update":
                        NodeUpdate();
                        break;
                    case "node all":
                        NodeAll();
                        break;
                    case "node read":
                        NodeRead();
                        break;
                    case "node delete":
                        NodeDelete();
                        break;
                    case "node search":
                        NodeSearch();
                        break;

                    case "node edges":
                        NodeEdges();
                        break;
                    case "node parents":
                        NodeParents();
                        break;
                    case "node children":
                        NodeChildren();
                        break;

                    case "edge exists":
                        EdgeExists();
                        break;
                    case "edge create":
                        EdgeCreate();
                        break;
                    case "edge update":
                        EdgeUpdate();
                        break;
                    case "edge all":
                        EdgeAll();
                        break;
                    case "edge read":
                        EdgeRead();
                        break;
                    case "edge delete":
                        EdgeDelete();
                        break;
                    case "edge search":
                        EdgeSearch();
                        break;

                    case "edges from":
                        EdgesFrom();
                        break;
                    case "edges to":
                        EdgesTo();
                        break;
                    case "edges between":
                        EdgesBetween();
                        break;

                    case "route":
                        Route();
                        break;
                }

                Console.WriteLine("");
            }
        }

        private static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  ?               help, this menu");
            Console.WriteLine("  q               quit");
            Console.WriteLine("  cls             clear the screen");
            Console.WriteLine("  conn            validate connectivity");
            Console.WriteLine("  tenant          set the tenant GUID (currently " + _Tenant + ")");
            Console.WriteLine("  graph           set the graph GUID (currently " + _Graph + ")");
            Console.WriteLine("");
            Console.WriteLine("Object commands:");
            Console.WriteLine("  Tenants     : tenant [create|update|all|read|delete|exists]");
            Console.WriteLine("  Users       : user [create|update|all|read|delete|exists]");
            Console.WriteLine("  Credentials : cred [create|update|all|read|delete|exists]");
            Console.WriteLine("  Tags        : tag [create|update|all|read|delete|exists]");
            Console.WriteLine("  Graphs      : graph [create|update|all|read|delete|exists|search]");
            Console.WriteLine("  Nodes       : node [create|update|all|read|delete|exists|search|edges|parents|children]");
            Console.WriteLine("  Edges       : edge [create|update|all|read|delete|exists|from|to|search|between]");
            Console.WriteLine("");
            Console.WriteLine("Routing commands:");
            Console.WriteLine("  route");
            Console.WriteLine("");
        }

        private static void Logger(SeverityEnum sev, string msg)
        {
            Console.WriteLine(sev.ToString() + " " + msg);
        }

        private static void SetTenant()
        {
            _Tenant = Inputty.GetGuid("Tenant GUID:", _Tenant);
        }

        private static void SetGraph()
        {
            _Graph = Inputty.GetGuid("Graph GUID:", _Graph);
        }

        private static string GetName()
        {
            return Inputty.GetString("Name:", null, false);
        }

        private static List<string> GetLabels()
        {
            string val = GetJson("Labels JSON:");
            if (!String.IsNullOrEmpty(val)) return Serializer.DeserializeJson<List<string>>(val);
            return null;
        }

        private static NameValueCollection GetTags()
        {
            string val = GetJson("Tags JSON:");
            if (!String.IsNullOrEmpty(val)) return Serializer.DeserializeJson<NameValueCollection>(val);
            return null;
        }

        private static object GetData()
        {
            string val = GetJson("Data JSON:");
            if (!String.IsNullOrEmpty(val)) return Serializer.DeserializeJson<object>(val);
            return null;
        }

        private static string GetJson(string prompt)
        {
            return Inputty.GetString(prompt, null, true);
        }

        private static Guid GetGuid(string prompt, Guid? guid = null)
        {
            if (guid == null) return Inputty.GetGuid(prompt, default(Guid));
            else return Inputty.GetGuid(prompt, guid.Value);
        }

        private static bool GetBoolean(string prompt)
        {
            return Inputty.GetBoolean(prompt, true);
        }

        private static SearchRequest BuildSearchRequest()
        {
            NameValueCollection nvc = GetNameValueCollection();

            Expr expr = GetExpression();
            if (expr == null) return null;

            SearchRequest req = new SearchRequest();
            req.TenantGUID = Inputty.GetGuid("Tenant GUID:", _Tenant);
            req.GraphGUID = Inputty.GetGuid("Graph GUID:", _Graph);
            req.Tags = nvc;
            req.Expr = expr;
            return req;
        }

        static NameValueCollection GetNameValueCollection()
        {
            Console.WriteLine("");
            Console.WriteLine("Add keys and values to build a name value collection");
            Console.WriteLine("Press ENTER on a key to end");

            NameValueCollection ret = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

            while (true)
            {
                string key = Inputty.GetString("Key   :", null, true);
                if (String.IsNullOrEmpty(key)) break;

                string val = Inputty.GetString("Value :", null, true);
                ret.Add(key, val);
            }

            return ret;
        }

        static Expr GetExpression()
        {
            Console.WriteLine("");
            Console.WriteLine("Add JSON values to build an expression");
            Console.WriteLine("Example expressions:");

            Expr e1 = new Expr("Age", OperatorEnum.GreaterThan, 38);
            e1.PrependAnd("Hobby.Name", OperatorEnum.Equals, "BJJ");
            Console.WriteLine(Serializer.SerializeJson(e1, false));

            Expr e2 = new Expr("Mbps", OperatorEnum.GreaterThan, 250);
            Console.WriteLine(Serializer.SerializeJson(e2, false));
            Console.WriteLine("");

            string json = Inputty.GetString("JSON:", null, true);
            if (String.IsNullOrEmpty(json)) return null;

            Expr expr = Serializer.DeserializeJson<Expr>(json);
            Console.WriteLine("");
            Console.WriteLine("Using expression: " + expr.ToString());
            Console.WriteLine("");
            return expr;
        }

        private static void EnumerateResult(object obj)
        {
            Console.WriteLine("");
            Console.Write("Result: ");
            if (obj == null) Console.WriteLine("(null)");
            else Console.WriteLine(Environment.NewLine + Serializer.SerializeJson(obj, true));
            Console.WriteLine("");
        }

        private static void ValidateConnectivity()
        {
            Console.WriteLine("Connected: " + _Sdk.ValidateConnectivity().Result);
        }

        #region Tenant
        
        private static void ShowSampleTenant()
        {
            Console.WriteLine("Sample JSON:");
            Console.WriteLine(Serializer.SerializeJson(new TenantMetadata
            {
                GUID = Guid.NewGuid(),
                Name = "My tenant"
            }, false));
        }

        private static void TenantExists()
        {
            EnumerateResult(_Sdk.TenantExists(
                GetGuid("GUID:", _Tenant)).Result);
        }

        private static void TenantCreate()
        {
            EnumerateResult(_Sdk.CreateTenant(
                GetGuid("GUID:"),
                GetName()).Result);
        }

        private static void TenantUpdate()
        {
            ShowSampleTenant();
            EnumerateResult(_Sdk.UpdateTenant(
                Serializer.DeserializeJson<TenantMetadata>(GetJson("Tenant JSON:"))).Result);
        }

        private static void TenantAll()
        {
            EnumerateResult(_Sdk.ReadTenants().Result);
        }

        private static void TenantRead()
        {
            EnumerateResult(_Sdk.ReadTenant(
                GetGuid("GUID:", _Tenant)).Result);
        }

        private static void TenantDelete()
        {
            _Sdk.DeleteTenant(
                GetGuid("GUID:", _Tenant),
                GetBoolean("Force:")).Wait();
        }

        #endregion

        #region User

        private static void ShowSampleUser()
        {
            Console.WriteLine("Sample JSON:");
            Console.WriteLine(Serializer.SerializeJson(new UserMaster
            {
                GUID = Guid.NewGuid(),
                FirstName = "First",
                LastName = "Last",
                Email = "first@last.com",
                Password = "password"
            }, false));
        }

        private static void UserExists()
        {
            EnumerateResult(_Sdk.UserExists(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Result);
        }

        private static void UserCreate()
        {
            ShowSampleUser();
            EnumerateResult(_Sdk.CreateUser(
                GetGuid("Tenant GUID:", _Tenant),
                Serializer.DeserializeJson<UserMaster>(GetJson("User JSON:"))).Result);
        }

        private static void UserAll()
        {
            EnumerateResult(_Sdk.ReadUsers(GetGuid("Tenant GUID:", _Tenant)).Result);
        }

        private static void UserRead()
        {
            EnumerateResult(_Sdk.ReadUser(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Result);
        }

        private static void UserUpdate()
        {
            ShowSampleUser();
            EnumerateResult(_Sdk.UpdateUser(
                GetGuid("Tenant GUID:", _Tenant),
                Serializer.DeserializeJson<UserMaster>(GetJson("User JSON:"))).Result);
        }

        private static void UserDelete()
        {
            _Sdk.DeleteUser(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Wait();
        }

        #endregion

        #region Credential

        private static void ShowSampleCredential()
        {
            Console.WriteLine("Sample JSON:");
            Console.WriteLine(Serializer.SerializeJson(new Credential
            {
                Name = "My credential"
            }, false));
        }

        private static void CredentialExists()
        {
            EnumerateResult(_Sdk.CredentialExists(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Result);
        }

        private static void CredentialCreate()
        {
            ShowSampleCredential();
            EnumerateResult(_Sdk.CreateCredential(
                GetGuid("Tenant GUID:", _Tenant),
                Serializer.DeserializeJson<Credential>(GetJson("Credential JSON:"))).Result);
        }

        private static void CredentialAll()
        {
            EnumerateResult(_Sdk.ReadCredentials(GetGuid("Tenant GUID:", _Tenant)).Result);
        }

        private static void CredentialRead()
        {
            EnumerateResult(_Sdk.ReadCredential(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Result);
        }

        private static void CredentialUpdate()
        {
            ShowSampleCredential();
            EnumerateResult(_Sdk.UpdateCredential(
                GetGuid("Tenant GUID:", _Tenant),
                Serializer.DeserializeJson<Credential>(GetJson("Credential JSON:"))).Result);
        }

        private static void CredentialDelete()
        {
            _Sdk.DeleteCredential(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Wait();
        }

        #endregion

        #region Tag

        private static void ShowSampleTag()
        {
            Console.WriteLine("Sample JSON:");
            Console.WriteLine(Serializer.SerializeJson(new TagMetadata
            {
                Key = "foo",
                Value = "bar"
            }, false));
        }

        private static void TagExists()
        {
            EnumerateResult(_Sdk.TagExists(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Result);
        }

        private static void TagCreate()
        {
            ShowSampleTag();
            EnumerateResult(_Sdk.CreateTag(
                GetGuid("Tenant GUID:", _Tenant),
                Serializer.DeserializeJson<TagMetadata>(GetJson("Tag JSON:"))).Result);
        }

        private static void TagAll()
        {
            EnumerateResult(_Sdk.ReadTags(GetGuid("Tenant GUID:", _Tenant)).Result);
        }

        private static void TagRead()
        {
            EnumerateResult(_Sdk.ReadTag(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Result);
        }

        private static void TagUpdate()
        {
            ShowSampleTag();
            EnumerateResult(_Sdk.UpdateTag(
                GetGuid("Tenant GUID:", _Tenant),
                Serializer.DeserializeJson<TagMetadata>(GetJson("Tag JSON:"))).Result);
        }

        private static void TagDelete()
        {
            _Sdk.DeleteTag(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Wait();
        }

        #endregion

        #region Graph

        private static void ShowSampleGraph()
        {
            List<string> labels = new List<string>();
            labels.Add("test");

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("key", "value");

            NameValueCollection tags = new NameValueCollection();
            tags.Add("key", "value");

            Console.WriteLine("Sample JSON:");
            Console.WriteLine(Serializer.SerializeJson(new Graph
            {
                Name = "Test graph",
                Labels = labels,
                Tags = tags,
                Data = data
            }, false));
        }

        private static void GraphExists()
        {
            EnumerateResult(_Sdk.GraphExists(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:")).Result);
        }

        private static void GraphCreate()
        {
            EnumerateResult(_Sdk.CreateGraph(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("Graph GUID:"), 
                GetName(), 
                GetLabels(),
                GetTags(),
                GetData()).Result);
        }

        private static void GraphAll()
        {
            EnumerateResult(_Sdk.ReadGraphs(GetGuid("Tenant GUID:", _Tenant)).Result);
        }

        private static void GraphRead()
        {
            EnumerateResult(_Sdk.ReadGraph(
                GetGuid("Tenant GUID:", _Tenant), 
                GetGuid("GUID:", _Graph)).Result);
        }

        private static void GraphUpdate()
        {
            ShowSampleGraph();
            EnumerateResult(_Sdk.UpdateGraph(
                GetGuid("Tenant GUID:", _Tenant),
                Serializer.DeserializeJson<Graph>(GetJson("Graph JSON:"))).Result);
        }

        private static void GraphDelete()
        {
            _Sdk.DeleteGraph(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("GUID:", _Graph), 
                GetBoolean("Force:")).Wait();
        }

        private static void GraphSearch()
        {
            SearchRequest req = BuildSearchRequest();
            if (req == null) return;
            EnumerateResult(_Sdk.SearchGraphs(
                GetGuid("Tenant GUID:", _Tenant),
                req).Result);
        }

        #endregion

        #region Node

        private static void ShowSampleNode()
        {
            List<string> labels = new List<string>();
            labels.Add("test");

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("key", "value");

            NameValueCollection tags = new NameValueCollection();
            tags.Add("key", "value");

            Console.WriteLine("Sample JSON:");

            Console.WriteLine(Serializer.SerializeJson(new Node
            {
                TenantGUID = _Tenant,
                GUID = _Graph,
                Name = "My node",
                Labels = labels,
                Tags = tags,
                Data = data
            }, false));
        }

        private static void NodeExists()
        {
            EnumerateResult(
                _Sdk.NodeExists(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void NodeCreate()
        {
            ShowSampleNode();
            EnumerateResult(
                _Sdk.CreateNode(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    Serializer.DeserializeJson<Node>(GetJson("Node JSON:"))
                )
                .Result);
        }

        private static void NodeAll()
        {
            EnumerateResult(_Sdk.ReadNodes(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("Graph GUID:", _Graph)).Result);
        }

        private static void NodeRead()
        {
            EnumerateResult(
                _Sdk.ReadNode(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void NodeUpdate()
        {
            ShowSampleNode();
            EnumerateResult(
                _Sdk.UpdateNode(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    Serializer.DeserializeJson<Node>(GetJson("Node JSON:"))
                )
                .Result);
        }

        private static void NodeDelete()
        {
            _Sdk.DeleteNode(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("Graph GUID:", _Graph), 
                GetGuid("Node GUID:")
                )
                .Wait();
        }
        
        private static void NodeSearch()
        {
            SearchRequest req = BuildSearchRequest();
            if (req == null) return;
            EnumerateResult(
                _Sdk.SearchNodes(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    req
                )
                .Result);
        }

        private static void NodeEdges()
        {
            EnumerateResult(
                _Sdk.GetAllNodeEdges(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:"),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void NodeParents()
        {
            EnumerateResult(
                _Sdk.GetParentsFromNode(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:"),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void NodeChildren()
        {
            EnumerateResult(
                _Sdk.GetChildrenFromNode(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:"),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        #endregion

        #region Edge

        private static void ShowSampleEdge()
        {
            List<string> labels = new List<string>();
            labels.Add("test");

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("key", "value");

            NameValueCollection tags = new NameValueCollection();
            tags.Add("key", "value");

            Console.WriteLine("Sample JSON:");

            Console.WriteLine(Serializer.SerializeJson(new Edge
            {
                TenantGUID = _Tenant,
                GUID = _Graph,
                From = Guid.NewGuid(),
                To = Guid.NewGuid(),
                Name = "My edge",
                Labels = labels,
                Tags = tags,
                Data = data
            }, false));
        }

        private static void EdgeExists()
        {
            EnumerateResult(
                _Sdk.EdgeExists(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    GetGuid("Edge GUID:")
                )
                .Result);
        }

        private static void EdgeCreate()
        {
            ShowSampleEdge();
            EnumerateResult(
                _Sdk.CreateEdge(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    Serializer.DeserializeJson<Edge>(GetJson("Edge JSON:"))
                )
                .Result);
        }

        private static void EdgeAll()
        {
            EnumerateResult(
                _Sdk.ReadEdges(
                    GetGuid("Tenant GUID:", _Tenant), 
                    GetGuid("Graph GUID:", _Graph)).Result);
        }

        private static void EdgeRead()
        {
            EnumerateResult(
                _Sdk.ReadEdge(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    GetGuid("Edge GUID:")
                )
                .Result);
        }

        private static void EdgeUpdate()
        {
            ShowSampleEdge();
            EnumerateResult(
                _Sdk.UpdateEdge(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    Serializer.DeserializeJson<Edge>(GetJson("Edge JSON:"))
                )
                .Result);
        }

        private static void EdgeDelete()
        {
            _Sdk.DeleteEdge(
                GetGuid("Tenant GUID:", _Tenant),
                GetGuid("Graph GUID:", _Graph),
                GetGuid("Edge GUID:")
                )
                .Wait();
        }

        private static void EdgeSearch()
        {
            SearchRequest req = BuildSearchRequest();
            if (req == null) return;
            EnumerateResult(
                _Sdk.SearchEdges(
                    GetGuid("Tenant GUID:", _Tenant), 
                    GetGuid("Graph GUID:", _Graph), 
                    req
                    )
                .Result);
        }

        private static void EdgesFrom()
        {
            EnumerateResult(
                _Sdk.GetEdgesFromNode(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    GetGuid("Node GUID:")
                )
                .Result);
        }

        private static void EdgesTo()
        {
            EnumerateResult(
                _Sdk.GetEdgesToNode(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    GetGuid("Edge GUID:")
                )
                .Result);
        }

        private static void EdgesBetween()
        {
            EnumerateResult(
                _Sdk.GetEdgesBetween(
                    GetGuid("Tenant GUID:", _Tenant),
                    GetGuid("Graph GUID:", _Graph),
                    GetGuid("From GUID :"),
                    GetGuid("To GUID   :")
                )
                .Result);
        }

        #endregion

        #region Route

        private static void Route()
        {
            EnumerateResult(
                _Sdk.GetRoutes(
                    GetGuid("Tenant GUID    :", _Tenant),
                    GetGuid("Graph GUID     :", _Graph),
                    GetGuid("From node GUID :"),
                    GetGuid("To node GUID   :")
                )
                .Result);
        }

        #endregion

#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier
    }
}