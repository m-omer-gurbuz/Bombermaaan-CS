/************************************************************************************

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

************************************************************************************/

/**
 *  \file CModeScreen.cs
 *  \brief The screen basics
 */

using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Abstract base class for all mode screens (menu, match, winner, etc.).
    /// </summary>
    public abstract class CModeScreen
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private const float BLACKSCREEN_DURATION = 0.750f; // Duration (in seconds) of each of the two black screens

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected CDisplay  m_pDisplay;     //!< Link to the display object to use
        protected CInput    m_pInput;       //!< Link to the input object to use
        protected COptions  m_pOptions;     //!< Link to the options object to use
        protected CTimer    m_pTimer;       //!< Link to the timer object to use
        protected CSound    m_pSound;       //!< Link to the sound object to use

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Constructor. Initialize some members.</summary>
        protected CModeScreen()
        {
            // Initialize the pointers to null so that we
            // can easily detect the ones we forgot to set.
            m_pDisplay = null;
            m_pInput   = null;
            m_pOptions = null;
            m_pTimer   = null;
            m_pSound   = null;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set link to the display object to use</summary>
        public virtual void SetDisplay(CDisplay pDisplay)
        {
            m_pDisplay = pDisplay;
        }

        /// <summary>Set link to the input object to use</summary>
        public virtual void SetInput(CInput pInput)
        {
            m_pInput = pInput;
        }

        /// <summary>Set link to the options object to use</summary>
        public virtual void SetOptions(COptions pOptions)
        {
            m_pOptions = pOptions;
        }

        /// <summary>Set link to the timer object to use</summary>
        public virtual void SetTimer(CTimer pTimer)
        {
            m_pTimer = pTimer;
        }

        /// <summary>Set link to the sound object to use</summary>
        public virtual void SetSound(CSound pSound)
        {
            m_pSound = pSound;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Initialize the object</summary>
        public virtual void Create()
        {
            // Check if all the objects to communicate with are set
            Debug.Assert(m_pDisplay != null);
            Debug.Assert(m_pInput   != null);
            Debug.Assert(m_pOptions != null);
            Debug.Assert(m_pTimer   != null);
            Debug.Assert(m_pSound   != null);

            OpenInput();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Uninitialize the object</summary>
        public virtual void Destroy()
        {
            CloseInput();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Get access to the input this object needs</summary>
        public abstract void OpenInput();

        /// <summary>Release access to the input this object needs</summary>
        public abstract void CloseInput();

        /// <summary>Update the object and return what game mode should be set</summary>
        public abstract EGameMode Update();

        /// <summary>Display on the screen</summary>
        public abstract void Display();

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
