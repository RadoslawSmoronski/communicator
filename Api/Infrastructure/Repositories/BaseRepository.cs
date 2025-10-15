using Application.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BaseRepository<TEntity, TDbEntity> : IBaseRepository<TEntity>
        where TDbEntity : class
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

        public void Delete(TEntity entity)
            => _dbSet.Remove(_mapper.Map<TDbEntity>(entity));

        public void Update(TEntity entity)
            => _dbSet.Update(_mapper.Map<TDbEntity>(entity));
    }
}
