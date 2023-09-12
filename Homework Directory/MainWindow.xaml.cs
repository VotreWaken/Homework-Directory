using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Homework_Directory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        public MainWindow()
        {
            InitializeComponent();

            foreach (var drive in DriveInfo.GetDrives())
            {
                driveComboBox.Items.Add(drive.Name);
            }
        }
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            resultListView.Items.Clear();

            string searchMask = searchMaskTextBox.Text;
            string selectedDrive = driveComboBox.SelectedItem as string;

            if (string.IsNullOrEmpty(searchMask) || string.IsNullOrEmpty(selectedDrive))
            {
                MessageBox.Show("Please Enter a Search Mask and Select a Drive");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await SearchFilesAndFoldersAsync(selectedDrive, searchMask, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Search operation canceled.");
            }
        }

        private async Task SearchFilesAndFoldersAsync(string drive, string searchMask, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    DirectoryInfo rootDirectory = new DirectoryInfo(drive);

                    SearchFilesAndFolders(rootDirectory, searchMask, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }, cancellationToken);
        }

        private void SearchFilesAndFolders(DirectoryInfo directory, string searchMask, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                foreach (var file in directory.GetFiles(searchMask))
                {
                    AddFileOrFolderToListView(file.Name, "File");
                }

                foreach (var subDirectory in directory.GetDirectories())
                {
                    SearchFilesAndFolders(subDirectory, searchMask, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }

        private void AddFileOrFolderToListView(string name, string type)
        {
            Dispatcher.Invoke(() =>
            {
                resultListView.Items.Add(new { Name = name, Type = type });
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
