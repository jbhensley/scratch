using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CoreConsoleApp1
{
    class IPEndPointParser
    {
        public const int MinPort = 0x00000000;
        public const int MaxPort = 0x0000FFFF;
        
        public static bool TryParse(ReadOnlySpan<char> endPointSpan, out IPEndPoint endPoint)
        {
            endPoint = null;
            IPAddress address;
            int sliceLength = endPointSpan.Length;  // If there's no port then send the entire string to the address parser

            int lastColonPos = endPointSpan.LastIndexOf(':');

            // Look to see if this is an IPv6 address with a port.
            if (lastColonPos > 0 && endPointSpan[lastColonPos - 1] == ']')
            {
                sliceLength = lastColonPos;
            }
            else if (lastColonPos > 0)
            {
                // This is either an IPv6 address without a port, or it's an IPv4 address with a port.
                // IFF it's the former, there will be at least one more semicolon.

                int secondToLastColonPos = -1;
                // Span does not have an overload for LastIndexOf that takes a starting position
                for (int i = lastColonPos - 1; i >= 0; i--)
                {
                    if (endPointSpan[i] == ':')
                    {
                        secondToLastColonPos = i;
                        break;
                    }
                }
                if (secondToLastColonPos == -1)
                {
                    sliceLength = lastColonPos;
                }
            }

            if (IPAddress.TryParse(endPointSpan.Slice(0, sliceLength), out address))
            {
                int port = 0;
                if (sliceLength < endPointSpan.Length &&
                    !int.TryParse(endPointSpan.Slice(sliceLength + 1), out port))
                {
                    return false;
                }

                if(port < MinPort || port > MaxPort)
                {
                    return false;
                }
                
                endPoint = new IPEndPoint(address, port);
                return true;
            }
            return false;
        }

        public static IPEndPoint Parse(string endPointString)
        {
            if (endPointString == null)  // Avoid null ref exception on endPointString.AsSpan()
            {
                throw new ArgumentNullException(nameof(endPointString));
            }

            return Parse(endPointString.AsSpan());
        }

        public static IPEndPoint Parse(ReadOnlySpan<char> endPointSpan)
        {
            if (TryParse(endPointSpan, out IPEndPoint result))
            {
                return result;
            }

            throw new FormatException();
        }
    }
}