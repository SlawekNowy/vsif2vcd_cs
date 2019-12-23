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
            Directory.CreateDirectory(Path.GetDirectoryName(VCDName));
            VCDFile = File.OpenWrite(VCDName);
        }

        internal static bool Decompile(ref FileStream VCDFile, ref MemoryStream sceneBuffer, ref MemoryStream image)
        {
            UInt32 eventCount, actorCount, channelCount;
            UInt32 _actor,_channel,_event; //event is reserved keyword

            BinaryReader reader_bvcd = new BinaryReader(sceneBuffer);
            BinaryReader reader_image = new BinaryReader(image);

            UInt32 ID = reader_bvcd.ReadUInt32();
            byte bvcd_4 = reader_bvcd.ReadByte();
            sceneBuffer.Position = 9;
            if (ID != Common.FourCC("bvcd", false))
            {
                return false;
            }
            if (bvcd_4 != 0x04)
            {
                return false;
            }
            StreamWriter VCD_writer = new StreamWriter(VCDFile);
            VCD_writer.WriteLine("// Choreo version 1");

            /*event iteration*/
            //this is a byte
            eventCount = reader_bvcd.ReadByte();
            for (_event = 0; _event < eventCount; _event++)
            {
                Event(ref VCD_writer, ref reader_bvcd, ref reader_image, false);
            }
            actorCount = reader_bvcd.ReadByte();
            for (_actor =0;_actor<actorCount;_actor++)
            {
                VCD_writer.WriteLine("actor \"{0}\" \n {{", PoolString(ref reader_bvcd, ref reader_image));
                sceneBuffer.Position += 2;
                channelCount = reader_bvcd.ReadByte();
                for (_channel =0;_channel <channelCount;_channel++)
                {
                    VCD_writer.WriteLine("\tchannel \"{0}\"\n {{", PoolString(ref reader_bvcd, ref reader_image));
                    sceneBuffer.Position += 2;
                    eventCount = reader_bvcd.ReadByte();
                    for(_event=0;_event<eventCount;_event++)
                    {
                        Event(ref VCD_writer, ref reader_bvcd, ref reader_image, true);
                    }
                    if(reader_bvcd.ReadByte()!=0)
                    {
                        VCD_writer.WriteLine("\t\tactive \"0\"");
                    }
                    VCD_writer.WriteLine("}");
                }
                Ramp(VCD_writer, reader_bvcd, false, false);

                VCD_writer.Write("scalesettings\n{\n" +
                    "\t\"CChoreoView\" \"100\"\n" +
                    "\t\"SceneRampTool\" \"100\"\n" +
                    "\t\"ExpressionTool\" \"100\"\n" +
                    "\t\"GestureTool\" \"100\"\n" +
                    "\t\"RampTool\" \"100\"\n" +
                    "fps 60\nsnap off \n"
                    );
            }

            return true;
        }


        private static string PoolString(ref BinaryReader reader_bvcd, ref BinaryReader reader_image)
        {
            //throw new NotImplementedException();

            long pointer_bak = reader_image.BaseStream.Position;
            long pointer_pos = 20 + (reader_bvcd.ReadUInt16() << 2);
            reader_bvcd.BaseStream.Position -= 2;
            reader_image.BaseStream.Position = pointer_pos;
            long pointer = reader_image.ReadUInt32();
            reader_image.BaseStream.Position = pointer - 2; //this is to not cut exclamation points eg. !picker !target !player
            string returned_string = reader_image.ReadString();


            reader_image.BaseStream.Position = pointer_bak ;
            string[] pulled_string_array = returned_string.Split('\0');

             return pulled_string_array[1];

            //imageStart + (Uint32) imageStart+sizeof(VSIF_Header)+(Uint16)(Source))<<2
        }

        private enum Event_Type
        {
            Event_Unspecified,
            Event_Section, Event_Expression, Event_LookAt, Event_MoveTo,
            Event_Speak, Event_Gesture, Event_Sequence, Event_Face,
            Event_FireTrigger, Event_FlexAnimation, Event_SubScene, Event_Loop,
            Event_Interrupt, Event_StopPoint, Event_PermitResponses, Event_Generic,
        }
        static string[] Event_TypeNames = new string[17]
        {
            "unspecified",
            "section", "expression", "lookat", "moveto",
            "speak", "gesture", "sequence", "face",
            "firetrigger", "flexanimation", "subscene", "loop",
            "interrupt", "stoppoint", "permitresponses", "generic"
        };
        private static void Event(ref StreamWriter VCDFile, ref BinaryReader sceneBuffer, ref BinaryReader image, bool InChannel)
        {
            //throw new NotImplementedException();
            Event_Type event_Type;
            UInt32 Flags;
            UInt32 AbsoluteTagType;
            UInt32 tagCount;
            UInt32 tag;
            string Tab="";
            if(InChannel)
            {
                Tab = "\t\t";
            }
            //header
            event_Type = (Event_Type)sceneBuffer.ReadByte();
            VCDFile.WriteLine("{0}event {1} \"{2}\"\n{0}",Tab,Event_TypeNames[(int)event_Type],PoolString(ref sceneBuffer,ref image));
            sceneBuffer.BaseStream.Position += 2;

            //time
            VCDFile.WriteLine("{0}\ttime {1} {2}", Tab, sceneBuffer.ReadSingle(), sceneBuffer.ReadSingle());

            //parameters
            //only three are suported, are there more?
            VCDFile.WriteLine("{0}\tparam \"{1}\"", Tab, PoolString(ref sceneBuffer, ref image));
            sceneBuffer.BaseStream.Position += 2;
            if (PoolString(ref sceneBuffer, ref image).Length != 0)
            {
                VCDFile.WriteLine("{0}\tparam2 \"{1}\"", Tab, PoolString(ref sceneBuffer, ref image));
            }
            sceneBuffer.BaseStream.Position += 2;
            if (PoolString(ref sceneBuffer, ref image).Length != 0)
            {
                VCDFile.WriteLine("{0}\tparam3 \"{1}\"", Tab, PoolString(ref sceneBuffer, ref image));
            }
            sceneBuffer.BaseStream.Position += 2;

            Ramp(VCDFile, sceneBuffer, true, true);

            //flags
            Flags = sceneBuffer.ReadByte();
            if ((Flags & 0x01) != 0)
            {
                VCDFile.WriteLine("{0}\tresumecondition", Tab);
            }
            if ((Flags & 0x02) != 0)
            {
                VCDFile.WriteLine("{0}\tlockbodyfacing", Tab);
            }
            if ((Flags & 0x04) != 0)
            {
                VCDFile.WriteLine("{0}\tfixedlength", Tab);
            }
            if ((Flags & 0x08) == 0)
            {
                VCDFile.WriteLine("{0}\tactive 0", Tab);
            }
            if ((Flags & 0x10) != 0)
            {
                VCDFile.WriteLine("{0}\tforceshortmovement", Tab);
            }
            if ((Flags & 0x20) != 0)
            {
                VCDFile.WriteLine("{0}\tplayoverscript", Tab);
            }

            //distance to target
            float distance = sceneBuffer.ReadSingle();
            if (distance>0.0f)
            {
                VCDFile.WriteLine("{0}\tdistancetotarget {1:F2}", Tab, distance);
            }

            //relative tags
            tagCount = sceneBuffer.ReadByte();
            if (tagCount != 0)
            {
                VCDFile.WriteLine("{0}\ttags\n{0}\t{{", Tab);
                for (tag = 0; tag < tagCount; tag++)
                {
                    string tagString = PoolString(ref sceneBuffer, ref image);
                    sceneBuffer.BaseStream.Position += 2;
                    byte tagValue = sceneBuffer.ReadByte();
                    VCDFile.WriteLine("{0}\t\t\"{1}\" {2}", Tab, tagString, tagValue * One255th);

                }

                VCDFile.WriteLine("{0}\tt}}", Tab);
            }
            
            //flex timing tags
            tagCount = sceneBuffer.ReadByte();
            if (tagCount != 0)
            {
                VCDFile.WriteLine("{0}\ttags\n{0}\t{{", Tab);
                for (tag = 0; tag < tagCount; tag++)
                {
                    string tagString = PoolString(ref sceneBuffer, ref image);
                    sceneBuffer.BaseStream.Position += 2;
                    byte tagValue = sceneBuffer.ReadByte();
                    VCDFile.WriteLine("{0}\t\t\"{1}\" {2} 1", Tab, tagString, tagValue * One255th);

                }

                VCDFile.WriteLine("{0}\tt}}", Tab);
            }

            //absolute tags, ps: this looks ugly
            for (AbsoluteTagType=0;AbsoluteTagType<2;AbsoluteTagType++)
            {
                tagCount = sceneBuffer.ReadByte();
                if (tagCount!=0)
                {

                    VCDFile.Write("{0}\tabsolutetags", Tab);
                    if (AbsoluteTagType == 1)
                    {
                        VCDFile.WriteLine(" shifted_time", Tab);
                    }
                    else
                    {

                        VCDFile.WriteLine(" playback_time", Tab);
                    }

                    VCDFile.WriteLine("{0}\t{{", Tab);
                    for (tag=0;tag<tagCount;tag++)
                    {
                        string tagString = PoolString(ref sceneBuffer, ref image);
                        sceneBuffer.BaseStream.Position += 2;
                        UInt16 tagValue = sceneBuffer.ReadUInt16();
                        VCDFile.WriteLine("{0}\t\t\"{1}\" {2}", Tab, tagString, tagValue * One255th);
                    }
                }
            }
            //sequence duration
            if (event_Type ==Event_Type.Event_Gesture)
            {
                float duration = sceneBuffer.ReadSingle();
                if (duration!=0)
                {
                    VCDFile.WriteLine("{0}\tsequenceduration {1}", Tab, duration);
                }
            }
            //relative tag
        }

        private static void Ramp(StreamWriter VCD_writer, BinaryReader reader_bvcd, bool InEvent, bool InChannel)
        {
            throw new NotImplementedException();
        }
    }
}