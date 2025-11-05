using CarvedRockFitness.Models;
using CarvedRockFitness.Data;
using Microsoft.EntityFrameworkCore;

namespace CarvedRockFitness.Repositories;

public class EFProductRepository : IProductRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public EFProductRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Product?>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products.FindAsync(id);
    }

    public async Task<IEnumerable<Product?>> GetByCategoryAsync(string? category)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        if (string.IsNullOrEmpty(category))
        {
            return await context.Products.ToListAsync();
        }
        return await context.Products
            .Where(p => p.Category.ToLower() == category.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Product?>> SearchAsync(string searchTerm)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm) || p.Category.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<Product> AddAsync(Product product)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Entry(product).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
            return product;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExistsAsync(product.Id))
            {
                return null;
            }
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var product = await context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> ProductExistsAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products.AnyAsync(e => e.Id == id);
    }
}