using System.Collections.Generic;
using System.Reflection;
using Flutnet.ServiceModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flutnet.Data
{
    internal class FlutnetMessage
    {
        [Obfuscation(Exclude = true)]
        [JsonProperty(PropertyName = "methodInfo")]
        public FlutnetMethodInfo MethodInfo { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(PropertyName = "arguments")]
        public Dictionary<string, object> Arguments { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(PropertyName = "result")]
        public Dictionary<string, object> Result { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(PropertyName = "errorCode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FlutnetErrorCode? ErrorCode { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(PropertyName = "event")] 
        public FlutnetEventInfo EventInfo { get; set; }

        [Obfuscation(Exclude = true)]
        [JsonProperty(PropertyName = "exception")]
        public PlatformOperationException Exception { get; set; }
    }
}