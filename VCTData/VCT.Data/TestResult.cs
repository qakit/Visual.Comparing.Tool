namespace VCT.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TestResult")]
    public partial class TestResult
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TestResultId { get; set; }

        public int RunningTestId { get; set; }

        public int StableFileId { get; set; }

        [Required]
        [StringLength(260)]
        public string ResultFilePath { get; set; }

        [Required]
        [StringLength(260)]
        public string DiffFilePath { get; set; }

        public virtual RunningTest RunningTest { get; set; }

        public virtual StableFile StableFile { get; set; }
    }
}
