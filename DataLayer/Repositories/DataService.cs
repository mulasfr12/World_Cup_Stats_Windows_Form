using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    public class DataService
    {
        private DataProcessor _dataProcessor;
        private ApiDataFetcher _dataFetcher;
        private JsonFileDataFetcher _jsonFileDataFetcher;
        private readonly string menJsonFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Men");
        private readonly string womenJsonFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Women");

        public DataService(bool useApi)
        {
            try
            {
                if (useApi)
                {
                    _dataProcessor = new DataProcessor(new ApiDataFetcher());
                }
                else
                {
                    _dataProcessor = new DataProcessor(new JsonFileDataFetcher());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing data service: {ex.Message}");
            }
        }

        public async Task<List<Team>> GetTeamsData(string endpointOrPath, bool isMen)
        {
            try
            {
                if (_dataProcessor != null)
                {
                    if (string.IsNullOrEmpty(endpointOrPath)) // JSON mode
                    {
                        // Define the JSON paths based on gender
                        string basePath = AppDomain.CurrentDomain.BaseDirectory;
                        string jsonPath = isMen
                            ? Path.Combine(basePath, "JsonFiles", "Men", "teams.json")
                            : Path.Combine(basePath, "JsonFiles", "Women", "teams.json");

                        // Fetch teams from the JSON file
                        return await _dataProcessor.GetTeamsFromJson(jsonPath);
                    }

                    // Fetch teams from API
                    return await _dataProcessor.GetTeams(endpointOrPath);
                }

                return new List<Team>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving teams data: {ex.Message}");
                return new List<Team>();
            }
        }
        public async Task<List<Country.Root>> GetMatchesData(string endpoint, bool isMen)
        {
            try
            {
                if (_dataProcessor != null)
                {
                    if (string.IsNullOrEmpty(endpoint)) // JSON mode
                    {
                        string jsonPath = isMen ? Path.Combine(menJsonFolderPath, "matches.json") : Path.Combine(womenJsonFolderPath, "matches.json");
                        return await _dataProcessor.GetMatchesFromJson(jsonPath); // Fetch matches from JSON
                    }
                    return await _dataProcessor.GetMatches(endpoint); // Fetch matches from API
                }
                return new List<Country.Root>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving matches data: {ex.Message}");
                return new List<Country.Root>();
            }
        }
        public async Task<List<Country.Root>> GetMatchesDataForCountry(string countryName, bool isMen)
        {
            try
            {
                // Set the correct endpoint based on the gender
                string endpoint = isMen
                    ? "https://worldcup-vua.nullbit.hr/men/matches"
                    : "https://worldcup-vua.nullbit.hr/women/matches";

                // Get all matches data
                var matches = await GetMatchesData(endpoint, isMen);

                if (matches == null || !matches.Any())
                {
                    return null;
                }

                // Filter matches where the country is either the home or away team
                var filteredMatches = matches.Where(m =>
                    m.home_team_statistics.country.Equals(countryName, StringComparison.OrdinalIgnoreCase) ||
                    m.away_team_statistics.country.Equals(countryName, StringComparison.OrdinalIgnoreCase)).ToList();

                return filteredMatches;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching matches for country {countryName}: {ex.Message}", ex);
            }
        }
        public async Task<List<Results>> GetResultsData(string fifaCode, bool isMen, string endpointOrPath)
        {
            var allResults = new List<Results>();

            if (string.IsNullOrEmpty(endpointOrPath)) // JSON mode
            {
                string jsonPath = isMen ? Path.Combine(menJsonFolderPath, "results.json") : Path.Combine(womenJsonFolderPath, "results.json");
                allResults = await _dataProcessor.GetResultsFromJson(jsonPath, fifaCode);
            }
            else
            {
                allResults = await _dataProcessor.GetResultsApi(fifaCode,isMen);
            }

            if (!allResults.Any())
            {
                Console.WriteLine("No results found in the data source for the specified FIFA code.");
            }

            return allResults;
        }


    }
}
