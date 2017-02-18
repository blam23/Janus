using System.Collections.Generic;

namespace Janus
{
    public class DataProvider
    {
        public readonly Dictionary<string, object> Data = new Dictionary<string, object>();
        public Dictionary<string, object> Dict => Data;

        public void Add(string key, object data)
        {
            Data.Add(key, data);
        }

        public void Remove(string key)
        {
            Data.Remove(key);
        }

        public T Get<T>(string key)
        {
            object data;
            if (Data.TryGetValue(key, out data))
            {
                return (T) data;
            }
            return default(T);
        }

        public T GetOr<T>(string key, T defaultValue)
        {
            object data;
            if (Data.TryGetValue(key, out data))
            {
                return (T)data;
            }
            return defaultValue;
        }

        public object this[string key]
        {
            get { return Dict[key]; }
            set { Dict[key] = value; }
        }
    }
}