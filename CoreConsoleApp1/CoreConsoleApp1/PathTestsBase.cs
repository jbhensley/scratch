﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CoreConsoleApp1
{
    public partial class PathTestsBase
    {
        protected static class PathAssert
        {
            public static void Equal(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual)
            {
                if (!actual.SequenceEqual(expected))
                    throw new Xunit.Sdk.EqualException(new string(expected), new string(actual));
            }

            public static void Empty(ReadOnlySpan<char> actual)
            {
                if (actual.Length > 0)
                    throw new Xunit.Sdk.NotEmptyException();
            }
        }
    }

    public partial class PathTestsBase //: RemoteExecutorTestBase
    {
        protected static string Sep = Path.DirectorySeparatorChar.ToString();
        protected static string AltSep = System.IO.Path.AltDirectorySeparatorChar.ToString();

        public static TheoryData<string> TestData_EmbeddedNull => new TheoryData<string>
        {
            "a\0b"
        };

        public static TheoryData<string> TestData_EmptyString => new TheoryData<string>
        {
            ""
        };

        public static TheoryData<string> TestData_ControlChars => new TheoryData<string>
        {
            "\t",
            "\r\n",
            "\b",
            "\v",
            "\n"
        };

        public static TheoryData<string> TestData_NonDriveColonPaths => new TheoryData<string>
        {
            @"bad:path",
            @"C:\some\bad:path",
            @"http://www.microsoft.com",
            @"file://www.microsoft.com",
            @"bad::$DATA",
            @"C  :",
            @"C  :\somedir"
        };

        public static TheoryData<string> TestData_Spaces => new TheoryData<string>
        {
            " ",
            "   "
        };

        public static TheoryData<string> TestData_Periods => new TheoryData<string>
        {
            // One and two periods have special meaning (current and parent dir)
            "...",
            "...."
        };

        public static TheoryData<string> TestData_Wildcards => new TheoryData<string>
        {
            "*",
            "?"
        };

        public static TheoryData<string> TestData_ExtendedWildcards => new TheoryData<string>
        {
            // These are supported by Windows although .NET blocked them historically
            "\"",
            "<",
            ">"
        };

        public static TheoryData<string> TestData_UnicodeWhiteSpace => new TheoryData<string>
        {
            "\u00A0", // Non-breaking Space
            "\u2028", // Line separator
            "\u2029", // Paragraph separator
        };

        public static TheoryData<string> TestData_InvalidUnc => new TheoryData<string>
        {
            // .NET used to validate properly formed UNCs
            @"\\",
            @"\\LOCALHOST",
            @"\\LOCALHOST\",
            @"\\LOCALHOST\\",
            @"\\LOCALHOST\.."
        };

        public static TheoryData<string> TestData_InvalidDriveLetters => new TheoryData<string>
        {
            { @"@:\foo" },  // 064 = @     065 = A
            { @"[:\\" },    // 091 = [     090 = Z
            { @"`:\foo "},  // 096 = `     097 = a
            { @"{:\\" },    // 123 = {     122 = z
            { @"@:/foo" },
            { @"[://" },
            { @"`:/foo "},
            { @"{:/" },
            { @"]:" }
        };

        public static TheoryData<string> TestData_ValidDriveLetters => new TheoryData<string>
        {
            { @"A:\foo" },  // 064 = @     065 = A
            { @"Z:\\" },    // 091 = [     090 = Z
            { @"a:\foo "},  // 096 = `     097 = a
            { @"z:\\" },    // 123 = {     122 = z
            { @"B:/foo" },
            { @"D://" },
            { @"E:/foo "},
            { @"F:/" },
            { @"G:" }
        };

        public static TheoryData<string, string> TestData_GetDirectoryName => new TheoryData<string, string>
        {
            { ".", "" },
            { "..", "" },
            { "baz", "" },
            { System.IO.Path.Combine("dir", "baz"), "dir" },
            { "dir.foo" + System.IO.Path.AltDirectorySeparatorChar + "baz.txt", "dir.foo" },
            { System.IO.Path.Combine("dir", "baz", "bar"), System.IO.Path.Combine("dir", "baz") },
            { System.IO.Path.Combine("..", "..", "files.txt"), System.IO.Path.Combine("..", "..") },
            { Path.DirectorySeparatorChar + "foo", Path.DirectorySeparatorChar.ToString() },
            { Path.DirectorySeparatorChar.ToString(), null }
        };

        public static TheoryData<string, string> TestData_GetDirectoryName_Windows => new TheoryData<string, string>
        {
            { @"C:\", null },
            { @"C:/", null },
            { @"C:", null },
            { @"dir\\baz", "dir" },
            { @"dir//baz", "dir" },
            { @"C:\foo", @"C:\" },
            { @"C:foo", "C:" }
        };

        public static TheoryData<string, string> TestData_GetExtension => new TheoryData<string, string>
        {
            { @"file.exe", ".exe" },
            { @"file", "" },
            { @"file.", "" },
            { @"file.s", ".s" },
            { @"test/file", "" },
            { @"test/file.extension", ".extension" },
            { @"test\file", "" },
            { @"test\file.extension", ".extension" },
            { "file.e xe", ".e xe"},
            { "file. ", ". "},
            { " file. ", ". "},
            { " file.extension", ".extension"}
        };

        public static TheoryData<string, string> TestData_GetFileName => new TheoryData<string, string>
        {
            { ".", "." },
            { "..", ".." },
            { "file", "file" },
            { "file.", "file." },
            { "file.exe", "file.exe" },
            { " . ", " . " },
            { " .. ", " .. " },
            { "fi le", "fi le" },
            { System.IO.Path.Combine("baz", "file.exe"), "file.exe" },
            { System.IO.Path.Combine("baz", "file.exe") + System.IO.Path.AltDirectorySeparatorChar, "" },
            { System.IO.Path.Combine("bar", "baz", "file.exe"), "file.exe" },
            { System.IO.Path.Combine("bar", "baz", "file.exe") + Path.DirectorySeparatorChar, "" }
        };

        public static TheoryData<string, string> TestData_GetFileNameWithoutExtension => new TheoryData<string, string>
        {
            { "", "" },
            { "file", "file" },
            { "file.exe", "file" },
            { System.IO.Path.Combine("bar", "baz", "file.exe"), "file" },
            { System.IO.Path.Combine("bar", "baz") + Path.DirectorySeparatorChar, "" }
        };

        public static TheoryData<string, string> TestData_GetPathRoot_Unc => new TheoryData<string, string>
        {
            { @"\\test\unc\path\to\something", @"\\test\unc" },
            { @"\\a\b\c\d\e", @"\\a\b" },
            { @"\\a\b\", @"\\a\b" },
            { @"\\a\b", @"\\a\b" },
            { @"\\test\unc", @"\\test\unc" },
        };

        // TODO: Include \\.\ as well
        public static TheoryData<string, string> TestData_GetPathRoot_DevicePaths => new TheoryData<string, string>
        {
            { @"\\?\UNC\test\unc\path\to\something", PathFeatures.IsUsingLegacyPathNormalization() ? @"\\?\UNC" : @"\\?\UNC\test\unc" },
            { @"\\?\UNC\test\unc", PathFeatures.IsUsingLegacyPathNormalization() ? @"\\?\UNC" : @"\\?\UNC\test\unc" },
            { @"\\?\UNC\a\b1", PathFeatures.IsUsingLegacyPathNormalization() ? @"\\?\UNC" : @"\\?\UNC\a\b1" },
            { @"\\?\UNC\a\b2\", PathFeatures.IsUsingLegacyPathNormalization() ? @"\\?\UNC" : @"\\?\UNC\a\b2" },
            { @"\\?\C:\foo\bar.txt", PathFeatures.IsUsingLegacyPathNormalization() ? @"\\?\C:" : @"\\?\C:\" }
        };

        public static TheoryData<string, string> TestData_GetPathRoot_Windows => new TheoryData<string, string>
        {
            { @"C:", @"C:" },
            { @"C:\", @"C:\" },
            { @"C:\\", @"C:\" },
            { @"C:\foo1", @"C:\" },
            { @"C:\\foo2", @"C:\" },
        };

        protected static void GetTempPath_SetEnvVar(string envVar, string expected, string newTempPath)
        {
            string original = System.IO.Path.GetTempPath();
            Assert.NotNull(original);
            try
            {
                Environment.SetEnvironmentVariable(envVar, newTempPath);
                Assert.Equal(
                    System.IO.Path.GetFullPath(expected),
                    System.IO.Path.GetFullPath(System.IO.Path.GetTempPath()));
            }
            finally
            {
                Environment.SetEnvironmentVariable(envVar, original);
                Assert.Equal(original, System.IO.Path.GetTempPath());
            }
        }
    }
}