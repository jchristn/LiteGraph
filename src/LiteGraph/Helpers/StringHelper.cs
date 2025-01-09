namespace LiteGraph.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// String collection helpers.
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// Combine two string lists.
        /// </summary>
        /// <param name="list1">String list 1.</param>
        /// <param name="list2">String list 2.</param>
        /// <returns>String list.</returns>
        public static List<string> Combine(List<string> list1, List<string> list2)
        {
            if (list1 == null) return list2;
            if (list2 == null) return list1;
            List<string> ret = new List<string>();
            ret.AddRange(list1);
            ret.AddRange(list2);
            return ret;
        }
    }
}
