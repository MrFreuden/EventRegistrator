using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventRegistrator.Application.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class CallbackCommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public CallbackCommandAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class StateAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public StateAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
}
