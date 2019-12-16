using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace vsif2vcd
{
    static class Common //static class that contains all of this assembly's variables
    {
       
       
        [Serializable]
        internal struct Scene
        {
            public UInt32 CRC;
            public string Name;
        }
        internal static UInt32 MapsCount;

        internal static List<string> Maps = new List<string>();

        internal static List<Scene> Scenes = new List<Scene>();
        internal static string Modname;
        internal static string GameDirectory;
        internal static bool usingEmbeddedHL2;


        internal static List<string> ResponseFiles = new List<string>();

        internal static void GetResponseFiles(string gameDirectory)
        {
            ResponseFiles.AddRange(Directory.GetFiles(gameDirectory + "/scripts/talker"));
            try
            {
                ResponseFiles.AddRange(Directory.GetFiles(gameDirectory + "/scripts/talker_player"));
            } catch (DirectoryNotFoundException)
            {
                //too bad
            }

        }


        internal static uint FourCC(string magic,bool isBigEndian)
        {
            char[] arr = magic.ToCharArray();
            if (isBigEndian)
            {
                Array.Reverse(arr);
            }
            byte[] byte_arr = Encoding.ASCII.GetBytes(arr);

            return BitConverter.ToUInt32(byte_arr);
        }

        internal static void ResolveStringProxies()
        {
            Common.Scenes = Common.Scenes.Aggregate(
               new List<Common.Scene>(),StrProxies);
        }

        private static List<Scene> StrProxies(List<Scene> list, Scene scene)
        {
            string tmp_name = scene.Name;
            if (tmp_name.Contains("$gender"))
            {

                Scene[] tmp_scenes = new Scene[2];
                tmp_scenes[0] = new Scene()
                {
                    Name = tmp_name.Replace("$gender", "male")
                };
                tmp_scenes[1] = new Scene()
                {
                    Name = tmp_name.Replace("$gender", "female")
                };
                list.AddRange(tmp_scenes);
            }
            else
            {
                list.Add(scene);
            }
            return list;

        }

        
    }
}
