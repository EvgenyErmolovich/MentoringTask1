using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Task1
{
    public class FileSystemVisitor : IEnumerable
    {
        private StringCollection _files = new StringCollection();

        private readonly Func<string, bool> _filter;

        public FileSystemVisitor(string path, Func<string, bool> filter = null)
        {
            _filter = filter;

            foreach (var file in GetDirectoryFiles(path))
            {
                _files.Add(file);
            }
        }

        private IEnumerable<string> GetDirectoryFiles(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            foreach (var file in directory.GetFiles())
            {               
                if (_filter == null || _filter(file.FullName)) yield return file.FullName;
            }

            foreach (DirectoryInfo dirInfo in directory.GetDirectories())
            {
                if (_filter == null || _filter(dirInfo.FullName)) yield return dirInfo.FullName;

                foreach (var file in GetDirectoryFiles(dirInfo.FullName))
                {
                    if (_filter == null || _filter(file)) yield return file;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        private FileSystemVisitorEnumerator GetEnumerator() => new FileSystemVisitorEnumerator(this);

        private struct FileSystemVisitorEnumerator : IEnumerator
        {
            private readonly FileSystemVisitor fileSystemVisitor;
            private int index;

            public FileSystemVisitorEnumerator(FileSystemVisitor visitor) : this()
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
