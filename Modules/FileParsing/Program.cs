using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLPEngineLibrary.Controllers;

namespace FileParsing
{
    class Program
    {
        static string[] localDic = new string[]
        {
            "секрет",
            "засекре",
            "служеб",
        };

        static void Main(string[] args)
        {
            FileFinder fileFinder = new FileFinder();
            fileFinder.Readers = new List<IFileReader>()
            {
                new DocReader()
            };

            List<string> dictionary = null;

            try
            {
                dictionary = FileController.ReadObjectFromFile(Path.Combine(Directory.GetCurrentDirectory(),
                    @"dictionary.json"), JsonController.ReadObjectFromJsonFile<List<string>>);
            }
            catch (Exception) { }
            finally
            {
                if (dictionary == null || dictionary.Count <= 0)
                {
                    dictionary = new List<string>();
                    dictionary.AddRange(localDic);
                }
            }
            
            FileController.WriteObjectToFile(Path.Combine(
                Directory.GetCurrentDirectory(), @"dictionary.json"), dictionary, JsonController.WriteObjectToJsonFile);

            fileFinder.Dictiontary = dictionary;

            var files = fileFinder.FindFiles();

            PrintRes(files);
        }

        internal static void PrintRes(List<MatchFile> files)
        {
            foreach (var mes in files)
            {
                Console.WriteLine(mes.Path);
                Console.Write("Найденные совпадения: ");
                foreach (var match in mes.Matches)
                    Console.Write(match + " ");
                Console.WriteLine(Environment.NewLine);
            }

            Console.ReadLine();
        }
    }
}
