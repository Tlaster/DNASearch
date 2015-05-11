using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

using static System.Console;

namespace DNASaerch
{
    class Program
    {
        static readonly char[] DNAMenber = { 'T', 'A', 'C', 'G' };
        const int Y_LENGTH = 1000000;
        const int X_LENGTH = 100;

        /// <summary>
        /// Storage the file in this array
        /// </summary>
        static string[] FileArray;
        static DNAHashDictionary DNADictionary;
        //static Dictionary<string, List<DNAPos>> PosDictionary;

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
                if (FileArray == null && DNADictionary == null)
                {
                    ReadFile().Wait();
                    GetHashDictionary(int.Parse(ReadLine()));
                }
                Search(ReadLine());
                //var pat = ReadLine();
                //ParallelSearch(pat);
                WriteLine("Search again?(Y/N)");
                isAgain = ReadLine().ToUpper() == "Y";
            }
        }

        #region Hash



        static void Search(string key)
        {

            using (Timer timer = new Timer())
            {
                double time = 0.0;
                timer.Elapsed += ((s, e) =>
                {
                    time += 0.1;
                });
                timer.Start();
                if (DNADictionary.Contain(key))
                {
                    timer.Stop();
                    WriteLine($"Find the key,pos count {DNADictionary[key].Count},time used {time}, show the pos?(Y/N)");
                    if (ReadLine().ToUpper() == "Y")
                    {
                        for (int i = 0; i < DNADictionary[key].Count; i++)
                        {
                            var a = DNADictionary[key].Positions[i];
                            WriteLine($"{{{a.X},{a.Y}}}");
                        }
                    }  
                }
                else
                {
                    timer.Stop();
                    WriteLine($"Can not find the key,time used {time}");
                }
            }
        }

        static void GetHashDictionary(int keylength)
        {
            WriteLine("Building Dictionary...");
            using (Timer timer = new Timer())
            {
                double time = 0.0;
                timer.Elapsed += ((s, e) =>
                {
                    time += 0.1;
                });
                timer.Start();
                DNADictionary = new DNAHashDictionary(keylength);
                for (int i = 0; i < Y_LENGTH; i++)
                {
                    for (int j = 0; j < X_LENGTH - keylength; j++)
                    {
                        string key = "";
                        for (int k = j; k < j+keylength; k++)
                        {
                            key += FileArray[i][k];
                        }
                        DNADictionary.Add(key, j, i);
                    } 
                }
                timer.Stop();
                FileArray = null;
                WriteLine($"Build Dictionary Complete,timer used {time}");

            }
        }
        #endregion

        #region FA
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

        #endregion

        #region File
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
        #endregion


    }

    class DNAHashDictionary
    {
        private const int HASHSEED = 1;

        //private const int DEFAULT_INITIAL_CAPACITY = 16;

        //private const int MAXIMUM_CAPACITY = 1 << 30;

        //// the default load factor
        //private const float DEFAULT_LOAD_FACTOR = 0.75f;

        //// The next size value at which to resize (capacity * load factor).
        //private int _threshold;
        //private const float loadFactor = DEFAULT_LOAD_FACTOR;


        public static readonly int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};


        private DNAEntry[] _bucket;
        private uint[] _cryptTable;

        public int Count { get; private set; }

        public DNAHashDictionary(int klength)
        {
            //_bucket = new DNAEntry[DEFAULT_INITIAL_CAPACITY]; 
            _bucket = new DNAEntry[GetPrime(klength)];
            Count = 0;
            InitCryptTable();
        }

        private int GetPrime(int keylength)
        {
            if (keylength <= 50)
            {
                var size = Math.Pow(4, keylength);
                for (int i = 0; i < primes.Length; i++)
                {
                    int prime = primes[i];
                    if (prime >= size) return prime;
                }
            }
            return 7199369;

        }


        /// <summary>
        /// Check if Contain
        /// </summary>
        /// <param name="key">DNA Value</param>
        /// <returns></returns>
        public bool Contain(string key) => this[key] != null;

        //private bool Contain(int pos) => _bucket?[pos] != null;

        /// <summary>
        /// Get DNA Position
        /// </summary>
        /// <param name="key">DNA Value</param>
        /// <returns></returns>
        public DNAEntry this[string key]
        {
            get
            {
                var hashcode = GetHashCode(key, HASHSEED);
                for (var i = _bucket[GetIndex(hashcode)]; i != null; i = i.Next)
                {
                    if (i.Key == key && i.HashCode == hashcode)
                    {
                        return i;
                    }
                }
                return null;
            }
        }
        

        /// <summary>
        /// Add DNA Position
        /// </summary>
        /// <param name="key">DNA Value</param>
        /// <param name="x">X Posion</param>
        /// <param name="y">Y Posion</param>
        public void Add(string key, int x, int y)
        {
            //if (Count > _bucket.Length)
            //{
            //    Resize(Count + 10);  
            //}
            var hashcode = GetHashCode(key, HASHSEED);
            var index = GetIndex(hashcode);
            for (var i = _bucket[index]; i != null; i = i.Next)
            {
                if (i.Key == key && i.HashCode == hashcode)
                {
                    i.Add(x, y);
                    return;
                }
            }
            var length = 0;
            if (key.Length>10)
            {
                length = 200;
            }
            else
            {
                length = (100 - key.Length) * 1000000 / ((int)Math.Pow(4, key.Length));
            }
             
            var e = _bucket[index];
            _bucket[index] = new DNAEntry(key, hashcode, length, e);
            _bucket[index].Add(x, y);
            Count++;

        }

        //private void Resize(int newCapacity)
        //{
        //    var oldTable = _bucket;
        //    int oldCapacity = oldTable.Length;
        //    if (oldCapacity == MAXIMUM_CAPACITY)
        //    {
        //        _threshold = int.MaxValue;
        //        return;
        //    }
        //    Transfer(newCapacity);
        //    _threshold = (int)Math.Min(newCapacity * loadFactor, MAXIMUM_CAPACITY + 1);
        //}

        //private void Transfer(int count)
        //{
        //    var newEntry = new DNAEntry[count];
        //    for (int i = 0; i < _bucket.Length; i++)
        //    {
        //        while (_bucket[i] != null)
        //        {
        //            var next = _bucket[i].Next;
        //            var index = GetIndex(_bucket[i].HashCode, count);
        //            _bucket[i].Next = newEntry[index];
        //            newEntry[index] = _bucket[i];
        //            _bucket[i] = next;
        //        }
        //    }
        //    _bucket = newEntry;
        //}

        private int GetIndex(uint hash) => (int)(hash & (_bucket.Length - 1));
        private int GetIndex(uint hash,int length) => (int)(hash & (length - 1));


        private void InitCryptTable()
        {
            _cryptTable = new uint[0x500];
            uint seed = 0x00100001;
            uint index1 = 0, index2 = 0, i;
            for (index1 = 0; index1 < 0x100; index1++)
            {
                for (index2 = index1, i = 0; i < 5; i++, index2 += 0x100)
                {
                    uint temp1, temp2;
                    seed = (seed * 125 + 3) % 0x2AAAAB;
                    temp1 = (seed & 0xFFFF) << 0x10;
                    seed = (seed * 125 + 3) % 0x2AAAAB;
                    temp2 = (seed & 0xFFFF);
                    _cryptTable[index2] = (temp1 | temp2);
                }
            }
        }
        
        //http://sfsrealm.hopto.org/inside_mopaq/chapter2.htm


        private uint GetHashCode(string key, int hashSeed)
        {
            uint seed1 = 0x7FED7FED, seed2 = 0xEEEEEEEE;
            for (int i = 0; i < key.Length; i++)
            {
                uint ch = key[i];
                seed1 = _cryptTable[hashSeed * (1 << 8) + ch]
                        ^ (seed1 + seed2);
                seed2 = ch + seed1 + seed2 + (seed2 << 5) + 3;
            }
            return seed1;
        }
    }

    class DNAEntry
    {
        public string Key { get; private set; }
        public uint HashCode { get; private set; }
        public int Count { get; private set; }
        public DNAPos[] Positions { get; private set; }
        public DNAEntry Next { get; set; }

        public DNAEntry(string key, uint hashCode,int length,DNAEntry next)
        {
            Key = key;
            HashCode = hashCode;
            Positions = new DNAPos[length];
            Count = 0;
            Next = next;
        }
        
        /// <summary>
        /// Add Posion
        /// </summary>
        /// <param name="x">X Posion</param>
        /// <param name="y">Y Posion</param>
        public void Add(int x,int y)
        {
            if (Count == Positions.Length)
            {
                Resize(Positions.Length + 100);
            }
            Positions[Count] = new DNAPos(x, y);
            Count++;
        }
        

        private void Resize(int newSize)
        {
            if (newSize < Positions.Length)
            {
                return;
            }
            DNAPos[] newpos = new DNAPos[newSize];
            Array.Copy(Positions, newpos,Positions.Length);
            Positions = newpos;
        }

        //public static bool operator ==(DNATable a, DNATable b)
        //{
        //    return a.Equals(b);
        //}
        //public static bool operator !=(DNATable a, DNATable b)
        //{
        //    return !a.Equals(b);
        //}

        //public override bool Equals(object obj)
        //{
        //    if (obj is DNATable)
        //    {
        //        var table = obj as DNATable;
        //        return table.HashCode == HashCode;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public override int GetHashCode()
        //{
        //    return (int)HashCode;
        //}
    }
    struct DNAPos
    {
        public int X, Y;
        public DNAPos(int x,int y)
        {
            X = x;
            Y = y;
        }
    }
}
