using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class FoglalasDto
{
    [JsonPropertyName("foglalasID")]
    public int Id { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(StringToIntConverter))]
    public int Status { get; set; }

    [JsonPropertyName("letrehozasDatum")]
    [JsonConverter(typeof(FlexibleDateTimeConverterNonNull))]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("foglalasKezdete")]
    [JsonConverter(typeof(FlexibleDateTimeConverter))]
    public DateTime? KezdetDatum { get; set; }

    [JsonPropertyName("kiadasIdopontja")]
    [JsonConverter(typeof(FlexibleDateTimeConverter))]
    public DateTime? KiadasDatum { get; set; }

    // FLAT STRUCTURE - nincs nested objektum!
    [JsonPropertyName("eszkozID")]
    public int EszkozID { get; set; }

    [JsonPropertyName("eszkozNev")]
    public string EszkozNev { get; set; } = null!;

    [JsonPropertyName("bevetel")]
    [JsonConverter(typeof(FlexibleNumberConverter))]
    public int? Ar { get; set; }

    [JsonPropertyName("nev")]
    public string? FelhasznaloNev { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("telefonszam")]
    public string? Telefonszam { get; set; }

    [JsonPropertyName("cim")]
    public string? Cim { get; set; }

    // Kompatibilitási property-k a régi XAML bindingokhoz
    [JsonIgnore]
    public EszkozDto Eszkoz => new EszkozDto
    {
        Nev = EszkozNev,
        Ar = Ar ?? 0
    };

    [JsonIgnore]
    public FelhasznaloDto Felhasznalo => new FelhasznaloDto
    {
        Nev = FelhasznaloNev
    };

    [JsonIgnore]
    public string StatusString => Status switch
    {
        1 => "Aktív",
        2 => "Kiadva",
        3 => "Lezárva",
        _ => Status.ToString()
    };
}

// Ezek most csak wrapper-ek
public class EszkozDto
{
    public string Nev { get; set; } = null!;
    public int Ar { get; set; }
}

public class FelhasznaloDto
{
    public string? Nev { get; set; }
}

// Custom JSON Converter string -> int
public class StringToIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            if (int.TryParse(stringValue, out int intValue))
            {
                return intValue;
            }

            // Backend státuszok
            return stringValue?.ToLower() switch
            {
                "aktiv" or "aktív" or "active" or "várakozik" or "varakozik" or "függőben" or "pending" => 1,
                "kiadva" or "issued" => 2,
                "lezarva" or "lezárva" or "closed" or "befejezett" or "completed" => 3,
                "torolt" or "törölt" or "deleted" => 0,  // Törölve = kihagyva
                _ => 0
            };
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }

        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

// Flexible Number Converter - kezeli a null, string és number típusokat
public class FlexibleNumberConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Ha null
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // Ha string
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            if (int.TryParse(stringValue, out int intValue))
            {
                return intValue;
            }

            if (double.TryParse(stringValue, out double doubleValue))
            {
                return (int)doubleValue;
            }

            return null;
        }

        // Ha szám
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int intValue))
            {
                return intValue;
            }

            if (reader.TryGetDouble(out double doubleValue))
            {
                return (int)doubleValue;
            }
        }

        return null;
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

// Flexible DateTime Converter - kezeli a string formátumú dátumokat
public class FlexibleDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            if (DateTime.TryParse(stringValue, out DateTime dateValue))
            {
                return dateValue;
            }

            return null;
        }

        // Ha már DateTime objektum
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

// Non-nullable DateTime converter
public class FlexibleDateTimeConverterNonNull : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (DateTime.TryParse(stringValue, out DateTime dateValue))
            {
                return dateValue;
            }
            return DateTime.MinValue;
        }

        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}