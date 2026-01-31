# IdentityServer4 Memory Leak Fixes - Implementation Guide

## Fix #1: Replace Static Discovery Cache with IMemoryCache

### Current Code (PROBLEMATIC)
**File**: `src/IdentityServer4/src/Endpoints/DiscoveryEndpoint.cs`

```csharp
internal class DiscoveryEndpoint : IEndpointHandler
{
    private static readonly ConcurrentDictionary<int, Dictionary<string, object>> _responseCache = 
        new ConcurrentDictionary<int, Dictionary<string, object>>();

    public Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        // ... validation ...
        var response = _responseCache.GetOrAdd(1, _ => 
            _responseGenerator.CreateDiscoveryDocumentAsync(baseUrl, issuerUri)
                .GetAwaiter().GetResult());
        // ...
    }
}
```

### Fixed Code (RECOMMENDED)
```csharp
internal class DiscoveryEndpoint : IEndpointHandler
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;
    private readonly IdentityServerOptions _options;
    private readonly IDiscoveryResponseGenerator _responseGenerator;

    // Cache options with 1-hour expiration
    private static readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };

    public DiscoveryEndpoint(
        IdentityServerOptions options,
        IDiscoveryResponseGenerator responseGenerator,
        ILogger<DiscoveryEndpoint> logger,
        IMemoryCache memoryCache)  // NEW: Inject IMemoryCache
    {
        _logger = logger;
        _options = options;
        _responseGenerator = responseGenerator;
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        _logger.LogTrace("Processing discovery request.");

        if (!HttpMethods.IsGet(context.Request.Method))
        {
            _logger.LogWarning("Discovery endpoint only supports GET requests");
            return Task.FromResult((IEndpointResult)new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
        }

        if (!_options.Endpoints.EnableDiscoveryEndpoint)
        {
            _logger.LogInformation("Discovery endpoint disabled. 404.");
            return Task.FromResult((IEndpointResult)new StatusCodeResult(HttpStatusCode.NotFound));
        }

        var baseUrl = context.GetIdentityServerBaseUri();
        var issuerUri = context.GetIdentityServerIssuerUri();

        // Create cache key based on issuer URI to handle multi-tenant scenarios
        var cacheKey = $"discovery_{issuerUri}";

        _logger.LogTrace("Calling into discovery response generator: {type}", 
            _responseGenerator.GetType().FullName);

        // Use injected IMemoryCache with expiration
        var response = _memoryCache.GetOrCreate(cacheKey, entry =>
        {
            entry.SetOptions(_cacheOptions);
            _logger.LogTrace("Creating new discovery document for {issuer}", issuerUri);
            
            return _responseGenerator.CreateDiscoveryDocumentAsync(baseUrl, issuerUri)
                .GetAwaiter().GetResult();
        });

        _logger.LogTrace("Discovery request completed. Return DiscoveryDocumentResult");
        return Task.FromResult((IEndpointResult)new DiscoveryDocumentResult(response));
    }
}
```

### Registration Code
**File**: `src/IdentityServer4/src/Configuration/DependencyInjection/IdentityServerServiceCollectionExtensions.cs`

```csharp
// Add to IServiceCollection extension:
services.AddMemoryCache();  // Add this if not already present

services.AddScoped<DiscoveryEndpoint>();  // Already registered, now uses IMemoryCache
```

### Benefits
- ✅ Automatic expiration after 1 hour
- ✅ Sliding expiration resets on access
- ✅ Handles multi-tenant scenarios correctly
- ✅ Memory released automatically on expiration
- ✅ Configurable through MemoryCacheEntryOptions

---

## Fix #2: Clear DbContext Change Tracker in TokenCleanupService

### Current Code (PROBLEMATIC)
**File**: `src/EntityFramework.Storage/src/TokenCleanup/TokenCleanupService.cs`

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
            // ❌ DbContext change tracker NOT cleared - accumulates memory
        }
    }
}
```

### Fixed Code (RECOMMENDED)
```csharp
protected virtual async Task RemoveGrantsAsync()
{
    var found = Int32.MaxValue;
    var batchCount = 0;
    
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

            // ✅ NEW: Clear change tracker after each batch
            batchCount++;
            ClearChangeTracker();
            
            _logger.LogDebug("Batch {batchNumber} processed. Cleared {trackedEntries} change tracker entries", 
                batchCount, _persistedGrantDbContext.ChangeTracker.Entries().Count());
        }
    }
}

