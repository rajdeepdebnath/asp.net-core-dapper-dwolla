using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace xCompany.UtilityService.Dwolla.Models
{
    public class xCompanyUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
    }
}
