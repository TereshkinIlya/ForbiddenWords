using SearchingSystems.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchingSystems
{
    public class ParallelProcessing
    {
        private ISearchFiles _searcher = null!;
        private FileInfo[] _copiedFiles;
        public ParallelProcessing(ISearchFiles searcher) => _searcher = searcher;
        public async Task<FileInfo[]> CopyFilesAsync(string[] _pattern, FileInfo[] _foundFiles, string _savingPath)
        {

            _copiedFiles = await Task<FileInfo[]>.Run(() =>
            {
                _copiedFiles = _searcher.CopyFiles(_pattern, _foundFiles, _savingPath);
                if (_copiedFiles != null)
                    return _copiedFiles;
                else
                    throw new ArgumentNullException();
            });
            return _copiedFiles;
        }
    }
}
