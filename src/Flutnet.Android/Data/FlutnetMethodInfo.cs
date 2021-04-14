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