namespace LiteGraph.Serialization
{
    using System;

    /// <summary>
    /// Serializer interface.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Copy an object.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="obj">Object.</param>
        /// <returns>Copied object.</returns>
        T CopyObject<T>(T obj);

        /// <summary>
        /// Serialize an object to JSON.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="pretty">Enable or disable pretty formatting.</param>
        /// <returns></returns>
        string SerializeJson(object obj, bool pretty = false);

        /// <summary>
        /// Deserialize an object to JSON.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="json">JSON.</param>
        /// <returns>Instance.</returns>
        T DeserializeJson<T>(string json);
    }
}
