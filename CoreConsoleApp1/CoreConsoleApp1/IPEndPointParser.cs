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

        public static bool TryParse(ReadOnlySpan<char> epSpan, out IPEndPoint endPoint)
        {
            // TODO: We could put some kind of easy fail here. For example, the max IPv6 address with max port length is 47 characters:
            //
            //          [ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff]:65535
            //
            //       However, IPv6 addresses can have an arbitarily named zone:
            //
            //          fe80::1ff:fe23:4567:890a%eth2
            //
            //       We'll punt on this idea for now.

            endPoint = null;
            if (epSpan != null)
            {
                // Start at the end of the string and work backward, looking for a port delimiter
                int port = 0;
                int multiplier = 1;
                int sliceLength = epSpan.Length;
                bool encounteredInvalidPortCharacter = false;
                for (int i = epSpan.Length - 1; i >= 0; i--)
                {
                    char digit = epSpan[i];

                    // Locating a delimiter ends the sequence. We either have IPv4 with a port or IPv6 (with or without) or we have garbage
                    if (digit == ':')
                    {
                        // IPv6 with a port
                        if ((i > 0 && epSpan[i - 1] == ']'))
                        {   
                            sliceLength = i;
                        }
                        else
                        {
                            /*     Now the hard work. This is either IPv4 with a port or IPv6 without a port. The existing IP parser takes the easy way out
                            *      by not supporting port numbers in IPv4 (it strips them out of IPv6), which means it can just assume that anything
                            *      with a colon is IPv6. We cannot do that. We have to determine whether to pass the entire span (i.e. this is
                            *      IPv6 without a port) or only a slice of the span (i.e. this is IPv4 with a port) to the address parser.
                            *      
                            *      Generally, we should be able to check positions 1 through 3 (zero-based) for a "dot".
                            *          .x..        (invalid leading dot - garbage input)
                            *          x.x.x.x     (dot a pos 1)
                            *          xx.x.x.x    (dot a pos 2)
                            *          xxx.x.x.x   (dot a pos 3)
                            *          
                            *      However, this fails for certain 6/4 interop addresses:
                            *          ::x.x.x.x   (dot a pos 3 - false positive)
                            *      
                            *      We should be able to look for a leading colon in this case. Be aware that not all interop addresses
                            *      begin with a colon:
                            *      
                            *          1::1.0.0.0  (valid)
                            *  
                            *      But the first position for a dot to appear in above is pos 4
                            */
                            
                            // Is this IPv4?
                            if (epSpan[0] != ':' && epSpan.Length > 4)  // Condition most likely to be false first. We hit a colon already, so we know length is at least 1
                            {
                                for (int j = 1; j < 4; j++) // Skip position zero. If a dot shows up there, this thing is invalid anyway
                                {
                                    if (epSpan[j] == '.')
                                    {
                                        sliceLength = i;    // IPv4 with a port
                                        break;
                                    }
                                }
                            }

                            // If the above check fails, this is IPv6 without a port and we pass the entire span to the address parser.
                            // sliceLength is set to the entire length by default, so there's nothing to do.
                            // Also, IPv4 without a port will run through the entire loop and exit without hiting a colon, so we're
                            // good there too.
                        }

                        break;
                    }
                    else if ('0' <= digit && digit <= '9')
                    {
                        // We'll avoid overflow in cases where someone passes in garbage
                        if (port < MaxPort)
                        {
                            port += multiplier * (digit - '0');
                            multiplier *= 10;
                        }
                    }
                    else
                    {
                        // If we see anything other than a numeric digit while processing the port number then we'll note that down for later.
                        // However, we cannot return false here because we do not actually know that there is a port. We won't know that
                        // until later. What we want to avoid is only processing the numeric digits of something like "192.168.0.1:HahaImNotValue37"
                        // and returning 192.168.0.1 on port 37 while also avoiding returning false for "192.168.0.1" because we encountered
                        // the last dot.
                        encounteredInvalidPortCharacter = true;
                    }

                    // The tendency here is to bail on the loop when we see a dot under the assumption that this is IPv4 without a port.
                    // That's a bad idea since "::x.x.x.x" is a valid IPv6 address. Leaving this here because the point is subtle.
                    //else if (digit == '.')
                    //{

                    //}
                }

                // We've either hit the delimiter or ran out of characters. Let's see what the IP parser thinks.
                // TODO: this can likey be optimized in core since we already know what kind of address we have
                if (IPAddress.TryParse(epSpan.Slice(0, sliceLength), out IPAddress address))
                {
                    // If the slice length is the entire span then we do not have a port
                    if (sliceLength == epSpan.Length)
                    {
                        port = 0;       // Reset the port calculation
                    }
                    else if(encounteredInvalidPortCharacter)
                    {
                        return false;   // Now that we know there is a port, it's valid to error upon having seen non-numeric data during port processing
                    }

                    // Avoid tossing on invalid port
                    if (port <= MaxPort)
                    {
                        endPoint = new IPEndPoint(address, port);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryParse2(ReadOnlySpan<char> epSpan, out IPEndPoint endPoint)
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
                    if (digit == ':')
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
                            if (port > MaxPort)
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
                    else if (digit == '.')
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
                    else if (!('a' <= digit && digit <= 'f') && !('A' <= digit && digit <= 'F'))
                    {
                        break;
                    }
                }

                // Let's see what the IP parser thinks
                if (IPAddress.TryParse(delimiterLocation == -1 ? epSpan : epSpan.Slice(0, delimiterLocation), out address))
                {
                    endPoint = new IPEndPoint(address, port);
                    return true;
                }
            }

            return false;
        }

        public static bool TryParseTratcher(string addressWithPort, out IPEndPoint endpoint)
        {
            string addressPart = null;
            string portPart = null;
            IPAddress address;
            endpoint = null;

            if (string.IsNullOrEmpty(addressWithPort))
            {
                return false;
            }

            var lastColonIndex = addressWithPort.LastIndexOf(':');
            if (lastColonIndex > 0)
            {
                // IPv4 with port or IPv6
                var closingIndex = addressWithPort.LastIndexOf(']');
                if (closingIndex > 0)
                {
                    // IPv6 with brackets
                    addressPart = addressWithPort.Substring(1, closingIndex - 1);
                    if (closingIndex < lastColonIndex)
                    {
                        // IPv6 with port [::1]:80
                        portPart = addressWithPort.Substring(lastColonIndex + 1);
                    }
                }
                else
                {
                    // IPv6 without port or IPv4
                    var firstColonIndex = addressWithPort.IndexOf(':');
                    if (firstColonIndex != lastColonIndex)
                    {
                        // IPv6 ::1
                        addressPart = addressWithPort;
                    }
                    else
                    {
                        // IPv4 with port 127.0.0.1:123
                        addressPart = addressWithPort.Substring(0, firstColonIndex);
                        portPart = addressWithPort.Substring(firstColonIndex + 1);
                    }
                }
            }
            else
            {
                // IPv4 without port
                addressPart = addressWithPort;
            }

            if (IPAddress.TryParse(addressPart, out address))
            {
                if (portPart != null)
                {
                    int port;
                    if (int.TryParse(portPart, out port))
                    {
                        endpoint = new IPEndPoint(address, port);
                        return true;
                    }
                    return false;
                }
                endpoint = new IPEndPoint(address, 0);
                return true;
            }
            return false;
        }
    }
}