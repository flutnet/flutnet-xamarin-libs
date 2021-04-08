using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Flutnet.Data
{
    internal class FlutnetEventInfo
    {
        [Obfuscation(Exclude = true)]
        [JsonProperty(propertyName: "instanceId")]
        public string InstanceId { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(propertyName: "event")]
        public string EventName { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(propertyName: "args")]
        public EventArgs EventData { get; set; }
    }
}