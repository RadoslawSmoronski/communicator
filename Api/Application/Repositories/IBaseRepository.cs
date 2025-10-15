namespace Application.Repositories
{
    public interface IBaseRepository<TEndity>
    {
        Task AddAsync(TEndity endity);
        void Update(TEndity endity);
        void Delete(TEndity endity);
        //void DeleteRange(IEnumerable<T> entities);
    }
}
