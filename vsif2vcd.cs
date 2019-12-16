using System;
using System.Collections.Generic;
using System.IO;
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
            Common.GameDirectory = gameDirectory;
            Common.Modname = new DirectoryInfo(gameDirectory).Name;
            Common.GetResponseFiles(gameDirectory);
            //throw new NotImplementedException();
            Common.MapsCount = MapParser.ExtractNames(gameDirectory);
            //if (Common.MapsCount != 0)
            //{
            //    Console.WriteLine("Warning. Extracted scenes might be unnamed.");
            // }
            //response rules parser entry
            //first we make sure we don't have any duplicates.
            //Common.ResponseFiles = Common.ResponseFiles.Distinct().ToList();
            ResponseRulesParser.Parse();
            Common.ResolveStringProxies();
            Common.Scenes = Common.Scenes.Distinct().ToList();

            //add hardcoded entries



            AddHardCodedEntries();


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

            List<String> listOfVCDFiles = Common.Scenes.Aggregate(
                new List<String>(),
                (newList, obj) =>
                {
                    newList.Add(obj.Name);
                    return newList;
                }
                );


            //TODO: Serialize final product to JSON.


            VSIFParser.Extract(gameDirectory);
        }

        private static void AddHardCodedEntries()
        {
            List<string> hardcodedEntriesToAdd = new List<string>();

            if (!Common.Modname.Contains("portal2")&& !Common.Modname.Contains("csgo")&& !Common.Modname.Contains("l4d2")) { //those games do not include hl2 scenes
                string[] hl2_entries = new string[]{
                    "scenes/Expressions/Barneyalert.vcd",
                    "scenes/Expressions/barneycombat.vcd",
                    "scenes/Expressions/barneyidle.vcd",
                    "scenes/Expressions/citizenalert_loop.vcd",
                    "scenes/Expressions/citizencombat_loop.vcd",
                    "scenes/Expressions/Citizenidle.vcd",
                    "scenes/Expressions/citizen_angry_alert_01.vcd",
                    "scenes/Expressions/citizen_angry_combat_01.vcd",
                    "scenes/Expressions/citizen_angry_idle_01.vcd",
                    "scenes/Expressions/citizen_normal_alert_01.vcd",
                    "scenes/Expressions/citizen_normal_combat_01.vcd",
                    "scenes/Expressions/citizen_normal_idle_01.vcd",
                    "scenes/Expressions/citizen_scared_alert_01.vcd",
                    "scenes/Expressions/citizen_scared_combat_01.vcd",
                    "scenes/Expressions/citizen_scared_idle_01.vcd"
                };
                hardcodedEntriesToAdd.AddRange(hl2_entries);
            }
                
            if (Common.Modname.Contains("ep2"))
            {
                string[] ep2_entries = new string[]
                {
                        "scenes/npc/hunter/hunter_scan.vcd",
                        "scenes/npc/hunter/hunter_eyeclose.vcd",
                        "scenes/npc/hunter/hunter_roar.vcd",
                        "scenes/npc/hunter/hunter_pain.vcd",
                        "scenes/npc/hunter/hunter_eyedarts_top.vcd",
                        "scenes/npc/hunter/hunter_eyedarts_bottom.vcd"
                };

                hardcodedEntriesToAdd.AddRange(ep2_entries);
            }
            if (Common.Modname.Contains("portal"))
            {
                string[] portal_entries = new string[]
                {
                    "scenes/general/generic_security_camera_destroyed-5.vcd",
                    "scenes/general/generic_security_camera_destroyed-4.vcd",
                    "scenes/general/generic_security_camera_destroyed-3.vcd",
                    "scenes/general/generic_security_camera_destroyed-2.vcd",
                    "scenes/general/generic_security_camera_destroyed-1.vcd",
                };

                hardcodedEntriesToAdd.AddRange(portal_entries);
            }
            if (Common.Modname.Contains("portal2"))
            {
                string[] portal2_entries = new string[]
                {
                    "scenes/npc/sp_proto_sphere/sphere_plug_attach.vcd",
                    "scenes/npc/sp_proto_sphere/sphere_plug_lock.vcd",
                    "scenes/npc/sp_proto_sphere/sphere_plug_unlock.vcd",
                    "scenes/npc/sp_proto_sphere/sphere_plug_detach.vcd"
                };

                hardcodedEntriesToAdd.AddRange(portal2_entries);
            }
            if (Common.Modname.Contains("tf"))
            {
                string[] tf2_scenes_merasmus = new string[]
                {
                    "scenes/bot/merasmus/low/bomb_attack_000.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_001.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_002.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_003.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_004.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_005.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_006.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_007.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_008.vcd",
                    "scenes/bot/merasmus/low/bomb_attack_009.vcd"
                };

                hardcodedEntriesToAdd.AddRange(tf2_scenes_merasmus);
            }

           // Common.Scenes.Add(hardcodedEntriesToAdd);

            foreach (string entry in hardcodedEntriesToAdd)
            {
                Common.Scenes.Add(new Common.Scene { Name = entry });
            }
        }
    }
}
