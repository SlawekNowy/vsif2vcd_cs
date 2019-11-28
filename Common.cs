using System;
using System.Collections.Generic;
using System.Text;

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

        internal static List<string> ResponseFiles = new List<string>() {"scripts/talker/response_rules.txt" }; //A non empty Response Rules file List containing an entrypoint.

        //this string indicates the moddir we're working on
        internal static string Modname;

        internal static uint FourCC(char A, char B, char C, char D)
        {
            char[] arr = new char[4];
            arr[0] = D;
            arr[1] = C;
            arr[2] = B;
            arr[3] = A;
            byte[] byte_arr = Encoding.ASCII.GetBytes(arr);

            return BitConverter.ToUInt32(byte_arr);
        }
    }
}
