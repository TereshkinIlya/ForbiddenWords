using SearchingSystems;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SearchingSystems.Interfaces;

namespace ForbiddenWords
{
    public delegate Task<string[]> GetFiles(string[] something);
    public partial class Form1 : Form
    {
        private DriveInfo[]? _drivers;
        private string[]? _selectedDirs;
        private FileInfo[] _foundFiles;
        private string[]? _pattern;
        private string _destinationFolder;
        private string _dictionaryFile;
        private StringBuilder _wordsFromTextBox;
        private FolderBrowserDialog _browserDialog;
        private ParallelProcessing _parallelCopying;
        private ISearchFiles Searcher { get; set; }
        public Form1()
        {
            InitializeComponent();
            Searcher = new SearchTXT();
            _parallelCopying = new ParallelProcessing(Searcher);
            _drivers = DriveInfo.GetDrives();
            _wordsFromTextBox = new StringBuilder();
            _browserDialog = new FolderBrowserDialog();
            _pattern = new string[0];
            foreach (DriveInfo driver in _drivers)
                listView1.Items.Add(driver.Name);
        }
        private void GetSelectedDirectories()
        {
            _selectedDirs = new string[listView1.CheckedItems.Count];

            for (int i = 0; i < listView1.CheckedItems.Count; i++)
            {
                _selectedDirs[i] = listView1.CheckedItems[i].Text;
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            

            ChooseTheSavingFolder();
            if (listView1.CheckedItems.Count == 0)
            {
                MessageBox.Show("Сделайте выбор!");
                return;
            }

            RestartSearching();
            if (!PreparePattern()) return;

            StartProgressBar();
            _foundFiles = await Searcher.SearchFilesAsync(_selectedDirs);
            StopProgressBar();

            listBox1.Items.AddRange(_foundFiles.Select(file => file.Name).ToArray());
            FileInfo[] _files;
            _files = await _parallelCopying.CopyFilesAsync(_pattern, _foundFiles, _destinationFolder);
            listBox2.Items.AddRange(_files.Select(file => file.Name).ToArray());

            label3.Text = _foundFiles.Length.ToString();
            _pattern = new string[0];
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            GetSelectedDirectories();
        }
        private void RestartSearching()
        {
            if (_foundFiles != null)
            {
                _foundFiles = null;
                listBox1.Items.Clear();
                button1.Enabled = false;
                progressBar1.Visible = true;
                label4.Visible = true;
                label8.Text = "";
            }
        }
        private void StartProgressBar()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            button1.Enabled = false;
        }
        private void StopProgressBar()
        {
            if (_foundFiles != null)
            {
                button1.Enabled = true;
                progressBar1.Visible = false;
                label4.Visible = false;
            }
        }
        private void ChooseTheSavingFolder()
        {
            _browserDialog.Description = "Выберите папку для сохранения найденных файлов";
            DialogResult _result = _browserDialog.ShowDialog();
            if (_result == DialogResult.OK)
            {
                _destinationFolder = _browserDialog.SelectedPath;
            }
            if(_result == DialogResult.Cancel)
            {
                MessageBox.Show("Выберите папку");
                ChooseTheSavingFolder();
            }
        }
        private void OpenTXTFile()
        {
            Regex separate = new Regex("[\\s\\p{P};:!?]");
            OpenFileDialog _fileDialog = new OpenFileDialog()
            {
                DefaultExt = "txt",
                Title = "Выберите файл",
                Filter = "Текстовые файлы (*.txt)|*.txt",
            };
            DialogResult result = _fileDialog.ShowDialog();
            if (result == DialogResult.Cancel) return;

            _dictionaryFile = _fileDialog.InitialDirectory+_fileDialog.FileName;
            label8.Text = _dictionaryFile;

            _pattern = _pattern.Concat(separate.Split(File.ReadAllText(_dictionaryFile))).ToArray();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _wordsFromTextBox.Clear();
            _wordsFromTextBox.Append(textBox1.Text);
        }
        private bool PreparePattern()
        {
            if (_wordsFromTextBox.ToString() == "" && _pattern.Length == 0)
            {
                MessageBox.Show("Введите что-нибудь");
                return false;
            }
            Regex separate = new Regex("[\\s\\p{P};:!?]");

            _pattern = _pattern.Concat(separate.Split(_wordsFromTextBox.ToString())).ToArray();
            _pattern = _pattern.Where(word => word != "" && !String.IsNullOrWhiteSpace(word)).Distinct().ToArray();
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenTXTFile();
        }
    }
}