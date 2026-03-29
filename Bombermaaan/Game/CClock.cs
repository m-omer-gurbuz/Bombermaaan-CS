/*************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
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

*************************************************************************************/


/// <summary>
/// CClock.cs - The clock
/// </summary>

using System.Diagnostics;

namespace Bombermaaan
{

    /// <summary>Clock type: describes how to make the date evolve.</summary>
    public enum EClockType
    {
        CLOCKTYPE_CHRONOMETER,  ///< Chronometer from a given date A:B:C
        CLOCKTYPE_COUNTDOWN     ///< Count down from a given date A:B:C
    }

    /// <summary>
    /// Clock mode: describes what time units to count.
    /// For instance, if the user of CClock wants the seconds count and the minutes count
    /// of the time, he will choose the 'MS' mode.
    /// </summary>
    public enum EClockMode
    {
        CLOCKMODE_HMSC,  ///< Compute Hours and Minutes and Seconds and Seconds100 for each update
        CLOCKMODE_HMS,   ///< Compute Hours and Minutes and Seconds for each update
        CLOCKMODE_HM,    ///< Compute Hours and Minutes for each update
        CLOCKMODE_MSC,   ///< Compute Minutes = total minutes, and Seconds and Seconds100 for each update
        CLOCKMODE_MS,    ///< Compute Minutes = total minutes, and Seconds for each update
        CLOCKMODE_SC,    ///< Compute Seconds = total seconds, and Seconds100 for each update
        CLOCKMODE_S      ///< Compute Seconds = total seconds, for each update
    }

    /// <summary>
    /// CClock is a reusable class which manages a clock with several types and modes.
    /// You have to periodically update the clock after creating it.
    /// You then just have to get the "date parts" (hours/minutes/seconds/seconds100)
    /// according to the clock mode you chose.
    /// </summary>
    public class CClock
    {
        private float      m_Date;            ///< Most accurate available date
        private int        m_Hours;           ///< Hours count in the date, according to clock mode
        private int        m_Minutes;         ///< Minutes count in the date, according to clock mode
        private int        m_Seconds;         ///< Seconds count in the date, according to clock mode
        private int        m_Seconds100;      ///< Seconds100 count in the date, according to clock mode
        private int        m_StartHours;      ///< Hours count to start with and to use when resetting
        private int        m_StartMinutes;    ///< Minutes count to start with and to use when resetting
        private int        m_StartSeconds;    ///< Seconds count to start with and to use when resetting
        private int        m_StartSeconds100; ///< Seconds100 count to start with and to use when resetting
        private EClockType m_ClockType;       ///< Type of the clock
        private EClockMode m_ClockMode;       ///< Mode of the clock
        private bool       m_Pause;           ///< Update the date (or not) when calling Update method

        public CClock()
        {
            m_Date = 0.0f;
            m_Hours = 0;
            m_Minutes = 0;
            m_Seconds = 0;
            m_Seconds100 = 0;
            m_StartHours = 0;
            m_StartMinutes = 0;
            m_StartSeconds = 0;
            m_StartSeconds100 = 0;
            m_Pause = false;
            m_ClockType = EClockType.CLOCKTYPE_COUNTDOWN;
            m_ClockMode = EClockMode.CLOCKMODE_MS;
        }

        /// <summary>Initialize the clock.</summary>
        public void Create(EClockType ClockType, EClockMode ClockMode, int Hours, int Minutes, int Seconds, int Seconds100)
        {
            m_ClockType = ClockType;
            m_ClockMode = ClockMode;

            // Assert the clock numbers are valid
            Debug.Assert(Hours >= 0 && Hours <= 23);
            Debug.Assert(Minutes >= 0 && Minutes <= 59);
            Debug.Assert(Seconds >= 0 && Seconds <= 59);
            Debug.Assert(Seconds100 >= 0 && Seconds100 <= 99);

            m_Hours = Hours;
            m_Minutes = Minutes;
            m_Seconds = Seconds;
            m_Seconds100 = Seconds100;

            // Count the total time in seconds
            m_Date = (float)(Hours * 3600 + Minutes * 60 + Seconds) + (float)Seconds100 * 0.010f;

            // Remember starting values to enable resets
            m_StartHours = Hours;
            m_StartMinutes = Minutes;
            m_StartSeconds = Seconds;
            m_StartSeconds100 = Seconds100;

            m_Pause = false;
        }

        /// <summary>Uninitialize the clock.</summary>
        public void Destroy()
        {
            // Nothing to do
        }

        /// <summary>Pause the clock.</summary>
        public void Pause()
        {
            m_Pause = true;
        }

        /// <summary>Resume the clock.</summary>
        public void Resume()
        {
            m_Pause = false;
        }

        /// <summary>Get the Hour component of the current date.</summary>
        public int GetHours()
        {
            Debug.Assert(
                m_ClockMode == EClockMode.CLOCKMODE_HMSC ||
                m_ClockMode == EClockMode.CLOCKMODE_HMS  ||
                m_ClockMode == EClockMode.CLOCKMODE_HM);
            return m_Hours;
        }

