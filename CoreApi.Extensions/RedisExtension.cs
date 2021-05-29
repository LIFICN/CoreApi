using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Extensions
{
    public class RedisExtension
    {
        private const string configuration = "127.0.0.1:6379";
        private ConnectionMultiplexer RedisMultiplexer { get; }

        public RedisExtension()
        {
            RedisMultiplexer = ConnectionMultiplexer.Connect(configuration);
        }

        public IDatabase GetDatabase() => RedisMultiplexer.GetDatabase();

        public object GetKey(string key)
        {
            return GetDatabase().StringGet(key);
        }

        public async Task<object> GetKeyAsync(string key)
        {
            return await GetDatabase().StringGetAsync(key);
        }

        public bool SetKey(string key, string value, TimeSpan? expiry = null)
        {
            return GetDatabase().StringSet(key, value, expiry);
        }

        public async Task<bool> SetKeyAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await GetDatabase().StringSetAsync(key, value, expiry);
        }

        public bool DeleteKey(string key)
        {
            return GetDatabase().KeyDelete(key);
        }

        public async Task<bool> DeleteKeyAsync(string key)
        {
            return await GetDatabase().KeyDeleteAsync(key);
        }

        public bool ExpireKey(string key, TimeSpan? expiry = null)
        {
            return GetDatabase().KeyExpire(key, expiry);
        }

        public async Task<bool> ExpireKeyAsync(string key, TimeSpan? expiry = null)
        {
            return await GetDatabase().KeyExpireAsync(key, expiry);
        }

        public bool ExistsKey(string key)
        {
            return GetDatabase().KeyExists(key);
        }

        public async Task<bool> ExistsKeyAsync(string key)
        {
            return await GetDatabase().KeyExistsAsync(key);
        }

        public void HashSet(string key, string field, string value)
        {
            GetDatabase().HashSet(key, field, value);
        }

        public async Task HashSetAsync(string key, string field, string value)
        {
            await GetDatabase().HashSetAsync(key, field, value);
        }

        public void HashSet(string key, KeyValuePair<string, string>[] valuePairs)
        {
            GetDatabase().HashSet(key, valuePairs.Select(v => new HashEntry(v.Key, v.Value)).ToArray());
        }

        public async Task HashSetAsync(string key, KeyValuePair<string, string>[] valuePairs)
        {
            await GetDatabase().HashSetAsync(key, valuePairs.Select(v => new HashEntry(v.Key, v.Value)).ToArray());
        }

        public object HashGet(string key, string field)
        {
            return GetDatabase().HashGet(key, field);
        }

        public async Task<object> HashGetAsync(string key, string field)
        {
            return await GetDatabase().HashGetAsync(key, field);
        }

        public object HashGetAll(string key)
        {
            return GetDatabase().HashGetAll(key);
        }

        public async Task<HashEntry[]> HashGetAllAsync(string key)
        {
            return await GetDatabase().HashGetAllAsync(key);
        }

        public bool HashDelete(string key, string field)
        {
            return GetDatabase().HashDelete(key, field);
        }

        public async Task<bool> HashDeleteAsync(string key, string field)
        {
            return await GetDatabase().HashDeleteAsync(key, field);
        }

        public bool HashExists(string key, string field)
        {
            return GetDatabase().HashExists(key, field);
        }

        public async Task<bool> HashExistsAsync(string key, string field)
        {
            return await GetDatabase().HashExistsAsync(key, field);
        }
    }
}