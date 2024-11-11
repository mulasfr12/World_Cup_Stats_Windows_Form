using DataLayer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    public class DataProcessor
    {
        private readonly DataFetcher _fetcher;

        public DataProcessor(DataFetcher fetcher)
        {
            _fetcher = fetcher;
        }

        public async Task<List<Team>> GetTeams(string endpoint)
        {
            try
            {
                var jsonData = await _fetcher.FetchData(endpoint);
                return string.IsNullOrWhiteSpace(jsonData) ? new List<Team>() :
                    JsonConvert.DeserializeObject<List<Team>>(jsonData);
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error parsing teams data: {jsonEx.Message}");
                return new List<Team>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return new List<Team>();
            }
        }

        public async Task<List<Country.Root>> GetMatches(string endpoint)
        {
            try
            {
                var jsonData = await _fetcher.FetchData(endpoint);
                return string.IsNullOrWhiteSpace(jsonData) ? new List<Country.Root>() :
                    JsonConvert.DeserializeObject<List<Country.Root>>(jsonData);
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error parsing matches data: {jsonEx.Message}");
                return new List<Country.Root>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return new List<Country.Root>();
            }
        }
        public Task<List<Team>> GetTeamsFromJson(string jsonFilePath)
        {
            try
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                var teams = JsonConvert.DeserializeObject<List<Team>>(jsonData);
                return Task.FromResult(teams);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading teams from JSON: {ex.Message}");
                return Task.FromResult(new List<Team>());
            }
        }

        public Task<List<Country.Root>> GetMatchesFromJson(string jsonFilePath)
        {
            try
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                var matches = JsonConvert.DeserializeObject<List<Country.Root>>(jsonData);
                return Task.FromResult(matches);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading matches from JSON: {ex.Message}");
                return Task.FromResult(new List<Country.Root>());
            }
        }
        public async Task<List<Results>> GetResultsApi(string fifaCode, bool isMen)
        {
            try
            {
                // Choose the endpoint based on gender
                var endpoint = isMen
                    ? "https://worldcup-vua.nullbit.hr/men/teams/results/"
                    : "https://worldcup-vua.nullbit.hr/women/teams/results/";

                var jsonData = await _fetcher.FetchData(endpoint);

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    Console.WriteLine("API returned empty or null JSON data.");
                    return new List<Results>();
                }

                var allResults = JsonConvert.DeserializeObject<List<Results>>(jsonData);

                // Filter for the selected FIFA code
                return allResults?.Where(r => r.Fifa_Code.Equals(fifaCode, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error parsing results data: {jsonEx.Message}");
                return new List<Results>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return new List<Results>();
            }
        }

        public Task<List<Results>> GetResultsFromJson(string jsonFilePath, string fifaCode)
        {
            try
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                var allResults = JsonConvert.DeserializeObject<List<Results>>(jsonData);

                // Check if any data was deserialized
                if (allResults == null || !allResults.Any())
                {
                    Console.WriteLine("JSON data deserialization returned no results.");
                    return Task.FromResult(new List<Results>());
                }

                // Filter results based on FIFA code
                var filteredResults = allResults.Where(r => r.Fifa_Code.Equals(fifaCode, StringComparison.OrdinalIgnoreCase)).ToList();
                Console.WriteLine($"Found {filteredResults.Count} result(s) for FIFA code: {fifaCode}");

                return Task.FromResult(filteredResults);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading results from JSON: {ex.Message}");
                return Task.FromResult(new List<Results>());
            }
        }

    }
}
