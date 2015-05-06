﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PSARCReader
{
    class PSARC
    {
        struct Header
        {
            public uint MagicNumber; // 0x50534152 'PSAR'
            public uint VersionNumber;
            public uint CompressionMethod; // zlib or lzma
            public uint TotalTOCSize; // Includes Header
            public uint TOCEntrySize; // Size of a single entry in the TOC, currently 30. This allows the size to be expanded in the future while maintaining backward compat.
            public uint numFiles;
            public uint blockSize; // The size of each block decompressed.
            public uint archiveFlags;
        }

        struct TOCEntry
        {
            public byte[] MD5; // len16, all 0's hash is used for the manifest file.
            public uint blockListStart;
            public ulong originalSize;
            public ulong startOffset; // BACK THE READING POINTER UP BY 3!!
        }

        public PSARC()
        {
            EndianReader br = new EndianReader(new FileStream(@"C:\Users\diwidog\Documents\Development Items\PC\pak23.psarc", FileMode.Open, FileAccess.Read), EndianType.BigEndian);
            Header psHeader = new Header();
            List<TOCEntry> TOCs = new List<TOCEntry>();
            psHeader.MagicNumber = br.ReadUInt32();
            psHeader.VersionNumber = br.ReadUInt32();
            psHeader.CompressionMethod = br.ReadUInt32();
            psHeader.TotalTOCSize = br.ReadUInt32();
            psHeader.TOCEntrySize = br.ReadUInt32();
            psHeader.numFiles = br.ReadUInt32();
            psHeader.blockSize = br.ReadUInt32();
            psHeader.archiveFlags = br.ReadUInt32();

            for (uint i = 0; i < psHeader.numFiles; i++)
            {
                TOCEntry tmp = new TOCEntry();
                tmp.MD5 = br.ReadBytes(16);
                tmp.blockListStart = br.ReadUInt32();
                tmp.originalSize = FortyBitInt(br.ReadUInt64());
                br.BaseStream.Position -= 3;
                tmp.startOffset = FortyBitInt(br.ReadUInt64());
                br.BaseStream.Position -= 3;
                TOCs.Add(tmp);
            }
            // Extract the Manifest File
            br.BaseStream.Position = (long)TOCs[0].startOffset; // Manifest File
            byte[] CompressedStream = br.ReadBytes((int)(TOCs[1].startOffset - TOCs[0].startOffset));
            byte[] DecompressedStream = zlib_net.Inflate(CompressedStream);
            
            File.WriteAllBytes(@"C:\Users\diwidog\Documents\Development Items\PC\pak23.manifest", DecompressedStream);
            br.Close();
        }

        private ulong FortyBitInt(ulong InputData)
        {
            return InputData >> 24;
        }

        

    }
}
