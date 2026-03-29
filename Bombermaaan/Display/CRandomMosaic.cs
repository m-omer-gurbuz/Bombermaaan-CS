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
/// CRandomMosaic.cs - A random mosaic
/// </summary>

namespace Bombermaaan
{

    public struct SMosaicTileProperties
    {
        public int SpriteTable;
        public int Sprite;
    }

    public enum EMosaicColor
    {
        MOSAICCOLOR_PURPLE,
        MOSAICCOLOR_GREEN,
        MOSAICCOLOR_BLUE,
        MOSAICCOLOR_RED
    }

    public enum EMosaicType
    {
        MOSAICTYPE_SOLID,
        MOSAICTYPE_BOMB,
        MOSAICTYPE_CHAR,
        MOSAICTYPE_FLAME
    }

    public class CRandomMosaic
    {
        private static SMosaicTileProperties[,] m_MosaicTileProperties = new SMosaicTileProperties[4, 4]
        {
        // PURPLE: SOLID, BOMB, CHAR, FLAME
        {
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_PURPLE_BACKGROUND_SOLID, Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_PURPLE_BACKGROUND_BOMB,  Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_PURPLE_BACKGROUND_CHAR,  Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_PURPLE_BACKGROUND_FLAME, Sprite = 0 },
        },
        // GREEN: SOLID, BOMB, CHAR, FLAME
        {
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_GREEN_BACKGROUND_SOLID,  Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_GREEN_BACKGROUND_BOMB,   Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_GREEN_BACKGROUND_CHAR,   Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_GREEN_BACKGROUND_FLAME,  Sprite = 0 },
        },
        // BLUE: SOLID, BOMB, CHAR, FLAME
        {
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_BLUE_BACKGROUND_SOLID,   Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_BLUE_BACKGROUND_BOMB,    Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_BLUE_BACKGROUND_CHAR,    Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_BLUE_BACKGROUND_FLAME,   Sprite = 0 },
        },
        // RED: SOLID, BOMB, CHAR, FLAME
        {
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_RED_BACKGROUND_SOLID,    Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_RED_BACKGROUND_BOMB,     Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_RED_BACKGROUND_CHAR,     Sprite = 0 },
            new SMosaicTileProperties { SpriteTable = BmpId.BMP_RED_BACKGROUND_FLAME,    Sprite = 0 },
        }
        };

        public static CMosaic CreateRandomMosaic(CDisplay pDisplay,
                                                 int SpriteLayer,
                                                 int PriorityInLayer,
                                                 float SpeedX,
                                                 float SpeedY,
                                                 EMosaicColor Color,
                                                 EMosaicType Type)
        {
            SMosaicTileProperties props = m_MosaicTileProperties[(int)Color, (int)Type];

            // Use actual sprite dimensions (border already excluded by LoadSpritesAuto)
            int tileW = pDisplay.GetSpriteWidth(props.SpriteTable, props.Sprite);
            int tileH = pDisplay.GetSpriteHeight(props.SpriteTable, props.Sprite);

            // Calculate how many tiles are needed to cover the screen (+2 for scroll overflow)
            int countX = (Globals.VIEW_WIDTH  / tileW) + 2;
            int countY = (Globals.VIEW_HEIGHT / tileH) + 2;

            CMosaic pNewMosaic = new CMosaic();
            pNewMosaic.SetDisplay(pDisplay);
            pNewMosaic.Create(props.SpriteTable,
                              props.Sprite,
                              SpriteLayer,
                              PriorityInLayer,
                              tileW,
                              tileH,
                              countX,
                              countY,
                              SpeedX,
                              SpeedY);

            return pNewMosaic;
        }
    }

} // namespace Bombermaaan
