using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileParsing
{
    internal class FileFinder
    {
        internal List<IFileReader> Readers { get; set; } = null;
        internal List<string> Dictiontary { get; set; } = null;

        internal List<MatchFile> FindFiles()
        {
            if (Readers == null || Dictiontary == null)
                throw new Exception("Readers or Dictionary must not be null");

            foreach (var reader in Readers)
                reader.Dictionary = Dictiontary;

            var Directories = GetAllDirestories();

            var tasks = new List<Task<List<MatchFile>>>();
            foreach (var reader in Readers)
                tasks.Add(Task.Factory.StartNew(reader.ReadFiles));

            var tasks1 = new List<Task<bool>>();
            foreach (var reader in Readers)
                tasks1.Add(Task.Factory.StartNew(() =>
                {
                    FindFilesForReader(reader, Directories);
                    return true;
                }));

            foreach (var task in tasks1)
            {
                bool res = task.Result;
            }

            foreach (var reader in Readers)
                reader.filesEnd.Set();

            var result = new List<MatchFile>();
            foreach (var task in tasks)
            {
                result.AddRange(task.Result);
            }

            return result;
        }

        private void FindFilesForReader(IFileReader reader, in List<string> directories)
        {
            string[] paths = null;
            foreach(var pattern in reader.Patterns)
            {
                foreach(var directory in directories)
                {
                    try
                    {
                        paths = Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
                        if (paths?.Length > 0)
                            reader.AddFiles(paths);
                    }
                    catch(Exception e)
                    {
                        continue;
                    }

                }
            }
        }

        private List<string> GetAllDirestories()
        {
            List<Task<List<string>>> tasks = new List<Task<List<string>>>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady)
                {
                    //drive is not ready
                }
                else
                {
                    tasks.Add(Task.Factory.StartNew<List<string>>(() =>
                    {
                        List<string> dir = new List<string>();
                        GetDir(drive.Name, ref dir);
                        return dir;
                    }));
                }
            }

            List<string> Directories = new List<string>();
            foreach (var task in tasks)
            {
                Directories.AddRange(task.Result);
            }

            return Directories;
        }

        private void GetDir(string path, ref List<string> outres)
        {
            string[] directories = null;
            try
            {
                directories = Directory.GetDirectories(path);
                if (directories == null)
                    return;
   
                outres.AddRange(directories);
                for (int i = 0; i < directories.Length; i++)
                    GetDir(directories[i], ref outres);
            }
            catch(Exception)
            {
                return;
            }
        }
    }
}
