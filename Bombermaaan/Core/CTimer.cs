/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007, 2008 Bernd Arnold
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
 *  \file CTimer.cs
 *  \brief Implementation of the timer (C# port)
 *
 *  Based on the code of Dhonn Lushine
 *  http://members.aol.com/dhonn
 *  dhonn@usa.net
 */

using System.Diagnostics;
using System.Threading;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /**
     *  \brief The CTimer class provides an accurate timer using System.Diagnostics.Stopwatch.
     *
     *  It can return the current
     *  time since the construction of the timer, and
     *  the deltatime (time between last call to get
     *  deltatime and the next call). It also handles
     *  pauses during the execution so that the deltatime
     *  when resuming is not huge but normal.
     *
     *  Replaces the original Win32 QueryPerformanceCounter / QueryPerformanceFrequency
     *  implementation with System.Diagnostics.Stopwatch, which wraps the same
     *  high-resolution timer on Windows and is cross-platform.
     */
    public class CTimer
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Stopwatch started at construction time; replaces m_StartClock + QueryPerformanceCounter.
        // Stopwatch.Elapsed.TotalSeconds gives the same value as
        //   (double)(EndClock - m_StartClock) * m_InvRate  from the C++ version.
        private readonly Stopwatch m_Stopwatch;

        // Latest saved time value (seconds since construction)
        private double m_Time;

        // Latest saved delta time value
        private float m_DeltaTime;

        // Is the timer paused?
        private bool m_Pause;

        // Delta time saved on pause and restored on resume
        private float m_DeltaTimeAtPause;

        // Coefficient to apply to the deltatime before returning it
        private float m_Speed;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // This method returns the time value at this moment (seconds since construction).
        // Replaces the inline GetCurrentTime() that used QueryPerformanceCounter.
        private double GetCurrentTime()
        {
            return m_Stopwatch.Elapsed.TotalSeconds;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // The CTimer constructor initializes the high-resolution timer.
        // Stopwatch.IsHighResolution indicates whether the underlying timer is high-resolution;
        // we log a warning when it is not (mirrors the QueryPerformanceFrequency failure path).
        public CTimer()
        {
            // Verify that a high-resolution timer is available
            if (!Stopwatch.IsHighResolution)
            {
                // Log failure – mirrors "QueryPerformanceFrequency failed" from the C++ version
                CLog.GetLog().WriteLine("Timer           => !!! High-resolution timer not available (Stopwatch.IsHighResolution == false).");
            }

            // Verify that the stopwatch frequency is non-zero
            if (Stopwatch.Frequency == 0)
            {
                // Log failure – mirrors "Rate is zero" from the C++ version
                CLog.GetLog().WriteLine("Timer           => !!! Stopwatch.Frequency is zero.");
            }

            // Start the stopwatch; this is equivalent to capturing m_StartClock with
            // QueryPerformanceCounter and storing the frequency in m_InvRate.
            m_Stopwatch = Stopwatch.StartNew();

            // Initialize members
            m_Time = 0.0;
            m_DeltaTime = 0.0f;
            m_DeltaTimeAtPause = 0.0f;
            m_Pause = false;
            m_Speed = 1.0f;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // This method updates the time value and deltatime value of the timer.
        public void Update()
        {
            // Without the sleep, the clouds hopped and the mouse pointer froze (tracker item #1870410)
            Thread.Sleep(1);

            // The timer must not be paused
            Debug.Assert(!m_Pause);

            // Get the current time value
            double time = GetCurrentTime();

            // If timer has already been updated
            if (m_Time > 0.0)
            {
                m_DeltaTime = (float)(time - m_Time);
                m_Time = time;
            }
            // If timer has never been updated
            else
            {
                m_Time = time;
                m_DeltaTime = 0.0f;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Pauses the timer, so that the delta time value is
        // still right when resuming.
        public void Pause()
        {
            if (!m_Pause)                               // Timer must not be already paused
            {
                m_Pause = true;                         // Set pause
                m_DeltaTimeAtPause = m_DeltaTime;       // Save deltatime
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Resume after a pause
        public void Resume()
        {
            if (m_Pause)                                            // Timer must not be already unpaused
            {
                m_Pause = false;                                    // Set unpaused
                m_Time = GetCurrentTime() - m_DeltaTimeAtPause;    // Update time
                m_DeltaTime = m_DeltaTimeAtPause;                   // Update deltatime
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Set coefficient to apply to the deltatime before returning it
        public void SetSpeed(float Speed)
        {
            m_Speed = Speed;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // These methods are used to get the time and deltatime values
        public float GetDeltaTime() { Debug.Assert(!m_Pause); return m_DeltaTime * m_Speed; }
        public double GetTime()     { Debug.Assert(!m_Pause); return m_Time; }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