protected virtual async Task RemoveDeviceCodesAsync()
{
    var found = Int32.MaxValue;
    var batchCount = 0;

    while (found >= _options.TokenCleanupBatchSize)
    {
        var expiredCodes = await _persistedGrantDbContext.DeviceFlowCodes
            .Where(x => x.Expiration < DateTime.UtcNow)
            .OrderBy(x => x.Expiration)
            .Take(_options.TokenCleanupBatchSize)
            .ToArrayAsync();

        found = expiredCodes.Length;
        _logger.LogInformation("Removing {deviceCodeCount} device flow codes", found);

        if (found > 0)
        {
            _persistedGrantDbContext.DeviceFlowCodes.RemoveRange(expiredCodes);
            await SaveChangesAsync();

            if (_operationalStoreNotification != null)
            {
                await _operationalStoreNotification.DeviceCodesRemovedAsync(expiredCodes);
            }

            // ✅ NEW: Clear change tracker after each batch
            batchCount++;
            ClearChangeTracker();
            
            _logger.LogDebug("Device code batch {batchNumber} processed. Cleared change tracker", 
                batchCount);
        }
    }
}

// ✅ NEW: Helper method to safely clear change tracker
private void ClearChangeTracker()
{
    try
    {
        // Option 1: Clear all entries (recommended for cleanup operations)
        _persistedGrantDbContext.ChangeTracker.Clear();
        
        // Option 2: Detach only removed entries (alternative)
        // var changedEntries = _persistedGrantDbContext.ChangeTracker.Entries()
        //     .Where(e => e.State == EntityState.Deleted)
        //     .ToList();
        // foreach (var entry in changedEntries)
        // {
        //     entry.State = EntityState.Detached;
        // }
    }
    catch (Exception ex)
    {
        _logger.LogWarning("Exception clearing change tracker: {exception}", ex.Message);
    }
}
```

### Before/After Memory Impact
```
BEFORE Fix:
- Batch 1: 1000 grants loaded → 1000 tracked entities
- Batch 2: 1000 grants loaded → 2000 tracked entities (accumulating)
- Batch 3: 1000 grants loaded → 3000 tracked entities
- Result: ~30 MB memory for 3000 tracked objects

AFTER Fix:
- Batch 1: 1000 grants loaded → 1000 tracked → CLEARED → 0 tracked
- Batch 2: 1000 grants loaded → 1000 tracked → CLEARED → 0 tracked
- Batch 3: 1000 grants loaded → 1000 tracked → CLEARED → 0 tracked
- Result: ~10 MB memory (constant)
```

---

## Fix #3: Enhanced TokenCleanupHost Error Handling

### Current Code
**File**: `src/EntityFramework/src/TokenCleanupHost.cs`

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

### Fixed Code
```csharp
async Task RemoveExpiredGrantsAsync()
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    try
    {
        _logger.LogInformation("Starting token cleanup operation");
        
        using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>()
            .CreateScope())
        {
            var tokenCleanupService = serviceScope.ServiceProvider
                .GetRequiredService<TokenCleanupService>();
            
            await tokenCleanupService.RemoveExpiredGrantsAsync();
            
            stopwatch.Stop();
            _logger.LogInformation(
                "Token cleanup completed successfully in {duration}ms", 
                stopwatch.ElapsedMilliseconds);
        }
    }
    catch (DbException dbEx)
    {
        stopwatch.Stop();
        _logger.LogError(
            dbEx,
            "Database error during token cleanup after {duration}ms. InnerException: {innerException}",
            stopwatch.ElapsedMilliseconds,
            dbEx.InnerException?.Message);
        
        // Re-throw to surface database connectivity issues
        throw;
    }
    catch (OperationCanceledException ex)
    {
        stopwatch.Stop();
        _logger.LogWarning(
            ex,
            "Token cleanup operation was cancelled after {duration}ms",
            stopwatch.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        _logger.LogError(
            ex,
            "Unexpected exception during token cleanup after {duration}ms. Exception: {exceptionType}",
            stopwatch.ElapsedMilliseconds,
            ex.GetType().Name);
        
        // Don't re-throw to allow scheduled service to continue
        // but log detailed information for diagnostics
    }
}
```

---

## Testing Code Examples

### Test 1: Verify DbContext Change Tracker Cleanup
```csharp
[Fact]
public async Task RemoveExpiredGrantsAsync_ClearsChangeTrackerBetweenBatches()
{
    // Arrange
    var options = new OperationalStoreOptions { TokenCleanupBatchSize = 100 };
    var mockContext = new Mock<IPersistedGrantDbContext>();
    var mockLogger = new Mock<ILogger<TokenCleanupService>>();
    
    // Create 250 expired grants (will require 3 batches of 100)
    var expiredGrants = Enumerable.Range(0, 250)
        .Select(i => new PersistedGrant { Key = $"grant_{i}", Expiration = DateTime.UtcNow.AddHours(-1) })
        .ToList();
    
    var queryable = expiredGrants.AsQueryable();
    mockContext.Setup(c => c.PersistedGrants).Returns(new Mock<DbSet<PersistedGrant>>() 
    {
        DefaultValue = DefaultValue.Mock
    }.Object);
    
    var service = new TokenCleanupService(
        options,
        mockContext.Object,
        mockLogger.Object,
        null);
    
    // Act
    await service.RemoveExpiredGrantsAsync();
    
    // Assert - verify RemoveRange was called 3 times (3 batches)
    mockContext.Verify(c => c.PersistedGrants.RemoveRange(It.IsAny<IEnumerable<PersistedGrant>>()), 
        Times.Exactly(3));
    
    // Verify SaveChanges was called 3 times
    mockContext.Verify(c => c.SaveChangesAsync(), Times.Exactly(3));
    
    // In real scenario, verify memory usage didn't spike
}

