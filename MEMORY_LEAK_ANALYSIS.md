# IdentityServer4 Memory Leak Analysis Report

## Date: January 29, 2026

---

## Executive Summary

This analysis identified **3 critical memory leak risks** and **5 moderate concerns** in the IdentityServer4 codebase. Most issues involve static collections that grow unbounded and improper DbContext handling patterns.

---

## Critical Issues

### 1. 🔴 **CRITICAL: Unbounded Static Cache in DiscoveryEndpoint**

**File**: [src/IdentityServer4/src/Endpoints/DiscoveryEndpoint.cs](src/IdentityServer4/src/Endpoints/DiscoveryEndpoint.cs#L19)

**Issue**: Static `ConcurrentDictionary` grows without bounds
```csharp
private static readonly ConcurrentDictionary<int, Dictionary<string, object>> _responseCache = 
    new ConcurrentDictionary<int, Dictionary<string, object>>();
```

**Problem**:
- Cache key is hardcoded to `1`, so only one entry should exist, BUT if configuration changes at runtime, entries could accumulate
- No eviction policy or expiration handling
- Static field persists for application lifetime
- In multi-tenant scenarios with different `IdentityServerOptions`, cache could return stale data

**Impact**: Medium memory growth (unless configuration changes frequently)

**Recommendations**:
- Remove static cache or implement proper expiration/eviction
- Use IMemoryCache with expiration policies instead
- Consider making cache configuration-aware

**Example Fix**:
```csharp
// Inject IMemoryCache instead of static cache
private readonly IMemoryCache _memoryCache;
private readonly MemoryCacheEntryOptions _cacheOptions = new()
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
};

// Use with expiration:
var response = _memoryCache.GetOrCreate("discovery_" + baseUrl, entry => 
{
    entry.SetOptions(_cacheOptions);
    return _responseGenerator.CreateDiscoveryDocumentAsync(baseUrl, issuerUri)
        .GetAwaiter().GetResult();
});
```

---

### 2. 🔴 **CRITICAL: DbContext Not Properly Scoped in TokenCleanupService**

**File**: [src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs](src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs#L18-L33)

**Issue**: DbContext dependency injection pattern issues:
```csharp
public class TokenCleanupService
{
    private readonly IPersistedGrantDbContext _persistedGrantDbContext;
    
    public TokenCleanupService(
        OperationalStoreOptions options,
        IPersistedGrantDbContext persistedGrantDbContext, 
        ILogger<TokenCleanupService> logger,
        IOperationalStoreNotification operationalStoreNotification = null)
    {
        _persistedGrantDbContext = persistedGrantDbContext;
        // ...
    }
}
```

**Problem**:
- DbContext is stored as instance field and reused across multiple operations
- `ToArrayAsync()` loads all expired grants into memory in one operation (line 77-79)
- For large databases with many expired grants, this can consume enormous memory
- Change tracking in DbContext accumulates entities, consuming memory
- No disposal of DbContext between batches

**Impact**: **HIGH** - Can cause Out Of Memory (OOM) exceptions on production systems with large token volumes

**Current Batching Issue** (lines 73-98):
```csharp
protected virtual async Task RemoveGrantsAsync()
{
    var found = Int32.MaxValue;
    while (found >= _options.TokenCleanupBatchSize)
    {
        var expiredGrants = await _persistedGrantDbContext.PersistedGrants
            .Where(x => x.Expiration < DateTime.UtcNow)
            .OrderBy(x => x.Expiration)
            .Take(_options.TokenCleanupBatchSize)  // Batch limit
            .ToArrayAsync();                        // Loads into memory
        
        // ... process ...
        _persistedGrantDbContext.PersistedGrants.RemoveRange(expiredGrants);
        await SaveChangesAsync();
        
        // DbContext change tracking not cleared between iterations
    }
}
```

**Recommendations**:
- Clear change tracker after each batch
- Consider scope-based DbContext creation for each operation
- Implement bulk delete operations when possible

**Example Fix**:
```csharp
protected virtual async Task RemoveGrantsAsync()
{
    var found = Int32.MaxValue;
    while (found >= _options.TokenCleanupBatchSize)
    {
        var expiredGrants = await _persistedGrantDbContext.PersistedGrants
            .Where(x => x.Expiration < DateTime.UtcNow)
            .OrderBy(x => x.Expiration)
            .Take(_options.TokenCleanupBatchSize)
            .ToArrayAsync();

        found = expiredGrants.Length;
        _logger.LogInformation("Removing {grantCount} grants", found);

        if (found > 0)
        {
            _persistedGrantDbContext.PersistedGrants.RemoveRange(expiredGrants);
            await SaveChangesAsync();

            if (_operationalStoreNotification != null)
            {
                await _operationalStoreNotification.PersistedGrantsRemovedAsync(expiredGrants);
            }
            
            // ADD: Clear change tracker between batches
            _persistedGrantDbContext.ChangeTracker.Clear();
        }
    }
}
```

---

### 3. 🔴 **CRITICAL: Potential DbContext Scope Issues in TokenCleanupHost**

**File**: [src/EntityFramework/src/TokenCleanupHost.cs](src/EntityFramework/src/TokenCleanupHost.cs#L113-L127)

**Issue**: Service scope properly created but no validation of disposal:
```csharp
async Task RemoveExpiredGrantsAsync()
{
    try
    {
        using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>()
            .CreateScope())
        {
            var tokenCleanupService = serviceScope.ServiceProvider
                .GetRequiredService<TokenCleanupService>();
            await tokenCleanupService.RemoveExpiredGrantsAsync();
        }
    }
    catch (Exception ex)
    {
        _logger.LogError("Exception removing expired grants: {exception}", ex.Message);
    }
}
```

**Problem**:
- Exception handling swallows errors, masking potential leak issues
- Background hosted service may keep running even if cleanup fails
- Timer threads not explicitly managed

**Recommendations**:
- Add more detailed exception logging
- Implement circuit breaker pattern
- Consider graceful degradation

---

## Moderate Issues

### 4. 🟡 **MODERATE: Event Handler Subscriptions Without Unsubscription**

**Potential Issue**: JavaScript event handlers in OIDC client library
**File**: Samples containing JavaScript

**Problem**:
- Event subscribers added via `addAccessTokenExpiring`, `addAccessTokenExpired` without corresponding removal
- Long-lived user sessions can accumulate event handlers
- Memory pressure increases as more handlers remain subscribed

**Recommendations**:
- Implement cleanup on logout/signout
- Provide cleanup methods for all event subscriptions
- Document proper cleanup procedures

---

### 5. 🟡 **MODERATE: Static Collections in Test Code Could Impact Production**

**Files**:
- [src/IdentityServer4/src/Logging/Models/DeviceAuthorizationRequestValidationLog.cs](src/IdentityServer4/src/Logging/Models/DeviceAuthorizationRequestValidationLog.cs#L21)
- [src/IdentityServer4/src/Infrastructure/ObjectSerializer.cs](src/IdentityServer4/src/Infrastructure/ObjectSerializer.cs#L15)

**Issue**: Static HashSet and JsonSerializerOptions:
```csharp
private static readonly HashSet<string> SensitiveValuesFilter = new(StringComparer.OrdinalIgnoreCase)
{
    // ... large collection ...
};

private static readonly JsonSerializerOptions Options = new()
{
    // ... options ...
};
```

**Problem**:
- Generally safe (fixed size, read-only after initialization)
- However, if these collections are modified at runtime, could cause issues
- Not configurable per application instance

**Recommendations**:
- Document immutability guarantees
- Add XML comments noting static nature
- Consider if configurability is needed

---

### 6. 🟡 **MODERATE: SHA Hash Objects Not Always Using Statements**

**Files Reviewed**:
- [src/IdentityServer4/src/Extensions/HashExtensions.cs](src/IdentityServer4/src/Extensions/HashExtensions.cs#L26)
- [src/IdentityServer4/src/Configuration/CryptoHelper.cs](src/IdentityServer4/src/Configuration/CryptoHelper.cs#L66)

**Good News**: These ARE using proper `using` statements ✅

```csharp
using (var sha = SHA256.Create())
{
    return Convert.ToBase64String(sha.ComputeHash(data));
}
```

**Status**: No issues found - properly disposed

---

### 7. 🟡 **MODERATE: Cache Key Collisions in InMemory Stores**

**Files**:
- [src/IdentityServer4/src/Stores/InMemory/InMemoryClientStore.cs](src/IdentityServer4/src/Stores/InMemory/InMemoryClientStore.cs)
- [src/IdentityServer4/src/Stores/InMemory/InMemoryDeviceFlowStore.cs](src/IdentityServer4/src/Stores/InMemory/InMemoryDeviceFlowStore.cs)

**Problem**:
- If default in-memory stores are used instead of database stores, they hold all configuration in memory
- Collections grow unbounded if clients/scopes are added dynamically
- No cache eviction or time-to-live implemented

**Recommendations**:
- Document that in-memory stores are for development only
- Implement size limits or TTL for entries
- Add warnings if approaching memory thresholds

---

## Analysis Summary Table

| Issue | Severity | File | Type | Resolution |
|-------|----------|------|------|-----------|
| Static Discovery Cache | 🔴 CRITICAL | DiscoveryEndpoint.cs | Unbounded Cache | Implement eviction policy |
| DbContext Change Tracking | 🔴 CRITICAL | TokenCleanupService.cs | Memory Accumulation | Clear tracker between batches |
| DbContext Scope Issues | 🔴 CRITICAL | TokenCleanupHost.cs | Error Handling | Enhance logging/monitoring |
| Event Subscriptions | 🟡 MODERATE | OIDC JS Client | Unsubscription | Implement cleanup |
| Static Collections | 🟡 MODERATE | Multiple files | Configuration | Document immutability |
| SHA Objects | ✅ SAFE | HashExtensions.cs | Resource Disposal | No action needed |
| In-Memory Stores | 🟡 MODERATE | InMemory stores | Unbounded Memory | Add size limits |

---

## Recommended Fixes Priority

### Priority 1 (Immediate)
1. **DbContext Change Tracking** - Add `ChangeTracker.Clear()` in cleanup loops
2. **Discovery Cache** - Replace static cache with injected IMemoryCache

### Priority 2 (Short-term)
3. **Event Handler Cleanup** - Document and implement unsubscription patterns
4. **Error Logging** - Enhance TokenCleanupHost exception handling

### Priority 3 (Long-term)
5. **In-Memory Store Limits** - Add configurable size limits
6. **Documentation** - Add memory leak prevention guidelines for developers

---

## Testing Recommendations

```csharp
// Unit Test: Verify DbContext cleanup
[Fact]
public async Task RemoveExpiredGrantsAsync_ClearsChangeTracker()
{
    var service = new TokenCleanupService(...);
    
    // Add many expired grants to trigger multiple batches
    // Verify change tracker is empty after cleanup
    Assert.Equal(0, context.ChangeTracker.Entries().Count());
}

// Integration Test: Memory usage under load
[Fact]
public async Task DiscoveryEndpoint_DoesNotGrowMemory()
{
    var endpoint = new DiscoveryEndpoint(...);
    var memoryBefore = GC.GetTotalMemory(true);
    
    // Make 1000 requests
    for (int i = 0; i < 1000; i++)
    {
        await endpoint.ProcessAsync(httpContext);
    }
    
    var memoryAfter = GC.GetTotalMemory(true);
    // Assert minimal growth (< 10MB)
    Assert.True(memoryAfter - memoryBefore < 10_485_760);
}
```

---

## Monitoring Recommendations

1. **Application Insights**: Track memory usage trends
2. **Performance Counters**: Monitor Gen 2 GC collections
3. **Custom Metrics**: 
   - Track DiscoveryEndpoint cache size
   - Monitor TokenCleanupService batch sizes
   - Log DbContext change tracker size

---

## Conclusion

IdentityServer4's memory leak risks are primarily in:
- **Static caching mechanisms** (Discovery endpoint)
- **DbContext entity tracking** (Token cleanup service)

Both can be mitigated with configuration changes and proper resource management. The framework itself is generally well-designed with proper `using` statements and disposal patterns.

**Recommended Action**: Implement the Priority 1 fixes immediately, particularly DbContext change tracking in production environments handling high token volumes.

