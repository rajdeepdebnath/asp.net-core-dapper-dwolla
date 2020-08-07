using Dapper;
using Dapper.Contrib.Extensions;
using xCompany.UtilityService.Dwolla.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace xCompany.UtilityService.Dwolla.Repository
{
    public class xCompanyRepository : IxCompanyRepository
    {
        private readonly IConfiguration Configuration;
        public xCompanyRepository(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public async Task<xCompanyUser> GetUserDetailsByEmail(string email)
        {
            using (var connection = new SqlConnection(Configuration["SQLConnectionString"]))
            {
                var sql = @"select U.Id, U.Email, UD.FirstName, UD.LastName, bdr.BankName,
                            bdr.AccountName, bdr.AccountType, bdr.RoutingNumber, bdr.AccountNumber
                            from [User] U
                            inner join [UserDetails] UD
                            on U.Id = UD.UserId 
                            inner join BankDetailsRecord bdr
                            on U.Id = bdr.UserId 
                            where U.Email = @email";
                var user = await connection.QueryAsync<xCompanyUser>(sql, new { email });
                return user.FirstOrDefault();
            }
        }

        public async Task<xCompanyUser> GetUserDetailsByUserId(long userId)
        {
            using (var connection = new SqlConnection(Configuration["SQLConnectionString"]))
            {
                var sql = @"select U.Id, U.Email, UD.FirstName, UD.LastName, bdr.BankName,
                            bdr.AccountName, bdr.AccountType, bdr.RoutingNumber, bdr.AccountNumber
                            from [User] U
                            inner join [UserDetails] UD
                            on U.Id = UD.UserId 
                            inner join BankDetailsRecord bdr
                            on U.Id = bdr.UserId 
                            where U.Id = @userId";
                var user = await connection.QueryAsync<xCompanyUser>(sql, new { userId });
                return user.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<UtilityDwollaLog>> GetDwollaLogByUserId(long userId)
        {
            using (var connection = new SqlConnection(Configuration["SQLConnectionString"]))
            {
                var sql = @"select * from [UtilityDwollaLog] u
                            where UserId = @userId";
                var logs = await connection.QueryAsync<UtilityDwollaLog>(sql, new { userId });
                return logs;
            }
        }

        public async Task SaveDwollaLog(UtilityDwollaLog log)
        {
            using (var connection = new SqlConnection(Configuration["SQLConnectionString"]))
            {
                await connection.InsertAsync(log);
            }
        }

        public async Task<UserProvider> GetUserProviderByUserId(long userId)
        {
            using (var connection = new SqlConnection(Configuration["SQLConnectionString"]))
            {
                var sql = @"select * from [UserProvider] up
                            where xCompanyUserId = @userId";
                var userProvider = await connection.QueryAsync<UserProvider>(sql, new { userId });
                return userProvider.FirstOrDefault();
            }
        }

        public async Task<UserProvider> SaveUserProvider(UserProvider provider)
        {
            using (var connection = new SqlConnection(Configuration["SQLConnectionString"]))
            {
                if (provider.Id > 0)
                {
                    await connection.UpdateAsync(provider);
                }
                else
                {
                    provider.Id = await connection.InsertAsync(provider);
                }

                return provider;
            }
        }
    }
}
