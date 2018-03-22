using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Task1
{
    public class FileSystemVisitor : IEnumerable
    {
        private StringCollection _files;
        private readonly string _path;
        private readonly Func<FileSystemInfo, bool> _filter;
        private bool _stopSearch;

        public bool StopSearchFlag { get; set; }
        public bool DeleteElementFlag { get; set; }

        public delegate void EventDelegate();
        public delegate void ElementFindedDelegate(string elementFullName);

        public event EventDelegate Start;
        public event EventDelegate Finish;
        public event ElementFindedDelegate FileFinded;
        public event ElementFindedDelegate DirectoryFinded;
        public event ElementFindedDelegate FilteredFileFinded;
        public event ElementFindedDelegate FilteredDirectoryFinded;

        public FileSystemVisitor(string path, Func<FileSystemInfo, bool> filter = null)
        {
            _files = new StringCollection();
            _path = path;
            _filter = filter;
        }

        public void Execute()
        {
            Start?.Invoke();
            foreach (var file in GetDirectoryFiles(_path))
            {
                _files.Add(file);
                if (_stopSearch) break;
            }
            Finish?.Invoke();
        }

        private IEnumerable<string> GetDirectoryFiles(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            foreach (var file in directory.GetFiles())
            {
                if (_filter == null)
                {
                    FileFinded?.Invoke(file.FullName);
                    if(!DeleteElementFlag) yield return file.FullName;
                    DeleteElementFlag = false;
                    _stopSearch = StopSearchFlag;
                }
                else
                {
                    FilteredFileFinded?.Invoke(file.FullName);
                    if (_filter(file) && !DeleteElementFlag) yield return file.FullName;
                    DeleteElementFlag = false;
                    _stopSearch = StopSearchFlag;
                }
            }

            foreach (DirectoryInfo dirInfo in directory.GetDirectories())
            {
                if (_filter == null)
                {
                    DirectoryFinded?.Invoke(dirInfo.FullName);
                    if(!DeleteElementFlag) yield return dirInfo.FullName;
                    DeleteElementFlag = false;
                    _stopSearch = StopSearchFlag;
                }
                else
                {
                    FilteredDirectoryFinded?.Invoke(dirInfo.FullName);
                    if (_filter(dirInfo) && !DeleteElementFlag) yield return dirInfo.FullName;
                    DeleteElementFlag = false;
                    _stopSearch = StopSearchFlag;
                }

                foreach (var file in GetDirectoryFiles(dirInfo.FullName))
                {
                     yield return file;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        private FileSystemVisitorEnumerator GetEnumerator() => new FileSystemVisitorEnumerator(this);

        private class FileSystemVisitorEnumerator : IEnumerator
        {
            private readonly FileSystemVisitor fileSystemVisitor;
            private int index;

            public FileSystemVisitorEnumerator(FileSystemVisitor visitor) 
            {
                fileSystemVisitor = visitor;
                index = -1;
            }

            public bool MoveNext() => ++index < fileSystemVisitor._files.Count;

            public string Current
            {
                get
                {
                    if (index <= -1 || index >= fileSystemVisitor._files.Count)
                        throw new InvalidOperationException();
                    return fileSystemVisitor._files[index];
                }
            }

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                index = -1;
            }
        }
    }
}
