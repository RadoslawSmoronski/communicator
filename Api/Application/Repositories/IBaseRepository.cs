namespace Application.Repositories
{
    public interface IBaseRepository<TEndity>
    {
        Task AddAsync(TEndity endity);
        void Update(TEndity endity);
        Task DeleteAsync(Guid id);
        //void DeleteRange(IEnumerable<T> entities);
    }
}
