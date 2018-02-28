using System.ComponentModel.DataAnnotations.Schema;

namespace TypeHierarchyModel.Models
{
    public class DerivedTypeB : DerivedTypeA
    {
        public string ThingB { get; set; }

        public virtual new DerivedOtherType Other { get; set; }
    }
}
