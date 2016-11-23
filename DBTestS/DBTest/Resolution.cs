namespace DBTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Resolution")]
    public partial class Resolution
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Resolution()
        {
            Environments = new HashSet<Environment>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ResolutionId { get; set; }

        public int Width { get; set; }

        public int Heigth { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Environment> Environments { get; set; }
    }
}
