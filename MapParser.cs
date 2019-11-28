using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace vsif2vcd
{
    static class MapParser
    {
        internal static UInt32 ExtractNames(string gameDirectory)
        {
            UInt32 MapsCount;
            MapsCount = ParseMaplist(gameDirectory);
            if (MapsCount == 0)
            {
                return 0;
            }
            Console.WriteLine("Extracting scene names from maps");
            for (UInt32 i = 0; i < MapsCount; i++)
            {
                AddMap(gameDirectory, i);
            }
            return MapsCount;
        }

        private static void AddMap(string gameDirectory, UInt32 i)
        {
            //Map MapToLoad = Maplist[(int)i];
            string MapToLoad = Common.Maps[(int)i];
            String MapPath = gameDirectory + "/maps/" + MapToLoad; //common path
            //byte[] MapFile = File.ReadAllBytes(MapPath);
            UInt32 ID = 0;
            byte[] Lump;
            UInt32 LumpOffset=0, LumpSize=0;

            FileStream lump0;

            try
            {
                FileStream entity_lump = File.OpenRead(MapPath + "_l_0.lmp");
                MapPath += "_l_0.lmp"; // this won't run when we got FileNotFoundException
                entity_lump.Seek(0, SeekOrigin.Begin);
                byte[] tmp = new byte[4];
                entity_lump.Read(tmp, 0, 4);
                //entity_lump.Seek(4, SeekOrigin.Current); //equivalent of fread
                LumpOffset = BitConverter.ToUInt32(tmp);
                if (LumpOffset != 20)
                {
                    Console.WriteLine("Lump patch of map {0} has incorrect data offset.", MapToLoad);
                    return;
                }
                entity_lump.Read(tmp, 0, 4);
                //entity_lump.Seek(4, SeekOrigin.Current); //equivalent of fread
                ID = BitConverter.ToUInt32(tmp);
                if (ID != 0)
                {
                    Console.WriteLine("Lump patch of map {0} has incorrect lump number.", MapToLoad);
                    return;
                }

                entity_lump.Read(tmp, 0, 4);
                //entity_lump.Seek(4, SeekOrigin.Current); //equivalent of fread
                ID = BitConverter.ToUInt32(tmp);

                if (ID != 0)
                {
                    Console.WriteLine("Lump patch of map {0} has incorrect version.", MapToLoad);
                    return;
                }
                entity_lump.Read(tmp, 0, 4);
                //entity_lump.Seek(4, SeekOrigin.Current); //equivalent of fread
                LumpSize = BitConverter.ToUInt32(tmp);
                //lump0 = entity_lump;
                entity_lump.Close();
            }
            catch (FileNotFoundException)
            {

                try
                {
                    FileStream MapFile = File.OpenRead(MapPath + ".bsp");
                    MapPath += ".bsp"; // this won't run when we got FileNotFoundException
                    byte[] tmp = new byte[4];
                    MapFile.Read(tmp, 0, 4);
                    //MapFile.Seek(4, SeekOrigin.Current); //equivalent of fread
                    ID = BitConverter.ToUInt32(tmp);
                    if (ID != Common.FourCC('V', 'B', 'S', 'P'))
                    {
                        Console.WriteLine("Map {0} has incorrect file signature", MapToLoad);
                        return;
                    }
                    MapFile.Read(tmp, 0, 4);
                   // MapFile.Seek(4, SeekOrigin.Current); //equivalent of fread
                    ID = BitConverter.ToUInt32(tmp);
                    if ((ID>18)&&(ID<29)) //no support for Titanfall
                    {
                        Console.WriteLine("Map {0} has incorrect version", MapToLoad);
                        return;
                    }

                    MapFile.Read(tmp, 0, 4);
                   // MapFile.Seek(4, SeekOrigin.Current); //equivalent of fread
                    LumpOffset = BitConverter.ToUInt32(tmp);

                    MapFile.Read(tmp, 0, 4);
                   // MapFile.Seek(4, SeekOrigin.Current); //equivalent of fread
                    LumpSize = BitConverter.ToUInt32(tmp);


                    MapFile.Read(tmp, 0, 4);
                   // MapFile.Seek(4, SeekOrigin.Current); //equivalent of fread
                    ID = BitConverter.ToUInt32(tmp);
                    if (ID!=0) //no support for Titanfall
                    {
                        Console.WriteLine("Entity lump for map {0} has incorrect version", MapToLoad);
                        return;
                    }
                    //lump0 = MapFile;
                    MapFile.Close();
                }
                catch (FileNotFoundException f)
                {
                    Console.Error.WriteLine(f.Message);
                    Console.Write("Couldn't find map {0} specified by maplist", MapToLoad);
                    return;
                } //no finally as file doesn't open
            }
            lump0 = File.OpenRead(MapPath);
            lump0.Seek((long)LumpOffset, SeekOrigin.Begin);
            byte[] lump = new byte[LumpSize];
            lump0.Read(lump, 0, (int)LumpSize);
            ParseEntities(lump, LumpSize);
            lump0.Close();

        }

        private static void ParseEntities(byte[] lump, uint lumpSize)
        {
            char[] lump_char = Encoding.ASCII.GetChars(lump);
            String lump_string = new string(lump_char);
            String[] lump_ready = lump_string.Split('\n');
            // Dictionary<string, string> entity = new Dictionary<string, string>();
            List<KeyValuePair<string, string>> entity = new List<KeyValuePair<string, string>>();
            Regex r = new Regex("\"(.*?)\"");

            //parsing start
            foreach (string Line in lump_ready)
            {
                if (Line == "{")
                {
                    //start of entity. Prepare the dictionary
                    entity.Clear();
                }
                else if (Line == "}")
                {
                    /* if(entity["classname"]== "logic_choreographed_scene")
                     {
                         Common.Scene scene = new Common.Scene
                         {
                             Name = entity["SceneFile"]
                         };
                         Common.Scenes.Add(scene);
                     }
                     if (entity["classname"] == "env_speaker")
                     {
                         Common.ResponseFiles.Add(entity["rulescript"]);
                     } */
                    Console.WriteLine("test");
                }
                else
                { //entity key and value
                    Match m = r.Match(Line);
                    string key, value;
                    key = m.Groups[1].Value;
                    m=m.NextMatch();
                    value = m.Groups[1].Value;
                    entity.Add(KeyValuePair.Create(key,value));

                }
            }
        }

        private static uint ParseMaplist(string gameDirectory)
        {
            String[] TXT;
            String TXTPath;
            UInt32 MapsCount;


            Console.WriteLine("Parsing maplist.txt");
            TXTPath = gameDirectory + "/maplist.txt";

            if (File.Exists(TXTPath))
            {
                TXT = File.ReadAllLines(TXTPath);
                if (TXT.Length == 0)
                {
                    Console.WriteLine("Map list is empty.");
                    return 0;
                }
                MapsCount = 0;
                foreach (string MapName in TXT)
                {
                    if (!String.IsNullOrEmpty(MapName))
                    {
                        MapsCount++;
                        Common.Maps.Add(MapName);
                    }
                }
                return MapsCount;
            }
            else
            {

                //fprintf(stderr, "Unable to open maplist.txt: {0}\n", strerror(errno));
                Console.WriteLine("Unable to open maplist.txt."); //TODO: Exception?
                return 0;
            }
        }
    }
}
    

       