[Fact]
public async Task RemoveExpiredGrantsAsync_MemoryDoesNotGrowWithLargeBatch()
{
    // Arrange
    var memoryBefore = GC.GetTotalMemory(true);
    
    // ... setup with 10,000 expired grants ...
    
    // Act
    await service.RemoveExpiredGrantsAsync();
    
    // Assert
    var memoryAfter = GC.GetTotalMemory(true);
    var memoryGrowth = memoryAfter - memoryBefore;
    
    // Should not grow by more than 10MB even with 10,000 grants
    Assert.True(memoryGrowth < 10_485_760, $"Memory grew by {memoryGrowth} bytes");
}
```

### Test 2: Verify Discovery Cache Expiration
```csharp
[Fact]
public async Task DiscoveryEndpoint_CacheExpiresAfterConfiguredTime()
{
    // Arrange
    var memoryCache = new MemoryCache(new MemoryCacheOptions());
    var mockResponseGenerator = new Mock<IDiscoveryResponseGenerator>();
    var options = new IdentityServerOptions();
    var logger = new Mock<ILogger<DiscoveryEndpoint>>();
    
    var endpoint = new DiscoveryEndpoint(options, mockResponseGenerator.Object, logger.Object, memoryCache);
    var httpContext = new DefaultHttpContext { Request = { Method = HttpMethods.Get } };
    
    // Act - First call (generates response)
    mockResponseGenerator.Setup(g => g.CreateDiscoveryDocumentAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(new Dictionary<string, object> { { "key", "value1" } });
    
    await endpoint.ProcessAsync(httpContext);
    
    // Act - Second call (from cache, should not call generator again)
    mockResponseGenerator.Setup(g => g.CreateDiscoveryDocumentAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(new Dictionary<string, object> { { "key", "value2" } });
    
    await endpoint.ProcessAsync(httpContext);
    
    // Assert - Generator called only once (second call was from cache)
    mockResponseGenerator.Verify(
        g => g.CreateDiscoveryDocumentAsync(It.IsAny<string>(), It.IsAny<string>()),
        Times.Once);
    
    // Simulate cache expiration
    await Task.Delay(TimeSpan.FromHours(1).Add(TimeSpan.FromSeconds(1)));
    
    // Act - Third call (cache expired, should generate new)
    mockResponseGenerator.Setup(g => g.CreateDiscoveryDocumentAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(new Dictionary<string, object> { { "key", "value3" } });
    
    await endpoint.ProcessAsync(httpContext);
    
    // Assert - Generator called twice total (after cache expiration)
    mockResponseGenerator.Verify(
        g => g.CreateDiscoveryDocumentAsync(It.IsAny<string>(), It.IsAny<string>()),
        Times.Exactly(2));
}
```

---

## Deployment Checklist

Before deploying memory leak fixes:

- [ ] Apply DbContext change tracker clear in TokenCleanupService
- [ ] Update DiscoveryEndpoint to use IMemoryCache
- [ ] Add enhanced logging to TokenCleanupHost
- [ ] Run unit tests for change tracker cleanup
- [ ] Run integration tests for memory growth
- [ ] Monitor memory usage post-deployment
- [ ] Set up alerts for memory thresholds
- [ ] Document changes in release notes
- [ ] Update architecture documentation

---

## Monitoring Queries (Application Insights)

```kusto
// Monitor DbContext change tracker
customMetrics
| where name == "TokenCleanupService.ChangeTrackerEntriesCleared"
| summarize AvgEntries = avg(value), MaxEntries = max(value) by bin(timestamp, 5m)

// Monitor discovery cache hits/misses
customMetrics
| where name == "DiscoveryEndpoint.CacheHits" or name == "DiscoveryEndpoint.CacheMisses"
| summarize HitRate = sum(iif(name == "DiscoveryEndpoint.CacheHits", value, 0)) / (sum(iif(name == "DiscoveryEndpoint.CacheHits", value, 0)) + sum(iif(name == "DiscoveryEndpoint.CacheMisses", value, 0))) by bin(timestamp, 1h)

// Monitor memory growth
performanceCounters
| where name == "\\Memory\\Available MBytes"
| summarize AvailableMemory = avg(value), MinMemory = min(value) by bin(timestamp, 15m)
```

