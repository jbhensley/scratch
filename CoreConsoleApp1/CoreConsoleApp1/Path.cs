using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreConsoleApp1
{
    class Path
    {
        public static readonly char DirectorySeparatorChar = PathInternal.DirectorySeparatorChar;

        public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
        {
            if (path1.Length == 0)
                return path2.ToString();
            if (path2.Length == 0)
                return path1.ToString();

            return JoinInternal(path1, path2);
        }

        public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3)
        {
            if (path1.Length == 0)
                return Join(path2, path3);

            if (path2.Length == 0)
                return Join(path1, path3);

            if (path3.Length == 0)
                return Join(path1, path2);

            return JoinInternal(path1, path2, path3);
        }    

        public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4)
        {
            if (path1.Length == 0)
                return Join(path2, path3, path4);

            if (path2.Length == 0)
                return Join(path1, path3, path4);

            if (path3.Length == 0)
                return Join(path1, path2, path4);

            if (path4.Length == 0)
                return Join(path1, path2, path3);

            return JoinInternal(path1, path2, path3, path4);
        }

        private static unsafe string JoinInternal(ReadOnlySpan<char> first, ReadOnlySpan<char> second)
        {
            Debug.Assert(first.Length > 0 && second.Length > 0, "should have dealt with empty paths");

            bool hasSeparator = PathInternal.IsDirectorySeparator(first[first.Length - 1])
                || PathInternal.IsDirectorySeparator(second[0]);

            fixed (char* f = &MemoryMarshal.GetReference(first), s = &MemoryMarshal.GetReference(second))
            {
#if MS_IO_REDIST
                return StringExtensions.Create(
#else
                return string.Create(
#endif
                    first.Length + second.Length + (hasSeparator ? 0 : 1),
                    (First: (IntPtr)f, FirstLength: first.Length, Second: (IntPtr)s, SecondLength: second.Length, HasSeparator: hasSeparator),
                    (destination, state) =>
                    {
                        new Span<char>((char*)state.First, state.FirstLength).CopyTo(destination);
                        if (!state.HasSeparator)
                            destination[state.FirstLength] = PathInternal.DirectorySeparatorChar;
                        new Span<char>((char*)state.Second, state.SecondLength).CopyTo(destination.Slice(state.FirstLength + (state.HasSeparator ? 0 : 1)));
                    });
            }
        }

        private static unsafe string JoinInternal(ReadOnlySpan<char> first, ReadOnlySpan<char> second, ReadOnlySpan<char> third)
        {
            Debug.Assert(first.Length > 0 && second.Length > 0 && third.Length > 0, "should have dealt with empty paths");

            bool firstHasSeparator = PathInternal.IsDirectorySeparator(first[first.Length - 1])
                || PathInternal.IsDirectorySeparator(second[0]);
            bool thirdHasSeparator = PathInternal.IsDirectorySeparator(second[second.Length - 1])
                || PathInternal.IsDirectorySeparator(third[0]);

            fixed (char* f = &MemoryMarshal.GetReference(first), s = &MemoryMarshal.GetReference(second), t = &MemoryMarshal.GetReference(third))
            {
#if MS_IO_REDIST
                return StringExtensions.Create(
#else
                return string.Create(
#endif
                    first.Length + second.Length + third.Length + (firstHasSeparator ? 0 : 1) + (thirdHasSeparator ? 0 : 1),
                    (First: (IntPtr)f, FirstLength: first.Length, Second: (IntPtr)s, SecondLength: second.Length,
                        Third: (IntPtr)t, ThirdLength: third.Length, FirstHasSeparator: firstHasSeparator, ThirdHasSeparator: thirdHasSeparator),
                    (destination, state) =>
                    {
                        new Span<char>((char*)state.First, state.FirstLength).CopyTo(destination);
                        if (!state.FirstHasSeparator)
                            destination[state.FirstLength] = PathInternal.DirectorySeparatorChar;
                        new Span<char>((char*)state.Second, state.SecondLength).CopyTo(destination.Slice(state.FirstLength + (state.FirstHasSeparator ? 0 : 1)));
                        if (!state.ThirdHasSeparator)
                            destination[destination.Length - state.ThirdLength - 1] = PathInternal.DirectorySeparatorChar;
                        new Span<char>((char*)state.Third, state.ThirdLength).CopyTo(destination.Slice(destination.Length - state.ThirdLength));
                    });
            }
        }

        private static unsafe string JoinInternal(ReadOnlySpan<char> first, ReadOnlySpan<char> second, ReadOnlySpan<char> third, ReadOnlySpan<char> fourth)
        {
            Debug.Assert(first.Length > 0 && second.Length > 0 && third.Length > 0 && fourth.Length > 0, "should have dealt with empty paths");

            bool firstHasSeparator = PathInternal.IsDirectorySeparator(first[first.Length - 1])
                || PathInternal.IsDirectorySeparator(second[0]);
            bool thirdHasSeparator = PathInternal.IsDirectorySeparator(second[second.Length - 1])
                || PathInternal.IsDirectorySeparator(third[0]);
            bool fourthHasSeparator = PathInternal.IsDirectorySeparator(third[third.Length - 1])
                || PathInternal.IsDirectorySeparator(fourth[0]);

            fixed (char* f = &MemoryMarshal.GetReference(first), s = &MemoryMarshal.GetReference(second), t = &MemoryMarshal.GetReference(third), u = &MemoryMarshal.GetReference(fourth))
            {

#if MS_IO_REDIST
                return StringExtensions.Create(
#else
                return string.Create(
#endif
                    first.Length + second.Length + third.Length + fourth.Length + (firstHasSeparator ? 0 : 1) + (thirdHasSeparator ? 0 : 1) + (fourthHasSeparator ? 0 : 1),
                    (First: (IntPtr)f, FirstLength: first.Length, Second: (IntPtr)s, SecondLength: second.Length,
                        Third: (IntPtr)t, ThirdLength: third.Length, Fourth: (IntPtr)u, FourthLength: fourth.Length,
                        FirstHasSeparator: firstHasSeparator, ThirdHasSeparator: thirdHasSeparator, FourthHasSeparator: fourthHasSeparator),
                    (destination, state) =>
                    {
                        new Span<char>((char*)state.First, state.FirstLength).CopyTo(destination);
                        if (!state.FirstHasSeparator)
                            destination[state.FirstLength] = PathInternal.DirectorySeparatorChar;
                        new Span<char>((char*)state.Second, state.SecondLength).CopyTo(destination.Slice(state.FirstLength + (state.FirstHasSeparator ? 0 : 1)));
                        if (!state.ThirdHasSeparator)
                            destination[state.FirstLength + state.SecondLength + (state.FirstHasSeparator ? 0 : 1)] = PathInternal.DirectorySeparatorChar;
                        new Span<char>((char*)state.Third, state.ThirdLength).CopyTo(destination.Slice(state.FirstLength + state.SecondLength + (state.FirstHasSeparator ? 0 : 1) + (state.ThirdHasSeparator ? 0 : 1)));
                        if (!state.FourthHasSeparator)
                            destination[destination.Length - state.FourthLength - 1] = PathInternal.DirectorySeparatorChar;
                        new Span<char>((char*)state.Fourth, state.FourthLength).CopyTo(destination.Slice(destination.Length - state.FourthLength));
                    });
            }
        }

        public static string Join(string path1, string path2)
        {
            return Join(path1.AsSpan(), path2.AsSpan());
        }

        public static string Join(string path1, string path2, string path3)
        {
            return Join(path1.AsSpan(), path2.AsSpan(), path3.AsSpan());
        }

        public static string Join(string path1, string path2, string path3, string path4)
        {
            return Join(path1.AsSpan(), path2.AsSpan(), path3.AsSpan(), path4.AsSpan());
        }

        public static bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (path1.Length == 0 && path2.Length == 0)
                return true;

            if (path1.Length == 0 || path2.Length == 0)
            {
                ref ReadOnlySpan<char> pathToUse = ref path1.Length == 0 ? ref path2 : ref path1;
                if (destination.Length < pathToUse.Length)
                {
                    return false;
                }

                pathToUse.CopyTo(destination);
                charsWritten = pathToUse.Length;
                return true;
            }

            bool needsSeparator = !(PathInternal.EndsInDirectorySeparator(path1) || PathInternal.StartsWithDirectorySeparator(path2));
            int charsNeeded = path1.Length + path2.Length + (needsSeparator ? 1 : 0);
            if (destination.Length < charsNeeded)
                return false;

            path1.CopyTo(destination);
            if (needsSeparator)
                destination[path1.Length] = DirectorySeparatorChar;

            path2.CopyTo(destination.Slice(path1.Length + (needsSeparator ? 1 : 0)));

            charsWritten = charsNeeded;
            return true;
        }

        public static bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (path1.Length == 0 && path2.Length == 0 && path3.Length == 0)
                return true;

            if (path1.Length == 0)
                return TryJoin(path2, path3, destination, out charsWritten);
            if (path2.Length == 0)
                return TryJoin(path1, path3, destination, out charsWritten);
            if (path3.Length == 0)
                return TryJoin(path1, path2, destination, out charsWritten);

            int neededSeparators = PathInternal.EndsInDirectorySeparator(path1) || PathInternal.StartsWithDirectorySeparator(path2) ? 0 : 1;
            bool needsSecondSeparator = !(PathInternal.EndsInDirectorySeparator(path2) || PathInternal.StartsWithDirectorySeparator(path3));
            if (needsSecondSeparator)
                neededSeparators++;

            int charsNeeded = path1.Length + path2.Length + path3.Length + neededSeparators;
            if (destination.Length < charsNeeded)
                return false;

            bool result = TryJoin(path1, path2, destination, out charsWritten);
            Debug.Assert(result, "should never fail joining first two paths");

            if (needsSecondSeparator)
                destination[charsWritten++] = DirectorySeparatorChar;

            path3.CopyTo(destination.Slice(charsWritten));
            charsWritten += path3.Length;

            return true;
        }

        public static bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (path1.Length == 0 && path2.Length == 0 && path3.Length == 0 && path4.Length == 0)
                return true;

            if (path1.Length == 0)
                return TryJoin(path2, path3, path4, destination, out charsWritten);
            if (path2.Length == 0)
                return TryJoin(path1, path3, path4, destination, out charsWritten);
            if (path3.Length == 0)
                return TryJoin(path1, path2, path4, destination, out charsWritten);
            if (path4.Length == 0)
                return TryJoin(path1, path2, path3, destination, out charsWritten);

            int neededSeparators = PathInternal.EndsInDirectorySeparator(path1) || PathInternal.StartsWithDirectorySeparator(path2) ? 0 : 1;
            neededSeparators += PathInternal.EndsInDirectorySeparator(path2) || PathInternal.StartsWithDirectorySeparator(path3) ? 0 : 1;

            bool needsThirdSeparator = !(PathInternal.EndsInDirectorySeparator(path3) || PathInternal.StartsWithDirectorySeparator(path4));
            if (needsThirdSeparator)
                neededSeparators++;

            int charsNeeded = path1.Length + path2.Length + path3.Length + path4.Length + neededSeparators;
            if (destination.Length < charsNeeded)
                return false;

            bool result = TryJoin(path1, path2, path3, destination, out charsWritten);
            Debug.Assert(result, "should never fail joining first three paths");

            if (needsThirdSeparator)
                destination[charsWritten++] = DirectorySeparatorChar;

            path4.CopyTo(destination.Slice(charsWritten));
            charsWritten += path4.Length;

            return true;
        }
    }
}
