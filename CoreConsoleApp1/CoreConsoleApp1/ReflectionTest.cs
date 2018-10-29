using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace CoreConsoleApp1
{
    public interface Interface1 { }

    public partial class TypeTests
    {
        [Fact]
        public void GetInterface_SameNameInterfaces_ThrowsAmbiguousMatchException()
        {
            Assert.Throws<AmbiguousMatchException>(() => typeof(ClassWithTwoSameNameInterfaces).GetInterface("Interface1", ignoreCase: true));
            Assert.Throws<NullReferenceException>(() => typeof(ClassWithTwoSameNameInterfaces).GetInterface("System.Tests.Inner.Interface1", ignoreCase: true));
        }
    }

    public class ClassWithTwoSameNameInterfaces : Interface1, Inner.Interface1 { }

    namespace Inner
    {
        public interface Interface1 { }
    }
}