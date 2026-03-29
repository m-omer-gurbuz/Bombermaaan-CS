// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2008-2010 Markus Drescher
    Copyright (C) 2008 Bernd Arnold
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
 *  \file CLog.cs
 *  \brief Handling the log
 *
 *  by Thibaut "Fury" Tollemer
 *
 *  Most of the code was taken in the
 *  Log.cpp/Log.h files of HaCKeR source,
 *  by Michaël Schoonbrood :
 *      - MadButch@OneCoolDude.Com
 *      - http://play.as/madbutch
 */

using System;
using System.IO;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Debug sections used when writing categorised debug messages.
    /// </summary>
    public enum EDebugSection
    {
        DEBUGSECT_BOMBER,
        DEBUGSECT_BOMB,
        DEBUGSECT_EXPLOSION
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Implements a log file where messages can be written to.
    /// </summary>
    public class CLog : IDisposable
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private StreamWriter m_theLog;
        private bool         m_bOpen;
        private bool         m_disposed;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CLog()
        {
            // Log file is not open yet
            m_bOpen    = false;
            m_disposed = false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Dispose()
        {
            // Close the log file
            if (m_bOpen)
            {
                Close();
            }
            m_disposed = true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Singleton instance for the main log
        private static readonly CLog s_rLog = new CLog();

        // Singleton instance for the debug log
        private static readonly CLog s_rDebugLog = new CLog();

        /// <summary>Get an instance of CLog (singleton)</summary>
        public static CLog GetLog()
        {
            return s_rLog;
        }

        /// <summary>Get an instance of CLog (singleton) for debug messages</summary>
        public static CLog GetDebugLog()
        {
            return s_rDebugLog;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Open the log</summary>
        public bool Open(string pFilename)
        {
            // Check if there already is an opened file
            if (m_bOpen)
            {
                // Close it first
                Close();
            }

            // Make sure the file is NOT read-only
            if (File.Exists(pFilename))
            {
                File.SetAttributes(pFilename, FileAttributes.Normal);
            }

            try
            {
                // Open the Log
                m_theLog = new StreamWriter(pFilename, append: false);

                // Set indicator bOpen to true
                m_bOpen = true;

                // Get current time
                DateTime now = DateTime.Now;

                // Write first log entry
                string firstLogEntry = string.Format(
                    "==> Log started on {0:D4}-{1:D2}-{2:D2} at {3:D2}:{4:D2}:{5:D2}.\n\n",
                    now.Year, now.Month,  now.Day,
                    now.Hour, now.Minute, now.Second);

                m_theLog.Write(firstLogEntry);
                m_theLog.Flush();
            }
            catch
            {
                // Set indicator bOpen to false
                m_bOpen = false;
            }

            return m_bOpen;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Close the log</summary>
        public bool Close()
        {
            // Close the Log
            if (m_bOpen)
            {
                // Write a blank line
                m_theLog.Write("\n");

                // Get current time
                DateTime now = DateTime.Now;

                // Store last log entry
                string lastLogEntry = string.Format(
                    "==> Log ended on {0:D4}-{1:D2}-{2:D2} at {3:D2}:{4:D2}:{5:D2}.\n\n",
                    now.Year, now.Month,  now.Day,
                    now.Hour, now.Minute, now.Second);

                // Write last log entry
                m_theLog.Write(lastLogEntry);

                // Close the file
                m_theLog.Flush();
                m_theLog.Close();
                m_theLog = null;
            }

            m_bOpen = false;

            return !m_bOpen;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Log the last occurred error</summary>
        public void LogLastError()
        {
            // Log the last system error message
            WriteLine(System.Runtime.InteropServices.Marshal.GetLastWin32Error().ToString());
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Write to the log (without appending a newline).
        /// Uses params object[] so callers can pass format args just like printf.
        /// If only one argument and no format args are provided the string is written as-is.
        /// </summary>
        public long Write(string pMessage, params object[] args)
        {
            // Format the given string using the given arguments
            string message = (args != null && args.Length > 0)
                ? string.Format(pMessage, args)
                : pMessage;

            // If the log is open
            if (m_bOpen)
            {
                // If the message starts with a blank line
                if (message.Length == 0 || message[0] != '\n')
                {
                    // Get current time
                    DateTime now = DateTime.Now;

                    // Store the time string (note: '\n' at end as per original)
                    string time = string.Format("{0:D2}:{1:D2}:{2:D2}\n",
                        now.Hour, now.Minute, now.Second);

                    // Write the time string
                    m_theLog.Write(time);

                    // Write the space between time and message
                    m_theLog.Write("  ");

                    // Write the message
                    m_theLog.Write(message);
                }
                // If the message doesn't start with a blank line
                else
                {
                    // Write the message without the time
                    m_theLog.Write(message);
                }

                m_theLog.Flush();
            }
            // If the log is not open
            else
            {
                // Couldn't write to Log!
                return 0;
            }

            return 1;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Write a line to the log (appends a newline after the message).
        /// Uses params object[] so callers can pass format args just like printf.
        /// </summary>
        public long WriteLine(string pMessage, params object[] args)
        {
            // Format the given string using the given arguments
            string message = (args != null && args.Length > 0)
                ? string.Format(pMessage, args)
                : pMessage;

            // If the log is open
            if (m_bOpen)
            {
                // If the message starts with a blank line
                if (message.Length == 0 || message[0] != '\n')
                {
                    // Get current time
                    DateTime now = DateTime.Now;

                    // Store the time string
                    string time = string.Format("{0:D2}:{1:D2}:{2:D2}  ",
                        now.Hour, now.Minute, now.Second);

                    // Write the time string
                    m_theLog.Write(time);

                    // Write the message
                    m_theLog.Write(message);
                }
                // If the message doesn't start with a blank line
                else
                {
                    // Write the message without the time
                    m_theLog.Write(message);
                }

                // Write a blank line
                m_theLog.Write("\n");

                m_theLog.Flush();
            }
            // If the log is not open
            else
            {
                // Couldn't write to Log!
                return 0;
            }

            return 1;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Write a categorised debug line to the log</summary>
        public long WriteDebugMsg(EDebugSection section, string pMessage, params object[] args)
        {
            // Format the given string using the given arguments
            string message = (args != null && args.Length > 0)
                ? string.Format(pMessage, args)
                : pMessage;

            // If the log is open
            if (m_bOpen)
            {
                // If the message starts with a blank line
                if (message.Length == 0 || message[0] != '\n')
                {
                    // Get current time
                    DateTime now = DateTime.Now;

                    // Store the time string
                    string time = string.Format("{0:D2}:{1:D2}:{2:D2}  ",
                        now.Hour, now.Minute, now.Second);

                    // Write the time string
                    m_theLog.Write(time);

                    string sectionString; // #3078839

                    switch (section)
                    {
                        case EDebugSection.DEBUGSECT_BOMBER:
                            sectionString = "BOMBER:     "; // #3078839
                            break;

                        case EDebugSection.DEBUGSECT_BOMB:
                            sectionString = "BOMB:       "; // #3078839
                            break;

                        case EDebugSection.DEBUGSECT_EXPLOSION:
                            sectionString = "EXPLOSION:  "; // #3078839
                            break;

                        default:
                            sectionString = "UNKNOWN:    "; // #3078839
                            break;
                    }

                    m_theLog.Write(sectionString); // #3078839

                    // Write the message
                    m_theLog.Write(message);
                }
                // If the message doesn't start with a blank line
                else
                {
                    // Write the message without the time
                    m_theLog.Write(message);
                }

                // Write a blank line
                m_theLog.Write("\n");

                m_theLog.Flush();
            }
            // If the log is not open
            else
            {
                // Couldn't write to Log!
                return 0;
            }

            return 1;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Return whether the log is open or not</summary>
        public bool IsOpen()
        {
            return m_bOpen;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
