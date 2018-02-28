using System.Collections.Generic;

namespace TypeHierarchyModel.Models
{
    public class DerivedOtherType : OtherBaseType
    {
        public virtual new ICollection<DerivedTypeB> Things { get; set; }
    }
}
