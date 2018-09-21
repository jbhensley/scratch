using System;
using System.Collections.Generic;
using System.Text;

namespace CoreConsoleApp1
{
    internal static class PathInternal
    {
        internal const char DirectorySeparatorChar = '\\';
        internal const char AltDirectorySeparatorChar = '/';

        /// <summary>
        /// Returns true if the path ends in a directory separator.
        /// </summary>
        internal static bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
            => path.Length > 0 && IsDirectorySeparator(path[path.Length - 1]);

        internal static bool IsDirectorySeparator(char c)
        {
            return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
        }

        /// <summary>
        /// Returns true if the path starts in a directory separator.
        /// </summary>
        internal static bool StartsWithDirectorySeparator(ReadOnlySpan<char> path) => path.Length > 0 && IsDirectorySeparator(path[0]);
    }
}
