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
 *  \file CPauseMessage.cs
 *  \brief Pause message (C# port of CPauseMessage.cpp/h)
 */

using System.Diagnostics;
using static Bombermaaan.Globals;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Handles the pause message during a match</summary>
    public class CPauseMessage
    {
        private CScroller m_Scroller;
        private CDisplay  m_pDisplay;
        private CSound    m_pSound;
        private bool      m_Waiting;
        private bool      m_HaveToGetOut;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CPauseMessage(CDisplay pDisplay, CSound pSound)
        {
            Debug.Assert(pDisplay != null);
            Debug.Assert(pSound != null);

            m_pDisplay = pDisplay;
            m_pSound   = pSound;

            // Pause or unpause the samples and songs being played
            m_pSound.SetPause(true);

            // Play the pause sound
            m_pSound.PlaySample(ESample.SAMPLE_PAUSE);

            // Not waiting since we must start by moving
            m_Waiting = false;

            // We're entering the screen, so don't have to get out of the screen for the moment
            m_HaveToGetOut = false;

            // Create the scroller object that will be used to move and display the pause message sprite
            m_Scroller = new CScroller();
            m_Scroller.Create((VIEW_WIDTH - 202) / 2, -16, 69, 16, 0.0f, 300.0f, -1.0f);   // 202 is X size of pause message sprite
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        ~CPauseMessage()
        {
            // Unpause the samples and songs being played
            m_pSound.SetPause(false);

            // Delete the scroller
            m_Scroller.Destroy();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Update(float DeltaTime)
        {
            // If we don't have to wait
            if (!m_Waiting)
            {
                // Update the scroller (move)
                m_Scroller.Update(DeltaTime);

                // If we don't have to get out of the screen and the message must stop moving
                if (!m_HaveToGetOut && m_Scroller.GetPositionY() >= 96)
                {
                    // We have to wait until we are told to get out
                    m_Waiting = true;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Display()
        {
            // We need to prepare a clip structure of the size of the game view
            // because of the tiled background which moves to animate
            RECT Clip;
            Clip.left   = 0;
            Clip.top    = 0;
            Clip.right  = VIEW_WIDTH;
            Clip.bottom = VIEW_HEIGHT;

            // Draw the pause message
            m_pDisplay.DrawSprite(m_Scroller.GetPositionX(),
                                  m_Scroller.GetPositionY(),
                                  null,    // Draw entire tile
                                  Clip,    // Clip with game view
                                  CDisplay.BMP_PAUSE,
                                  0,
                                  700,
                                  -1);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool IsWaiting()
        {
            return m_Waiting;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void GetOut()
        {
            // Don't tell the pause message to get out if he is not waiting for you
            Debug.Assert(m_Waiting);

            // The pause message is now getting out of the screen
            m_Waiting      = false;
            m_HaveToGetOut = true;

            // Play the pause sound
            m_pSound.PlaySample(ESample.SAMPLE_PAUSE);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool IsOutOfBounds()
        {
            return m_Scroller.OutOfBounds();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
