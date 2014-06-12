using System.Collections.Generic;

namespace Ais.Internal.Dcm.Web.Models
{
    public class GetUsersResponse
    {
        public List<UserModel> Users { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalUsers { get; set; }
    }
}