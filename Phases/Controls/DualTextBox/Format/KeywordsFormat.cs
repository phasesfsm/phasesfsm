using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualText
{
    public class KeywordsFormat : TextFormat
    {
        public List<string> Keywords { get; private set; }
        public KeywordsFormat(Color color, FontStyle style)
            : base(color, style)
        {
            Keywords = new List<string>();
        }

        public KeywordsFormat(string[] keywords, Color color, FontStyle style)
            : base(color, style)
        {
            Keywords = new List<string>(keywords);
        }
    }
}
