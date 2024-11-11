using DataLayer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    public class JsonFileDataFetcher :DataFetcher
    {
        public override Task<string> FetchData(string filePath)
        {
            try
            {
                return Task.FromResult(File.ReadAllText(filePath));
            }
            catch (FileNotFoundException fileEx)
            {
                Console.WriteLine($"File not found: {fileEx.Message}");
                return Task.FromResult(string.Empty);
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error reading file: {ioEx.Message}");
                return Task.FromResult(string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return Task.FromResult(string.Empty);
            }
        }

    }
}
