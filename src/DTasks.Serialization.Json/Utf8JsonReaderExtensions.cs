﻿using System.Text.Json;

namespace DTasks.Serialization.Json;

internal static class Utf8JsonReaderExtensions
{
    public static bool IsProperty(this ref readonly Utf8JsonReader reader, string propertyName)
    {
        return reader.TokenType is JsonTokenType.PropertyName && reader.ValueTextEquals(propertyName);
    }

    public static bool IsProperty(this ref readonly Utf8JsonReader reader, ReadOnlySpan<byte> propertyName)
    {
        return reader.TokenType is JsonTokenType.PropertyName && reader.ValueTextEquals(propertyName);
    }

    public static void MoveNext(this ref Utf8JsonReader reader)
    {
        if (!reader.Read())
            throw new JsonException("Unexpected end of json.");
    }

    public static void ExpectEnd(this ref Utf8JsonReader reader)
    {
        if (reader.Read())
            throw new JsonException("Expected end of json.");
    }

    public static void ExpectType(this ref readonly Utf8JsonReader reader, JsonTokenType expectedType)
    {
        JsonTokenType actualType = reader.TokenType;

        if (actualType != expectedType)
            throw new JsonException($"Expected token type '{expectedType}', got '{actualType}' instead.");
    }
}
