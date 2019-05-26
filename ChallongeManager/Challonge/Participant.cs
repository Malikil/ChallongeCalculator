using Newtonsoft.Json;

namespace ChallongeManager.Challonge
{
    class Participant
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("group_player_ids")] public int[] GroupIds { get; set; }
    }
}
