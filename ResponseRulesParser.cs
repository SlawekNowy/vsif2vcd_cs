using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace vsif2vcd
{

    internal static class ResponseRulesParser
    {
        static private List<string> ResponseData = new List<string>();
        internal static void Parse()
        {
            foreach (string file in Common.ResponseFiles)
            {
                string[] TXT = File.ReadAllLines(file);
                for (int i= 0; i < TXT.Length;i++)
                {
                    string line = TXT[i];
                    if (line.Contains("//",StringComparison.Ordinal))
                    {
                        //first we remove the comments
                        Regex rgx = new Regex("(.*?)\\/\\/");
                        line = rgx.Match(line).Groups[1].Value;
                        TXT[i] = line;
                    }
                }
                ResponseData.AddRange(TXT);

            }
            ResponseData.RemoveAll(String.IsNullOrEmpty);
            string[] finished = ResponseData.ToArray();
            for (int i = 0; i < finished.Length; i++)
            {
                string line = finished[i];
                if (line.Contains("scene", StringComparison.Ordinal))
                {
                    //first we remove the comments
                    Regex rgx = new Regex("\\Wscene \"(.*)\"");
                    line = rgx.Match(line).Groups[1].Value;
                    Common.Scene scene = new Common.Scene
                    {
                        Name = line
                    };
                    Common.Scenes.Add(scene);
                }
            }
        }

        
    }
}
