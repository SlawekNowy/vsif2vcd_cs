using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Force.Crc32;
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
            Common.GetResponseFiles(gameDirectory);
            //throw new NotImplementedException();
            Common.MapsCount = MapParser.ExtractNames(gameDirectory);
            if (Common.MapsCount != 0)
            {
                Console.WriteLine("Warning. Extracted scenes might be unnamed.");
            }
            //response rules parser entry
            //first we make sure we don't have any duplicates.
            //Common.ResponseFiles = Common.ResponseFiles.Distinct().ToList();
            ResponseRulesParser.Parse();
            Common.Scenes = Common.Scenes.Distinct().ToList();
            //now that we have list of non-duplicates we can now generate crc

            //Common.Scenes.ForEach(delegate(Common.Scene obj)
            //{
            //    obj.CRC = Crc32Algorithm.Compute(Encoding.ASCII.GetBytes(obj.Name));
            //});
            Common.Scenes = Common.Scenes.Aggregate(
            new List<Common.Scene>(),
            (newList, obj) =>
            {

                string tmp = obj.Name.Replace("/", "\\");
                tmp = tmp.ToLower();
                obj.CRC = Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(tmp));
                newList.Add(obj);
                return newList;
            });



            SceneDecompiler.Extract(gameDirectory);
        }

    }
}
