﻿namespace LiteGraph.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using ExpressionTree;

    /// <summary>
    /// Serialization helper.
    /// </summary>
    public class SerializationHelper : ISerializer
    {
        /// <inheritdoc/>
        public T CopyObject<T>(T obj)
        {
            if (obj == null) return default(T);

            string json = SerializeJson(obj, false);
            try
            {
                return DeserializeJson<T>(json);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <inheritdoc/>
        public T DeserializeJson<T>(string json)
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new ExceptionConverter<Exception>());
            options.Converters.Add(new NameValueCollectionConverter());
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new DateTimeConverter());
            options.Converters.Add(new IPAddressConverter());
            options.Converters.Add(new ExpressionConverter());

            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <inheritdoc/>
        public string SerializeJson(object obj, bool pretty = false)
        {
            if (obj == null) return null;
            string json;

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.Converters.Add(new ExceptionConverter<Exception>());
            options.Converters.Add(new NameValueCollectionConverter());
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new DateTimeConverter());
            options.Converters.Add(new IPAddressConverter());
            options.Converters.Add(new ExpressionConverter());

            if (pretty)
            {
                options.WriteIndented = true;
                json = JsonSerializer.Serialize(obj, options);
            }
            else
            {
                options.WriteIndented = false;
                json = JsonSerializer.Serialize(obj, options);
            }

            return json;
        }

        private class ExceptionConverter<TExceptionType> : JsonConverter<TExceptionType>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeof(Exception).IsAssignableFrom(typeToConvert);
            }

            public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotSupportedException("Deserializing exceptions is not allowed");
            }

            public override void Write(Utf8JsonWriter writer, TExceptionType value, JsonSerializerOptions options)
            {
                var serializableProperties = value.GetType()
                    .GetProperties()
                    .Select(uu => new { uu.Name, Value = uu.GetValue(value) })
                    .Where(uu => uu.Name != nameof(Exception.TargetSite));

                if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
                {
                    serializableProperties = serializableProperties.Where(uu => uu.Value != null);
                }

                var propList = serializableProperties.ToList();

                if (propList.Count == 0)
                {
                    // Nothing to write
                    return;
                }

                writer.WriteStartObject();

                foreach (var prop in propList)
                {
                    writer.WritePropertyName(prop.Name);
                    JsonSerializer.Serialize(writer, prop.Value, options);
                }

                writer.WriteEndObject();
            }
        }

        private class NameValueCollectionConverter : JsonConverter<NameValueCollection>
        {
            public override NameValueCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected start of object");
                }

                var collection = new NameValueCollection();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return collection;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException("Expected property name");
                    }

                    string key = reader.GetString();

                    reader.Read();
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        collection.Add(key, null);
                        continue;
                    }

                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException("Expected string value");
                    }

                    string value = reader.GetString();

                    // If the value contains commas, split it and add each value separately
                    if (!string.IsNullOrEmpty(value) && value.Contains(","))
                    {
                        var values = value.Split(',')
                                        .Select(v => v.Trim());
                        foreach (var v in values)
                        {
                            collection.Add(key, v);
                        }
                    }
                    else
                    {
                        collection.Add(key, value);
                    }
                }

                throw new JsonException("Expected end of object");
            }

            public override void Write(Utf8JsonWriter writer, NameValueCollection value, JsonSerializerOptions options)
            {
                var val = value.Keys.Cast<string>()
                    .ToDictionary(k => k, k => string.Join(", ", value.GetValues(k)));
                System.Text.Json.JsonSerializer.Serialize(writer, val);
            }
        }

        private class IPAddressConverter : JsonConverter<IPAddress>
        {
            public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string str = reader.GetString();
                return IPAddress.Parse(str);
            }

            public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class DateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(
                        ref Utf8JsonReader reader,
                        Type typeToConvert,
                        JsonSerializerOptions options)
            {
                string str = reader.GetString();

                DateTime val;
                if (DateTime.TryParse(str, out val)) return val;

                throw new FormatException("The JSON value '" + str + "' could not be converted to System.DateTime.");
            }

            public override void Write(
                Utf8JsonWriter writer,
                DateTime dateTimeValue,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(dateTimeValue.ToString(
                    "yyyy-MM-ddTHH:mm:ss.ffffffZ", CultureInfo.InvariantCulture));
            }

            private List<string> _AcceptedFormats = new List<string>
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssK",
                "yyyy-MM-dd HH:mm:ss.ffffff",
                "yyyy-MM-ddTHH:mm:ss.ffffff",
                "yyyy-MM-ddTHH:mm:ss.fffffffK",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy hh:mm tt",
                "MM/dd/yyyy H:mm",
                "MM/dd/yyyy h:mm tt",
                "MM/dd/yyyy HH:mm:ss"
            };
        }

        private class ExpressionConverter : JsonConverter<Expr>
        {
            public override Expr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected start of object");
                }

                Expr expr = new Expr();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return expr;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "Left":
                                expr.Left = ReadValue(ref reader);
                                break;
                            case "Operator":
                                expr.Operator = Enum.Parse<OperatorEnum>(reader.GetString());
                                break;
                            case "Right":
                                expr.Right = ReadValue(ref reader);
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }

                return expr;
            }

            private object ReadValue(ref Utf8JsonReader reader)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        return reader.GetString();
                    case JsonTokenType.Number:
                        if (reader.TryGetInt64(out long longValue))
                            return longValue;
                        return reader.GetDouble();
                    case JsonTokenType.True:
                        return true;
                    case JsonTokenType.False:
                        return false;
                    case JsonTokenType.Null:
                        return null;
                    case JsonTokenType.StartObject:
                        return JsonSerializer.Deserialize<Expr>(ref reader, new JsonSerializerOptions { Converters = { new ExpressionConverter() } });
                    case JsonTokenType.StartArray:
                        List<object> list = new List<object>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            list.Add(ReadValue(ref reader));
                        }
                        return list;
                    default:
                        throw new JsonException($"Unexpected token type: {reader.TokenType}");
                }
            }

            public override void Write(Utf8JsonWriter writer, Expr value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("Left");
                WriteValue(writer, value.Left);

                writer.WritePropertyName("Operator");
                writer.WriteStringValue(value.Operator.ToString());

                writer.WritePropertyName("Right");
                WriteValue(writer, value.Right);

                writer.WriteEndObject();
            }

            private void WriteValue(Utf8JsonWriter writer, object value)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                }
                else if (value is string str)
                {
                    writer.WriteStringValue(str);
                }
                else if (value is long l)
                {
                    writer.WriteNumberValue(l);
                }
                else if (value is int i)
                {
                    writer.WriteNumberValue(i);
                }
                else if (value is double d)
                {
                    writer.WriteNumberValue(d);
                }
                else if (value is bool b)
                {
                    writer.WriteBooleanValue(b);
                }
                else if (value is Expr expr)
                {
                    Write(writer, expr, null);
                }
                else if (value is IEnumerable<object> list)
                {
                    writer.WriteStartArray();
                    foreach (var item in list)
                    {
                        WriteValue(writer, item);
                    }
                    writer.WriteEndArray();
                }
                else
                {
                    throw new JsonException($"Unexpected value type: {value.GetType()}");
                }
            }
        }
    }
}
