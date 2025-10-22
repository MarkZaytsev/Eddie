using System;
using Eddie.EventDispatching.Events;
using FrostLib.Signals;

namespace Eddie.EventDispatching.Binding
{
    public class Binder<TConstraint>
    {
        internal Group Group { get; private set; } = new();

        internal EventType EventType { get; private set; }

        internal readonly Signal<Binder<TConstraint>> OnPropagatedSignal = new();

        private readonly Action<Group, EventType> _action;

        internal Binder(Action<Group, EventType> adder) => _action = adder;

        internal void Add(Type type) => Group.Add(type);

        public Binder<TConstraint> AsSequenceAsync()
        {
            Group.IsSequential = true;
            return this;
        }

        public Binder<TConstraint> AsSceneSwitchPersistent()
        {
            Group.CancelOnSceneSwitch = false;
            return this;
        }

        public Binder<TConstraint> Once()
        {
            Group.UnbindAfterFirstExecution = true;
            return this;
        }

        public void To(EventType eventType)
        {
            EventType = eventType;
            Finish(eventType);
        }

        private void Finish(EventType eventType) => _action(Group, eventType);

        public Binder<TConstraint> Propagate()
        {
            var newBinder = new Binder<TConstraint>(_action)
            {
                EventType = EventType,
                Group = Group.Clone()
            };

            OnPropagatedSignal.Dispatch(newBinder);
            return newBinder;
        }
    }
}