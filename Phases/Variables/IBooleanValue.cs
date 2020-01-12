
using Cottle;
using System.Collections.Generic;

namespace Phases.Variables
{
    interface IBooleanValue
    {
        string Name { get; }
        bool DefaultValue { get; set; }
        bool Value { get; set; }

        Dictionary<Value, Value> GetDictionary();
    }
}
