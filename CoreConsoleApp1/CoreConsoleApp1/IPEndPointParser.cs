using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CoreConsoleApp1
{
    class IPEndPointParser
    {
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
                    else if (encounteredInvalidPortCharacter)
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

        public static bool TryParse2(ReadOnlySpan<char> endPointSpan, out IPEndPoint endPoint)
        {
            endPoint = null;
            int port;
            IPAddress address;

            // Look for the last colon.  If we don't find one, it's at best an IPv4 address without a port.
            // We can also simplify later cases by handling the case where we find a colon at the 0th
            // place, in which case there's either an invalid address or this is an IPv6 address without
            // a port, and we can just parse this as an IPAddress as well.
            int lastColonPos = endPointSpan.LastIndexOf(':');
            if (lastColonPos <= 1)
            {
                if (IPAddress.TryParse(endPointSpan, out address))
                {
                    endPoint = new IPEndPoint(address, 0);
                    return true;
                }
                return false;
            }

            // Look to see if this is an IPv6 address with a port.
            if (endPointSpan[lastColonPos - 1] == ']')
            {
                if (IPAddress.TryParse(endPointSpan.Slice(0, lastColonPos), out address) &&
                    int.TryParse(endPointSpan.Slice(lastColonPos + 1), out port))
                {
                    endPoint = new IPEndPoint(address, port);
                    return true;
                }
                return false;
            }


            // This is either an IPv6 address without a port, or it's an IPv4 address with a port.
            // IFF it's the former, there will be at least one more semicolon.
            int secondToLastColonPos = -1;
            // Span does not have an overload for LastIndexOf that takes a starting position
            for (int i = lastColonPos - 1; i <= 0; i--)
            {
                if(endPointSpan[i] == ':')
                {
                    secondToLastColonPos = i;
                    break;
                }
            }
            if (secondToLastColonPos == -1)
            {
                if (IPAddress.TryParse(endPointSpan.Slice(0, lastColonPos), out address) &&
                    int.TryParse(endPointSpan.Slice(lastColonPos + 1), out port))
                {
                    endPoint = new IPEndPoint(address, port);
                    return true;
                }
                return false;
            }


            if (IPAddress.TryParse(endPointSpan, out address))
            {
                endPoint = new IPEndPoint(address, 0);
                return true;
            }
            return false;
        }

        public static bool TryParse3(ReadOnlySpan<char> endPointSpan, out IPEndPoint endPoint)
        {
            endPoint = null;
            int port;
            IPAddress address;
            int sliceLength = endPointSpan.Length;

            // Look for the last colon.  If we don't find one, it's at best an IPv4 address without a port.
            // We can also simplify later cases by handling the case where we find a colon at the 0th
            // place, in which case there's either an invalid address or this is an IPv6 address without
            // a port, and we can just parse this as an IPAddress as well.
            int lastColonPos = endPointSpan.LastIndexOf(':');
            if (lastColonPos <= 1)
            {
                if (IPAddress.TryParse(endPointSpan, out address))
                {
                    endPoint = new IPEndPoint(address, 0);
                    return true;
                }
                return false;
            }

            // Look to see if this is an IPv6 address with a port.
            if (endPointSpan[lastColonPos - 1] == ']')
            {
                if (IPAddress.TryParse(endPointSpan.Slice(0, lastColonPos), out address) &&
                    int.TryParse(endPointSpan.Slice(lastColonPos + 1), out port))
                {
                    endPoint = new IPEndPoint(address, port);
                    return true;
                }
                return false;
            }


            // This is either an IPv6 address without a port, or it's an IPv4 address with a port.
            // IFF it's the former, there will be at least one more semicolon.
            int secondToLastColonPos = -1;
            // Span does not have an overload for LastIndexOf that takes a starting position
            for (int i = lastColonPos - 1; i <= 0; i--)
            {
                if (endPointSpan[i] == ':')
                {
                    secondToLastColonPos = i;
                    break;
                }
            }
            if (secondToLastColonPos == -1)
            {
                if (IPAddress.TryParse(endPointSpan.Slice(0, lastColonPos), out address) &&
                    int.TryParse(endPointSpan.Slice(lastColonPos + 1), out port))
                {
                    endPoint = new IPEndPoint(address, port);
                    return true;
                }
                return false;
            }


            if (IPAddress.TryParse(endPointSpan, out address))
            {
                endPoint = new IPEndPoint(address, 0);
                return true;
            }
            return false;
        }
    }
}