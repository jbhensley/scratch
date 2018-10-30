using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var type = typeof(ClassWithTwoSameNameInterfaces).GetInterface("System.Tests.Inner.Interface1", ignoreCase: true);
            //var type = typeof(Interface1);

            Console.WriteLine($"{type.Namespace}.{type.Name}");
            Console.ReadLine();
        }
    }

    namespace Inner
    {
        public interface Interface1 { }
    }
    

    public class ClassWithTwoSameNameInterfaces : Interface1, Inner.Interface1 { }

    
}

public interface Interface1 { }

