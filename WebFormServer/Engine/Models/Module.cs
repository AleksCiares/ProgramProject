
namespace WebFormServer.Engine.Models
{
    public class Module
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool AutoStart { get; set; }
        public byte[] Data { get; set; }
    }
}
