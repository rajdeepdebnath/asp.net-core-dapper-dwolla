using Dapper;
using xCompany.UtilityService.Dwolla.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace xCompany.UtilityService.Dwolla.Repository
{
    public interface IxCompanyRepository
    {
        Task<xCompanyUser> GetUserDetailsByUserId(long userId);
        Task<xCompanyUser> GetUserDetailsByEmail(string email);
        Task<IEnumerable<UtilityDwollaLog>> GetDwollaLogByUserId(long userId);
        Task SaveDwollaLog(UtilityDwollaLog log);

        Task<UserProvider> GetUserProviderByUserId(long userId);
        Task<UserProvider> SaveUserProvider(UserProvider provider);
    }
}
