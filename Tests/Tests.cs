using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SvgAwesome;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestBasic()
        {
            var names = SvgFontAwesome.GetAllNames().ToList();
            Assert.IsTrue(names.Count > 100);
            foreach (var name in names)
            {
                var svg = SvgFontAwesome.GetSvg(name);
                XDocument.Parse(svg); // check it's valid XML
                var svg2 = SvgFontAwesome.GetSvg(name, "#0f0");
                XDocument.Parse(svg2); // check it's valid XML
                var svg3 = SvgFontAwesome.GetSvgOrNull(name, "#0f0");
                Assert.AreEqual(svg2, svg3);
                Assert.IsTrue(SvgFontAwesome.ContainsSvg(name));
                Assert.IsTrue(SvgFontAwesome.ContainsSvg(name.ToUpper()));
                Assert.IsTrue(SvgFontAwesome.ContainsSvg(name.ToLower()));
            }

            Assert.IsFalse(SvgFontAwesome.ContainsSvg("no-such-icon-gahlkwi"));
            Assert.IsNull(SvgFontAwesome.GetSvgOrNull("no-such-icon-gahlkwi"));
            try
            {
                SvgFontAwesome.GetSvg("no-such-icon-gahlkwi");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
