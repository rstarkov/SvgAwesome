using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                using (var bw = new BinaryWriter(cs, Encoding.UTF8, leaveOpen: true))
                {
                    foreach (var file in new DirectoryInfo(@"..\..\Generator\Font-Awesome-SVG-PNG\black\svg").GetFiles("*.svg").OrderBy(f => f.Name))
                    {
                        var name = Path.GetFileNameWithoutExtension(file.Name);
                        var svg = XDocument.Load(file.FullName);
                        var width = short.Parse(svg.Root.Attribute("width").Value);
                        var height = short.Parse(svg.Root.Attribute("height").Value);
                        var ns = svg.Root.Name.Namespace;
                        var pathEl = svg.Root.Element(ns + "path");
                        var path = pathEl.Attribute("d").Value;

                        // Validate our assumptions about what's in the SVG
                        var viewbox = svg.Root.Attribute("viewBox").Value;
                        assert(viewbox == $"0 0 {width} {height}");
                        svg.Root.Attribute("width").Remove();
                        svg.Root.Attribute("height").Remove();
                        svg.Root.Attribute("viewBox").Remove();
                        svg.Root.Attribute("xmlns").Remove();
                        assert(!svg.Root.HasAttributes);
                        pathEl.Remove();
                        assert(!svg.Root.HasElements);
                        pathEl.Attribute("d").Remove();
                        assert(!pathEl.HasAttributes);
                        assert(!pathEl.HasElements);

                        // Write output
                        bw.Write(name);
                        bw.Write(width);
                        bw.Write(height);
                        bw.Write(path);
                    }
                    bw.Write("");
                }

                var data = ms.ToArray();
                var code = @"
// This file has been auto-generated using the Generator project
namespace SvgAwesome
{
    public static partial class SvgFontAwesome
    {
        private static byte[] _data = new byte[] {<bytes>
        };
    }
}".Trim();

                var sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    if (i % 32 == 0)
                        sb.Append("\r\n            ");
                    sb.Append(data[i]);
                    sb.Append(", ");
                }

                code = code.Replace("<bytes>", sb.ToString().TrimEnd());

                File.WriteAllText(@"..\..\Src\SvgData.cs", code);
            }
        }

        static void assert(bool condition)
        {
            if (!condition)
                throw new Exception();
        }
    }
}
