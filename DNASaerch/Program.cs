using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

using static System.Console;

namespace DNASaerch
{
    class Program
    {
        static readonly char[] DNAMenber = { 'T', 'A', 'C', 'G' };
        /// <summary>
        /// Storage the file in this array
        /// </summary>
        static string[] FileArray;

        /// <summary>
        /// Before use Search method,please ensure the input file exist
        /// (You can use CreatFile() method to create a random file
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            bool isAgain = true;
            while (isAgain)
            {
                if (FileArray == null)
                {
                    ReadFile().Wait();
                }
                var pat = ReadLine();
                ParallelSearch(pat);
                WriteLine("Search again?(Y/N)");
                isAgain = ReadLine().ToUpper() == "Y";
            }
        }

        static int[,] BuildingFA(string patten)
        {
            WriteLine("Building FA...");
            int[,] FA = new int[patten.Length, 4];
            for (int i = 0; i < patten.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    FA[i, j] = GetNextState(patten, i, j);
                }
            }
            WriteLine("Building FA Complete");
            return FA;
        }


        static int GetNextState(string patten, int i, int j)
        {            
            if (i < patten.Length && DNAMenber[j] == patten[i])
                return i + 1;

            for (int a = i; a > 0; a--)
            {
                if (patten[a - 1] == DNAMenber[j])
                {
                    for (int b = 0; b < a - 1; b++)
                    {
                        if (patten[b] != patten[i - a + b + 1] && b == a - 1)
                        {
                            return b;
                        }
                    }
                }
            }
            return 0;
        }


        static void Search(int[,] FA, string txt, int pos)
        {
            int state = 0, patlength = FA.Length / 4;
            for (int i = 0; i < txt.Length; i++)
            {
                int j = 0;
                for (; j < 4; j++)
                {
                    if (DNAMenber[j] == txt[i])
                    {
                        break;
                    }
                }
                state = FA[state, j];
                if (state == patlength)
                {
                    WriteLine($"{{{pos}, {i - patlength + 1}}}");
                    state = 0;
                }
            }
        }

        static async Task ReadFile()
        {
            #region Timer
            WriteLine("Reading File...");
            FileArray = new string[1000000];
            using (Timer timer = new Timer())
            {
                double time = 0.0;
                timer.Elapsed += ((s, e) =>
                {
                    time += 0.1;
                });
                timer.Start();

           #endregion
                using (var streamReader = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "file.txt"))
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        FileArray[i] = await streamReader.ReadLineAsync();
                    }
                }
            #region Timer
                timer.Stop();
                WriteLine($"Reading File Complete,time used {time}");
            }
            #endregion
        }

        static void ParallelSearch(string pattern)
        {
            var FA = BuildingFA(pattern);
            #region Timer
            using (Timer timer = new Timer())
            {
                double time = 0.0;
                timer.Elapsed += ((s, e) =>
                {
                    time += 0.1;
                });
                timer.Start();
                WriteLine("Searching...");

                #endregion
                Parallel.Invoke(
                    () => SearchFromArray(FA, 0, FileArray.Length / 2),
                    () => SearchFromArray(FA, FileArray.Length / 2, FileArray.Length)
                    );
            #region Timer
                timer.Stop();
                WriteLine($"Search Complete,time used {time}");
            }
            #endregion
        }

        static void SearchFromArray(int[,] FA, int begin, int end)
        {
            for (int i = begin; i < end; i++)
            {
                Search(FA, FileArray[i], i);
            }
        }


        /// <summary>
        /// Create a random file that we used
        /// </summary>
        /// <returns></returns>
        static async Task CreatFile()
        {
            #region Timer
            using (Timer timer = new Timer())
            {
                double time = 0.0;
                timer.Elapsed += ((s, e) =>
                {
                    time += 0.1;
                });
                timer.Start();
                #endregion
                Random random = new Random(); 
                using (var writer = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "file.txt"))
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        for (int j = 0; j < 100; j++)
                        {
                            await writer.WriteAsync(DNAMenber[random.Next(4)]);
                        }
                        await writer.WriteLineAsync();
                    }
                }
            #region Timer
                timer.Stop();
                WriteLine($"Create Complete ,time used {time}");
            }

            #endregion
        }
    }
}
