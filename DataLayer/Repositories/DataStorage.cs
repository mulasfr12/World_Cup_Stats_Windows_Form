using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repositories
{
    public class DataStorage
    {
        public void SaveDataToFile(string fileName, string data)
        {
            try
            {
                File.WriteAllText(fileName, data);
            }
            catch (UnauthorizedAccessException authEx)
            {
                Console.WriteLine($"Permission error writing to file: {authEx.Message}");
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error writing to file: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        public string ReadDataFromFile(string fileName)
        {
            try
            {
                return File.ReadAllText(fileName);
            }
            catch (FileNotFoundException fileEx)
            {
                Console.WriteLine($"File not found: {fileEx.Message}");
                return string.Empty;
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error reading file: {ioEx.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
