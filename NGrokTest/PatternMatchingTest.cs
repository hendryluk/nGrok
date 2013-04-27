using NGrok;
using NUnit.Framework;
using FluentAssertions;

namespace NGrokTest
{
    public class PatternMatchingTest
    {
        [Test]
        public void Blah()
        {
            var grok = new Grok();
            grok.AddPattern("USERNAME", "[a-z]+");
            grok.AddPattern("HENLOG", "%{USERNAME:user};");

            grok.Compile("%{HENLOG:mylog}");
            var match = grok.Match("aaaaa;");
            match.Should().NotBeNull();

            var captures = match.Captures;
        }
    }
}