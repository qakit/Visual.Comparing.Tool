namespace DBTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("StableFile")]
    public partial class StableFile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StableFile()
        {
            TestResults = new HashSet<TestResult>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StableFileId { get; set; }

        public int TestId { get; set; }

        public int EnvironmentId { get; set; }

        [Required]
        [StringLength(260)]
        public string FilePath { get; set; }

        public virtual Environment Environment { get; set; }

        public virtual Test Test { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestResult> TestResults { get; set; }
    }
}
