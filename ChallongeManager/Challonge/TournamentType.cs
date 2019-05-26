using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChallongeManager.Challonge
{
    class TournamentTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TournamentType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            switch (token.ToString())
            {
                case "double elimination":
                    return TournamentType.DoubleElimination;
                case "free for all":
                    return TournamentType.FreeForAll;
                case "swiss":
                    return TournamentType.Swiss;
                default:
                    return TournamentType.Unrecognized;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    enum TournamentType
    {
        Unrecognized, FreeForAll, Swiss, DoubleElimination
    }
}
