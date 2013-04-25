using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Common.Logging;
using System.Linq;
using Match = NGrok.Match;

namespace NGrok
{
    public class Grok
    {
        private readonly IDictionary<string, string> _patterns = new Dictionary<string, string>();
        private readonly Regex _patternRe;
        private IDictionary<string, string> _captureMap = new Dictionary<string, string>();

        private Regex _regexp;

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public Grok()
        {
            _patternRe = new Regex(string.Join("", @"
            %\{
               (?<name>
                 (?<pattern>[A-z0-9]+)
                 (?::(?<subname>[A-z0-9_:]+))?
               )
               (?:=(?<definition>
                 (?:
                   (?:[^{}\\]+|\\.+)+
                   |
                   (?<curly>\{(?:(?>[^{}]+|(?>\\[{}])+)|(\G<curly>))*\})+
                 )+
               ))?
               [^}]*
             \}"
             .Where(x=> !char.IsWhiteSpace(x))));
        }

        public string ExpandedPattern { get; private set; }

        public void AddPattern(string name, string pattern)
        {
            Log.InfoFormat("Adding pattern, {0}, {1}", name, pattern);
            _patterns[name] = pattern;
        }

        public void Compile(string pattern)
        {
            _captureMap = new Dictionary<string, string>();

            var iterationLeft = 10000;

            ExpandedPattern = pattern;
            var index = 0;

            while(true)
            {
                if(iterationLeft == 0)
                    throw new FormatException(string.Format("Deep recursion pattern of compilation of {0} - expanded: {1}", pattern, ExpandedPattern));

                iterationLeft--;
                var m = _patternRe.Match(ExpandedPattern);
                if (!m.Success)
                    break;

                var matchedPattern = m.Groups["pattern"].Value;
                if (m.Groups["definition"].Success)
                {
                    AddPattern(matchedPattern, m.Groups["definition"].Value);
                }

                if (_patterns.ContainsKey(matchedPattern))
                {
                    var regex = _patterns[matchedPattern];
                    var capture = string.Format("a{0}", index);
                    var replacement_pattern = string.Format("(?<{0}>{1})", capture, regex);
                    _captureMap[capture] = m.Groups["name"].Value;

                    ExpandedPattern = ExpandedPattern.Replace(m.Groups[0].Value, replacement_pattern);
                    index++;
                }
                else
                {
                    throw new FormatException(string.Format("Pattern {0} not defined", m.Groups[0]));
                }
            }

            _regexp = new Regex(ExpandedPattern);
            Log.DebugFormat("Grok compiled OK, pattern: {0}, expandedPattern: {1}", pattern, ExpandedPattern);
        }

        public Match Match(string text)
        {
            var match = _regexp.Match(text);

            if (match.Success)
            {
                var grokMatch = new Match(text, match.Index, this, _regexp, match);
                Log.DebugFormat("Regexp match object, names: {0}, captures: {1}", _regexp.GetGroupNames(), match.Groups);
                return grokMatch;
            }

            return null;
        }

        public string GetCaptureName(string key)
        {
            string name;
            return _captureMap.TryGetValue(key, out name)?name: null;
        }
    }
}