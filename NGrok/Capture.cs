using System.Collections.Generic;

namespace NGrok
{
    public struct Capture
    {
        public Capture(string patternName, string identifier, string[] value): this()
        {
            PatternName = patternName;
            Identifier = identifier;
            Value = value;
        }

        public string PatternName { get; private set; }
        public string Identifier { get; private set; }

        public IEnumerable<string> Value { get; private set; }
    }
}