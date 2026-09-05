namespace Eddie.EventDispatching.Injections
{
    public class InjectAttribute : TaggedAttribute
    {
        public InjectAttribute(string tag = "") : base(tag)
        {
        }
    }
}