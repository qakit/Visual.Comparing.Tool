namespace VCT.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RunningTest")]
    public partial class RunningTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RunningTest()
        {
            TestResults = new HashSet<TestResult>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RunningTestId { get; set; }

        public int TestId { get; set; }

        public int RunningSuiteId { get; set; }

        public DateTime Started { get; set; }

        public DateTime? Completed { get; set; }

        public virtual RunningSuite RunningSuite { get; set; }

        public virtual Test Test { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestResult> TestResults { get; set; }
    }
}
