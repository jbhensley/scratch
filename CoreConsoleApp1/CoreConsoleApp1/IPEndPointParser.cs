using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CoreConsoleApp1
{
    class IPEndPointParser
    {
        private const char AddressPortDelimiter = ':';

        public static void Parse(ReadOnlySpan<char> epSpan)
        {
            if (epSpan != null)
            {
                int port = 0;
                int multiplier = 1;
                for (int i = epSpan.Length - 1; i >= 0; i--)
                {
                    char digit = epSpan[i];
                    if(digit == AddressPortDelimiter)
                    {
                        // We've reached the end.
                        Console.WriteLine($"Port: {port}");
                    }
                    else if ('0' <= digit && digit <= '9')
                    {
                        port += multiplier * (digit - '0');
                        multiplier *= 10;
                    }
                    else
                    {
                        break;  // Invalid digit
                    }
                }
            }

            //return null;
        }
    }
}
