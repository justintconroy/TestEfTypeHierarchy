using System.Collections.Generic;

namespace TypeHierarchyModel.Models
{
    public class OtherBaseType
    {
        public int Id { get; set; }

        public string Something { get; set; }

        public virtual ICollection<DerivedTypeA> Things { get; set; }
    }
}
