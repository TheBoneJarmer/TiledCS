using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents the index of a tile in a <see cref="TiledMapTileset"/>.
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("{Index} {HasHorizontalFlip?'🡄':'🡆'} {HasVerticalFlip?'🡇':'🡅'} {HasDiagonalFlip?'⤵':' '}")]
    public readonly struct TiledIndex
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        const uint INDEX_MASK = ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);

        public TiledIndex(uint rawValue) { RawValue = rawValue; }

        private readonly uint RawValue;

        public int Index => (int)(RawValue & INDEX_MASK);

        public bool HasHorizontalFlip => (RawValue & FLIPPED_HORIZONTALLY_FLAG) != 0;
        public bool HasVerticalFlip => (RawValue & FLIPPED_VERTICALLY_FLAG) != 0;
        public bool HasDiagonalFlip => (RawValue & FLIPPED_DIAGONALLY_FLAG) != 0;
    

        public static TiledIndex[] TryCreateFrom(XmlNode nodeData)
        {
            return DecodeRawData(nodeData).Select(raw => new TiledIndex(raw)).ToArray();
        }

        private static string GetEncoding(XmlNode node)
        {
            if (node.Name == "chunk") return GetEncoding(node.ParentNode);
            if (node.Name == "data") return node.Attributes.GetStringOrDefault("encoding", null);
            throw new TiledException($"{node.Name} doesn't have an encoding attribute.");
        }

        private static string GetCompression(XmlNode node)
        {
            if (node.Name == "chunk") return GetCompression(node.ParentNode);
            if (node.Name == "data") return node.Attributes.GetStringOrDefault("compression", null);
            throw new TiledException($"{node.Name} doesn't have an compression attribute.");
        }

        private static IEnumerable<uint> DecodeRawData(XmlNode node)
        {
            string encoding = GetEncoding(node);

            if (encoding == "csv") return DecodeRawCSV(node);
            if (encoding == "base64") return DecodeRawBase64(node);

            throw new TiledException("Only CSV and Base64 encodings are currently supported");
        }

        private static IEnumerable<uint> DecodeRawCSV(XmlNode node)
        {
            var csvs = node.InnerText.Split(',');

            // Parse the comma separated csv string
            for (var i = 0; i < csvs.Length; i++)
            {
                yield return uint.Parse(csvs[i]);
            }
        }

        private static IEnumerable<uint> DecodeRawBase64(XmlNode node)
        {
            var compression = GetCompression(node);
            var rawdata = Convert.FromBase64String(node.InnerText);
            return DecodeStream(rawdata, compression);
        }

        private static IEnumerable<uint> DecodeStream(byte[] rawdata, string compression)
        {
            using (var rawStream = new MemoryStream(rawdata, false))
            {
                if (compression == null)
                {
                    // Parse the decoded bytes
                    var rawBytes = new byte[4];

                    for (var i = 0; i < rawStream.Length; i++)
                    {
                        rawStream.Read(rawBytes, 0, rawBytes.Length);
                        yield return BitConverter.ToUInt32(rawBytes, 0);
                    }
                }
                else if (compression == "zlib")
                {
                    // .NET doesn't play well with the headered zlib data that Tiled produces,
                    // so we have to manually skip the 2-byte header to get what DeflateStream's looking for
                    // Should an external library be used instead of this hack?
                    rawStream.ReadByte();
                    rawStream.ReadByte();

                    using (var decompressionStream = new DeflateStream(rawStream, CompressionMode.Decompress))
                    {
                        // Parse the raw decompressed bytes
                        var decompressedDataBuffer = new byte[4]; // size of each tile
                        var dataRotationFlagsList = new List<byte>();
                        var layerDataList = new List<int>();

                        while (decompressionStream.Read(decompressedDataBuffer, 0, decompressedDataBuffer.Length) == decompressedDataBuffer.Length)
                        {
                            yield return BitConverter.ToUInt32(decompressedDataBuffer, 0);
                        }
                    }
                }
                else if (compression == "gzip")
                {
                    using (var decompressionStream = new GZipStream(rawStream, CompressionMode.Decompress))
                    {
                        // Parse the raw decompressed bytes and update the inner data as well as the data rotation flags
                        var decompressedDataBuffer = new byte[4]; // size of each tile
                        var dataRotationFlagsList = new List<byte>();
                        var layerDataList = new List<int>();

                        while (decompressionStream.Read(decompressedDataBuffer, 0, decompressedDataBuffer.Length) == decompressedDataBuffer.Length)
                        {
                            yield return BitConverter.ToUInt32(decompressedDataBuffer, 0);
                        }
                    }
                }
                else
                {
                    throw new TiledException($"{compression} compression is currently not supported");
                }
            }
        }
    }
}
