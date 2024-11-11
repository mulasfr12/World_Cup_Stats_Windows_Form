using DataLayer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DataLayer.Repositories
{
    public class ApiDataFetcher :DataFetcher
    {
        private readonly HttpClient _client;
        private string BaseApiUrl = "https://worldcup-vua.nullbit.hr";

        public ApiDataFetcher()
        {
            _client = new HttpClient();
        }

        public override async Task<string> FetchData(string endpoint)
        {
            try
            {
                var response = await _client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Error fetching data from API: {httpEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return string.Empty;
            }
        }
        // In ApiDataFetcher
        public async Task<List<Results>> GetResultsAsync(string fifaCode, bool isMen)
        {
            // API URL construction (example)
            string url = $"{BaseApiUrl}/results?team={fifaCode}&gender={(isMen ? "male" : "female")}";
            // Call API and deserialize JSON to List<Results>
            var response = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<Results>>(response);
        }

    }
}
