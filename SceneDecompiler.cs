using System;
using System.IO;

namespace vsif2vcd
{
    static class SceneDecompiler
    {
        static FileStream ScenesImage;
        internal static void Extract(string gameDirectory)
        {
            ScenesImage = File.OpenRead(gameDirectory + "/scenes/scenes.image");
            ;
        }
    }
}