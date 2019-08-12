using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class GeneratorData
    {
        private static async Task GenerateFileAsync(int min, int max)
        {
            using (var file = File.AppendText(Path.Combine(Settings.Source.BasePath, Settings.Source.InputFileName)))
            {
                var rand = new Random();
                for (var i = min; i < max; i++)
                {
                    if (i % 5000 == 0)
                    {
                        var position = 100.0 * i / (max - min);
                        Console.Write("{0:f2}%   \r", position);
                    }
                    await file.WriteLineAsync((string) GenerateData(rand));
                }
            }
        }

        private static string GenerateData(Random rand)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append((int) RandomNumber(1, 999_999, rand));
            builder.Append(". ");
            builder.Append((string) RandomString(30, rand));
            return builder.ToString();
        }

        public static int RandomNumber(int min, int max, Random rand)
        {
            return rand.Next(min, max);
        }

        public static string RandomString(int size, Random rand)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
            return new string(Enumerable.Repeat(chars, size)
                .Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}