using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SvgAwesome
{
    /// <summary>Provides methods to retrieve FontAwesome icons in SVG format. All methods in this class are thread-safe.</summary>
    public static partial class SvgFontAwesome
    {
        private struct Icon
        {
            public short Width;
            public short Height;
            public string Path;
        }

        private static Dictionary<string, Icon> _icons = null;

        private static void decode()
        {
            var data = _data; // take a copy of the reference for thread-safety; multiple threads can call decode without errors
            if (data == null)
                return;
            var icons = new Dictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);
            using (var ms = new MemoryStream(data))
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            using (var bs = new BinaryReader(ds))
            {
                while (true)
                {
                    var name = bs.ReadString();
                    if (name == "")
                        break;
                    var width = bs.ReadInt16();
                    var height = bs.ReadInt16();
                    var path = bs.ReadString();
                    icons.Add(name, new Icon { Width = width, Height = height, Path = path });
                }
            }
            _icons = icons; // assign _icons before _data for thread safety - this ensures that the return at the top of this method can only occur with a non-null _icons
            _data = null; // make the original compressed byte array eligible for GC as we no longer need it
        }

        /// <summary>Returns the names of all FontAwesome icons available through this library.</summary>
        public static IEnumerable<string> GetAllNames()
        {
            if (_icons == null)
                decode();
            return _icons.Keys;
        }

        /// <summary>
        ///     Returns a value to indicate whether this library contains a FontAwesome icon with the specified name.</summary>
        /// <param name="name">
        ///     Icon name to check. Not case-sensitive.</param>
        /// <returns>
        ///     True if an icon with this name exists; false otherwise.</returns>
        public static bool ContainsSvg(string name)
        {
            if (_icons == null)
                decode();
            return _icons.ContainsKey(name);
        }

        /// <summary>
        ///     Retrieves SVG for the specified FontAwesome icon. Throws <see cref="ArgumentException"/> if no icon exists
        ///     with the specified name. See also <see cref="GetSvgOrNull"/>. Retrieval is reasonably efficient but involves
        ///     some string concatenation; for maximum performance consider caching the result.</summary>
        /// <param name="name">
        ///     Name of the icon to retrieve. Not case sensitive.</param>
        /// <param name="color">
        ///     Fill color for the icon. This parameter is not validated; you must ensure that the value you supply is a valid
        ///     SVG value for a path fill.</param>
        /// <returns>
        ///     A string containing XML for the specified icon's SVG representation.</returns>
        public static string GetSvg(string name, string color = "#000")
        {
            if (_icons == null)
                decode();
            if (!_icons.TryGetValue(name, out var icon))
                throw new ArgumentException($"No such FontAwesome icon: {name}", nameof(name));

            var width = icon.Width.ToString();
            var height = icon.Height.ToString();
            var part1 = @"<?xml version=""1.0"" encoding=""utf-8""?><svg width=""";
            var part2 = @""" height=""";
            var part3 = @""" viewBox=""0 0 ";
            var part4 = @""" xmlns=""http://www.w3.org/2000/svg""><path d=""";
            if (color == "#000")
                color = "";
            else
                color = @""" fill=""" + color;
            var part5 = @"""/></svg>";
            var sb = new StringBuilder(part1.Length + width.Length + part2.Length + height.Length + part3.Length + width.Length + 1 + height.Length + part4.Length
                + icon.Path.Length + color.Length + part5.Length + 16); // extra 16 just in case there's something I don't know. TODO: performance test this vs zero; zero should in theory be equally fast.
            sb.Append(part1);
            sb.Append(width);
            sb.Append(part2);
            sb.Append(height);
            sb.Append(part3);
            sb.Append(width);
            sb.Append(" ");
            sb.Append(height);
            sb.Append(part4);
            sb.Append(icon.Path);
            sb.Append(color);
            sb.Append(part5);
            return sb.ToString();
        }

        /// <summary>
        ///     Retrieves SVG for the specified FontAwesome icon. Returns null if no icon exists with the specified name. See
        ///     also <see cref="GetSvg"/>. Retrieval is reasonably efficient but involves some string concatenation; for
        ///     maximum performance consider caching the result.</summary>
        /// <param name="name">
        ///     Name of the icon to retrieve. Not case sensitive.</param>
        /// <param name="color">
        ///     Fill color for the icon. This parameter is not validated; you must ensure that the value you supply is a valid
        ///     SVG value for a path fill.</param>
        /// <returns>
        ///     A string containing XML for the specified icon's SVG representation, or null if no such icon exists.</returns>
        public static string GetSvgOrNull(string name, string color = "#000")
        {
            return ContainsSvg(name) ? GetSvg(name, color) : null;
        }
    }
}