        /// <summary>Get the Minute component of the current date.</summary>
        public int GetMinutes()
        {
            Debug.Assert(
                m_ClockMode == EClockMode.CLOCKMODE_HMSC ||
                m_ClockMode == EClockMode.CLOCKMODE_HMS  ||
                m_ClockMode == EClockMode.CLOCKMODE_HM   ||
                m_ClockMode == EClockMode.CLOCKMODE_MSC  ||
                m_ClockMode == EClockMode.CLOCKMODE_MS);
            return m_Minutes;
        }

        /// <summary>Get the Second component of the current date.</summary>
        public int GetSeconds()
        {
            Debug.Assert(
                m_ClockMode == EClockMode.CLOCKMODE_HMSC ||
                m_ClockMode == EClockMode.CLOCKMODE_HMS  ||
                m_ClockMode == EClockMode.CLOCKMODE_MSC  ||
                m_ClockMode == EClockMode.CLOCKMODE_MS   ||
                m_ClockMode == EClockMode.CLOCKMODE_SC   ||
                m_ClockMode == EClockMode.CLOCKMODE_S);
            return m_Seconds;
        }

        /// <summary>Get the Second100 component of the current date.</summary>
        public int GetSeconds100()
        {
            Debug.Assert(
                m_ClockMode == EClockMode.CLOCKMODE_HMSC ||
                m_ClockMode == EClockMode.CLOCKMODE_MSC  ||
                m_ClockMode == EClockMode.CLOCKMODE_SC);
            return m_Seconds100;
        }

        /// <summary>Update the clock's date.</summary>
        public void Update(float DeltaTime)
        {
            if (!m_Pause)
            {
                switch (m_ClockType)
                {
                    case EClockType.CLOCKTYPE_COUNTDOWN:
                        {
                            m_Date -= DeltaTime;
                            if (m_Date < 0.0f)
                                m_Date = 0.0f;
                            break;
                        }
                    case EClockType.CLOCKTYPE_CHRONOMETER:
                        {
                            m_Date += DeltaTime;
                            break;
                        }
                }

                float RemainingDate = m_Date;

                m_Hours = 0;
                m_Minutes = 0;
                m_Seconds = 0;
                m_Seconds100 = 0;

                switch (m_ClockMode)
                {
                    case EClockMode.CLOCKMODE_HMSC:
                        CountHours(ref RemainingDate);
                        CountMinutes(ref RemainingDate);
                        CountSeconds(ref RemainingDate);
                        CountSeconds100(ref RemainingDate);
                        break;
                    case EClockMode.CLOCKMODE_HMS:
                        CountHours(ref RemainingDate);
                        CountMinutes(ref RemainingDate);
                        CountSeconds(ref RemainingDate);
                        break;
                    case EClockMode.CLOCKMODE_HM:
                        CountHours(ref RemainingDate);
                        CountMinutes(ref RemainingDate);
                        break;
                    case EClockMode.CLOCKMODE_MSC:
                        CountMinutes(ref RemainingDate);
                        CountSeconds(ref RemainingDate);
                        CountSeconds100(ref RemainingDate);
                        break;
                    case EClockMode.CLOCKMODE_MS:
                        CountMinutes(ref RemainingDate);
                        CountSeconds(ref RemainingDate);
                        break;
                    case EClockMode.CLOCKMODE_SC:
                        CountSeconds(ref RemainingDate);
                        CountSeconds100(ref RemainingDate);
                        break;
                    case EClockMode.CLOCKMODE_S:
                        CountSeconds(ref RemainingDate);
                        break;
                }
            }
        }

        /// <summary>Reset the date to the starting date (which was set on last call to Create()).</summary>
        public void Reset()
        {
            m_Hours = m_StartHours;
            m_Minutes = m_StartMinutes;
            m_Seconds = m_StartSeconds;
            m_Seconds100 = m_StartSeconds100;

            m_Date = (float)(m_Hours * 3600 + m_Minutes * 60 + m_Seconds) + (float)m_Seconds100 * 0.010f;
        }

        /// <summary>Count the hours in the remaining date.</summary>
        private void CountHours(ref float RemainingDate)
        {
            while (RemainingDate >= 3600.0f)
            {
                RemainingDate -= 3600.0f;
                m_Hours++;
                if (m_Hours == 24)
                    m_Hours = 0;
            }
        }

        /// <summary>Count the minutes in the remaining date.</summary>
        private void CountMinutes(ref float RemainingDate)
        {
            while (RemainingDate >= 60.0f)
            {
                RemainingDate -= 60.0f;
                m_Minutes++;
                if (m_Minutes == 60)
                    m_Minutes = 0;
            }
        }

        /// <summary>Count the seconds in the remaining date.</summary>
        private void CountSeconds(ref float RemainingDate)
        {
            while (RemainingDate >= 1.0f)
            {
                RemainingDate -= 1.0f;
                m_Seconds++;
                if (m_Seconds == 60)
                    m_Seconds = 0;
            }
        }

        /// <summary>Count the seconds100 in the remaining date.</summary>
        private void CountSeconds100(ref float RemainingDate)
        {
            while (RemainingDate >= 0.010f)
            {
                RemainingDate -= 0.010f;
                m_Seconds100++;
                if (m_Seconds100 == 100)
                    m_Seconds100 = 0;
            }
        }
    }

} // namespace Bombermaaan
