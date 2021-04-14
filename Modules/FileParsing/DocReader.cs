using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileParsing
{
    internal class DocReader : IFileReader
    {
        public DocReader()
        {
            _patterns.Add("*.doc");
            _patterns.Add("*.docx");
            _patterns.Add("*.odt");
        }

        protected override MatchFile GetMatchFile(string path)
        {
            MatchFile matchFile = new MatchFile();
            var text = string.Empty;

            try
            {
                using (var wordDocument = WordprocessingDocument.Open(path, false))
                {
                    var body = wordDocument.MainDocumentPart.Document.Body;
                    text = body.InnerText;
                    wordDocument.Close();
                }

                foreach (var pattern in Dictionary)
                {
                    var regex = new Regex(@"" + pattern + @"(\w*)", RegexOptions.Compiled |
                        RegexOptions.IgnoreCase);
                    var matches = regex.Matches(text);
                    if (matches.Count > 0)
                    {
                        matchFile.Path = path;
                        for (int i = 0; i < matches.Count; i++)
                            matchFile.Matches.Add(matches[i].Value);
                    }
                }

                if (matchFile.Path != null)
                    return matchFile;
                else
                    return null;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}
