using System;

namespace CoreConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //var epSpan = "157.59.0x25.033".AsSpan();
            //System.Net.IPEndPoint endPoint;
            //if (IPEndPointParser.TryParse3(epSpan, out endPoint))
            //{
            //    Console.WriteLine(endPoint.Address.ToString());
            //    Console.WriteLine(endPoint.Port);
            //}
            //else
            //{
            //    Console.WriteLine("Not valid");
            //}

            var tester = new IPEndPointParsing();
            tester.Parse_InvalidPort_IPv4_Throws("0", "0.0.0.0");

            //if (IPEndPointParser.TryParseTratcher("[3731:54:65fe:2::1]IAmNotValid65535", out System.Net.IPEndPoint endPoint))
            //{
            //    Console.WriteLine(endPoint.Address.ToString());
            //    Console.WriteLine(endPoint.Port);
            //}
            //else
            //{
            //    Console.WriteLine("Not valid");
            //}


            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        private static void TestPathJoin()
        {
            var path1 = "c:";
            var path2 = "temp";
            var path3 = "sub1";
            var path4 = "sub2";

            var outputBuffer = new char[5000];
            //var outputBuffer = new char[ path1.Length + path2.Length + path3.Length + path4.Length];
            if (Path.TryJoin(path1, path2, path3, path4, new Span<char>(outputBuffer), out int charsWritten))
            {
                Console.WriteLine($"Path joined as '{new string(outputBuffer, 0, charsWritten)}'");
            }
            else
                Console.WriteLine("Join failed");
        }
    }
}
