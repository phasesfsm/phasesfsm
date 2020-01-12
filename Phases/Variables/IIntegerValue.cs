
using Cottle;
using System.Collections.Generic;

namespace Phases.Variables
{
    interface IIntegerValue
    {
        string Name { get; }
        int DefaultValue { get; set; }
        int Value { get; set; }
        int MaximumValue { get; set; }
        int MinimumValue { get; set; }

        Dictionary<Value, Value> GetDictionary();
    }
}
