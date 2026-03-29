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
 *  \file CHurryMessage.cs
 *  \brief The hurry message (C# port of CHurryMessage.cpp/h)
 */

using static Bombermaaan.Globals;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Manages the hurry message (match end almost reached) during a match</summary>
    public class CHurryMessage
    {
        private CDisplay  m_pDisplay;
        private CSound    m_pSound;
        private CScroller m_Scroller;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CHurryMessage(CDisplay pDisplay, CSound pSound)
        {
            System.Diagnostics.Debug.Assert(pDisplay != null);
            System.Diagnostics.Debug.Assert(pSound != null);

            m_pDisplay = pDisplay;
            m_pSound   = pSound;

            // Play the hurry up sound
            m_pSound.PlaySample(ESample.SAMPLE_HURRY);

            // Stop the match music song
            m_pSound.StopSong(ESong.SONG_MATCH_MUSIC);

            m_Scroller = new CScroller();
            m_Scroller.Create(-68, 96, 69, 16, 308.0f, 0.0f, -1.0f);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        ~CHurryMessage()
        {
            // Delete the scroller
            m_Scroller.Destroy();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Update the hurry message scroller.
        /// </summary>
        /// <returns>true if the scroller is out of the screen</returns>
        public bool Update(float DeltaTime)
        {
            // Update the scroller (move)
            m_Scroller.Update(DeltaTime);

            // Return whether the hurry message is over,
            // that is to say if the scroller is out of the screen.
            return m_Scroller.OutOfBounds();
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

            // Draw the hurry message
            m_pDisplay.DrawSprite(m_Scroller.GetPositionX(),
                                  m_Scroller.GetPositionY(),
                                  null,    // Draw entire tile
                                  Clip,    // Clip with game view
                                  CDisplay.BMP_HURRY,
                                  0,
                                  700,
                                  -1);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
