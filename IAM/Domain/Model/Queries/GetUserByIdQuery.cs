// filepath: C:\Users\GIGABYTE\RiderProjects\BackendRent2Go\IAM\Domain\Model\Queries\GetUserByIdQuery.cs
namespace Rent2Go.API.IAM.Domain.Model.Queries
{
    public class GetUserByIdQuery
    {
        public string Id { get; private set; }

        public GetUserByIdQuery(string id)
        {
            Id = id;
        }
    }
}
