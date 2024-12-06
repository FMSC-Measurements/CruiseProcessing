using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable

namespace CruiseProcessing.OutputModels
{
    //[JsonSerializable]
    public class TeaReportItem
    {
        [Required]
        [JsonRequired]
        public string CuttingUnit { get; set; }
        [Required]
        [JsonRequired]
        public string Stratum { get; set; }
        [Required]
        [JsonRequired]
        public string SampleGroup { get; set; }
        [Required]
        [JsonRequired]
        public string SpeciesFIA { get; set; }
        [Required]
        [JsonRequired]
        public string LiveDead { get; set; }
        [Required]
        [JsonRequired]
        public bool IsSubjectToAgreement { get; set; }
        [Required]
        [JsonRequired]
        public int UOM { get; set; }
        [Required]
        [JsonRequired]
        public int TreeGrade { get; set; }
        [Required]
        [JsonRequired]
        public int Product { get; set; }
        public string CuttingUnitPrescription { get; set; }
        [Required]
        [JsonRequired]
        public string CuttingUnitLoggingMethod { get; set; }
        [Required]
        [JsonRequired]
        public double SumExpansionFactors { get; set; }
        [Required]
        [JsonRequired]
        public int EstNumberOfTrees { get; set; }
        [Required]
        [JsonRequired]
        public double SumDBHOB { get; set; }
        [Required]
        [JsonRequired]
        public double SumDBHOBsqrd { get; set; }
        public double SumTotalHeight { get; set; }
        public double SumMerchHeight { get; set; }
        public int SumLogs { get; set; }
        public double SumGrossBdft { get; set; }
        public double SumNetBdft { get; set; }
        public double SumGrossBdftRem { get; set; }
        public double SumGrossCuft { get; set; }
        public double SumNetCuft { get; set; }
        public double SumGrossCuftRem { get; set; }
        public double SumCords { get; set; }
        public double SumWeight { get; set; }

    }
}
