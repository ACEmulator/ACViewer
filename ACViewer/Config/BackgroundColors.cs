using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace ACViewer.Config
{
    public class BackgroundColors
    {
        [JsonConverter(typeof(JsonConverter_Color))]
        public Color ModelViewer { get; set; }
        
        [JsonConverter(typeof(JsonConverter_Color))] 
        public Color ParticleViewer { get; set; }
        
        [JsonConverter(typeof(JsonConverter_Color))] 
        public Color TextureViewer { get; set; }
        
        [JsonConverter(typeof(JsonConverter_Color))] 
        public Color WorldViewer { get; set; }

        public BackgroundColors()
        {
            // defaults
            ModelViewer = Color.Black;
            ParticleViewer = Color.Black;
            TextureViewer = Color.Black;
            WorldViewer = new Color(32, 32, 32);
        }
    }

    public sealed class JsonConverter_Color : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Color).Equals(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var color = (Color)value;

            writer.WriteValue($"#{color.R:X2}{color.G:X2}{color.B:X2}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.Value.ToString();

            if (str == null || str.Length != 7 || str[0] != '#')
                return Color.Black;

            if (!byte.TryParse(str.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
                !byte.TryParse(str.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
                !byte.TryParse(str.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
            {
                return Color.Black;
            }
            return new Color(r, g, b);
        }
    }
}
