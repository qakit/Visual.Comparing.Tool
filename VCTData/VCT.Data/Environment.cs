namespace VCT.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Environment")]
    public partial class Environment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Environment()
        {
            RunningSuites = new HashSet<RunningSuite>();
            StableFiles = new HashSet<StableFile>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EnvironmentId { get; set; }

        public int OSId { get; set; }

        public int BrowserId { get; set; }

        public int ResolutionId { get; set; }

        public virtual Browser Browser { get; set; }

        public virtual RunningOS RunningOs { get; set; }

        public virtual Resolution Resolution { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RunningSuite> RunningSuites { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StableFile> StableFiles { get; set; }
    }
}
