using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace MediCare.Web.Services;

// Keeps the login session (and the API's JWT inside it) in the memory of this server
// The browser's cookie holds only a random key. If MediCare.Web restarts, everyone logs in again
public class MemoryCacheTicketStore : ITicketStore
{
    private readonly IMemoryCache _cache;

    public MemoryCacheTicketStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        string key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        Save(key, ticket);
        return Task.FromResult(key);
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        Save(key, ticket);
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        _cache.TryGetValue(key, out AuthenticationTicket? ticket);
        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    private void Save(string key, AuthenticationTicket ticket)
    {
        MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

        // The session is removed from memory when the login expires
        if (ticket.Properties.ExpiresUtc.HasValue)
        {
            options.SetAbsoluteExpiration(ticket.Properties.ExpiresUtc.Value);
        }

        _cache.Set(key, ticket, options);
    }
}