﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit;

namespace CoreConsoleApp1
{
    public class PathTests_Join : PathTestsBase
    {
        public static TheoryData<string, string, string> TestData_JoinTwoPaths = new TheoryData<string, string, string>
        {
            { "", "", "" },
            { Sep, "", Sep },
            { AltSep, "", AltSep },
            { "", Sep, Sep },
            { "", AltSep, AltSep },
            { Sep, Sep, $"{Sep}{Sep}" },
            { AltSep, AltSep, $"{AltSep}{AltSep}" },
            { "a", "", "a" },
            { "", "a", "a" },
            { "a", "a", $"a{Sep}a" },
            { $"a{Sep}", "a", $"a{Sep}a" },
            { "a", $"{Sep}a", $"a{Sep}a" },
            { $"a{Sep}", $"{Sep}a", $"a{Sep}{Sep}a" },
            { "a", $"a{Sep}", $"a{Sep}a{Sep}" },
            { $"a{AltSep}", "a", $"a{AltSep}a" },
            { "a", $"{AltSep}a", $"a{AltSep}a" },
            { $"a{Sep}", $"{AltSep}a", $"a{Sep}{AltSep}a" },
            { $"a{AltSep}", $"{AltSep}a", $"a{AltSep}{AltSep}a" },
            { "a", $"a{AltSep}", $"a{Sep}a{AltSep}" },
            { null, null, ""},
            { null, "a", "a"},
            { "a", null, "a"}
        };

        [Theory, MemberData(nameof(TestData_JoinTwoPaths))]
        public void JoinTwoPaths(string path1, string path2, string expected)
        {
            Assert.Equal(expected, System.IO.Path.Join(path1.AsSpan(), path2.AsSpan()));
            Assert.Equal(expected, System.IO.Path.Join(path1, path2));
        }

        [Theory, MemberData(nameof(TestData_JoinTwoPaths))]
        public void TryJoinTwoPaths(string path1, string path2, string expected)
        {
            char[] output = new char[expected.Length];

            Assert.True(Path.TryJoin(path1, path2, output, out int written));
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, new string(output));

            if (expected.Length > 0)
            {
                Assert.False(Path.TryJoin(path1, path2, Span<char>.Empty, out written));
                Assert.Equal(0, written);

                output = new char[expected.Length - 1];
                Assert.False(Path.TryJoin(path1, path2, output, out written));
                Assert.Equal(0, written);
                Assert.Equal(output, new char[output.Length]);
            }
        }

        public static TheoryData<string, string, string, string> TestData_JoinThreePaths = new TheoryData<string, string, string, string>
        {
            { "", "", "", "" },
            { Sep, Sep, Sep, $"{Sep}{Sep}{Sep}" },
            { AltSep, AltSep, AltSep, $"{AltSep}{AltSep}{AltSep}" },
            { "a", "", "", "a" },
            { "", "a", "", "a" },
            { "", "", "a", "a" },
            { "a", "", "a", $"a{Sep}a" },
            { "a", "a", "", $"a{Sep}a" },
            { "", "a", "a", $"a{Sep}a" },
            { "a", "a", "a", $"a{Sep}a{Sep}a" },
            { "a", Sep, "a", $"a{Sep}a" },
            { $"a{Sep}", "", "a", $"a{Sep}a" },
            { $"a{Sep}", "a", "", $"a{Sep}a" },
            { "", $"a{Sep}", "a", $"a{Sep}a" },
            { "a", "", $"{Sep}a", $"a{Sep}a" },
            { $"a{AltSep}", "", "a", $"a{AltSep}a" },
            { $"a{AltSep}", "a", "", $"a{AltSep}a" },
            { "", $"a{AltSep}", "a", $"a{AltSep}a" },
            { "a", "", $"{AltSep}a", $"a{AltSep}a" },
            { null, null, null, "" },
            { "a", null, null, "a" },
            { null, "a", null, "a" },
            { null, null, "a", "a" },
            { "a", null, "a", $"a{Sep}a" }
        };

        [Theory, MemberData(nameof(TestData_JoinThreePaths))]
        public void JoinThreePaths(string path1, string path2, string path3, string expected)
        {
            Assert.Equal(expected, System.IO.Path.Join(path1.AsSpan(), path2.AsSpan(), path3.AsSpan()));
            Assert.Equal(expected, System.IO.Path.Join(path1, path2, path3));
        }

        [Theory, MemberData(nameof(TestData_JoinThreePaths))]
        public void TryJoinThreePaths(string path1, string path2, string path3, string expected)
        {
            char[] output = new char[expected.Length];

            Assert.True(Path.TryJoin(path1, path2, path3, output, out int written));
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, new string(output));

