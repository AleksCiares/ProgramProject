
namespace DLPEngineLibrary.Models
{
    internal class Packet
    {
        public string GUID { get; set; }
        public string HostName { get; set; }
        public string Task { get; set; }
        public byte[] Data { get; set; }
    }
}
