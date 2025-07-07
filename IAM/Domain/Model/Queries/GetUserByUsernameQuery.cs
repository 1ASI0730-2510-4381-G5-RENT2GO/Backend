// filepath: C:\Users\GIGABYTE\RiderProjects\BackendRent2Go\IAM\Domain\Model\Queries\GetUserByUsernameQuery.cs
namespace Rent2Go.API.IAM.Domain.Model.Queries
{
    public class GetUserByUsernameQuery
    {
        public string Email { get; private set; }

        public GetUserByUsernameQuery(string email)
        {
            Email = email;
        }
    }
}
