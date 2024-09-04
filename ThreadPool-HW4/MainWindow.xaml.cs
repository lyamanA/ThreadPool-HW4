using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;


namespace ThreadPool_HW4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isCancelled;

        public MainWindow()
        {
            InitializeComponent();
            _isCancelled = false;
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.OpenFileDialog предоставляет пользователю
            //возможность выбора одного или нескольких файлов на его компьютере через графический интерфейс. 
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePathTextBox.Text) || !File.Exists(FilePathTextBox.Text))
            {
                MessageBox.Show("Please select a valid file.");
                return;
            }

            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Please enter a password.");
                return;
            }

            _isCancelled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(EncryptDecryptFile));
        }

        private void EncryptDecryptFile(object state)
        {
            string filePath = FilePathTextBox.Dispatcher.Invoke(() => FilePathTextBox.Text);
            string password = PasswordBox.Dispatcher.Invoke(() => PasswordBox.Password);
            bool isEncrypt = EncryptRadioButton.Dispatcher.Invoke(() => EncryptRadioButton.IsChecked == true);

            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] resultBytes = new byte[fileBytes.Length];

                ProgressBar.Dispatcher.Invoke(() => ProgressBar.Maximum = fileBytes.Length);

                for (int i = 0; i < fileBytes.Length; i++)
                {
                    if (_isCancelled)
                    {
                        ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value = 0);
                        MessageBox.Show("Operation cancelled.");
                        return;
                    }

                    resultBytes[i] = (byte)(fileBytes[i] ^ passwordBytes[i % passwordBytes.Length]);
                    ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value = i + 1);

                    Thread.Sleep(1); //задержка для демонстрации прогресса
                }

                //"encrypted" (зашифрованный) - расширение помогает легко отличить зашифрованные
                //файлы от обычных текстовых или других файлов
                string outputFilePath = isEncrypt ? filePath + ".enc" : filePath.Replace(".enc", "");
                File.WriteAllBytes(outputFilePath, resultBytes);

                MessageBox.Show(isEncrypt ? "File encrypted successfully!" : "File decrypted successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value = 0);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isCancelled = true;
        }

    }
}