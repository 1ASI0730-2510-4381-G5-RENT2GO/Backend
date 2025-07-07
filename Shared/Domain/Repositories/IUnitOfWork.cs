namespace BackendRent2Go.Shared.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> CompleteAsync();
}
