using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Task1
{
    public class FileSystemVisitor : IEnumerable
    {
        /// <summary>
        /// List of finded files and foldes
        /// </summary>
        private List<FileSystemInfo> _files;
        /// <summary>
        /// Path of directory in which start finding files and folders
        /// </summary>
        private readonly string _path;
        /// <summary>
        /// Delegate for filtering files and folders
        /// </summary>
        private readonly Func<FileSystemInfo, bool> _filter;
        private bool _stopSearch;

        /// <summary>
        /// Flag to stop searching files and folders
        /// </summary>
        public bool StopSearchFlag { get; set; }
        /// <summary>
        /// Flag to delete file or folder
        /// </summary>
        public bool DeleteElementFlag { get; set; }

        public delegate void EventDelegate();
        public delegate void ElementFindedDelegate(string elementFullName);

        /// <summary>
        /// Event for starting search
        /// </summary>
        public event EventDelegate Start;
        /// <summary>
        /// Event for finishing search
        /// </summary>
        public event EventDelegate Finish;
        /// <summary>
        /// Event for finded file
        /// </summary>
        public event ElementFindedDelegate FileFinded;
        /// <summary>
        /// Event for finded folder
        /// </summary>
        public event ElementFindedDelegate DirectoryFinded;
        /// <summary>
        /// Event for filtered finded file
        /// </summary>
        public event ElementFindedDelegate FilteredFileFinded;
        /// <summary>
        /// Event for filtered finded folder
        /// </summary>
        public event ElementFindedDelegate FilteredDirectoryFinded;

        /// <summary>
        /// Constructor for creating object FileSystemVisitor
        /// </summary>
        /// <param name="path">Path of directory in which start finding files and folders</param>
        /// <param name="filter">Delegate for filtering files and folders</param>
        public FileSystemVisitor(string path, Func<FileSystemInfo, bool> filter = null)
        {
            _files = new List<FileSystemInfo>();
            _path = path;
            _filter = filter;
        }

        /// <summary>
        /// Start finding files and folders
        /// </summary>
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
                    _stopSearch = StopSearchFlag;
                }
                else
                {
                    if (file.GetType() == typeof(FileInfo)) FilteredFileFinded?.Invoke(file.FullName);
                    else FilteredDirectoryFinded?.Invoke(file.FullName);

                    if (!DeleteElementFlag && _filter(file)) _files.Add(file);
                    DeleteElementFlag = false;
                    _stopSearch = StopSearchFlag;
                }
            }
            Finish?.Invoke();
        }

        /// <summary>
        /// Finding files and folsers in folder
        /// </summary>
        /// <param name="path">Folder in which find files and folders</param>
        /// <returns>FileInfo and DirectoryInfo which were finded in current folder</returns>
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
