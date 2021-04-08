using System.Reflection;
using Newtonsoft.Json;

namespace Flutnet.Data
{
    internal class FlutnetMethodInfo
    {
        [Obfuscation(Exclude = true)]
        [JsonProperty(propertyName: "requestId")]
        public long RequestId { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(propertyName: "instance")]
        public string Instance { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(propertyName: "service")]
        public string Service { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(propertyName: "operation")]
        public string Operation { get; set; }
    }
}