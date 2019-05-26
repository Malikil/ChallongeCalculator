using System.Collections.Generic;
using System.Threading.Tasks;
using Glicko2;
using System.IO;
using System.Linq;

namespace ChallongeManager
{
    class GlickoSystem
    {
        private readonly Dictionary<string, Rating> playerlist;
        private readonly RatingCalculator calculator;
        private readonly RatingPeriodResults results;

        public GlickoSystem()
        {
            playerlist = new Dictionary<string, Rating>();
            calculator = new RatingCalculator();
            results = new RatingPeriodResults();
        }

        public async Task LoadPlayersAsync(string filename)
        {
            using (StreamReader infile = new StreamReader(filename))
            {
                string line;
                while ((line = await infile.ReadLineAsync()) != null)
                {
                    string[] info = line.Split(',');
                    Rating player = new Rating(
                        calculator,
                        double.Parse(info[1]),
                        double.Parse(info[2]),
                        double.Parse(info[3])
                    );
                    results.AddParticipant(player);
                    playerlist.Add(info[0], player);
                }
            }
        }

        public async Task WritePlayersAsync(string filename)
        {
            using (StreamWriter outfile = new StreamWriter(filename))
            {
                foreach (string key in playerlist.Keys.OrderBy(k => k))
                {
                    Rating player = playerlist[key];
                    await outfile.WriteLineAsync($"{key}," +
                        $"{player.GetRating()}," +
                        $"{player.GetRatingDeviation()}," +
                        $"{player.GetVolatility()}");
                }
            }
        }

        public void AddGame(string winner, string loser) =>
            AddGame(winner, loser, false);
        public void AddDraw(string player1, string player2) =>
            AddGame(player1, player2, true);

        public void UpdateRatings()
        {
            calculator.UpdateRatings(results);
        }

        // Private methods

        private void AddGame(string winner, string loser, bool isDraw)
        {
            // If the players don't already exist, add them
            if (!playerlist.ContainsKey(winner))
                playerlist.Add(winner, new Rating(calculator));
            if (!playerlist.ContainsKey(loser))
                playerlist.Add(loser, new Rating(calculator));

            if (isDraw)
                results.AddDraw(playerlist[winner], playerlist[loser]);
            else
                results.AddResult(playerlist[winner], playerlist[loser]);
        }
    }
}
