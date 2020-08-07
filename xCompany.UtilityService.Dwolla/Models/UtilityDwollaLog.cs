using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xCompany.UtilityService.Dwolla.Models
{
    [Table("UtilityDwollaLog")]
    public class UtilityDwollaLog
    {
        public string EventName { get; set; }
        public string LogData { get; set; }
        public long UserId { get; set; }
        [Write(false)]
        public DateTime CreatedUtcDateTime { get; set; }
        public string CreatedByUser { get; set; }
    }
}
