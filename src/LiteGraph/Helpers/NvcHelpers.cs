namespace LiteGraph.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Name-value collection helpers.
    /// </summary>
    public static class NvcHelpers
    {
        /// <summary>
        /// Combine two name-value collections.
        /// </summary>
        /// <param name="nvc1">Name-value collection 1.</param>
        /// <param name="nvc2">Name-value collection 2.</param>
        /// <returns>Name-value collection.</returns>
        public static NameValueCollection Combine(NameValueCollection nvc1, NameValueCollection nvc2)
        {
            if (nvc1 == null) return nvc2;
            if (nvc2 == null) return nvc1;
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add(nvc1);
            nvc.Add(nvc2);
            return nvc;
        }
    }
}
