using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParsing
{
    internal class MatchFile
    {
        public string Path { get; set; }
        public HashSet<string> Matches { get; set; } = new HashSet<string>();
    }
}
