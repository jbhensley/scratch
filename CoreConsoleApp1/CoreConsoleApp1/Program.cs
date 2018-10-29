using System;
using System.Security.Principal;
using System.Diagnostics.Tracing;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Diagnostics.Tracing;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;

namespace CoreConsoleApp1
{
    class Program
    {
        [DllImport("kernel32.dll", EntryPoint = "MoveFileExW", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false)]
        private static extern bool MoveFileExPrivate(string src, string dst, uint flags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        internal static extern bool MoveFile(string src, string dst);
        static void Main(string[] args)
        {
            var if1 = typeof(ClassWithTwoSameNameInterfaces).GetInterface("Interface1", ignoreCase: true);
            Console.WriteLine(if1.Name);
            var if2 = typeof(ClassWithTwoSameNameInterfaces).GetInterface("System.Tests.Inner.Interface1", ignoreCase: true);
            Console.WriteLine(if2.Name);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static void TestFileMove(bool useEx)
        {
            var action = useEx 
                ? new Action<string, string>((src, dst) => MoveFileExPrivate(src, dst, 1)) 
                : new Action<string, string>((src, dst) => MoveFile(src, dst));

            var bytes = new byte[1024 * 1024 * 200];

            var files = new string[4];
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = System.IO.Path.GetTempFileName();
                File.WriteAllBytes(files[i], bytes);
                Console.WriteLine(files[i]);
            }

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Pass #{i + 1}...");

                action(files[0], files[3]);
                action(files[1], files[3]);
                action(files[2], files[3]);
                for (int j = 0; j < 3; j++)
                {
                    File.Copy(files[3], files[j]);
                }
            }
        }

        private static void TestHttpListener()
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnListener();
            }
        }

        private static void SpawnListener()
        {
            var listener = new HttpListener();
            var port = GetFreeTcpPort();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            listener.Start();

            Console.WriteLine($"Listening on port {port}...");
            listener.BeginGetContext(AsyncCallback, port);
        }

        private static void AsyncCallback(IAsyncResult result)
        {
            //var listenerAsyncResult = (ListenerAsyncResult)result;
            Console.WriteLine($"Received result from port {result.AsyncState}. Complete: {result.IsCompleted}");
        }

        static int GetFreeTcpPort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        static void TestPathJoin()
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
