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
 *  \file CSnowManager.cs
 *  \brief The snow manager
 */

using System;

namespace Bombermaaan
{
    /// <summary>
    /// Manages animated snowflakes on the title screen.
    /// </summary>
    public class CSnowManager
    {
        private const int NUMBER_OF_SNOWFLAKES = 30;

        private CDisplay    m_pDisplay;
        private CScroller[] m_Snows;
        private int[]       m_SnowSprites;

        private static readonly Random s_Rand = new Random();

        public CSnowManager()
        {
            m_pDisplay   = null!;
            m_Snows      = new CScroller[NUMBER_OF_SNOWFLAKES];
            m_SnowSprites = new int[NUMBER_OF_SNOWFLAKES];

            for (int i = 0; i < NUMBER_OF_SNOWFLAKES; i++)
            {
                m_Snows[i]      = new CScroller();
                m_SnowSprites[i] = BmpId.BMP_TITLE_SNOWFLAKE;
            }
        }

        public void SetDisplay(CDisplay pDisplay)
        {
            m_pDisplay = pDisplay;
        }

        public void Create()
        {
            System.Diagnostics.Debug.Assert(m_pDisplay != null);

            for (int i = 0; i < NUMBER_OF_SNOWFLAKES; i++)
            {
                m_SnowSprites[i] = BmpId.BMP_TITLE_SNOWFLAKE;
                m_Snows[i].Create(
                    s_Rand.Next(Globals.VIEW_WIDTH),
                    s_Rand.Next(Globals.VIEW_HEIGHT),
                    21, 24,
                    0.0f, 50.0f + s_Rand.Next(20) * 1.0f,
                    8.0f);
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < NUMBER_OF_SNOWFLAKES; i++)
            {
                m_Snows[i].Destroy();
            }
        }

        public void Update(float DeltaTime)
        {
            for (int i = 0; i < NUMBER_OF_SNOWFLAKES; i++)
            {
                m_Snows[i].Update(DeltaTime);
            }
        }

        public void Display()
        {
            for (int i = 0; i < NUMBER_OF_SNOWFLAKES; i++)
            {
                RECT Clip = new RECT
                {
                    left   = 0,
                    top    = 0,
                    right  = Globals.VIEW_WIDTH,
                    bottom = Globals.VIEW_HEIGHT
                };

                m_pDisplay.DrawSprite(
                    m_Snows[i].GetPositionX(),
                    m_Snows[i].GetPositionY(),
                    null,
                    Clip,
                    m_SnowSprites[i], 0, 0, 1);
            }
        }
    }
}
