using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CoreConsoleApp1
{
    class IPEndPointParser
    {
        //private const char AddressPortDelimiter = ':';
        public const int MaxPort = 0x0000FFFF;

        public static bool TryParse2(ReadOnlySpan<char> epSpan, out IPEndPoint endPoint)
        {
            endPoint = null;
            if (epSpan != null)
            {
                // Determine whether to process as IPv4 or IPv6
                bool processAsIPv4 = false;
                if (epSpan.Length > 4)
                {
                    // Check to see if this might be IPv4
                    for (int i = 1; i < 4; i++) // Skip position zero. If a dot shows up there, this thing is invalid anyway
                    {
                        if (epSpan[i] == '.')
                        {
                            processAsIPv4 = true;
                            break;
                        }
                    }
                }

                // Start at the end of the string and work backward, looking for a port delimiter
                int portDecimal = 0;
                int multiplier = 1;
                // TODO: we need to process cases where the port is expressed as Hex
                int sliceLength = epSpan.Length;
                for (int i = epSpan.Length - 1; i >= 0; i--)
                {
                    char digit = epSpan[i];

                    // Locating a delimiter ends the sequence. We either have IPv4 with a port or IPv6 (with or without) or we have garbage
                    if (digit == ':')
                    {
                        // Determine how far to slice into the span (pre-set to entire length)

                        // For IPv4, the address portion resides directly to the left of ':'
                        if (processAsIPv4)
                        {
                            sliceLength = i;
                        }
                        // IPv6 with "]:" port sequence
                        else if (i > 0 && epSpan[i - 1] == ']')
                        {
                            sliceLength = i - 1;
                        }

                        break;
                    }
                    else if ('0' <= digit && digit <= '9')
                    {
                        // We'll avoid overflow in cases where someone passes in garbage
                        if (portDecimal < MaxPort)
                        {
                            portDecimal += multiplier * (digit - '0');
                            multiplier *= 10;
                        }
                    }
                }

                // Let's see what the IP parser thinks.
                // TODO: this can likey be optimized in core since we already know what kind of address we have
                if (IPAddress.TryParse(epSpan.Slice(0, sliceLength), out IPAddress address))
                {
                    endPoint = new IPEndPoint(address, portDecimal);
                    return true;
                }
            }

            return false;
        }

        public static bool TryParse(ReadOnlySpan<char> epSpan, out IPEndPoint endPoint)
        {
            endPoint = null;

            if (epSpan != null)
            {
                IPAddress address;
                int port = 0;
                int multiplier = 1;
                int delimiterLocation = -1;

                // We'll start at the end of the string, looking for a port number or any indication that there is not one
                for (int i = epSpan.Length - 1; i >= 0; i--)
                {
                    char digit = epSpan[i];
                    if(digit == ':')
                    {
                        if (delimiterLocation > -1)
                        {
                            // This block could be combined with the one below, but would require repeated checking of
                            // encounteredPortDelimiter to determine which condition we are in.

                            // We've encounterd two colons without seeing a "]:" sequence. This can only be an IPv6 address
                            // without a port (or garbage). Note that this should work for even the shortest possible IPv6
                            // address of "::"

                            // TODO: This can probably be optimized in core since we already know which version IP address this is
                            if (IPAddress.TryParse(epSpan, out address))
                            {
                                endPoint = new IPEndPoint(address, 0);
                                return true;
                            }

                            // Garbage input
                            return false;
                        }

                        // Question: what happens when just "]:" is passed or "[2001:db8::1]:"?
                        if (i > 1 && epSpan[i - 1] == ']') // i > 1 because we slice i - 2 below (can't slice a negative length)
                        {
                            // Port is calculated as we go, so we should have a valid one by now. Otherwise fail.
                            if(port > MaxPort)
                            {
                                return false;
                            }

                            // TODO: This can probably be optimized in core since we already know which version IP address this is
                            if (IPAddress.TryParse(epSpan.Slice(0, i - 2), out address))
                            {
                                endPoint = new IPEndPoint(address, port);
                                return true;
                            }

                            // Garbage input
                            return false;
                        }

                        // Note that we've seen ':' without a preceding ']'. If we see another ':' then this is
                        // an IPv6 address with no port (or garbage)
                        delimiterLocation = i;
                    }
                    // We'll process digits until we see the port delimiter
                    else if ('0' <= digit && digit <= '9' && delimiterLocation == -1)
                    {
                        // We'll avoid overflow in cases where someone passes in garbage
                        if (port < MaxPort)
                        {
                            port += multiplier * (digit - '0');
                            multiplier *= 10;
                        }
                    }
                    else if(digit == '.')
                    {
                        // IPv4 (with or without a port)
                        if (IPAddress.TryParse(delimiterLocation == -1 ? epSpan : epSpan.Slice(0, delimiterLocation), out address))
                        {
                            endPoint = new IPEndPoint(address, delimiterLocation == -1 ? 0 : port);
                            return true;
                        }

                        return false;
                    }
                    // If this is a hex value (a through f or A through F) then ignore. Otherwise we've encountered garbage
                    else if(!('a' <= digit && digit <= 'f') && !('A' <= digit && digit <= 'F'))
                    {
                        break;
                    }
                }

                // Let's see what the IP parser thinks
                if(IPAddress.TryParse(delimiterLocation == -1 ? epSpan : epSpan.Slice(0, delimiterLocation), out address))
                {
                    endPoint = new IPEndPoint(address, port);
                    return true;
                }
            }

            return false;
        }
    }
}
