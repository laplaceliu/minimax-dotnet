using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiniMax.Core;

public class JsonEnumStringConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumStringConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private class EnumStringConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        private readonly Dictionary<string, TEnum> _stringToEnum;
        private readonly Dictionary<TEnum, string> _enumToString;

        public EnumStringConverter()
        {
            var enumType = typeof(TEnum);
            var enumNames = enumType.GetEnumNames();
            var enumValues = enumType.GetEnumValues();

            _stringToEnum = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);
            _enumToString = new Dictionary<TEnum, string>();

            for (int i = 0; i < enumNames.Length; i++)
            {
                var enumName = enumNames[i];
                var enumValue = (TEnum)enumValues.GetValue(i)!;
                var field = enumType.GetField(enumName);

                var jsonPropertyName = field?.GetCustomAttribute<JsonPropertyNameAttribute>();
                var stringValue = jsonPropertyName?.Name ?? enumName;

                _stringToEnum[stringValue] = enumValue;
                _enumToString[enumValue] = stringValue;
            }
        }

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? stringValue = null;

            if (reader.TokenType == JsonTokenType.String)
            {
                stringValue = reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                stringValue = reader.GetInt64().ToString();
            }

            if (stringValue != null && _stringToEnum.TryGetValue(stringValue, out var enumValue))
            {
                return enumValue;
            }

            throw new JsonException($"Invalid value '{stringValue}' for enum {typeToConvert.Name}");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            if (_enumToString.TryGetValue(value, out var stringValue))
            {
                if (long.TryParse(stringValue, out var numValue))
                {
                    writer.WriteNumberValue(numValue);
                }
                else
                {
                    writer.WriteStringValue(stringValue);
                }
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}