namespace TypeHierarchyModel.Models
{
    public class DerivedTypeA : BaseType
    {
        public string ThingA { get; set; }

        public int OtherId { get; set; }
        public virtual OtherBaseType Other { get; set; }
    }
}
