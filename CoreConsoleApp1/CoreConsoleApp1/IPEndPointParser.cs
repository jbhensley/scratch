using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CoreConsoleApp1
{
    class IPEndPointParser
    {
        private const char AddressPortDelimiter = ':';
        public const int MaxPort = 0x0000FFFF;

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
                        Console.WriteLine(epSpan.Slice(0, i).ToString());
                    }
                    else if ('0' <= digit && digit <= '9')
                    {
                        port += multiplier * (digit - '0');
                        if(port > MaxPort)
                        {
                            // If we've already exceeded the max port value then bail. No point going until we (potentially) hit an overflow
                            break;
                        }
                        multiplier *= 10;
                    }
                    else
                    {
                        break;  // Invalid digit
                    }
                }
            }

            // If the loop runs out without hitting a delimiter then there's no IP Address portion

            //return null;
        }
    }
}
