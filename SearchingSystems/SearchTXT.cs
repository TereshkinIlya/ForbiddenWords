using System.Linq;
using System.Text;
using SearchingSystems.Interfaces;

namespace SearchingSystems
{
    public class SearchTXT : ISearchFiles
    {
        private List<FileInfo> _allPaths;
        private FileInfo[] _filesTXT;
        private List<FileInfo> _copiedFiles;
        public SearchTXT()
        {
            _allPaths = new();
            _copiedFiles = new();
        }
        public async Task<FileInfo[]> SearchFilesAsync(string[] _dirNames)
        {
            _allPaths.Clear();
            foreach (string _currentDir in _dirNames)
                await GetTXTAsync(_currentDir);

            _allPaths.AddRange(_filesTXT);

            return _allPaths.ToArray();
        }
        private async Task<FileInfo[]> GetTXTAsync(string _currentDir)
        {
            _filesTXT = null;
            var options = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            if (Directory.Exists(_currentDir))
            {
                _filesTXT = await Task<FileInfo>.Run(() =>
                {
                    string[] _paths = Directory.GetFiles(_currentDir, "*.txt", options);
                    FileInfo[] _filesTXT = _paths.Select(path => new FileInfo(path)).ToArray();

                    if (_filesTXT != null)
                        return _filesTXT;
                    else 
                        throw new ArgumentNullException();
                });
            }

            return _filesTXT;
        }
        public FileInfo[] CopyFiles(string[]? _pattern, FileInfo[]? _foundFiles, string _savingPath)
        {
            ArgumentNullException.ThrowIfNull(_pattern, "пустой шаблон");
            ArgumentNullException.ThrowIfNull(_foundFiles, "нет файлов для поиска соответствия");
            ArgumentNullException.ThrowIfNull(_savingPath, "некорректный путь для сохранения файлов");

            StringBuilder _textFromFile = new StringBuilder ();
            Dictionary<FileInfo, int> _sameFiles = new Dictionary<FileInfo, int> ();
            int indexForSameFileName = 1;

            var _filesInDirectory = Directory.GetFiles(_savingPath).
                Select(file => new FileInfo(file));

            var _filesForDeleting = from fd in _filesInDirectory
                                    join cf in _copiedFiles on
                                    fd.Name.Remove(fd.Name.IndexOf('.'))
                                    equals
                                    cf.Name.Remove(cf.Name.IndexOf('.'))
                                    select fd;

            foreach (FileInfo file in _filesForDeleting)
            {
                File.SetAttributes(file.FullName, FileAttributes.Normal);
                File.Delete(file.FullName);
            }

            _copiedFiles.Clear();

            for (int i = 0; i < _foundFiles.Length; i++)
            {
                _textFromFile.Clear();
                try
                {
                    _textFromFile.Append(File.ReadAllText(_foundFiles[i].FullName));
                }
                catch (Exception) { }

                foreach (string word in _pattern)
                {
                    if (_textFromFile.ToString().SequenceEqual(word))
                    {
                        try
                        {
                            File.Copy(_foundFiles[i].FullName, _savingPath + "\\" + _foundFiles[i].Name);

                            _copiedFiles.Add(_foundFiles[i]);
                        }
                        catch (IOException)
                        {
                            if (!_sameFiles.ContainsKey(_foundFiles[i]))
                            {
                                _sameFiles.Add(_foundFiles[i], indexForSameFileName++);
                            }
                            else
                            {
                                _sameFiles[_foundFiles[i]] = indexForSameFileName++;
                            }
                        }
                    }
                }
            }
            if (_sameFiles.Any())
            {
                string _newFileName;
                foreach (var file in _sameFiles)
                {
                    _newFileName = file.Key.Name.Insert(file.Key.Name.LastIndexOf('.'), $" - копия({file.Value})");

                    if (File.Exists(_savingPath + "\\" + _newFileName.Substring(
                        _newFileName.LastIndexOf('\\') + 1)))
                    {
                        continue;
                    }
                    else
                    {
                        File.Copy(file.Key.FullName, _savingPath + "\\" + _newFileName.
                            Substring(_newFileName.LastIndexOf('\\') + 1));
                    }

                    _copiedFiles.Add(new FileInfo(_savingPath + "\\" + _newFileName.
                            Substring(_newFileName.LastIndexOf('\\') + 1)));
                }
            }
            return _copiedFiles.ToArray(); 
        }
    }
}