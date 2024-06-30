using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EvershadeTexture.DataSys {
    public class DataTexture {
        public TextureMetadata Metadata = new();
        public byte[] Data;

        public BitmapImage PreviewTexture;
        public int MatchIndex;

        public void SetTextureData(byte[] data, int index) {
            if (data == null || data.Length < index + TextureMetadata.MetadataSize) {
                throw new IndexOutOfRangeException("Texture data is smaller than required size.");
            }

            using (MemoryStream metadataStream = new MemoryStream(data, index, TextureMetadata.MetadataSize))
            using (BinaryReader metadataReader = new BinaryReader(metadataStream)) {
                metadataReader.BaseStream.Seek(4, SeekOrigin.Current); // Skip Identifier

                Metadata.HashID = metadataReader.ReadUInt32();
                Metadata.TextureSize = metadataReader.ReadUInt32();
                Metadata.HashID2 = metadataReader.ReadUInt32();
                Metadata.Padding = metadataReader.ReadUInt32();

                metadataReader.BaseStream.Seek(4, SeekOrigin.Current); // Skip Unknown 4 bytes

                Metadata.Width = metadataReader.ReadUInt16();
                Metadata.Height = metadataReader.ReadUInt16();

                metadataReader.BaseStream.Seek(3, SeekOrigin.Current); // Skip Unknown 2 bytes

                Metadata.MipmapLevel = metadataReader.ReadByte();
            }

            int textureDataSize = (int)Metadata.TextureSize;
            if (data.Length < index + TextureMetadata.MetadataSize + textureDataSize) {
                throw new IndexOutOfRangeException("Texture data is smaller that texture size.");
            }

            Data = new byte[textureDataSize];
            Array.Copy(data, index + TextureMetadata.MetadataSize, Data, 0, textureDataSize);

            byte[] compression = new byte[4];

            Array.Copy(Data, 84, compression, 0, 4);
            Console.WriteLine(compression);

            if (DataFile.IsByteSequence(compression, TextureMetadata.DXT1, 0)) { Metadata.CompressionFormat = TexCompressionFormat.DXT1; } else
            if (DataFile.IsByteSequence(compression, TextureMetadata.DXT5, 0)) { Metadata.CompressionFormat = TexCompressionFormat.DXT5; } else
            { throw new NotSupportedException("Only DXT1 and DXT5 encoding formats are supported."); }

            MatchIndex = index;
        }

        public void MakePreviewTexture() {
            if (Data == null) { return; }

            BitmapImage bitmap = new BitmapImage();
            using (var stream = new System.IO.MemoryStream(Data)) {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            PreviewTexture = bitmap;
        }

        public string GetFormatedMetadata() {
            uint mipmapLevel = TextureMetadata.GetFormatedMipmapLevel(Metadata.MipmapLevel);
            uint minMipmapSize = TextureMetadata.GetMinMipmapSize(Metadata.Width, Metadata.Height, mipmapLevel);
            string compressionFormat = TextureMetadata.GetFormatedCompression(Metadata.CompressionFormat);

            string output = $"""
                Hash ID: 0x{Metadata.HashID:X8}
                Size: {Metadata.TextureSize} bytes
                Dimensions: {Metadata.Width}x{Metadata.Height}
                Mipmap Level: Levels: {mipmapLevel} ({minMipmapSize}px)
                Compression Format: {Metadata.CompressionFormat} ({compressionFormat})
                """;

            return output;
        }
    }

    public class TextureMetadata {
        public static readonly byte[] Identifier = { 0x50, 0xD3, 0x77, 0xE9 };
        public static readonly byte[] DXT1 = { 0x44, 0x58, 0x54, 0x31 };
        public static readonly byte[] DXT5 = { 0x44, 0x58, 0x54, 0x35 };
        public static readonly byte[] DDSIdentifier = { 0x44, 0x44, 0x53, 0x20};

        public const int MetadataSize = 48;

        public UInt32 HashID;
        public UInt32 TextureSize;
        public UInt32 HashID2;
        public UInt32 Padding;
        // Unknown (4 Bytes)
        public UInt16 Width;
        public UInt16 Height;
        // Unknown (2 Bytes)
        public byte MipmapLevel;
        // Unknown (20 Bytes)
        public TexCompressionFormat CompressionFormat;
        // Unknown (2 Bytes)
        public static uint GetFormatedMipmapLevel(byte mipmapLevel) {
            switch (mipmapLevel) {
                case 0x00: return 0;
                case 0x11: return 1;
                case 0x22: return 2;
                case 0x33: return 3;
                case 0x44: return 4;
                case 0x55: return 5;
                case 0x66: return 6;
                case 0x77: return 7;
                case 0x88: return 8;
                case 0x99: return 9;
                case 0xAA: return 10;
                case 0xBB: return 11;
                case 0xCC: return 12;
                case 0xDD: return 13;
                case 0xEE: return 14;
                case 0xFF: return 15;
            }

            return 0;
        }

        public static uint GetMinMipmapSize(uint width, uint height, uint mipmapLevels) {
            for (uint i = 0; i < mipmapLevels - 1; i++) {
                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);
            }

            return Math.Max(width, height);
        }

        public static string GetFormatedCompression(TexCompressionFormat format) {
            if (format == TexCompressionFormat.DXT1) { return "BC1"; } else
            if (format == TexCompressionFormat.DXT5) { return "BC3"; }

            return "?";
        }
    }

    public enum TexCompressionFormat {
        DXT1, // BC1
        DXT5  // BC3
    }
}
