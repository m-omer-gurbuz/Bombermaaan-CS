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
/// CMosaic.cs - Mosaic background in menus
/// </summary>

namespace Bombermaaan
{

    public class CMosaic
    {
        private CDisplay m_pDisplay;              ///< Link to the display object to use to draw the mosaic
        private int      m_SpriteTable;           ///< Sprite table of the mosaic tile sprite to use
        private int      m_Sprite;                ///< Number of the mosaic tile sprite to use
        private int      m_SpriteLayer;           ///< Sprite layer where to draw the mosaic
        private int      m_PriorityInLayer;       ///< Priority to use in the sprite layer where to draw the mosaic
        private int      m_SpriteWidth;           ///< Width in pixels of the mosaic tile sprite
        private int      m_SpriteHeight;          ///< Height in pixels of the mosaic tile sprite
        private int      m_TileCountX;            ///< How many mosaic tiles to draw horizontally?
        private int      m_TileCountY;            ///< How many mosaic tiles to draw vertically?
        private float    m_SpeedX;                ///< Scrolling speed X of the mosaic
        private float    m_SpeedY;                ///< Scrolling speed Y of the mosaic
        private float    m_BackgroundPositionX;   ///< Top left corner of the animated tiled background
        private float    m_BackgroundPositionY;   ///< Modified to animate the background on the game view
        private int      m_iBackgroundPositionX;  ///< Integer X coordinate of the position above (screen coords)
        private int      m_iBackgroundPositionY;  ///< Integer Y coordinate of the position above (screen coords)

        public CMosaic()
        {
            m_pDisplay = null;
            m_SpriteTable = 0;
            m_Sprite = 0;
            m_SpriteLayer = 0;
            m_PriorityInLayer = 0;
            m_SpriteWidth = 0;
            m_SpriteHeight = 0;
            m_TileCountX = 0;
            m_TileCountY = 0;
            m_SpeedX = 0.0f;
            m_SpeedY = 0.0f;
            m_BackgroundPositionX = 0.0f;
            m_BackgroundPositionY = 0.0f;
            m_iBackgroundPositionX = 0;
            m_iBackgroundPositionY = 0;
        }

        ~CMosaic()
        {
            Destroy();
        }

        /// <summary>Set link to the display object.</summary>
        public void SetDisplay(CDisplay pDisplay)
        {
            m_pDisplay = pDisplay;
        }

        public void Create(int SpriteTable, int Sprite, int SpriteLayer, int PriorityInLayer,
                           int SpriteWidth, int SpriteHeight, int TileCountX, int TileCountY,
                           float SpeedX, float SpeedY)
        {
            System.Diagnostics.Debug.Assert(m_pDisplay != null);

            // Currently, this class has to be used with a positive SpeedX and a negative SpeedY.
            System.Diagnostics.Debug.Assert(SpeedX >= 0.0f);
            System.Diagnostics.Debug.Assert(SpeedY <= 0.0f);

            m_SpriteTable = SpriteTable;
            m_Sprite = Sprite;
            m_SpriteLayer = SpriteLayer;
            m_PriorityInLayer = PriorityInLayer;
            m_SpriteWidth = SpriteWidth;
            m_SpriteHeight = SpriteHeight;
            m_TileCountX = TileCountX;
            m_TileCountY = TileCountY;
            m_SpeedX = SpeedX;
            m_SpeedY = SpeedY;
            m_BackgroundPositionX = 0.0f;
            m_BackgroundPositionY = 0.0f;
            m_iBackgroundPositionX = 0;
            m_iBackgroundPositionY = 0;
        }

        public void Destroy()
        {
            // Nothing to do
        }

        /// <summary>Update the mosaic state.</summary>
        public void Update(float DeltaTime)
        {
            if (m_SpeedX > 0.0f || m_SpeedY > 0.0f)
            {
                m_BackgroundPositionX += m_SpeedX * DeltaTime;
                m_BackgroundPositionY += m_SpeedY * DeltaTime;

                m_iBackgroundPositionX = (int)m_BackgroundPositionX;
                m_iBackgroundPositionY = (int)m_BackgroundPositionY;

                // While the background is too much on the right
                while (m_iBackgroundPositionX > 0)
                {
                    m_iBackgroundPositionX -= m_SpriteWidth;
                }

                // While the background is too much above
                while (m_iBackgroundPositionY + m_SpriteHeight < 0)
                {
                    m_iBackgroundPositionY += m_SpriteHeight;
                }
            }
        }

        /// <summary>Display the mosaic in its current state.</summary>
        public void Display()
        {
            // Prepare a clip of the size of the game view
            RECT Clip;
            Clip.left   = 0;
            Clip.top    = 0;
            Clip.right  = Globals.VIEW_WIDTH;
            Clip.bottom = Globals.VIEW_HEIGHT;

            int X = m_iBackgroundPositionX;
            int Y = m_iBackgroundPositionY;

            for (int TileY = 0; TileY < m_TileCountY; TileY++)
            {
                for (int TileX = 0; TileX < m_TileCountX; TileX++)
                {
                    m_pDisplay.DrawSprite(X, Y,
                                          null,
                                          Clip,
                                          m_SpriteTable,
                                          m_Sprite,
                                          m_SpriteLayer,
                                          m_PriorityInLayer);
                    X += m_SpriteWidth;
                }

                X = m_iBackgroundPositionX;
                Y += m_SpriteHeight;
            }
        }
    }

} // namespace Bombermaaan
