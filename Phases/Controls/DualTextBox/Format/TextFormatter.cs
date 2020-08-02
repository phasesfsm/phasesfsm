using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DualText
{
    public class TextFormatter : GroupsFormat
    {
        public List<GroupsFormat> GroupsFormats { get; private set; }
        private MatchCollection textGroups;
        private const string UNGROUPED = "ungrouped";
        private const string TOKENS_GROUP = "tokens";
        private const string UNGROUPED_OTHER = "other";
        private const string NEW_LINE = "newline";
        private static readonly Regex ungrupedRegex = new Regex(@"(?<" + NEW_LINE + @">\r(?:\n)?)|(?<" + UNGROUPED_OTHER + @">[^\r\n]+)");
        private static readonly Regex grupedRegex = new Regex(@"(?<" + NEW_LINE + @">\r(?:\n)?)|(?<" + TOKENS_GROUP + @">\w+)|(?<" + UNGROUPED_OTHER + @">[^\r\n\w]*)");
        private Regex groupsRegex;
        public TextFormatter()
            : base(UNGROUPED, @"(?<" + NEW_LINE + @">\r(?:\n)?)|(?<" + UNGROUPED_OTHER + @">[^\r\n]+)", BaseFormat.ForeColor, BaseFormat.Style)
        {
            GroupsFormats = new List<GroupsFormat>();
        }
        public GroupsFormat AddGroup(string groupName, string regexString, Color color, FontStyle style)
        {
            if (groupName == UNGROUPED || UNGROUPED_OTHER == "") throw new Exception("Reserved group name.");
            if (GroupsFormats.Exists(g => g.GroupName == groupName)) throw new Exception("Group name repeated.");
            var group = new GroupsFormat(groupName, regexString, color, style);
            GroupsFormats.Add(group);
            return group;
        }
        public void BuildTextGroups()
        {
            groupsRegex = new Regex(GroupsFormats.Any() ?
                string.Join("|", GroupsFormats.ConvertAll(f => string.Format("(?<{0}>{1})", f.GroupName, f.RegexString)).ToArray()) :
                @"(?<" + UNGROUPED + ">.*)", GroupsFormats.Any() ? RegexOptions.Multiline : RegexOptions.Singleline);
        }
        internal void FormatString(string text, Text formattedText)
        {
            BuildTextGroups();
            formattedText.Clear();
            int index = 0;
            textGroups = groupsRegex.Matches(text);
            foreach (Match match in textGroups)
            {
                if (match.Index > index)
                {
                    AppendText(formattedText, text.Substring(index, match.Index - index), this);
                    index = match.Index;
                }
                var group = GroupsFormats.FirstOrDefault(gf => match.Groups[gf.GroupName].Success) ?? this;
                AppendText(formattedText, match.Value, group);
                index += match.Length;
            }
            if (index < text.Length)
            {
                AppendText(formattedText, text.Substring(index, text.Length - index), this);
            }
        }

        private void AppendText(Text formattedText, string text, GroupsFormat format)
        {
            if (format.KeywordsFormats.Any())
            {
                var mtokens = grupedRegex.Matches(text);
                foreach (Match match in mtokens)
                {
                    if (match.Groups[NEW_LINE].Success)
                    {
                        //AppendString(formattedText, format, match.Value);
                        formattedText.Lines.Add(new TextLine());
                    }
                    else if (match.Groups[TOKENS_GROUP].Success && format.KeywordsFormats.Any(gk => gk.Keywords.Contains(match.Value)))
                    {
                        var keyword = format.KeywordsFormats.First(gk => gk.Keywords.Contains(match.Value));
                        AppendString(formattedText, keyword, match.Value);
                    }
                    else
                    {
                        AppendString(formattedText, format, match.Value);
                    }
                }
            }
            else
            {
                var mtokens = ungrupedRegex.Matches(text);
                foreach (Match match in mtokens)
                {
                    if (match.Groups[NEW_LINE].Success)
                    {
                        //AppendString(formattedText, format, match.Value);
                        formattedText.Lines.Add(new TextLine());
                    }
                    else
                    {
                        AppendString(formattedText, format, match.Value);
                    }
                }
            }
        }
        private void AppendString(Text formattedText, TextFormat format, string text)
        {
            foreach (char ch in text)
            {
                formattedText.Lines.Last().AddChar(ch, format);
            }
        }
    }
}
