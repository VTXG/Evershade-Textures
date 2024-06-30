using System.IO;
using System.Windows;
using System.Windows.Media;

namespace EvershadeTexture.DataSys {
    public class DataFile {
        public List<DataTexture> Textures = new List<DataTexture>();
        public string FilePath;
        public byte[] FileData;

        public void GetDataFromFile() {
            if (File.Exists(FilePath)) {
                FileData = File.ReadAllBytes(FilePath);
            }
        }

        public void SaveDataToFile(string? filePath = null) {
            if (filePath == null) { File.WriteAllBytes(FilePath, FileData); return; }

            if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
                throw new DirectoryNotFoundException("Directory does not exist.");
            }

            File.WriteAllBytes(filePath, FileData);
        }

        public void GetTextures() {
            for (int index = 0; index < FileData.Length - TextureMetadata.Identifier.Length - 8; index++) {
                if (IsByteSequence(FileData, TextureMetadata.Identifier, index)) {
                    DataTexture texture = new DataTexture();
                    texture.SetTextureData(FileData, index);
                    texture.MakePreviewTexture();
                    Textures.Add(texture);

                    index += TextureMetadata.MetadataSize + (int)texture.Metadata.TextureSize - 1;
                }
            }
        }

        public void SetTextures() {
            foreach (DataTexture texture in Textures) {
                Array.Copy(texture.Data, 0, FileData, texture.MatchIndex + TextureMetadata.MetadataSize, texture.Data.Length);
            }
        }

        public void ImportTexture(string path, int index) {
            byte[] data = File.ReadAllBytes(path);

            if (data.Length != Textures[index].Data.Length) {
                MessageBox.Show("Imported texture does not have the same byte size.", "Importing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try {
                using (MemoryStream memoryStream = new MemoryStream(data)) {

                    byte[] fileIdentifier = new byte[4];
                    memoryStream.Read(fileIdentifier, 0, 4);

                    if (!IsByteSequence(fileIdentifier, TextureMetadata.DDSIdentifier, 0)) {
                        MessageBox.Show("Imported texture is not a DDS image.", "Importing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    byte[] compression = new byte[4];
                    memoryStream.Seek(84, SeekOrigin.Begin);
                    memoryStream.Read(compression, 0, 4);

                    if (IsByteSequence(compression, TextureMetadata.DXT1, 0)) { } else
                    if (IsByteSequence(compression, TextureMetadata.DXT5, 0)) { } else
                    { MessageBox.Show("Only DXT1 (BC1) and DXT5 (BC3) encoding formats are supported.", "Importing Error", MessageBoxButton.OK, MessageBoxImage.Error); return;  }

                    memoryStream.Seek(0x47, SeekOrigin.Begin);
                    memoryStream.WriteByte(0x54);
                }

                Textures[index].Data = data;
                Textures[index].MakePreviewTexture();
            } catch (Exception ex) {
                MessageBox.Show($"Unexpected importing error: {ex.Message}", "Importing Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportTexture(string path, int index) {
            try {
                File.WriteAllBytes(path, Textures[index].Data);
            } catch (Exception ex) {
                MessageBox.Show($"Unexpected exporting error: {ex.Message}", "Exporting Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static bool IsByteSequence(byte[] fileData, byte[] sequence, int index) {
            for (int i = 0; i < sequence.Length; i++) {
                if (fileData[index + i] != sequence[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}
