using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchingSystems.Interfaces
{
    public interface ISearchFiles
    {
        public Task<FileInfo[]> SearchFilesAsync(string[] _dirNames);
        public FileInfo[] CopyFiles(string[] _pattern, FileInfo[] _foundFiles, string _savingPath);
    }
}
