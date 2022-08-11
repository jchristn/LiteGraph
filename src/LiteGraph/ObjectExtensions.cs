using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace LiteGraph
{
    /// <summary>
    /// Object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Create a JSON representation of the object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public static string ToJson(this object obj, bool pretty = true)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return JsonConvert.SerializeObject(
                obj, 
                (pretty ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None), new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }
            );
        } 
    }
}
