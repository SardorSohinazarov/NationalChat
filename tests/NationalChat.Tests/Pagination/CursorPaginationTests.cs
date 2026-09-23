using Application.DataTransferObjects.Pagination;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace NationalChat.Tests.Pagination;

/// <summary>Keyset pagination (QueryableExtensions.ToCursorPagedResponseAsync) on an in-memory EF Core store.</summary>
public sealed class CursorPaginationTests
{
    private sealed class Item
    {
        public int Id { get; set; }
    }

    private sealed class ItemsDb(DbContextOptions<ItemsDb> options) : DbContext(options)
    {
        public DbSet<Item> Items => Set<Item>();
    }

    private static async Task<ItemsDb> CreateDbAsync(int count)
    {
        var db = new ItemsDb(new DbContextOptionsBuilder<ItemsDb>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Items.AddRange(Enumerable.Range(1, count).Select(id => new Item { Id = id }));
        await db.SaveChangesAsync();
        return db;
    }

    private static Task<CursorPagedResponse<int>> PageAsync(ItemsDb db, int limit, int? beforeId) =>
        db.Items.ToCursorPagedResponseAsync(new CursorPaginationRequest { Limit = limit, BeforeId = beforeId }, x => x.Id, x => x.Id, id => id);

    [Fact]
    public async Task FirstPage_ReturnsNewestItemsAndCursor()
    {
        await using var db = await CreateDbAsync(25);

        var page = await PageAsync(db, 10, null);

        Assert.Equal(Enumerable.Range(16, 10).Reverse(), page.Items);
        Assert.True(page.HasMore);
        Assert.Equal(16, page.NextCursor);
    }

    [Fact]
    public async Task BeforeId_ReturnsOlderItemsOnly()
    {
        await using var db = await CreateDbAsync(25);

        var page = await PageAsync(db, 10, 16);

        Assert.Equal(Enumerable.Range(6, 10).Reverse(), page.Items);
        Assert.True(page.HasMore);
        Assert.Equal(6, page.NextCursor);
    }

    [Fact]
    public async Task LastPage_HasNoCursor()
    {
        await using var db = await CreateDbAsync(25);

        var page = await PageAsync(db, 10, 6);

        Assert.Equal(Enumerable.Range(1, 5).Reverse(), page.Items);
        Assert.False(page.HasMore);
        Assert.Null(page.NextCursor);
    }
}
