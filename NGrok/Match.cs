using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace NGrok
{
    public class Match
    {
        public string Text { get; private set; }
        public int Offset { get; private set; }
        public Grok Grok { get; private set; }

        private readonly Regex _regexp;
        private readonly System.Text.RegularExpressions.Match _match;

        public Match(string text, int offset, Grok grok, Regex regexp, System.Text.RegularExpressions.Match match)
        {
            Text = text;
            Offset = offset;
            Grok = grok;
            _regexp = regexp;
            _match = match;
        }

        public IDictionary<string, string[]> Captures
        {
            get
            {
                return _regexp.GetGroupNames()
                       .Select(key => new { key, group = _match.Groups[key] })
                       .GroupBy(x => x.key, x => x.group)
                       .ToDictionary(x => x.Key, x => x.Where(g => g.Success).Select(g => g.Value).ToArray());
            }
        }
    }
}