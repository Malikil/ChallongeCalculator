using Newtonsoft.Json;

namespace ChallongeManager.Challonge
{
    class Tournament
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("state")]
        [JsonConverter(typeof(TournamentStateConverter))]
        public TournamentState State { get; set; }

        [JsonProperty("tournament_type")]
        [JsonConverter(typeof(TournamentTypeConverter))]
        public TournamentType TournamentType { get; set; }

        [JsonProperty("participants")]
        [JsonConverter(typeof(ChallongeConverter<Participant>))]
        public Participant[] Participants { get; set; }

        [JsonProperty("matches")]
        [JsonConverter(typeof(ChallongeConverter<Match>))]
        public Match[] Matches { get; set; }
    }
}
