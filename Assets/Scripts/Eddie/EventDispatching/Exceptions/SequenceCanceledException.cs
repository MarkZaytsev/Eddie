using System;

namespace Eddie.EventDispatching.Exceptions
{
    public class SequenceCanceledException : Exception
    {
        public SequenceCanceledException(string message) : base(message)
        {
        }
    }
}