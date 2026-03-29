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
 *  \file CFont.cs
 *  \brief Font
 */

using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum EFontColor
    {
        FONTCOLOR_BLUE,
        FONTCOLOR_YELLOW,
        FONTCOLOR_RED,
        FONTCOLOR_GREEN,
        FONTCOLOR_WHITE,
        FONTCOLOR_BLACK
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum EShadowDirection
    {
        SHADOWDIRECTION_UP,
        SHADOWDIRECTION_DOWN,
        SHADOWDIRECTION_LEFT,
        SHADOWDIRECTION_RIGHT,
        SHADOWDIRECTION_UPLEFT,
        SHADOWDIRECTION_UPRIGHT,
        SHADOWDIRECTION_DOWNLEFT,
        SHADOWDIRECTION_DOWNRIGHT
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public class CFont
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private const int MAX_STRING_LENGTH            = 2048; //!< Maximum length for a string to draw
        private const int CHAR_COUNT_PER_FONTCOLOR     = 46;   //!< Number of characters per font color in the font sprite table
        private const int CHAR_WIDTH                   = 10;   //!< Size (in pixels) of one character
        private const int CHAR_HEIGHT                  = 10;
        private const int CHAR_SPACE                   = 1;    //!< Space (in pixels) between two chars when drawing a string

        // Character offset definitions
        private const int LETTERS_CHAR_OFFSET_BEGIN    = 0;    //!< Beginning character offset for letters
        private const int NUMBERS_CHAR_OFFSET_BEGIN    = 26;   //!< Beginning character offset for numbers
        private const int SPECIAL_CHAR_OFFSET_BEGIN    = 36;   //!< Beginning character offset for special characters
        private const int MINUS_CHAR_OFFSET            = 0;
        private const int PLUS_CHAR_OFFSET             = 1;
        private const int PERIOD_CHAR_OFFSET           = 2;
        private const int COLON_CHAR_OFFSET            = 3;
        private const int EXCLAMATION_CHAR_OFFSET      = 4;
        private const int INTERROGATIVE_CHAR_OFFSET    = 5;
        private const int COMMA_CHAR_OFFSET            = 6;
        private const int LEFTPARENTHESIS_CHAR_OFFSET  = 7;
        private const int RIGHTPARENTHESIS_CHAR_OFFSET = 8;
        private const int AT_CHAR_OFFSET               = 9;

        private const int TEXT_PRIORITY                = 1; //!< Priority for the text in the sprite layer
        private const int SHADOW_PRIORITY              = 0; //!< Priority for the text shadow in the sprite layer

        private const int SHADOW_ABS_OFFSET_X          = 2; //!< Absolute offset to apply to text position for shadow
        private const int SHADOW_ABS_OFFSET_Y          = 2;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private CDisplay m_pDisplay;          //!< We need a display object to draw the string characters
        private int      m_TextColorOffset;   //!< Sprite offset to use to write text using the selected font color
        private int      m_ShadowColorOffset; //!< Sprite offset to use to write text shadow using the selected font color
        private int      m_SpriteLayer;       //!< Sprite layer in which string characters will be drawn
        private bool     m_DrawShadow;        //!< Do we have to draw a shadow under the string we draw?
        private int      m_ShadowOffsetX;     //!< Offset to apply to text position in order to get shadow position
        private int      m_ShadowOffsetY;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CFont()
        {
            m_pDisplay         = null;
            m_SpriteLayer      = 0;
            m_DrawShadow       = false;
            m_TextColorOffset  = 0;
            m_ShadowColorOffset = 0;
            m_ShadowOffsetX    = 0;
            m_ShadowOffsetY    = 0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void SetDisplay(CDisplay pDisplay)
        {
            m_pDisplay = pDisplay;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Create()
        {
            // Check if all the objects to communicate with are set
            Debug.Assert(m_pDisplay != null);

            // Set default text/shadow color
            m_TextColorOffset = GetColorOffset(EFontColor.FONTCOLOR_WHITE);
            m_ShadowColorOffset = GetColorOffset(EFontColor.FONTCOLOR_BLACK);

            // Draw the characters in sprite layer 0 by default
            m_SpriteLayer = 0;

            // By default don't draw any text shadow
            m_DrawShadow = false;

            // Set default shadow direction
            SetShadowDirection(EShadowDirection.SHADOWDIRECTION_UP);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Destroy()
        {
            // Nothing to do
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void SetSpriteLayer(int SpriteLayer)
        {
            m_SpriteLayer = SpriteLayer;
        }

        public void SetShadow(bool DrawShadow)
        {
            m_DrawShadow = DrawShadow;
        }

        public void SetTextColor(EFontColor FontColor)
        {
            m_TextColorOffset = GetColorOffset(FontColor);
        }

        public void SetShadowColor(EFontColor FontColor)
        {
            m_ShadowColorOffset = GetColorOffset(FontColor);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void SetShadowDirection(EShadowDirection ShadowDirection)
        {
            switch (ShadowDirection)
            {
                case EShadowDirection.SHADOWDIRECTION_UP:
                    m_ShadowOffsetX = 0;
                    m_ShadowOffsetY = -SHADOW_ABS_OFFSET_Y;
                    break;

                case EShadowDirection.SHADOWDIRECTION_DOWN:
                    m_ShadowOffsetX = 0;
                    m_ShadowOffsetY = SHADOW_ABS_OFFSET_Y;
                    break;

                case EShadowDirection.SHADOWDIRECTION_LEFT:
                    m_ShadowOffsetX = -SHADOW_ABS_OFFSET_X;
                    m_ShadowOffsetY = 0;
                    break;

                case EShadowDirection.SHADOWDIRECTION_RIGHT:
                    m_ShadowOffsetX = SHADOW_ABS_OFFSET_X;
                    m_ShadowOffsetY = 0;
                    break;

                case EShadowDirection.SHADOWDIRECTION_UPLEFT:
                    m_ShadowOffsetX = -SHADOW_ABS_OFFSET_X;
                    m_ShadowOffsetY = -SHADOW_ABS_OFFSET_Y;
                    break;

                case EShadowDirection.SHADOWDIRECTION_UPRIGHT:
                    m_ShadowOffsetX = SHADOW_ABS_OFFSET_X;
                    m_ShadowOffsetY = -SHADOW_ABS_OFFSET_Y;
                    break;

                case EShadowDirection.SHADOWDIRECTION_DOWNLEFT:
                    m_ShadowOffsetX = -SHADOW_ABS_OFFSET_X;
                    m_ShadowOffsetY = SHADOW_ABS_OFFSET_Y;
                    break;

                case EShadowDirection.SHADOWDIRECTION_DOWNRIGHT:
                    m_ShadowOffsetX = SHADOW_ABS_OFFSET_X;
                    m_ShadowOffsetY = SHADOW_ABS_OFFSET_Y;
                    break;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Draw(int PositionX, int PositionY, string pString, params object[] args)
        {
            string str = (args != null && args.Length > 0) ? string.Format(pString, args) : pString;
            DrawString(PositionX, PositionY, str);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void DrawCenteredX(int BorderLeft, int BorderRight, int PositionY, string pString, params object[] args)
        {
            string str = (args != null && args.Length > 0) ? string.Format(pString, args) : pString;

            // Compute X position so that the string we write is centered between the two borders
            int PositionX = ((BorderRight - BorderLeft) - (str.Length * (CHAR_WIDTH + CHAR_SPACE) - CHAR_SPACE)) / 2;

            DrawString(PositionX, PositionY, str);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void DrawCenteredY(int PositionX, int BorderUp, int BorderDown, string pString, params object[] args)
        {
            string str = (args != null && args.Length > 0) ? string.Format(pString, args) : pString;

            // Compute Y position so that the string we write is centered between the two borders
            int PositionY = ((BorderDown - BorderUp) - CHAR_HEIGHT) / 2;

            DrawString(PositionX, PositionY, str);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void DrawCenteredXY(int BorderUp, int BorderDown, int BorderLeft, int BorderRight, string pString, params object[] args)
        {
            string str = (args != null && args.Length > 0) ? string.Format(pString, args) : pString;

            int PositionX = ((BorderRight - BorderLeft) - (str.Length * (CHAR_WIDTH + CHAR_SPACE) - CHAR_SPACE)) / 2;
            int PositionY = ((BorderDown - BorderUp) - CHAR_HEIGHT) / 2;

            DrawString(PositionX, PositionY, str);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void DrawString(int PositionX, int PositionY, string pString)
        {
            foreach (char c in pString)
            {
                int characterOffset;

                if (c >= 'a' && c <= 'z')
                {
                    characterOffset = LETTERS_CHAR_OFFSET_BEGIN + (c - 'a');
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    characterOffset = LETTERS_CHAR_OFFSET_BEGIN + (c - 'A');
                }
                else if (c >= '0' && c <= '9')
                {
                    characterOffset = NUMBERS_CHAR_OFFSET_BEGIN + (c - '0');
                }
                else
                {
                    switch (c)
                    {
                        case '.': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + PERIOD_CHAR_OFFSET;            break;
                        case ',': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + COMMA_CHAR_OFFSET;             break;
                        case '!': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + EXCLAMATION_CHAR_OFFSET;       break;
                        case '?': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + INTERROGATIVE_CHAR_OFFSET;     break;
                        case '(': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + LEFTPARENTHESIS_CHAR_OFFSET;   break;
                        case ')': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + RIGHTPARENTHESIS_CHAR_OFFSET;  break;
                        case '-': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + MINUS_CHAR_OFFSET;             break;
                        case '+': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + PLUS_CHAR_OFFSET;              break;
                        case '@': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + AT_CHAR_OFFSET;                break;
                        case ':': characterOffset = SPECIAL_CHAR_OFFSET_BEGIN + COLON_CHAR_OFFSET;             break;
                        default:  characterOffset = -1; break; // Unsupported character
                    }
                }

                // If the character to draw is supported
                if (characterOffset != -1)
                {
                    // Draw the text character
                    m_pDisplay.DrawSprite(
                        PositionX,
                        PositionY,
                        null,
                        null,
                        BmpId.BMP_GLOBAL_FONT,
                        m_TextColorOffset + characterOffset,
                        m_SpriteLayer,
                        TEXT_PRIORITY);

                    // If we have to draw a shadow under the text
                    if (m_DrawShadow)
                    {
                        m_pDisplay.DrawSprite(
                            PositionX + m_ShadowOffsetX,
                            PositionY + m_ShadowOffsetY,
                            null,
                            null,
                            BmpId.BMP_GLOBAL_FONT,
                            m_ShadowColorOffset + characterOffset,
                            m_SpriteLayer,
                            SHADOW_PRIORITY);
                    }
                }

                // Update position where to draw the next character
                PositionX += CHAR_WIDTH + CHAR_SPACE;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private int GetColorOffset(EFontColor FontColor)
        {
            switch (FontColor)
            {
                case EFontColor.FONTCOLOR_BLUE:   return 0 * CHAR_COUNT_PER_FONTCOLOR;
                case EFontColor.FONTCOLOR_YELLOW: return 1 * CHAR_COUNT_PER_FONTCOLOR;
                case EFontColor.FONTCOLOR_RED:    return 2 * CHAR_COUNT_PER_FONTCOLOR;
                case EFontColor.FONTCOLOR_GREEN:  return 3 * CHAR_COUNT_PER_FONTCOLOR;
                case EFontColor.FONTCOLOR_WHITE:  return 4 * CHAR_COUNT_PER_FONTCOLOR;
                case EFontColor.FONTCOLOR_BLACK:  return 5 * CHAR_COUNT_PER_FONTCOLOR;
            }
            return 0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
