using Newtonsoft.Json;

namespace ChallongeManager.Challonge
{
    class Match
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("tournament_id")] public int TournamentId { get; set; }
        [JsonProperty("state")] public string State { get; set; }
        [JsonProperty("player1_id")] public int Player1Id { get; set; }
        [JsonProperty("player2_id")] public int Player2Id { get; set; }
        [JsonProperty("winner_id")] public int WinnerId { get; set; }
        [JsonProperty("loser_id")] public int LoserId { get; set; }
    }
}
