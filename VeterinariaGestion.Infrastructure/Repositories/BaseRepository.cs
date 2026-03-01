using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Interfaces;
using VeterinariaGestion.Infrastructure.Data;

namespace VeterinariaGestion.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly VeterinariaDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(VeterinariaDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        var entry = _context.ChangeTracker.Entries<T>().FirstOrDefault();
        if (entry != null) entry.State = EntityState.Detached;
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
        => await _dbSet.FindAsync(id) is not null;
}