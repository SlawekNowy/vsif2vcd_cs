using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Compressors.LZMA;

namespace vsif2vcd
{
    static class VSIFParser
    {

        struct VSIF_Header
        {
            public UInt32 ID;
            public UInt32 Version;
            public UInt32 ScenesCount;
            public UInt32 StringsCount;
            public UInt32 EntryOffset;
        }
        struct VSIF_Entry
        {
            public UInt32 CRC;
            public UInt32 Offset;
            public UInt32 Size;
            public UInt32 SummaryOffset;

        }
        internal static void Extract(string gameDirectory)
        {
            FileStream ImageFile;
            try
            {
                ImageFile = File.OpenRead(gameDirectory + "/scenes/scenes.image");
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Didn't find scenes.image");
                Console.WriteLine(e.Message);
                return;
            }
            catch (UnauthorizedAccessException f)
            {
                Console.WriteLine("Cannot read scenes.image file.");
                Console.WriteLine(f.Message);
                return;
            }
            MemoryStream image = new MemoryStream();
            ImageFile.CopyTo(image);
            ImageFile.Close();
            image.Position = 0;
            VSIF_Header Header = PopulateHeader(image);

            if (Header.ID != Common.FourCC("VSIF",false))
            {
                Console.WriteLine("Scenes.image has incorrect Magic");
                return;
            }
            if (Header.Version != 2)
            {
                Console.WriteLine("Scenes.image has incorrect version");
                return;
            }
            if (Header.ScenesCount == 0)
            {
                Console.WriteLine("Scenes.image is empty");
                return;
            }
            Console.WriteLine("Extracting scenes.image ({0} scenes)\n", Header.ScenesCount);

            /* Extraction */
            for (int i = 0; i < Header.ScenesCount; i++)
            {
                ExtractScene(ref image, i,Header); //there's only one VSIF Header
            }
            //VSIF_ExtractScene(Image, i, Maps, MapsCount, GameDirectory);

            //free(Image);
            //printf("Finished extracting scenes from game %s\n", GameDirectory);
            //return 0;
            image.Close();
            Console.WriteLine("Finished extracting scenes from {0}", gameDirectory);
        }

        private static VSIF_Header PopulateHeader(MemoryStream image)
        {

            using (BinaryReader bin_img = new BinaryReader(image, System.Text.Encoding.UTF8, true))
            {
                long pnt = image.Position;
                VSIF_Header Header = new VSIF_Header()
                {
                    ID = bin_img.ReadUInt32(),
                    Version = bin_img.ReadUInt32(),
                    ScenesCount = bin_img.ReadUInt32(),
                    StringsCount = bin_img.ReadUInt32(),
                    EntryOffset = bin_img.ReadUInt32()
                };
                image.Seek(pnt, SeekOrigin.Begin);
                return Header;
            }
            //BinaryReader bin_img = new BinaryReader(image);

            //bin_img.Close();

        }
        private static VSIF_Entry PopulateEntry(MemoryStream image)
        {

            using BinaryReader bin_img = new BinaryReader(image, System.Text.Encoding.UTF8, true);
            long pnt = image.Position;
            VSIF_Entry Entry = new VSIF_Entry()
            {
                CRC = bin_img.ReadUInt32(),
                Offset = bin_img.ReadUInt32(),
                Size = bin_img.ReadUInt32(),
                SummaryOffset = bin_img.ReadUInt32()
            };
            image.Seek(pnt, SeekOrigin.Begin);
            return Entry;
            //BinaryReader bin_img = new BinaryReader(image);

            //bin_img.Close();

        }

        private static void ExtractScene( ref MemoryStream image, int i,VSIF_Header Header)
        {
            //VSIF_Header sceneHeader = PopulateHeader(image);
            UInt32 entryStart = (UInt32)(Header.EntryOffset + (i * sizeof(UInt32) * 4));
            image.Seek(entryStart, SeekOrigin.Begin);
            VSIF_Entry Entry = PopulateEntry(image);
            MemoryStream SceneBuffer;
            FileStream VCDFile =null;

            UInt32 SceneBufferSize = UncompressScene(ref image,Entry.Offset, out SceneBuffer, Entry.Size);

            if (SceneBufferSize==0)
            {
                Console.Error.WriteLine("Failed to uncompress scene with CRC {0}", Entry.CRC);
                return;
            }
            //if (BVCDDecompiler.OpenVCDForWriting(out VCDFile,Entry.CRC))
            //{

            //}

           // throw new NotImplementedException();

            BVCDDecompiler.OpenVCDForWriting(out VCDFile, Entry.CRC);
            SceneBuffer.Position = 0;
            image.Position = 0;
            if (VCDFile != null)
            {
                if (!BVCDDecompiler.Decompile(ref VCDFile, ref SceneBuffer, ref image))
                {
                    Console.WriteLine("Cannot decompile VCD with CRC {0:x}", Entry.CRC);
                }
                //VCDFile.Close();
            }
            //SceneBuffer.Close();

            //Close methods of BinaryReader and StringWriter close underlying streams by default
            
        }

        private static uint UncompressScene(ref MemoryStream image,uint offset, out MemoryStream sceneBuffer, uint size) 

        {
            // throw new NotImplementedException();
            sceneBuffer = new MemoryStream();
            //BinaryReader bin = new BinaryReader()
            MemoryStream rawSceneBuffer = new MemoryStream();
            image.Position = offset;
            int _size = (int)Convert.ToInt64(size);
            CopyStream(image,rawSceneBuffer,_size);
            rawSceneBuffer.Position = 0;
            BinaryReader reader = new BinaryReader(rawSceneBuffer);
            UInt32 Magic = reader.ReadUInt32();
            rawSceneBuffer.Position = 0;

            if (Magic == Common.FourCC("LZMA", false))
            {
                rawSceneBuffer.Position = 4;
                UInt32 RealSize = reader.ReadUInt32();
                UInt32 CompressedSize = reader.ReadUInt32();
                byte[] properties = new byte[5];
                properties = reader.ReadBytes(5);

                //rawSceneBuffer.Position = 0;

                LzmaStream lzmaStream = new LzmaStream(properties, rawSceneBuffer, size, CompressedSize);
                //_ = lzmaStream.Position;

                throw new NotImplementedException();
            }
            else if (Magic == Common.FourCC("bvcd", false))
            {

                rawSceneBuffer.CopyTo(sceneBuffer);
                return size;
            }
            return 0;
            
        }

        private static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}