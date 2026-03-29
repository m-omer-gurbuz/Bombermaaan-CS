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
/// CScroller.cs - The scroller used by pause and hurry message
/// </summary>

namespace Bombermaaan
{

    public class CScroller
    {
        private int   m_SpriteWidth;
        private int   m_SpriteHeight;
        private float m_SpeedX;
        private float m_SpeedY;
        private float m_LoopTime;
        private float m_RemainingLoopTime;
        private bool  m_Looping;
        private float m_fPositionX;
        private float m_fPositionY;
        private int   m_iPositionX;
        private int   m_iPositionY;

        public CScroller()
        {
            m_SpriteWidth = 0;
            m_SpriteHeight = 0;
            m_SpeedX = 0.0f;
            m_SpeedY = 0.0f;
            m_LoopTime = 0;
            m_RemainingLoopTime = 0.0f;
            m_iPositionX = -1;
            m_iPositionY = -1;
            m_fPositionX = (float)m_iPositionX;
            m_fPositionY = (float)m_iPositionY;
            m_Looping = false;
        }

        public void Create(int PositionX, int PositionY, int SpriteWidth, int SpriteHeight,
                           float SpeedX, float SpeedY, float LoopTime)
        {
            System.Diagnostics.Debug.Assert(LoopTime >= 0.0f || LoopTime == -1.0f);

            m_SpriteWidth = SpriteWidth;
            m_SpriteHeight = SpriteHeight;
            m_SpeedX = SpeedX;
            m_SpeedY = SpeedY;
            m_LoopTime = LoopTime;
            m_RemainingLoopTime = 0.0f;
            m_iPositionX = PositionX;
            m_iPositionY = PositionY;
            m_fPositionX = (float)m_iPositionX;
            m_fPositionY = (float)m_iPositionY;
            m_Looping = false;
        }

        public void Destroy()
        {
            // Nothing to do
        }

        /// <summary>Get the X position of the scroller.</summary>
        public int GetPositionX()
        {
            return m_iPositionX;
        }

        /// <summary>Get the Y position of the scroller.</summary>
        public int GetPositionY()
        {
            return m_iPositionY;
        }

        /// <summary>Set the scroll speed.</summary>
        public void SetSpeed(float SpeedX, float SpeedY)
        {
            m_SpeedX = SpeedX;
            m_SpeedY = SpeedY;
        }

        public void Update(float DeltaTime)
        {
            if (!m_Looping)
            {
                m_fPositionX += m_SpeedX * DeltaTime;
                m_fPositionY += m_SpeedY * DeltaTime;

                m_iPositionX = (int)m_fPositionX;
                m_iPositionY = (int)m_fPositionY;

                if (OutOfBounds() && m_LoopTime != -1.0f)
                {
                    m_RemainingLoopTime = m_LoopTime;
                    m_Looping = true;
                }
            }
            else
            {
                m_RemainingLoopTime -= DeltaTime;

                if (m_RemainingLoopTime <= 0.0f)
                {
                    m_RemainingLoopTime = 0.0f;
                    m_Looping = false;

                    if (m_iPositionX + m_SpriteWidth < 0)
                    {
                        m_iPositionX = Globals.VIEW_WIDTH;
                        m_fPositionX = (float)m_iPositionX;
                    }
                    else if (m_iPositionX >= Globals.VIEW_WIDTH)
                    {
                        m_iPositionX = -m_SpriteWidth;
                        m_fPositionX = (float)m_iPositionX;
                    }
                    else if (m_iPositionY + m_SpriteHeight < 0)
                    {
                        m_iPositionY = Globals.VIEW_HEIGHT;
                        m_fPositionY = (float)m_iPositionY;
                    }
                    else if (m_iPositionY >= Globals.VIEW_HEIGHT)
                    {
                        m_iPositionY = -m_SpriteHeight;
                        m_fPositionY = (float)m_iPositionY;
                    }
                }
            }
        }

        public bool OutOfBounds()
        {
            return (m_iPositionX > Globals.VIEW_WIDTH  || m_iPositionX + m_SpriteWidth < 0 ||
                    m_iPositionY > Globals.VIEW_HEIGHT || m_iPositionY + m_SpriteHeight < 0);
        }
    }

} // namespace Bombermaaan
