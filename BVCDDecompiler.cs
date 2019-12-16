using System;
using System.IO;

namespace vsif2vcd
{

    
    internal class BVCDDecompiler
    {
        //those were macros
        private const float One255th = 0.003922F;
        private const float One4096th = 0.000244F;
        
        //TODO: This can be instanced
        internal static void OpenVCDForWriting(out FileStream VCDFile, uint CRC)
        {
            //throw new NotImplementedException();
            string VCDName="";
            bool PathFound = false;
            UInt32 sceneIterator;

            for (sceneIterator=0; sceneIterator < Common.Scenes.Count; sceneIterator++)
            {
                Common.Scene scene = Common.Scenes[(int)Convert.ToInt64(sceneIterator)];
                if (CRC == scene.CRC)
                {
                    string sceneName= scene.Name;
                    VCDName = Common.GameDirectory + Path.DirectorySeparatorChar + sceneName;
                    PathFound = true;
                    break;
                }
            }
            if(!PathFound)
            {
                VCDName = $"{Common.GameDirectory}{Path.DirectorySeparatorChar}scenes{Path.DirectorySeparatorChar}{CRC.ToString()}.vcd";
            }
            VCDFile = File.OpenWrite(VCDName);
        }

        internal static void Decompile(ref FileStream VCDFile, ref MemoryStream sceneBuffer, ref MemoryStream image)
        {
            UInt32 eventCount, actorCount, channelCount;
            UInt32 _actor,_channel,_event; //event is reserved keyword

            BinaryReader reader_image = new BinaryReader(image);
            //throw new NotImplementedException();
        }
    }
}