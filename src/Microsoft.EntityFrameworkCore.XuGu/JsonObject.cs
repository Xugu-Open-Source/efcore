using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public class JsonObject<T> : IEquatable<JsonObject<T>>, IEquatable<JsonObject>, IEquatable<string> where T : class
    {
        private string _originalValue { get; set; }

        private T _originalObject { get; set; }

        private Type _internalType { get; set; }

        public T Object
        {
            get
            {
                return _originalObject;
            }
            set
            {
                _originalObject = value;
                _originalValue = ((_originalObject != null) ? JsonConvert.SerializeObject(Object) : string.Empty);
            }
        }

        public string Json
        {
            get
            {
                return _originalValue;
            }
            set
            {
                try
                {
                    Object = (string.IsNullOrWhiteSpace(value) ? null : JsonConvert.DeserializeObject<T>(value));
                    _originalValue = value;
                }
                catch
                {
                    Object = null;
                    _originalValue = string.Empty;
                }
            }
        }

        public JsonObject()
        {
            _internalType = typeof(T);
        }

        public JsonObject(T instance)
            : this()
        {
            Object = instance;
        }

        public JsonObject(string json)
            : this()
        {
            Json = json;
            Object = Object;
        }

        public override string ToString()
        {
            return Json;
        }

        public override bool Equals(object obj)
        {
            if (obj == null && Json == JsonConvert.Null)
            {
                return true;
            }

            if (obj.GetType().Name == "String")
            {
                string text = obj as string;
                if (text == JsonConvert.NaN && Json == JsonConvert.NaN)
                {
                    return false;
                }

                return Equals(text);
            }

            try
            {
                return Equals((string)((dynamic)obj).Json);
            }
            catch
            {
                return base.Equals(obj);
            }
        }

        public bool Equals(JsonObject<T> other)
        {
            if (other == null && Json == JsonConvert.Null)
            {
                return true;
            }

            return Equals(other.Json);
        }

        public bool Equals(JsonObject other)
        {
            if (other == null && Json == JsonConvert.Null)
            {
                return true;
            }

            return Equals(other.Json);
        }

        public bool Equals(string other)
        {
            if (other == JsonConvert.NaN && Json == JsonConvert.NaN)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(other) || IsJsonConstant(other))
            {
                return string.CompareOrdinal(other, Json) == 0;
            }

            if (!IsSameType(Json, other))
            {
                return false;
            }

            JsonObject jsonObject = new JsonObject(other);
            return GetHashCode() == jsonObject.GetHashCode();
        }

        private Type GetInternalObjectType()
        {
            return _internalType;
        }

        public static implicit operator JsonObject<T>(string json)
        {
            return new JsonObject<T>(json);
        }

        public static implicit operator JsonObject<T>(T obj)
        {
            return new JsonObject<T>(obj);
        }

        public static implicit operator JsonObject<T>(JsonObject<object> obj)
        {
            return new JsonObject<T>(obj.Json);
        }

        private static bool IsObject(string json)
        {
            JsonObject jsonObject;
            return IsObject(json, out jsonObject);
        }

        private static bool IsObject(string json, out JsonObject jsonObject)
        {
            jsonObject = null;
            if (string.IsNullOrWhiteSpace(json) || IsJsonConstant(json))
            {
                return false;
            }

            if (string.CompareOrdinal(json, JsonConvert.Null) == 0)
            {
                return true;
            }

            jsonObject = new JsonObject(json);
            return jsonObject.Object != null;
        }

        private static bool IsJsonConstant(string json)
        {
            if (string.CompareOrdinal(json, JsonConvert.NaN) == 0 || string.CompareOrdinal(json, JsonConvert.Undefined) == 0 || string.CompareOrdinal(json, JsonConvert.True) == 0 || string.CompareOrdinal(json, JsonConvert.False) == 0 || string.CompareOrdinal(json, JsonConvert.NegativeInfinity) == 0 || string.CompareOrdinal(json, JsonConvert.PositiveInfinity) == 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsSameType(string json1, string json2)
        {
            if ((string.IsNullOrWhiteSpace(json1) && string.IsNullOrWhiteSpace(json2)) || (json1 == JsonConvert.Null && json2 == JsonConvert.Null))
            {
                return true;
            }

            if (IsJsonConstant(json1) && IsJsonConstant(json2))
            {
                return json1 == json2;
            }

            if (!IsObject(json1, out var jsonObject) || !IsObject(json2, out var jsonObject2))
            {
                return false;
            }

            return jsonObject.GetInternalObjectType().FullName == jsonObject2.GetInternalObjectType().FullName;
        }

        public static bool operator ==(JsonObject<T> a, JsonObject<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(JsonObject<T> a, JsonObject<T> b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Json.GetHashCode();
        }
    }
    public class JsonObject : JsonObject<object>
    {
        public JsonObject(string json)
            : base(json)
        {
        }
    }
}
