using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CruiseProcessing.OutputModels
{
    public class TeaReport
    {
        [Required]
        [JsonRequired]
        public string SaleNumber { get; set; }
        [Required]
        [JsonRequired]
        public string SaleName { get; set; }

        public IReadOnlyCollection<TeaReportItem> Items { get; set; } = new List<TeaReportItem>();
    }
}
