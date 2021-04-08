using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Flutnet.Data;
using Flutnet.ServiceModel;
using Flutnet.Utilities;
using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Flutnet
{
    internal class FlutterInterop
    {
        private static readonly JsonConverter[] JsonConverters =
        {
            new StringEnumConverter(),
            new IsoDateTimeConverter
            {
                DateTimeStyles = DateTimeStyles.AdjustToUniversal
            }
        };

        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
            Converters = JsonConverters
        };

        public static JsonSerializer Serializer = JsonSerializer.Create(JsonSerializerSettings);

        /// <summary>
        /// Converts a .NET object to a valid <see cref="NSObject"/>
        /// that can be sent to Flutter as a successful result of
        /// a <see cref="Flutnet.Interop.FlutterMethodChannel"/> method invoke.
        /// </summary>
        public static NSObject ToMethodChannelResult(int value)
        {
            string json = JsonConvert.SerializeObject(value, JsonSerializerSettings);
            return NSObject.FromObject(json);
        }

        /// <summary>
        /// Converts a .NET object to a valid <see cref="NSObject"/>
        /// that can be sent to Flutter as a successful result of
        /// a <see cref="Flutnet.Interop.FlutterMethodChannel"/> method invoke.
        /// </summary>
        public static NSObject ToMethodChannelResult(FlutnetMessage message)
        {
            // FIX ISSUES ABOUT DICTIONARY
            JObject jsonObject = JObject.FromObject(message, Serializer);
            CleanObjectFromInvalidTypes(ref jsonObject);
            return NSObject.FromObject(jsonObject.ToString(Formatting.None));
        }

        /// <summary>
        /// Converts a .NET object to a valid <see cref="NSObject"/>
        /// that can be sent to Flutter as a successful result of
        /// a <see cref="Flutnet.Interop.FlutterMethodChannel"/> method invoke.
        /// </summary>
        public static NSObject ToMethodChannelResult(FlutnetEventInfo message)
        {
            // FIX ISSUES ABOUT DICTIONARY
            JObject jsonObject = JObject.FromObject(message, Serializer);
            CleanObjectFromInvalidTypes(ref jsonObject);
            return NSObject.FromObject(jsonObject.ToString(Formatting.None));
        }

        public static void CleanObjectFromInvalidTypes(ref JObject jobject)
        {
            if (jobject.Type == JTokenType.Object && jobject.ContainsKey("$type"))
            {
                JToken value = jobject.GetValue("$type");
                if (value.Value<string>().StartsWith("System")) // Remove system types like Dictionaries
                {
                    jobject.Remove("$type");
                }

                foreach (JProperty jp in jobject.Properties())
                {
                    JToken propValue = jp.Value;
                    if (propValue.Type == JTokenType.Object)
                    {
                        JObject propJObject = (JObject)propValue;
                        CleanObjectFromInvalidTypes(ref propJObject);
                    }
                }
            }
        }

        private static NSObject ToFlutterObject(object value)
        {
            if (value == null)
                return null;

            Type type = value.GetType();
            Type properType = Nullable.GetUnderlyingType(type) ?? type;

            if (properType.IsPrimitive || properType == typeof(string))
            {
                return NSObject.FromObject(value);
            }

            if (properType.IsArray && properType.IsGenericType == false)
            {
                Type elementType = properType.GetElementType();
                Type properElementType = Nullable.GetUnderlyingType(elementType) ?? elementType;

                if (properElementType == typeof(byte))
                {
                    // for performance reasons, we adopt Newtonsoft approach
                    // and convert byte[] to a base-64 encoded string
                    return NSObject.FromObject(JsonConversionExtensions.ConvertByteArrayToBase64String(value));
                }

                NSMutableArray list = new NSMutableArray();
                foreach (object item in (IList) value)
                {
                    list.Add(ToFlutterObject(item));
                }
                return list;
            }

            if (properType.IsGenericType)
            {
                bool implementsDictionary = properType
                        .GetInterfaces()
                        .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                //bool isDictionary = properType.GetGenericTypeDefinition() == typeof(IDictionary<,>);

                if (implementsDictionary)
                {
                    //Type[] types = properType.GetGenericArguments();
                    //Type valueType = Nullable.GetUnderlyingType(types[1]) ?? types[1];

                    NSMutableDictionary map = new NSMutableDictionary();
                    foreach (dynamic kvp in (IEnumerable) value)
                    {
                        map.Add(NSObject.FromObject(kvp.Key), ToFlutterObject(kvp.Value));
                    }
                    return map;
                }

                bool implementsEnumerable = properType
                    .GetInterfaces()
                    .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                //bool isEnumerable = properType.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsEnumerable)
                {
                    //Type[] types = properType.GetGenericArguments();
                    //Type itemType = Nullable.GetUnderlyingType(types[0]) ?? types[0];

                    NSMutableArray list = new NSMutableArray();
                    foreach (object item in (IEnumerable) value)
                    {
                        list.Add(ToFlutterObject(item));
                    }
                    return list;
                }

                throw new InvalidOperationException();
            }

            if (properType.GetCustomAttributes(typeof(PlatformDataAttribute), false).Length > 0)
            { 
                return JObject.FromObject(value).ToNSDictionary();
            }

            throw new InvalidOperationException();
        }
    }
}