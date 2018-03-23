using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Task1
{
    public class FileSystemVisitor : IEnumerable
    {
        private List<FileSystemInfo> _files;
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
            _files = new List<FileSystemInfo>();
            _path = path;
            _filter = filter;
        }

        public void Execute()
        {
            Start?.Invoke();
            foreach (var file in GetDirectoryFiles(_path))
            {
                if (_stopSearch) break;
                if (_filter == null)
                {
                    if (file.GetType() == typeof(FileInfo)) FileFinded?.Invoke(file.FullName);
                    else DirectoryFinded?.Invoke(file.FullName);

                    if (!DeleteElementFlag) _files.Add(file);
                    DeleteElementFlag = false;
                    _stopSearch = true;
                }
                else
                {
                    if (file.GetType() == typeof(FileInfo)) FilteredFileFinded?.Invoke(file.FullName);
                    else FilteredDirectoryFinded?.Invoke(file.FullName);

                    if (!DeleteElementFlag) _files.Add(file);
                    DeleteElementFlag = false;
                    _stopSearch = StopSearchFlag;
                }
            }
            Finish?.Invoke();
        }

        private IEnumerable<FileSystemInfo> GetDirectoryFiles(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            foreach (var file in directory.GetFiles())
            {
                yield return file;
            }

            foreach (DirectoryInfo dirInfo in directory.GetDirectories())
            {
                yield return dirInfo;

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

            public FileSystemInfo Current
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
