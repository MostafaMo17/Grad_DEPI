
namespace MediCare.Services.Interfaces
{
    // Groups several database changes into one transaction:
    // either all are saved or none.
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();

        Task<int> SaveChangesAsync();

        Task CommitAsync();

        Task RollbackAsync();
    }
}
