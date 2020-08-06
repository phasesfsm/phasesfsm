using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualText
{
    public class GroupsFormat : TextFormat
    {
        public List<KeywordsFormat> KeywordsFormats { get; private set; }
        public string GroupName { get; private set; }
        public string RegexString { get; private set; }
        public GroupsFormat(string groupName, string regexString, Color color, FontStyle style)
            : base(color, style)
        {
            KeywordsFormats = new List<KeywordsFormat>();
            GroupName = groupName;
            RegexString = regexString;
        }

        public void AddKeywords(string[] keywords, Color color, FontStyle style)
        {
            KeywordsFormats.Add(new KeywordsFormat(keywords, color, style));
        }

    }
}
