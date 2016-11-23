namespace VCT.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RunningSuite")]
    public partial class RunningSuite
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RunningSuite()
        {
            RunningTests = new HashSet<RunningTest>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RunningSuiteId { get; set; }

        public int SuiteId { get; set; }

        public int EnvironmentId { get; set; }

        public DateTime Started { get; set; }

        public DateTime? Completed { get; set; }

        public virtual Environment Environment { get; set; }

        public virtual Suite Suite { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RunningTest> RunningTests { get; set; }
    }
}
