namespace Eddie.EventDispatching.Injections
{
    public class OptionalInjectAttribute : TaggedAttribute
    {
        public OptionalInjectAttribute(string tag = "") : base(tag)
        {
        }
    }
}