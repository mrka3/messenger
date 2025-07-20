namespace Abstractions;

public interface IBaseRepository<T>
{
    T[] GetAll();
    T Get(long id);
    long Save(T entity);
}