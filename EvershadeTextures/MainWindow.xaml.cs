using EvershadeTexture.DataSys;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using Microsoft.Windows.Themes;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Reflection.Emit;
using System.ComponentModel;

namespace EvershadeTextures {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public DataFile DataFile { get; set; }
        public int SelectedTextureIndex;
        private bool IndexTextboxIsUserChange = false;

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
        }

        private void SelectTexture(int index, bool scrollToIndex = false) {
            index = Math.Clamp(index, 0, DataFile.Textures.Count - 1);

            SelectedTextureIndex = index;

            IndexTextboxIsUserChange = false;
            TextureIndexBox.Text = index.ToString();
            IndexTextboxIsUserChange = true;

            PreviewTextureImage.Source = DataFile.Textures[SelectedTextureIndex].PreviewTexture;
            PreviewTextureInfo.Text = DataFile.Textures[SelectedTextureIndex].GetFormatedMetadata();

            if (scrollToIndex) {
                TextureListScroll.ScrollToVerticalOffset(index * 100);
            }
        }

        private void ToggleFileOptions(bool enable) {
            SaveFileButton.IsEnabled = enable;
            SaveFileAsButton.IsEnabled = enable;
            ImportButton.IsEnabled = enable;
            AutoImportButton.IsEnabled = false;
            ExportButton.IsEnabled = enable;
            ExportAllButton.IsEnabled = enable;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void TitleButtonMinimize(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void TitleButtonClose(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void ClickOpenFileButton(object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "Data files|*.data";
            bool? dialogResult = fileDialog.ShowDialog();

            if (dialogResult == true) {
                DataFile = new();
                DataFile.FilePath = fileDialog.FileName;

                DataFile.GetDataFromFile();
                DataFile.GetTextures();

                if (DataFile.Textures.Count > 0) {
                    SelectTexture(0);
                    ToggleFileOptions(true);
                } else {
                    PreviewTextureImage.Source = null;
                    PreviewTextureInfo.Text = String.Empty;
                    ToggleFileOptions(false);
                }

                UpdateTextureListPanel();
            }
        }

        private void ClickSaveFileButton(object sender, RoutedEventArgs e) {
            if (File.Exists(DataFile.FilePath)) {
                DataFile.SetTextures();
                DataFile.SaveDataToFile();
            } else {
                ClickSaveFileAsButton(sender, e);
            }
        }

        private void ClickSaveFileAsButton(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Data files|*.data";
            bool? result = saveFileDialog.ShowDialog();

            if (result == true) {
                DataFile.SetTextures();
                DataFile.SaveDataToFile(saveFileDialog.FileName);
            }
        }

        private void ClickImportButton(object sender, RoutedEventArgs e) {
            int index = SelectedTextureIndex;

            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "DirectDraw Surface|*.dds";
            bool? dialogResult = fileDialog.ShowDialog();

            if (dialogResult == true) {
                DataFile.ImportTexture(fileDialog.FileName, index);
                DataFile.SetTextures();
                SelectTexture(index);
            }
        }

        private void ClickExportButton(object sender, RoutedEventArgs e) {
            int index = SelectedTextureIndex;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "DirectDraw Surface|*.dds";
            bool? result = saveFileDialog.ShowDialog();

            if (result == true) {
                DataFile.ExportTexture(saveFileDialog.FileName, index);
                DataFile.SetTextures();
            }
        }

        private void ClickExportAllButton(object sender, RoutedEventArgs e) {
            CommonOpenFileDialog folderDialog = new();
            folderDialog.IsFolderPicker = true;
            CommonFileDialogResult result = folderDialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok) {
                for (int index = 0; index < DataFile.Textures.Count; index++) {
                    string path = Path.Combine(folderDialog.FileName, Path.GetFileNameWithoutExtension(DataFile.FilePath) + $"_{index}.dds");
                    DataFile.ExportTexture(path, index);
                }
            }
        }

        private void UpdateTextureListPanel() {
            TextureListPanel.Children.Clear();

            for (int index = 0; index < DataFile.Textures.Count; index++) {
                TextureListPanel.Children.Add(MakeTextureDisplay(index));
            }
        }

        private Button MakeTextureDisplay(int index) {
            Button displayButton = new() {
                Style = (Style)Application.Current.Resources["ModernButton"],
                Height = 90,
                Margin = new Thickness(0, 0, 0, 10),
                Tag = index
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(displayButton.Height) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Image image = new Image {
                Margin = new Thickness(5),
                Source = DataFile.Textures[index].PreviewTexture
            };

            TextBlock textBlock = new TextBlock {
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                Text = DataFile.Textures[index].GetFormatedMetadata(),
            };
            Grid.SetColumn(textBlock, 1);

            grid.Children.Add(image);
            grid.Children.Add(textBlock);
            displayButton.Content = grid;

            displayButton.Click += PreviewTexture_OnDisplayClick;

            return displayButton;
        }

        private void PreviewTexture_OnDisplayClick(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            SelectTexture(Convert.ToInt32(button.Tag));
        }

        private void PreviewTexture_NumboxChanged(object sender, TextChangedEventArgs e) {
            if (DataFile == null || DataFile.Textures == null || DataFile.Textures.Count == 0) {
                return;
            }

            if (!IndexTextboxIsUserChange) { return; }

            int caretIndex = TextureIndexBox.CaretIndex;
            string text = new String(TextureIndexBox.Text.Where(char.IsDigit).ToArray());

            if (String.IsNullOrWhiteSpace(text)) {
                text = "0";
            }

            int inputIndex = Math.Clamp(int.Parse(text), 0, DataFile.Textures.Count - 1);
            SelectTexture(inputIndex, true);

            TextureIndexBox.CaretIndex = Math.Min(caretIndex, TextureIndexBox.Text.Length);
        }

        private void PreviewTexture_SkipButtonClick(object sender, RoutedEventArgs e) {
            if (DataFile == null || DataFile.Textures == null || DataFile.Textures.Count == 0) {
                return;
            }

            Button button = (Button)sender;
            SelectTexture(SelectedTextureIndex + Convert.ToInt32(button.Tag), true);
        }
    }
}