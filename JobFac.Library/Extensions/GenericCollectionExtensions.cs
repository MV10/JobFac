using System.Collections.Generic;

namespace JobFac.Library
{
    public static class GenericCollectionExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if(dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}
