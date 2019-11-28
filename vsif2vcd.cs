using System;
using System.Collections.Generic;
using System.IO;

namespace vsif2vcd
{
    class Program
    {
        /*
         * This class contains only the entry points of this program
         */

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            Console.WriteLine("VSIF2VCD - inital code from SiPlus, ported to C# by MrSoup678");
            if (args.Length ==0)
            {
                Console.WriteLine("Usage: vsif2vcd [game_directory]");
                Environment.Exit(1);
            }


                Convert(args[0]);
        }

        /*
         * TODO: consult mod's gameinfo.txt for additional directory mountpoint
         */
        static void Convert(string gameDirectory)
        {
            Common.Modname = new DirectoryInfo(gameDirectory).Name;
            //throw new NotImplementedException();
            Common.MapsCount = MapParser.ExtractNames(gameDirectory);
            if (Common.MapsCount != 0)
            {
                Console.WriteLine("Warning. Extracted scenes might be unnamed.");
            }
            //response rules parser

            SceneDecompiler.Extract(gameDirectory);
        }
    }
}
