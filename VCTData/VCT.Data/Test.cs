namespace VCT.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Test")]
    public partial class Test
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Test()
        {
            RunningTests = new HashSet<RunningTest>();
            StableFiles = new HashSet<StableFile>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TestId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public int SuiteId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RunningTest> RunningTests { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StableFile> StableFiles { get; set; }

        public virtual Suite Suite { get; set; }
    }
}
