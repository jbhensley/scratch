using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace CoreConsoleApp1
{
    public enum TestEnum
    {
        val1, val2, val3
    }

    public sealed class TestEventSource : EventSource
    {
        [Event(1)]
        public void Method1(int? nullableIntArg, int intArg, byte[] byteArrayArg, TestEnum? enumArg) { WriteEvent(1, nullableIntArg, intArg, byteArrayArg, enumArg); }
    }
}