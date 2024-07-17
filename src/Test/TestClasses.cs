using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class GraphMetadata
    {
        public string Description { get; set; } = null;
    }

    public class Person
    {
        public string Name { get; set; } = null;
        public bool IsHandsome { get; set; } = true;
        public int Age { get; set; } = 0;
        public override string ToString()
        {
            return "Name: " + Name + " handsome: " + IsHandsome + " age: " + Age;
        }
    }

    public class ISP
    {
        public string Name { get; set; } = null;
    }

    public class Internet
    {
        public string Name { get; } = "Internet";
    }

    public class HostingProvider
    {
        public string Name { get; set; } = null;
    }

    public class Application
    {
        public string Name { get; set; } = null;
    }
}
