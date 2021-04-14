using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileParsing
{
    internal abstract class IFileReader
    {
        protected List<string> _patterns = new List<string>();
        internal List<string> Patterns
        {
            get { return _patterns; }
        }

        internal List<string> Dictionary { get; set; }

        protected Queue<string> _files = new Queue<string>();
        protected readonly object filesLocker = new object();
        protected ManualResetEventSlim filesChanged = new ManualResetEventSlim(false);
        internal ManualResetEventSlim filesEnd = new ManualResetEventSlim(false);
        protected string GetFile()
        {
            string temp = null;
            lock (filesLocker)
            {
                temp = _files.Dequeue();
                if (_files.Count <= 0)
                    filesChanged.Reset();
            }

            return temp;
        }
        internal void AddFiles(string[] files)
        {
            lock (filesLocker)
            {
                for (int i = 0; i < files.Length; i++)
                    _files.Enqueue(files[i]);
                filesChanged.Set();
            }
        }

        protected abstract MatchFile GetMatchFile(string path);

        internal List<MatchFile> ReadFiles()
        {
            var matchFiles = new List<MatchFile>();
            var waitHandles = new WaitHandle[]
            {
                filesEnd.WaitHandle,
                filesChanged.WaitHandle
            };

            do
            {
                int index = EventWaitHandle.WaitAny(waitHandles);

                if (index == 0 && _files.Count <= 0)
                {
                    return matchFiles;
                }
                else
                {
                    var file = GetMatchFile(GetFile());
                    if (file != null)
                        matchFiles.Add(file);
                }
            } while (true);
        }
    }
}
