using System;
using System.Collections.Generic;

namespace Janus
{
    public class DataProvider
    {
        public readonly Dictionary<string, object> Data = new Dictionary<string, object>();

        public void Add(string key, object data)
        {

            Data.Add(key, data);
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
    }
}