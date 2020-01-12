using Phases.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases
{
    class CodeGenerationProfile
    {
        public string ProjectName { set; get; }
        public string Path { set; get; }
        public List<string> PriorityList { get; set; }
        public CodeGeneratorProperties Properties { get; set; }
    }
}
