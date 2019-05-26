using System.Collections.Generic;
using System.Threading.Tasks;
using Glicko2;
using System.IO;

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
                foreach (string key in playerlist.Keys)
                {
                    Rating player = playerlist[key];
                    await outfile.WriteLineAsync($"{key}," +
                        $"{player.GetRating()}," +
                        $"{player.GetRatingDeviation()}," +
                        $"{player.GetVolatility()}");
                }
            }
        }
    }
}
