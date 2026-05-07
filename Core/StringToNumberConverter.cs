using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiniMax.Core;

public class StringToNumberConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(int) || typeToConvert == typeof(long) ||
               typeToConvert == typeof(int?) || typeToConvert == typeof(long?);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(int))
        {
            return new IntConverter();
        }
        if (typeToConvert == typeof(int?))
        {
            return new NullableIntConverter();
        }
        if (typeToConvert == typeof(long))
        {
            return new LongConverter();
        }
        if (typeToConvert == typeof(long?))
        {
            return new NullableLongConverter();
        }
        throw new ArgumentException($"Unsupported type: {typeToConvert}");
    }

    private class IntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    throw new JsonException("Cannot convert empty or null string to int");
                }
                if (int.TryParse(stringValue, out var result))
                {
                    return result;
                }
                throw new JsonException($"Cannot convert string '{stringValue}' to int");
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    private class NullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }
                if (int.TryParse(stringValue, out var result))
                {
                    return result;
                }
                throw new JsonException($"Cannot convert string '{stringValue}' to int?");
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    private class LongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    throw new JsonException("Cannot convert empty or null string to long");
                }
                if (long.TryParse(stringValue, out var result))
                {
                    return result;
                }
                throw new JsonException($"Cannot convert string '{stringValue}' to long");
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    private class NullableLongConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }
                if (long.TryParse(stringValue, out var result))
                {
                    return result;
                }
                throw new JsonException($"Cannot convert string '{stringValue}' to long?");
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}