            if (expected.Length > 0)
            {
                Assert.False(Path.TryJoin(path1, path2, path3, Span<char>.Empty, out written));
                Assert.Equal(0, written);

                output = new char[expected.Length - 1];
                Assert.False(Path.TryJoin(path1, path2, path3, output, out written));
                Assert.Equal(0, written);
                Assert.Equal(output, new char[output.Length]);
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  Additional unit tests for Join of four
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static TheoryData<string, string, string, string, string> TestData_JoinFourPaths = new TheoryData<string, string, string, string, string>
        {
            { "", "", "", "", "" },
            { Sep, Sep, Sep, Sep, $"{Sep}{Sep}{Sep}{Sep}" },
            { AltSep, AltSep, AltSep, AltSep, $"{AltSep}{AltSep}{AltSep}{AltSep}" },
            //
            { "a", "", "", "", "a" },
            { "", "a", "", "", "a" },
            { "", "", "a", "", "a" },
            { "", "", "", "a", "a" },
            //
            { "a", "a", "", "", $"a{Sep}a" },
            { "a", "", "a", "", $"a{Sep}a" },
            { "a", "", "", "a", $"a{Sep}a" },
            { "", "a", "a", "", $"a{Sep}a" },
            { "", "a", "", "a", $"a{Sep}a" },
            { "", "", "a", "a", $"a{Sep}a" },
            //
            { "a", "a", "a", "", $"a{Sep}a{Sep}a" },
            { "a", "a", "", "a", $"a{Sep}a{Sep}a" },
            { "a", "", "a", "a", $"a{Sep}a{Sep}a" },
            { "", "a", "a", "a", $"a{Sep}a{Sep}a" },
            //
            { "a", "a", "a", "a", $"a{Sep}a{Sep}a{Sep}a" },
            //
            { "a", Sep, "a", "", $"a{Sep}a" },
            { "a", Sep, "", "a", $"a{Sep}a" },
            { "a", "", Sep, "a", $"a{Sep}a" },
            { "", "a", Sep, "a", $"a{Sep}a" },
            //
            { $"a{Sep}", "a", "", "", $"a{Sep}a" },
            { $"a{Sep}", "", "a", "", $"a{Sep}a" },
            { $"a{Sep}", "", "", "a", $"a{Sep}a" },
            { "", $"a{Sep}", "a", "", $"a{Sep}a" },
            { "", $"a{Sep}", "", "a", $"a{Sep}a" },
            { "", "", $"a{Sep}", "a", $"a{Sep}a" },
            //
            { "a", "", $"{Sep}a", "", $"a{Sep}a" },
            { "a", "", "", $"{Sep}a", $"a{Sep}a" },
            { "", "a", $"{Sep}a", "", $"a{Sep}a" },
            { "", "a", "", $"{Sep}a", $"a{Sep}a" },
            //
            { $"a{AltSep}", "a", "", "", $"a{AltSep}a" },
            { $"a{AltSep}", "", "a", "", $"a{AltSep}a" },
            { $"a{AltSep}", "", "", "a", $"a{AltSep}a" },
            { "", $"a{AltSep}", "a", "", $"a{AltSep}a" },
            { "", $"a{AltSep}", "", "a", $"a{AltSep}a" },
            { "", "", $"a{AltSep}", "a", $"a{AltSep}a" },
            //
            { "a", "", $"{AltSep}a", "", $"a{AltSep}a" },
            { "a", "", "", $"{AltSep}a", $"a{AltSep}a" },
            //
            { null, null, null, null, "" },
            { "a", null, null, null, "a" },
            { null, "a", null, null, "a" },
            { null, null, "a", null, "a" },
            { null, null, null, "a", "a" },
            //
            { "a", "a", null, null, $"a{Sep}a" },
            { "a", null, "a", null, $"a{Sep}a" },
            { "a", null, null, "a", $"a{Sep}a" },
            { null, "a", "a", null, $"a{Sep}a" },
            { null, "a", null, "a", $"a{Sep}a" },
            { null, null, "a", "a", $"a{Sep}a" }
        };

        [Theory, MemberData(nameof(TestData_JoinFourPaths))]
        public void JoinFourPaths(string path1, string path2, string path3, string path4, string expected)
        {
            Assert.Equal(expected, Path.Join(path1.AsSpan(), path2.AsSpan(), path3.AsSpan(), path4.AsSpan()));
            Assert.Equal(expected, Path.Join(path1, path2, path3, path4));
        }

        [Theory, MemberData(nameof(TestData_JoinFourPaths))]
        public void TryJoinFourPaths(string path1, string path2, string path3, string path4, string expected)
        {
            char[] output = new char[expected.Length];

            Assert.True(Path.TryJoin(path1, path2, path3, path4, output, out int written));
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, new string(output));

            if (expected.Length > 0)
            {
                Assert.False(Path.TryJoin(path1, path2, path3, path4, Span<char>.Empty, out written));
                Assert.Equal(0, written);

                output = new char[expected.Length - 1];
                Assert.False(Path.TryJoin(path1, path2, path3, path4, output, out written));
                Assert.Equal(0, written);
                Assert.Equal(output, new char[output.Length]);
            }
        }
    }
}
