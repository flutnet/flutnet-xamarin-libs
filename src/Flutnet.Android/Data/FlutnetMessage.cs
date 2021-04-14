// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

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