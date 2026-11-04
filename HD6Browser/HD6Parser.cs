using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HD6Browser
{
    public class HD6Entry
    {
        public string? Path { get; set; }
        public long DatOffset { get; set; }
        public long FileSize { get; set; }
    }

    public static class HD6Parser
    {
        private static readonly Encoding FileNameEncoding;

        static HD6Parser()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileNameEncoding = Encoding.GetEncoding(932);
        }

        public static List<HD6Entry> Parse(string hd6Path)
        {
            var entries = new List<HD6Entry>();

            using (FileStream fs = new FileStream(hd6Path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                // 1. Parse 52-byte Header
                string magic = Encoding.ASCII.GetString(br.ReadBytes(4));
                if (magic != "HD6\0")
                    throw new InvalidDataException("Not a valid HD6 file!");

                uint dictOffset = br.ReadUInt32();          // 0x34
                uint dictSize = br.ReadUInt32();            // 0x70A8
                uint dictCount = br.ReadUInt32();           // 4282
                br.ReadUInt32();                            // Reserved
                uint filenameTableOffset = br.ReadUInt32(); // 0x70DC
                br.ReadUInt32();                            // Filename table size
                br.ReadUInt32();                            // Reserved
                uint sizeMultiplier = br.ReadUInt32();      // 0x10
                uint entryCount = br.ReadUInt32();          // 13369
                uint entryTableOffset = br.ReadUInt32();    // 0x25C70

                // 2. Load Dictionary
                // We load the raw byte arrays instead of pre-decoding them into strings.
                // This prevents multi-byte Shift-JIS characters from getting mangled if they 
                // are split across two different dictionary fragments.
                fs.Position = dictOffset;
                var dict = new List<byte[]>();
                var nameBytes = new List<byte>();
                long dictEnd = dictOffset + dictSize;

                while (fs.Position < dictEnd && dict.Count < dictCount)
                {
                    byte b = br.ReadByte();
                    if (b == 0)
                    {
                        dict.Add(nameBytes.ToArray());
                        nameBytes.Clear();
                    }
                    else
                    {
                        nameBytes.Add(b);
                    }
                }

                // 3. Unpack Entries
                for (int i = 0; i < entryCount; i++)
                {
                    fs.Position = entryTableOffset + (i * 8);
                    ulong rawEntry = br.ReadUInt64();

                    // Bitfield unpacking
                    uint nameOffset = (uint)(rawEntry & 0x3FFFF);        // 18 bits
                    ulong fileOffset = (rawEntry >> 18) & 0x3FFFFF;     // 22 bits
                    ulong fileSize = (rawEntry >> 40) & 0xFFFFFF;       // 24 bits

                    long datOffsetBytes = (long)fileOffset * 0x800;     // 0x800 multiplier
                    long sizeBytes = (long)fileSize * sizeMultiplier;  // 0x10 multiplier

                    // Decode Filename String using the raw byte dictionary
                    fs.Position = filenameTableOffset + nameOffset;
                    string fullPath = DecodePath(br, dict);

                    if (!string.IsNullOrWhiteSpace(fullPath))
                    {
                        entries.Add(new HD6Entry
                        {
                            Path = fullPath,
                            DatOffset = datOffsetBytes,
                            FileSize = sizeBytes
                        });
                    }
                }
            }

            return entries;
        }

        private static string DecodePath(BinaryReader br, List<byte[]> dict)
        {
            var pathBytes = new List<byte>();

            while (true)
            {
                // Decode LEB128 Variable-Length Integer
                uint index = 0;
                int shift = 0;
                byte b;
                do
                {
                    b = br.ReadByte();
                    index |= (uint)(b & 0x7F) << shift;
                    shift += 7;
                } while ((b & 0x80) != 0);

                if (index == 0 || index >= dict.Count)
                    break;

                // Concatenate the raw byte fragments together first
                pathBytes.AddRange(dict[(int)index]);
            }

            // Decode the fully assembled byte array as Shift-JIS.
            // This guarantees Japanese file paths decode correctly even if 
            // a single multi-byte character was split across two fragments.
            return FileNameEncoding.GetString(pathBytes.ToArray());
        }
    }
}