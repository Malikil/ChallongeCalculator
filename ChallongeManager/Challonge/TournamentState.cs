using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChallongeManager.Challonge
{
    class TournamentStateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TournamentState);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            switch (token.ToString())
            {
                case "complete":
                    return TournamentState.Complete;
                case "underway":
                    return TournamentState.Underway;
                case "pending":
                    return TournamentState.Pending;
                case "open":
                    return TournamentState.Open;
                case "awaiting_review":
                    return TournamentState.AwaitingReview;
                default:
                    throw new JsonSerializationException($"Error converting value {token.ToString()} to type 'TournamentState'.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    enum TournamentState
    {
        Complete, Underway, Pending, Open, AwaitingReview
    }
}
