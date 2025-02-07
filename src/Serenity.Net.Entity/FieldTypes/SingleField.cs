﻿namespace Serenity.Data;

/// <summary>
/// Field with a Single value
/// </summary>
public sealed class SingleField : GenericValueField<float>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleField"/> class.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="name">The name.</param>
    /// <param name="caption">The caption.</param>
    /// <param name="size">The size.</param>
    /// <param name="flags">The flags.</param>
    /// <param name="getValue">The get value.</param>
    /// <param name="setValue">The set value.</param>
    public SingleField(ICollection<Field> collection, string name, LocalText caption = null, int size = 0, FieldFlags flags = FieldFlags.Default,
        Func<IRow, float?> getValue = null, Action<IRow, float?> setValue = null)
        : base(collection, FieldType.Single, name, caption, size, flags, getValue, setValue)
    {
    }

    /// <summary>
    /// Static factory for field, for backward compatibility, avoid using.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="name">The name.</param>
    /// <param name="caption">The caption.</param>
    /// <param name="size">The size.</param>
    /// <param name="flags">The flags.</param>
    /// <param name="getValue">The get value.</param>
    /// <param name="setValue">The set value.</param>
    /// <returns></returns>
    public static SingleField Factory(ICollection<Field> collection, string name, LocalText caption, int size, FieldFlags flags,
        Func<IRow, float?> getValue, Action<IRow, float?> setValue)
    {
        return new SingleField(collection, name, caption, size, flags, getValue, setValue);
    }

    /// <summary>
    /// Gets field value from a data reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="row">The row.</param>
    /// <exception cref="ArgumentNullException">reader</exception>
    public override void GetFromReader(IDataReader reader, int index, IRow row)
    {
        if (reader == null)
            throw new ArgumentNullException("reader");

        var value = reader.GetValue(index);
        if (value is DBNull)
            _setValue(row, null);
        else if (value is float f)
            _setValue(row, f);
        else
            _setValue(row, Convert.ToSingle(value, CultureInfo.InvariantCulture));

        row.FieldAssignedValue(this);
    }

    /// <summary>
    /// Serializes this fields value to JSON
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="row">The row.</param>
    /// <param name="serializer">The serializer.</param>
    public override void ValueToJson(JsonWriter writer, IRow row, JsonSerializer serializer)
    {
        writer.WriteValue(_getValue(row));
    }

    /// <summary>
    /// Deserializes this fields value from JSON
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="row">The row.</param>
    /// <param name="serializer">The serializer.</param>
    /// <exception cref="ArgumentNullException">reader</exception>
    public override void ValueFromJson(JsonReader reader, IRow row, JsonSerializer serializer)
    {
        if (reader == null)
            throw new ArgumentNullException("reader");

        switch (reader.TokenType)
        {
            case JsonToken.Null:
            case JsonToken.Undefined:
                _setValue(row, null);
                break;
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.Boolean:
                _setValue(row, Convert.ToSingle(reader.Value, CultureInfo.InvariantCulture));
                break;
            case JsonToken.String:
                var s = ((string)reader.Value).TrimToNull();
                if (s == null)
                    _setValue(row, null);
                else
                    _setValue(row, Convert.ToSingle(s, CultureInfo.InvariantCulture));
                break;
            default:
                throw JsonUnexpectedToken(reader);
        }

        row.FieldAssignedValue(this);
    }
}
