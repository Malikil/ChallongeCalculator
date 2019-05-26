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
                if (args.Length != 3 && args.Length != 5)
                    throw new ArgumentException();
                if (args[1] == "-i" || args[1] == "-o")
                    fileinfo.Add(args[1], args[2]);
                else
                    throw new ArgumentException();
                if (args.Length == 5)
                    if (args[3] == "-i" || args[3] == "-o")
                        fileinfo.Add(args[3], args[4]);
                    else
                        throw new ArgumentException();
                if (!fileinfo.ContainsKey("-o"))
                    fileinfo.Add("-o", fileinfo["-i"]);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid arguments were given. Proper usage:\n" +
                    "ChallongeManager.exe [tournament url] [file args]\n\n" +
                    "File argumens:\n" +
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
            Console.WriteLine($"Finding tournament at https://challonge.com/{args[0]}");
            Task<Tournament> apitask = GetTourneyInfo(args[0]);
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
            Console.WriteLine($"Got tournament successfully. Found: {tourn.Name}");

            // Make sure the tournament is complete. The glicko-2 system is intended
            // to have multiple games per rating period
            if (tourn.State != TournamentState.Complete)
            {
                Console.WriteLine("This tournament isn't completed yet! Try again after it's done.");
                return;
            }

            // The rating calculator uses names, so create a dictionary for those
            Console.WriteLine("Players:");
            Dictionary<int, string> names = new Dictionary<int, string>();
            foreach (Participant p in tourn.Participants)
            {
                names.Add(p.Id, p.Name);
                Console.Write($"{p.Name} ");
            }
            Console.WriteLine();

            Console.WriteLine("Adding games:");
            // Add games from tournament
            foreach (Match match in tourn.Matches)
            {
                Console.WriteLine($"{names[match.Player1Id]} {match.Score} {names[match.Player2Id]}");
                glicko.AddGame(names[match.WinnerId], names[match.LoserId]);
            }

            Console.WriteLine("Updating ratings");
            glicko.UpdateRatings();
            Console.WriteLine($"Writing updated scores to {fileinfo["-o"]}");
            await glicko.WritePlayersAsync(fileinfo["-o"]);

            client.Dispose();
            Console.Write("All Finished. Press any key to continue...");
            Console.ReadKey();
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
