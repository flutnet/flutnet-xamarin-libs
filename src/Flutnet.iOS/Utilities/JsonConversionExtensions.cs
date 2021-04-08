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

        public static object[] ToArray(this JArray array)
        {
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

        #region JObect to iOS NSDictionary

        public static Foundation.NSDictionary ToNSDictionary(this JObject json)
        {
            Dictionary<string, object> propertyValuePairs = json.ToObject<Dictionary<string, object>>();
            ProcessJObjectProperties2(propertyValuePairs);
            ProcessJArrayProperties2(propertyValuePairs);

            Foundation.NSMutableDictionary nsdict = new Foundation.NSMutableDictionary();
            foreach (KeyValuePair<string, object> kvp in propertyValuePairs)
            {
                nsdict.Add(new Foundation.NSString(kvp.Key), Foundation.NSObject.FromObject(kvp.Value));
            }
            return nsdict;
        }

        private static void ProcessJObjectProperties2(IDictionary<string, object> propertyValuePairs)
        {
            List<string> objectPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value is JObject
                select propertyName).ToList();

            objectPropertyNames.ForEach(propertyName =>
                propertyValuePairs[propertyName] = ToNSDictionary((JObject) propertyValuePairs[propertyName]));
        }

        private static void ProcessJArrayProperties2(IDictionary<string, object> propertyValuePairs)
        {
            List<string> arrayPropertyNames = (from property in propertyValuePairs
                let propertyName = property.Key
                let value = property.Value
                where value is JArray
                select propertyName).ToList();

            arrayPropertyNames.ForEach(propertyName =>
                propertyValuePairs[propertyName] = ToNSArray((JArray) propertyValuePairs[propertyName]));
        }

        public static Foundation.NSArray ToNSArray(this JArray array)
        {
            Foundation.NSMutableArray nsarray = new Foundation.NSMutableArray();
            foreach (JToken token in array)
            {
                nsarray.Add(ProcessArrayEntry2(token.ToObject<object>()));
            }
            return nsarray;
        }

        private static Foundation.NSObject ProcessArrayEntry2(object value)
        {
            if (value is JObject)
            {
                return ToNSDictionary((JObject) value);
            }

            if (value is JArray)
            {
                return ToNSArray((JArray )value);
            }

            return Foundation.NSObject.FromObject(value);
        }

        #endregion

        public static string ConvertByteArrayToBase64String(object byteArray)
        {
            string json = JsonConvert.SerializeObject(byteArray);
            return json.Length >= 2 ? json.Substring(1, json.Length - 2) : string.Empty;
        }
    }
}