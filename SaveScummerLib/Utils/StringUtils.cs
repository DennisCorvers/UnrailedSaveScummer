using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveScummerLib.Utils
{
    internal static class StringUtils
    {
        public static string NormaliseExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("Extension cannot be null or empty.", nameof(extension));
            }

            var normalizedExtension = extension
                .ToLower()
                .AsSpan()
                .Trim()
                .TrimStart('*')
                .TrimStart('.');

            return $"*.{normalizedExtension}";
        }
    }
}
