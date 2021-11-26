using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace TiledCS
{
    static class TiledXmlTileDecoder
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        /// <summary>
        /// How many times we shift the FLIPPED flags to the right in order to store it in a byte.
        /// For example: 0b10100000000000000000000000000000 >> SHIFT_FLIP_FLAG_TO_BYTE = 0b00000101
        /// </summary>
        const int SHIFT_FLIP_FLAG_TO_BYTE = 29;

        public static (int[], byte[]) ParseTiles(XmlNode nodeData)
        {            
            var rawTiles = DecodeRawData(nodeData).ToList();

            var data = new int[rawTiles.Count];
            var flag = new byte[rawTiles.Count];

            for (var i = 0; i < rawTiles.Count; i++)
            {
                var rawID = rawTiles[i];
                var hor = rawID & FLIPPED_HORIZONTALLY_FLAG;
                var ver = rawID & FLIPPED_VERTICALLY_FLAG;
                var dia = rawID & FLIPPED_DIAGONALLY_FLAG;

                flag[i] = (byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE);

                // assign data to rawID with the rotation flags cleared
                data[i] = (int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
            }

            return (data, flag);
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

            using (var s = new MemoryStream(rawdata))
            {
                return DecodeStream(s, compression);
            }            
        }

        private static IEnumerable<uint> DecodeStream(MemoryStream rawStream, string compression)
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
