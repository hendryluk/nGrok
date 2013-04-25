using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace NGrok
{
    public class Pile
    {
        private readonly IDictionary<string, string> _patterns = new Dictionary<string, string>();
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IList<Grok> _groks = new List<Grok>(); 

        public void AddPattern(string name, string pattern)
         {
             _patterns[name] = pattern;
         }

        public void Compile(string pattern)
        {
            var grok = new Grok();
            foreach(var p in _patterns)
                grok.AddPattern(p.Key, p.Value);

            grok.Compile(pattern);
            Log.InfoFormat("Pile compiled new grok, pattern: {0}, expandedPattern = {1}", pattern, grok.ExpandedPattern);
            _groks.Add(grok);
        }

        public IEnumerable<KeyValuePair<Grok, Match>> Match(string text)
        {
            return from grok in _groks 
                   let match = grok.Match(text) 
                   where match != null 
                   select new KeyValuePair<Grok, Match>(grok, match);
        }
    }
}