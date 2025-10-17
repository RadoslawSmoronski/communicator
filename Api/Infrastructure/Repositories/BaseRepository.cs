using Application.Repositories;
using AutoMapper;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BaseRepository<TEntity, TDbEntity> : IBaseRepository<TEntity>
        where TDbEntity : class, IInfraEntity
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TDbEntity> _dbSet;

        protected readonly IMapper _mapper;

        public BaseRepository(DbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = _context.Set<TDbEntity>();
            _mapper = mapper;
        }

        public async Task AddAsync(TEntity entity)
                => await _dbSet.AddAsync(_mapper.Map<TDbEntity>(entity));

        public async Task DeleteAsync(Guid id)
        {
            var elementToRemove = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            _dbSet.Remove(elementToRemove!);
        }

        public void Update(TEntity entity)
            => _dbSet.Update(_mapper.Map<TDbEntity>(entity));
    }
}
