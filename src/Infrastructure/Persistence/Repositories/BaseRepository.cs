using Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : class
{
    protected BaseRepository(ChatDb db)
    {
        Db = db;
    }

    protected ChatDb Db { get; }
    protected DbSet<TEntity> Entities => Db.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Entities.FindAsync([id], cancellationToken).AsTask();

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Entities.AsNoTracking().ToListAsync(cancellationToken);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Entities.AddAsync(entity, cancellationToken).AsTask();

    public void Update(TEntity entity) => Entities.Update(entity);

    public void Remove(TEntity entity) => Entities.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Db.SaveChangesAsync(cancellationToken);
}
