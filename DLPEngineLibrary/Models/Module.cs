
namespace DLPEngineLibrary.Models
{
    internal class Module
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public bool AutoStart { get; set; }
        public byte[] Data { get; set; }
    }
}
