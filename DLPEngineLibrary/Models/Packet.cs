
using Newtonsoft.Json;

namespace DLPEngineLibrary.Models
{
    internal class Packet
    {
        public string GUID { get; set; }
        public string HostName { get; set; }
        public string Task { get; set; }
        [JsonProperty]
        public byte[] Data { get; set; }
    }
}
