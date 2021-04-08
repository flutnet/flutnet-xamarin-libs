using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flutnet.Utilities
{
    internal static class JsonConversionExtensions
    {
        #region JObect to .NET IDictionary

        public static IDictionary<string, object> ToDictionary(this JObject json)
        {
            Dictionary<string, object> propertyValuePairs = json.ToObject<Dictionary<string, object>>();
            ProcessJObjectProperties(propertyValuePairs);
            ProcessJArrayProperties(propertyValuePairs);
            return propertyValuePairs;
        }

        private static void ProcessJObjectProperties(IDictionary<string, object> propertyValuePairs)
        {
            List<string> objectPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value is JObject
                select propertyName).ToList();
            
            objectPropertyNames.ForEach(propertyName =>
                propertyValuePairs[propertyName] = ToDictionary((JObject) propertyValuePairs[propertyName]));
        }

        private static void ProcessJArrayProperties(IDictionary<string, object> propertyValuePairs)
        {
            List<string> arrayPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value is JArray
                select propertyName).ToList();

            arrayPropertyNames.ForEach(propertyName =>
                propertyValuePairs[propertyName] = ToArray((JArray) propertyValuePairs[propertyName]));
        }

        public static object ToArray(this JArray array)
        {
            if (array.Type == JTokenType.Bytes)
            {
                return JsonConvert.ToString(array);
            }

            return array.ToObject<object[]>().Select(ProcessArrayEntry).ToArray();
        }

        private static object ProcessArrayEntry(object value)
        {
            if (value is JObject)
            {
                return ToDictionary((JObject) value);
            }

            if (value is JArray)
            {
                return ToArray((JArray) value);
            }

            return value;
        }

        #endregion

        #region JObect to Android Map

        public static Android.Runtime.JavaDictionary ToJavaDictionary(this JObject json)
        {
            Dictionary<string, object> propertyValuePairs = json.ToObject<Dictionary<string, object>>();
            ProcessJObjectProperties2(propertyValuePairs);
            ProcessJArrayProperties2(propertyValuePairs);          
            ProcessByteArrayProperties2(propertyValuePairs);
            return new Android.Runtime.JavaDictionary(propertyValuePairs);
        }

        private static void ProcessJObjectProperties2(IDictionary<string, object> propertyValuePairs)
        {
            List<string> objectPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value is JObject
                select propertyName).ToList();

            objectPropertyNames.ForEach(propertyName =>
                propertyValuePairs[propertyName] = ToJavaDictionary((JObject) propertyValuePairs[propertyName]));
        }

        private static void ProcessJArrayProperties2(IDictionary<string, object> propertyValuePairs)
        {
            List<string> arrayPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value is JArray
                select propertyName).ToList();

            arrayPropertyNames.ForEach(propertyName =>
                propertyValuePairs[propertyName] = ToJavaList((JArray) propertyValuePairs[propertyName]));
        }

        private static void ProcessByteArrayProperties2(IDictionary<string, object> propertyValuePairs)
        {
            List<string> objectPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value.GetType().IsArray && value.GetType().GetElementType() == typeof(byte)
                select propertyName).ToList();

            objectPropertyNames.ForEach((propertyName) => 
                propertyValuePairs[propertyName] = ConvertByteArrayToBase64String(propertyValuePairs[propertyName])
            );
        }

        public static Android.Runtime.JavaList ToJavaList(this JArray array)
        {
            Android.Runtime.JavaList list = new Android.Runtime.JavaList();

            foreach (JToken token in array.ToList())
            {
                list.Add(ProcessArrayEntry2(token.ToObject<object>()));
            }
            return list;
        }

        private static object ProcessArrayEntry2(object value)
        {
            if (value is JObject)
            {
                return ToJavaDictionary((JObject) value);
            }

            if (value is JArray)
            {
                return ToJavaList((JArray) value);
            }

            return value;
        }

        #endregion

        public static string ConvertByteArrayToBase64String(object byteArray)
        {
            string json = JsonConvert.SerializeObject(byteArray);
            return json.Length >= 2 ? json.Substring(1, json.Length - 2) : string.Empty;
        }
    }
}