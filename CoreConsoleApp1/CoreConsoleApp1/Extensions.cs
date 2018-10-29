using System;
using System.Collections.Generic;
using System.Text;

namespace CoreConsoleApp1
{
    public static class Extensions
    {
        public static bool IsEnum(this Type type) { return type.IsEnum; }
    }
}
