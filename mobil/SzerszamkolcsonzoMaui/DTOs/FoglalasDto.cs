using System.Text.Json;
using System.Text.Json.Serialization;

namespace SzerszamkolcsonzoMaui.DTOs
{
    public class FoglalasDto
    {
        [JsonPropertyName("foglalasID")]
        public int Id { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(StatusStringToIntConverter))]
        public int Status { get; set; }

        [JsonPropertyName("letrehozasDatum")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("foglalasKezdete")]
        public DateTime? KezdetDatum { get; set; }

        [JsonPropertyName("foglalasVege")]
        public DateTime? VegeDatum { get; set; }

        [JsonPropertyName("kiadasIdopontja")]
        public DateTime? KiadasDatum { get; set; }

        [JsonPropertyName("eszkozID")]
        public int EszkozID { get; set; }

        [JsonPropertyName("eszkozNev")]
        public string EszkozNev { get; set; } = "";

        [JsonPropertyName("bevetel")]
        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? Ar { get; set; }

        [JsonPropertyName("nev")]
        public string? FelhasznaloNev { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("telefonszam")]
        public string? Telefonszam { get; set; }

        // ── Computed properties ──────────────────────────────────────────
        [JsonIgnore]
        public string StatusString => Status switch
        {
            1 => "FOGLALVA",
            2 => "KIADVA",
            3 => "LEZÁRVA",
            _ => "TÖRÖLT"
        };

        [JsonIgnore]
        public Color StatusColor => Status switch
        {
            1 => Color.FromArgb("#FF9800"),
            2 => Color.FromArgb("#2196F3"),
            3 => Color.FromArgb("#4CAF50"),
            _ => Color.FromArgb("#9E9E9E")
        };

        [JsonIgnore]
        public bool IsKiadva => Status == 2;

        [JsonIgnore]
        public bool IsFoglalva => Status == 1;

        [JsonIgnore]
        public string IdopontString =>
            KezdetDatum.HasValue
                ? VegeDatum.HasValue
                    ? $"{KezdetDatum:MM.dd. HH:mm} – {VegeDatum:HH:mm}"
                    : $"{KezdetDatum:MM.dd. HH:mm}"
                : "-";

        [JsonIgnore]
        public string ArString => Ar.HasValue ? $"{Ar:N0} Ft" : "-";
    }

    /// <summary>
    /// Kezeli a null / string / int / float → int? konverziót (pl. bevetel mező).
    /// </summary>
    public class FlexibleIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int i)) return i;
                if (reader.TryGetDouble(out double d)) return (int)d;
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (int.TryParse(s, out int i)) return i;
                if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double d))
                    return (int)d;
                return null;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue) writer.WriteNumberValue(value.Value);
            else writer.WriteNullValue();
        }
    }

    /// <summary>
    /// Kezeli a backend string státuszait (pl. "Foglalva") és számokat is.
    /// </summary>
    public class StatusStringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString()?.ToLower() switch
                {
                    "foglalva" or "aktiv" or "aktív" or "pending" or "varakozik" => 1,
                    "kiadva" or "issued" => 2,
                    "lezart" or "lezárva" or "lezarva" or "completed" or "closed" => 3,
                    "torolt" or "törölt" or "deleted" => 0,
                    _ => 0
                };
            }

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value);
    }
}
