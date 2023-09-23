using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonEx;

public class RedisExtension
{
    private const string configuration = "127.0.0.1:6379";
    private ConnectionMultiplexer RedisMultiplexer { get; }

    public RedisExtension()
    {
        RedisMultiplexer = ConnectionMultiplexer.Connect(configuration);
    }

    public IDatabase GetDatabase() => RedisMultiplexer.GetDatabase();

    public T Deserialize<T>(string value) => value.MapTo<T>();

    public string Serialize<T>(T value) => value.ToJson<T>();

    public T GetKey<T>(string key)
    {
        return Deserialize<T>(GetDatabase().StringGet(key));
    }

    public async Task<T> GetKeyAsync<T>(string key)
    {
        return Deserialize<T>(await GetDatabase().StringGetAsync(key));
    }

    public bool SetKey<T>(string key, T value, TimeSpan? expiry = null)
    {
        return GetDatabase().StringSet(key, Serialize<T>(value), expiry);
    }

    public async Task<bool> SetKeyAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        return await GetDatabase().StringSetAsync(key, Serialize<T>(value), expiry);
    }

    public bool DeleteKey(string key)
    {
        return GetDatabase().KeyDelete(key);
    }

    public async Task<bool> DeleteKeyAsync(string key)
    {
        return await GetDatabase().KeyDeleteAsync(key);
    }

    public long DeleteKey(string[] keys)
    {
        return GetDatabase().KeyDelete(keys.Select(v => new RedisKey(v)).ToArray());
    }

    public async Task<long> DeleteKeyAsync(string[] keys)
    {
        return await GetDatabase().KeyDeleteAsync(keys.Select(v => new RedisKey(v)).ToArray());
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

    public bool HashSet<T>(string key, string field, T value)
    {
        return GetDatabase().HashSet(key, field, Serialize<T>(value));
    }

    public async Task<bool> HashSetAsync<T>(string key, string field, T value)
    {
        return await GetDatabase().HashSetAsync(key, field, Serialize<T>(value));
    }

    public void HashSet<T>(string key, KeyValuePair<string, T>[] valuePairs)
    {
        GetDatabase().HashSet(key, valuePairs.Select(v => new HashEntry(v.Key, Serialize<T>(v.Value))).ToArray());
    }

    public async Task HashSetAsync<T>(string key, KeyValuePair<string, T>[] valuePairs)
    {
        await GetDatabase().HashSetAsync(key, valuePairs.Select(v => new HashEntry(v.Key, Serialize<T>(v.Value))).ToArray());
    }

    public T HashGet<T>(string key, string field)
    {
        return Deserialize<T>(GetDatabase().HashGet(key, field));
    }

    public async Task<T> HashGetAsync<T>(string key, string field)
    {
        return Deserialize<T>(await GetDatabase().HashGetAsync(key, field));
    }

    public KeyValuePair<string, string>[] HashGetAll(string key)
    {
        var res = GetDatabase().HashGetAll(key);
        return res != null && res.Length > 0 ? res.Select(v => new KeyValuePair<string, string>(v.Name, v.Value)).ToArray() : null;
    }

    public async Task<KeyValuePair<string, string>[]> HashGetAllAsync(string key)
    {
        var res = await GetDatabase().HashGetAllAsync(key);
        return res != null && res.Length > 0 ? res.Select(v => new KeyValuePair<string, string>(v.Name, v.Value)).ToArray() : null;
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
