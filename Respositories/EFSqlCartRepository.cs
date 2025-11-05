using Microsoft.EntityFrameworkCore;
using CarvedRockFitness.Data;

public class EFSqlCartRepository : ICartRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public EFSqlCartRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<CartItem>> GetCartAsync(string sessionId, string? userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CartItems
            .Where(c => c.UserId == userId || (userId == null && c.UserId == sessionId))
            .ToListAsync();
    }

    public async Task SaveCartAsync(string sessionId, string? userId, List<CartItem> items)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Remove existing cart items for this user/session
        var existingItems = await context.CartItems
            .Where(c => c.UserId == userId || (userId == null && c.UserId == sessionId))
            .ToListAsync();
        
        context.CartItems.RemoveRange(existingItems);

        // Add new items
        foreach (var item in items)
        {
            item.UserId = userId ?? sessionId;
            context.CartItems.Add(item);
        }

        await context.SaveChangesAsync();
    }

    public async Task AddToCartAsync(string sessionId, string? userId, CartItem item)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        item.UserId = userId ?? sessionId;
        
        // Check if item already exists in cart
        var existingItem = await context.CartItems
            .FirstOrDefaultAsync(c => 
                (c.UserId == userId || (userId == null && c.UserId == sessionId)) && 
                c.ProductId == item.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
            context.CartItems.Update(existingItem);
        }
        else
        {
            context.CartItems.Add(item);
        }

        await context.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(string sessionId, string? userId, int productId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.CartItems
            .FirstOrDefaultAsync(c => 
                (c.UserId == userId || (userId == null && c.UserId == sessionId)) && 
                c.ProductId == productId);

        if (item != null)
        {
            context.CartItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }

    public async Task UpdateQuantityAsync(string sessionId, string? userId, int productId, int quantity)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.CartItems
            .FirstOrDefaultAsync(c => 
                (c.UserId == userId || (userId == null && c.UserId == sessionId)) && 
                c.ProductId == productId);

        if (item != null)
        {
            if (quantity <= 0)
            {
                context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
                context.CartItems.Update(item);
            }
            await context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string sessionId, string? userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var items = await context.CartItems
            .Where(c => c.UserId == userId || (userId == null && c.UserId == sessionId))
            .ToListAsync();

        context.CartItems.RemoveRange(items);
        await context.SaveChangesAsync();
    }
}