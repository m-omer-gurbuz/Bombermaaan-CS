// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2008 Markus Drescher
    Copyright (C) 2026 Ömer Gürbüz

    This file is part of Bombermaaan.

    Bombermaaan is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Bombermaaan is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Bombermaaan.  If not, see <http://www.gnu.org/licenses/>.

************************************************************************************/


/**
 *  \file CConsole.cs
 *  \brief The console (helpful during development)
 */

using System;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    // Flags to use to specify the text color.
    // Example:
    // CONSOLE_FOREGROUND_RED | CONSOLE_FOREGROUND_BLUE | CONSOLE_FOREGROUND_GREEN |
    // CONSOLE_BACKGROUND_INTENSITY | CONSOLE_BACKGROUND_RED
    // Grey foreground and light red background.
    //
    // These values mirror the Windows FOREGROUND_*/BACKGROUND_* constants so that
    // existing call-sites compile unchanged.
    [Flags]
    public enum ConsoleColorFlags : ushort
    {
        CONSOLE_FOREGROUND_RED       = 0x0004,
        CONSOLE_FOREGROUND_GREEN     = 0x0002,
        CONSOLE_FOREGROUND_BLUE      = 0x0001,
        CONSOLE_FOREGROUND_INTENSITY = 0x0008,
        CONSOLE_BACKGROUND_RED       = 0x0040,
        CONSOLE_BACKGROUND_GREEN     = 0x0020,
        CONSOLE_BACKGROUND_BLUE      = 0x0010,
        CONSOLE_BACKGROUND_INTENSITY = 0x0080
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// CConsole is a class which provides a console window besides the main game window.
    /// </summary>
    public class CConsole
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        //! Display a dot on the console every X repeated messages
        private const int REPEATED_MESSAGES_LIMIT = 300;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private bool               m_Open;                         //!< Is the console opened?
        private ushort             m_Color;                        //!< Current text color (background/foreground)
        private string             m_Message;                      //!< Last message written to the console
        private int                m_NumberOfRepeatedMessages;     //!< How many consecutive identical messages have been sent?
        private bool               m_FilterRepeatedMessage;        //!< Should we manage message repetition by not displaying all consecutive identical messages?

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CConsole()
        {
            // The console is not opened yet
            m_Open = false;

            // Default console text color : black background, grey foreground.
            m_Color = (ushort)(ConsoleColorFlags.CONSOLE_FOREGROUND_RED |
                               ConsoleColorFlags.CONSOLE_FOREGROUND_GREEN |
                               ConsoleColorFlags.CONSOLE_FOREGROUND_BLUE);

            // No message
            m_Message = string.Empty;
            m_NumberOfRepeatedMessages = 0;

            // Filter repeated messages by default
            m_FilterRepeatedMessage = true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        ~CConsole()
        {
            // If the console is opened
            if (m_Open)
            {
                // Close the console
                Close();
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Singleton instance
        private static readonly CConsole s_rConsole = new CConsole();

        /// <summary>Get the console singleton</summary>
        public static CConsole GetConsole()
        {
            // Return the console singleton
            return s_rConsole;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Open the console window</summary>
        public void Open()
        {
            // If the console is not opened yet
            if (!m_Open)
            {
                // In C# the console is always available via System.Console –
                // no platform-specific allocation needed.

                // The console window is now opened
                m_Open = true;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Close the console window</summary>
        public void Close()
        {
            // If the console is opened
            if (m_Open)
            {
                // The console window is not opened anymore
                m_Open = false;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Write a formatted string to the console</summary>
        public void Write(string pMessage, params object[] args)
        {
            // If the console is opened
            if (m_Open)
            {
                // Format the given string using the given arguments ("..." parameter)
                string message = (args != null && args.Length > 0)
                    ? string.Format(pMessage, args)
                    : pMessage;

                // If we have to filter repeated messages
                if (m_FilterRepeatedMessage)
                {
                    // If the last message written to the console is not the same
                    if (message != m_Message)
                    {
                        // Send the formatted string to the console output
                        Console.Write(message);

                        // Save the message
                        m_Message = message;

                        // Stop the chain of repeated messages (if there is one)
                        m_NumberOfRepeatedMessages = 0;
                    }
                    // If the last message written to the console is the same
                    else
                    {
                        // It's a repeated message
                        m_NumberOfRepeatedMessages++;

                        // Show that messages are being repeated, by writing a dot
                        // every REPEATED_MESSAGES_LIMIT repeated messages.
                        if ((m_NumberOfRepeatedMessages % REPEATED_MESSAGES_LIMIT) == 0)
                        {
                            Console.Write(".");
                        }
                    }
                }
                // If we don't have to filter repeated messages
                else
                {
                    // Send the formatted string to the console output
                    Console.Write(message);
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Returns whether the console window is opened</summary>
        public bool IsOpen()
        {
            return m_Open;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set the color to use when writing text to the console</summary>
        public void SetTextColor(ushort Color)
        {
            // Map the Windows-style color flags to System.Console foreground/background colors
            // where practical; otherwise the value is stored and ignored on non-Windows builds.
            m_Color = Color;

            // Attempt a simple mapping: if any foreground bits are set, apply a matching
            // ConsoleColor.  Full 16-color mapping is intentionally kept straightforward.
            bool fgRed   = (Color & (ushort)ConsoleColorFlags.CONSOLE_FOREGROUND_RED)       != 0;
            bool fgGreen = (Color & (ushort)ConsoleColorFlags.CONSOLE_FOREGROUND_GREEN)     != 0;
            bool fgBlue  = (Color & (ushort)ConsoleColorFlags.CONSOLE_FOREGROUND_BLUE)      != 0;
            bool fgHigh  = (Color & (ushort)ConsoleColorFlags.CONSOLE_FOREGROUND_INTENSITY) != 0;
            bool bgRed   = (Color & (ushort)ConsoleColorFlags.CONSOLE_BACKGROUND_RED)       != 0;
            bool bgGreen = (Color & (ushort)ConsoleColorFlags.CONSOLE_BACKGROUND_GREEN)     != 0;
            bool bgBlue  = (Color & (ushort)ConsoleColorFlags.CONSOLE_BACKGROUND_BLUE)      != 0;
            bool bgHigh  = (Color & (ushort)ConsoleColorFlags.CONSOLE_BACKGROUND_INTENSITY) != 0;

            Console.ForegroundColor = MapToConsoleColor(fgRed, fgGreen, fgBlue, fgHigh);
            Console.BackgroundColor = MapToConsoleColor(bgRed, bgGreen, bgBlue, bgHigh);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Get the color to use when writing text to the console</summary>
        public ushort GetTextColor()
        {
            return m_Color;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Tell whether consecutive identical messages should be filtered or not</summary>
        public void SetFilterRepeatedMessages(bool Filter)
        {
            m_FilterRepeatedMessage = Filter;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Helper: map RGB+intensity bits to a System.ConsoleColor value
        private static ConsoleColor MapToConsoleColor(bool r, bool g, bool b, bool intensity)
        {
            if (!r && !g && !b)  return intensity ? ConsoleColor.DarkGray  : ConsoleColor.Black;
            if ( r && !g && !b)  return intensity ? ConsoleColor.Red        : ConsoleColor.DarkRed;
            if (!r &&  g && !b)  return intensity ? ConsoleColor.Green      : ConsoleColor.DarkGreen;
            if (!r && !g &&  b)  return intensity ? ConsoleColor.Blue       : ConsoleColor.DarkBlue;
            if ( r &&  g && !b)  return intensity ? ConsoleColor.Yellow     : ConsoleColor.DarkYellow;
            if ( r && !g &&  b)  return intensity ? ConsoleColor.Magenta    : ConsoleColor.DarkMagenta;
            if (!r &&  g &&  b)  return intensity ? ConsoleColor.Cyan       : ConsoleColor.DarkCyan;
            /* r && g && b */    return intensity ? ConsoleColor.White      : ConsoleColor.Gray;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
