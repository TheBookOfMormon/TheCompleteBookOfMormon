using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentsModel;

[JsonConverter(typeof(EquatableListJsonConverterFactory))]
[CollectionBuilder(typeof(EquatableList), nameof(EquatableList.Create))]
public sealed record EquatableList<T>(ImmutableList<T> Items) : IReadOnlyList<T>
{
    public static readonly EquatableList<T> Empty = new(ImmutableList<T>.Empty);

    public int Count => Items.Count;
    public T this[int index] => Items[index];

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public bool Equals(EquatableList<T>? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        return Items.SequenceEqual(other.Items);
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        foreach (T item in Items)
            hash.Add(item);
        return hash.ToHashCode();
    }

    public static implicit operator EquatableList<T>(ImmutableList<T> items) => new(items);
    public static implicit operator ImmutableList<T>(EquatableList<T> wrapper) => wrapper.Items;
}

public static class EquatableList
{
    public static EquatableList<T> Create<T>(ReadOnlySpan<T> items)
    {
        ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
        foreach (T item in items)
            builder.Add(item);
        return new EquatableList<T>(builder.ToImmutable());
    }
}

public class EquatableListJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(EquatableList<>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type elementType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(EquatableListJsonConverter<>).MakeGenericType(elementType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

public class EquatableListJsonConverter<T> : JsonConverter<EquatableList<T>>
{
    public override EquatableList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            T? item = JsonSerializer.Deserialize<T>(ref reader, options);
            if (item is not null)
                builder.Add(item);
        }
        return new EquatableList<T>(builder.ToImmutable());
    }

    public override void Write(Utf8JsonWriter writer, EquatableList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (T item in value.Items)
            JsonSerializer.Serialize(writer, item, options);
        writer.WriteEndArray();
    }
}
