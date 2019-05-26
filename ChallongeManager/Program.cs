using System;
using System.Collections.Generic;
//using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using ChallongeManager.Challonge;
using System.IO;

namespace ChallongeManager
{
    class Program
    {
        private static SettingsObject settings;
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            // First argument will be tournament id
            // Use -i and -o to indicate input file and output file
            // One or both of input or output must be included
            // If input file is included and output is not, overwrite the input file
            // All these put together is 5 arguments maximum
            Dictionary<string, string> fileinfo = new Dictionary<string, string>();
            try
            {
                if (args.Length < 2 || args.Length % 2 == 1)
                    throw new ArgumentException();
                for (int i = 0; i < args.Length; i += 2)
                    fileinfo.Add(args[i], args[i + 1]);
                if (!fileinfo.ContainsKey("-o"))
                    fileinfo.Add("-o", fileinfo["-i"]);
                if (!fileinfo.ContainsKey("-t"))
                    throw new ArgumentException();
            }
            catch (Exception ex) when (
                    ex is ArgumentException ||
                    ex is IndexOutOfRangeException)
            {
                Console.WriteLine("Invalid arguments were given. " +
                    "Tournament and output file must be specified:\n" +
                    "Options:\n" +
                    "   -t Tournament url. This is the last part of the challonge " +
                    "url, challonge.com/<this part here>" +
                    "   -i Input file. Should be csv format, " +
                    "\"Player Name,Rating,Deviation,Volatility\", one player per line. " +
                    "If omitted will begin all players with default ratings.\n" +
                    "   -o Output file. If omitted will overwrite input file.");
                return;
            }

            // Get the settings information for connecting to challonge
            using (JsonTextReader settingsfile = new JsonTextReader(new StreamReader("settings.json")))
                settings = new JsonSerializer().Deserialize<SettingsObject>(settingsfile);

            GlickoSystem glicko = new GlickoSystem();

            // Get tournament info from challonge
            Console.WriteLine($"Finding tournament at https://challonge.com/{fileinfo["-t"]}");
            Task<Tournament> apitask = GetTourneyInfo(fileinfo["-t"]);
            // Load players
            try
            {
                if (fileinfo.ContainsKey("-i"))
                {
                    Console.WriteLine($"Loading players from {fileinfo["-i"]}");
                    await glicko.LoadPlayersAsync(fileinfo["-i"]);
                }
            }
            catch (FileNotFoundException)
            {
                Console.Write("Input file wasn't found or wasn't accessible. Continue anyways? (Y/N) ");
                string conf;
                while ((conf = Console.ReadLine().ToUpper()) != "Y"
                    || conf != "N") ;
                if (conf == "N")
                    return;
            }
            Tournament tourn = await apitask;
            if (tourn != null)
                Console.WriteLine($"Got tournament successfully. Found: {tourn.Name}");
            else
            {
                Console.WriteLine("Couldn't get tournament info.");
                return;
            }

            // Make sure the tournament is complete. The glicko-2 system is intended
            // to have multiple games per rating period
            if (tourn.State != TournamentState.Complete)
                Console.WriteLine("This tournament isn't completed yet! Try again after it's done.");
            else if (tourn.TournamentType == TournamentType.FreeForAll)
                Console.WriteLine("This looks like a Free For All tournament. " +
                    "Unfortunately only 1v1 matches can be calculated.");
            else if (tourn.TournamentType == TournamentType.Unrecognized)
                Console.WriteLine("This type of tournament hasn't been added to the program yet. " +
                    "Let me know so I can add it.");
            else
            {

                // The rating calculator uses names, so create a dictionary for those
                Console.WriteLine("Players:");
                Dictionary<int, string> names = new Dictionary<int, string>();
                foreach (Participant p in tourn.Participants)
                {
                    names.Add(p.Id, p.Name);
                    // If the tournament had a group stage, the players will have
                    // different ids for that stage. Associate those ids with names
                    if (p.GroupIds != null)
                        foreach (int id in p.GroupIds)
                            names.Add(id, p.Name);
                    Console.Write($"{p.Name} ");
                }
                Console.WriteLine();

                Console.WriteLine("Adding games:");
                // Add games from tournament
                foreach (Match match in tourn.Matches)
                {
                    if (match.State != TournamentState.Complete)
                        continue;
                    Console.WriteLine($"{names[(int)match.Player1Id]} {match.Score} {names[(int)match.Player2Id]}");
                    if (match.WinnerId != null)
                        glicko.AddGame(names[(int)match.WinnerId], names[(int)match.LoserId]);
                    else
                        glicko.AddDraw(names[(int)match.Player1Id], names[(int)match.Player2Id]);
                }

                Console.WriteLine("Updating ratings");
                glicko.UpdateRatings();
                Console.WriteLine($"Writing updated scores to {fileinfo["-o"]}");
                await glicko.WritePlayersAsync(fileinfo["-o"]);
                Console.Write("All Finished.");
            }
            client.Dispose();
        }

        private static async Task<Tournament> GetTourneyInfo(string tourney)
        {
            Tournament tournament = null;
            try
            {
                string response = await client.GetStringAsync(
                    $"{settings.ChallongeAPI}/tournaments/{tourney}.json" +
                    $"?api_key={settings.ChallongeKey}" +
                    $"&include_participants=1&include_matches=1"
                );
                tournament = JsonConvert.DeserializeAnonymousType(response, new { tournament = new Tournament() }).tournament;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return tournament;
        }
    }
}
