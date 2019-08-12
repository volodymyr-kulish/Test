using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    public class Program
    {
        private static string GetInputFileName => Path.Combine(Settings.Source.BasePath, Settings.Source.InputFileName);
        private static string GetOutputFileName => Path.Combine(Settings.Source.BasePath, Settings.Source.OutputFileName);

        static void Main(string[] args)
        {
            try
            {
                var availableFreeSpace = DriveInfo.GetDrives()
                    .FirstOrDefault(x => x.RootDirectory.Root.FullName == Path.GetPathRoot(Settings.Source.BasePath))
                    ?.AvailableFreeSpace;
                var requireFreeSpace = new FileInfo(GetInputFileName)
                                           .Length * 3;
                if (availableFreeSpace < requireFreeSpace)
                {
                    Log($"Not enough free space. The program requires a minimum {Utils.FormatBytes(requireFreeSpace)}");
                    Console.ReadLine();
                    return;
                }

                SplitInput(GetInputFileName);

                SortTheChunks();

                MergeTheChunks();

                Log("Done");
            }
            catch (Exception e)
            {
                Log($"An error occurred during operation. Detail information {e.Message}");
            }
            Console.ReadLine();
        }

        /// <summary>
        /// Split the big file into chunks
        /// </summary>
        static void SplitInput(string filePath)
        {
            Log("Splitting");
            var splitNum = 1;

            var writer = new StreamWriter(string.Format(Settings.Source.SplitedFileNameFormat, splitNum));
            long readLine = 0;
            using (var reader = new StreamReader(filePath))
            {
                while (reader.Peek() >= 0)
                {
                    if (++readLine % 5000 == 0)
                    {
                        Console.Write("{0:f2}%   \r",
                            100.0 * reader.BaseStream.Position / reader.BaseStream.Length);
                    }

                    writer.WriteLine(reader.ReadLine());

                    if (writer.BaseStream.Length > Settings.General.MaxSplitedFileSize && reader.Peek() >= 0)
                    {
                        writer.Close();
                        splitNum++;
                        writer = new StreamWriter(string.Format(Settings.Source.SplitedFileNameFormat, splitNum));
                    }
                }
            }
            writer.Close();
            Log("Splitting complete");
        }

        /// <summary>
        /// Go through all the "split00058.dat" files, and sort them
        /// into "sorted00058.dat" files, removing the original
        /// </summary>
        static void SortTheChunks()
        {
            Log("Sorting chunks");
            var comparer = GetComparer();

            foreach (var path in Directory.GetFiles(Settings.Source.BasePath, "split*.dat"))
            {
                Console.Write("{0}     \r", path);

                var contents = File.ReadAllLines(path);

                Array.Sort(contents, comparer);

                var newPath = path.Replace("split", "sorted");

                File.WriteAllLines(newPath, contents);
                File.Delete(path);

                contents = null;
                GC.Collect();
            }
            Log("Sorting chunks completed");
        }

        /// <summary>
        /// Merge all the "sorted00058.dat" chunks together 
        /// Uses 45MB of ram, for 100 chunks
        /// Takes 5 minutes, for 100 chunks of 10 megs each ie 1 gig total
        /// </summary>
        static void MergeTheChunks()
        {
            Log("Merging");

            var fileNames = Directory.GetFiles(Settings.Source.BasePath, "sorted*.dat");
            var chunksCount = fileNames.Length;
            var bufferSize = Settings.General.MaxUsage / chunksCount;
            var recordOverHead = 7.5; // The overhead of using Queue<>
            var bufferLen = (int)(bufferSize / Settings.General.RecordSize / recordOverHead);

            var readers = new StreamReader[chunksCount];
            for (var i = 0; i < chunksCount; i++)
                readers[i] = new StreamReader(fileNames[i]);

            var queues = new Queue<string>[chunksCount];
            for (var i = 0; i < chunksCount; i++)
                queues[i] = new Queue<string>(bufferLen);

            Log("Priming the queues");
            for (var i = 0; i < chunksCount; i++)
                LoadQueue(queues[i], readers[i], bufferLen);
            Log("Priming the queues complete");

            var writer = new StreamWriter(GetOutputFileName);
            var comparer = GetComparer();

            var done = false;
            var progress = 0;
            int lowestIndex;
            string lowestValue;
            while (!done)
            {
                if (++progress % 5000 == 0)
                    Console.Write("{0:f2}%   \r",
                      100.0 * progress / Settings.General.Records);

                lowestIndex = -1;
                lowestValue = "";
                for (var j = 0; j < chunksCount; j++)
                {
                    if (queues[j] == null) continue;

                    if (lowestIndex < 0
                        || comparer.Compare(queues[j].Peek(), lowestValue) < 0)
                    {
                        lowestIndex = j;
                        lowestValue = queues[j].Peek();
                    }
                }

                if (lowestIndex == -1)
                {
                    done = true;
                    break;
                }

                writer.WriteLine(lowestValue);

                queues[lowestIndex].Dequeue();

                if (queues[lowestIndex].Count == 0)
                {
                    LoadQueue(queues[lowestIndex], readers[lowestIndex], bufferLen);
                    if (queues[lowestIndex].Count == 0)
                    {
                        queues[lowestIndex] = null;
                    }
                }
            }
            writer.Close();

            for (var i = 0; i < chunksCount; i++)
            {
                readers[i].Close();
                File.Delete(fileNames[i]);
            }

            Log("Merging complete");
        }

        /// <summary>
        /// Loads up to a number of records into a queue
        /// </summary>
        static void LoadQueue(Queue<string> queue, StreamReader file, int records)
        {
            for (var i = 0; i < records; i++)
            {
                if (file.Peek() < 0) break;
                queue.Enqueue(file.ReadLine());
            }
        }

        /// <summary>
        /// Write to console, with the time
        /// </summary>
        static void Log(string s)
        {
            Console.WriteLine("{0}: {1}", DateTime.Now.ToLongTimeString(), s);
        }

        static StringComparer GetComparer()
        {
            return new StringComparer();
        }

    }
}
