using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xCompany.UtilityService.Dwolla.Models
{
    [Table("UserProvider")]
    public class UserProvider
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string MerchantId { get; set; }
        public string Scope { get; set; }
        public long xCompanyUserId { get; set; }
        public int? GoCardlessId { get; set; }
        public int? MandateResourceId { get; set; }
        public int? MandateScheme { get; set; }
        public int? WebhookAction { get; set; }
        public int? WebhookCause { get; set; }
        public int? RedirectFlowId { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}
