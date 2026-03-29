/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007, 2010 Bernd Arnold
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
 *  \file CWinner.cs
 *  \brief The winner screen (after a match has ended and one bomber has won the match)
 */

using System.Diagnostics;

namespace Bombermaaan
{
    /// <summary>
    /// The winner (scoreboard) screen that appears after a player has won one battle.
    /// </summary>
    public class CWinner : CModeScreen
    {
        //----------------------------------------------------------------------
        // Constants
        //----------------------------------------------------------------------

        private const float WINNER_BLACKSCREEN_DURATION    = 0.750f;
        private const float WINNER_MINIMUM_DURATION        = 0.0f;

        private const int   WINNER_DISPLAY_ORIGIN_X        = 0;
        private const int   WINNER_DISPLAY_ORIGIN_Y        = 0;

        // WINNER_SPRITES_OFFSET_X/Y use VIEW_WIDTH/VIEW_HEIGHT — computed inline

        private const int   WINNER_SPRITE_LAYER            = 0;
        private const int   WINNER_LIGHTS_PRIORITY         = 1;
        private const int   WINNER_BOMBER_PRIORITY         = 1;
        private const int   WINNER_COIN_PRIORITY           = 1;
        private const int   WINNER_SCOREBOARD_PRIORITY     = 1;
        private const int   WINNER_CROSS_PRIORITY          = 1;

        private const int   SCOREBOARD_TITLE_POSITION_X    = 41;
        private const int   SCOREBOARD_TITLE_POSITION_Y    = 19;
        private const int   SCOREBOARD_SPRITE              = 0;

        private const int   LIGHTS_FULLROW1_POSITION_X     = 9;
        private const int   LIGHTS_FULLROW1_POSITION_Y     = 7;
        private const int   LIGHTS_FULLROW2_POSITION_X     = 9;
        private const int   LIGHTS_FULLROW2_POSITION_Y     = 42;
        private const int   LIGHTS_FULLROW3_POSITION_X     = 9;
        private const int   LIGHTS_FULLROW3_POSITION_Y     = 224;
        private const int   LIGHTS_FULLCOLUMN1_POSITION_X  = 9;
        private const int   LIGHTS_FULLCOLUMN1_POSITION_Y  = 7;
        private const int   LIGHTS_FULLCOLUMN2_POSITION_X  = 226;
        private const int   LIGHTS_FULLCOLUMN2_POSITION_Y  = 7;
        private const int   LIGHTS_SEMICOLUMN_POSITION_X   = 51;
        private const int   LIGHTS_SEMICOLUMN_POSITION_Y   = 42;
        private const int   LIGHTS_FULLROW_COUNT            = 32;
        private const int   LIGHTS_FULLCOLUMN_COUNT         = 32;
        private const int   LIGHTS_SEMICOLUMN_COUNT         = 27;
        private const int   LIGHTS_SPACE_X                  = 7;
        private const int   LIGHTS_SPACE_Y                  = 7;
        private const float LIGHTS_ANIMATION_TIME_0         = 0.150f;
        private const float LIGHTS_ANIMATION_TIME_1         = LIGHTS_ANIMATION_TIME_0 * 2;
        private const float LIGHTS_ANIMATION_TIME_2         = LIGHTS_ANIMATION_TIME_0 * 3;
        private const float LIGHTS_ANIMATION_TIME_3         = LIGHTS_ANIMATION_TIME_0 * 4;
        private const int   LIGHTS_COLORS_COUNT             = 4;

        private const int   BOMBER_INITIAL_POSITION_X       = 20;
        private const int   BOMBER_INITIAL_POSITION_Y       = 55;
        private const int   BOMBER_SPACE_Y                  = 33;
        private const int   BOMBER_HAPPY_SPRITE_0           = 0;
        private const int   BOMBER_HAPPY_SPRITE_1           = 1;
        private const int   BOMBER_SAD_SPRITE_0             = 2;
        private const int   BOMBER_SAD_SPRITE_1             = 3;
        private const int   BOMBER_SPRITES_COUNT_PER_COLOR  = 4;
        private const float BOMBER_HAPPY_ANIMATION_TIME_0   = 0.700f;
        private const float BOMBER_HAPPY_ANIMATION_TIME_1   = 1.600f;
        private const float BOMBER_SAD_ANIMATION_TIME_0     = 0.100f;
        private const float BOMBER_SAD_ANIMATION_TIME_1     = 0.250f;
        private const float BOMBER_SAD_ANIMATION_TIME_2     = 0.600f;
        private const float BOMBER_SAD_ANIMATION_TIME_3     = 1.200f;

        private const int   COINS_INITIAL_POSITION_X        = 68;
        private const int   COINS_INITIAL_POSITION_Y        = 61;
        private const int   COINS_SPACE_X                   = 31;
        private const int   COINS_SPACE_Y                   = 33;
        private const int   COINS_STATIC_SPRITE             = 0;
        private const float COINS_ANIMATION_TIME            = 0.2f;
        private const int   COINS_ANIMATION_TURNS           = 2;
        private const int   COINS_SPRITE_COUNT              = 16;

        private const int   CROSS_SPACE_X                   = 1;
        private const int   CROSS_SPACE_Y                   = 4;

        private const int   MOSAIC_SPRITE_LAYER                = 0;
        private const int   MOSAIC_SPRITE_PRIORITY_IN_LAYER    = 0;
        private const float MOSAIC_SPEED_X                     = 25.0f;
        private const float MOSAIC_SPEED_Y                     = -25.0f;

        //----------------------------------------------------------------------
        // Private members
        //----------------------------------------------------------------------

        private CScores     m_pScores;                  //!< Link to the scores object
        private CMatch      m_pMatch;                   //!< Link to the match object
        private float       m_LightsTimer;              //!< Lights animation timer
        private float       m_HappyBomberTimer;         //!< Happy bomber animation timer
        private float       m_SadBomberTimer;           //!< Sad bomber animation timer
        private int         m_LightSpriteOffset;        //!< Sprite offset for light colors
        private int         m_HappyBomberSpriteOffset;  //!< Sprite offset for happy bomber
        private int         m_SadBomberSpriteOffset;    //!< Sprite offset for sad bomber
        private bool        m_PlayedSound;              //!< Did we start playing the victory sound?
        private CMosaic     m_pMosaic;
        private float       m_ModeTime;                 //!< Time since mode started
        private float       m_ExitModeTime;             //!< Mode time for last black screen
        private float       m_CoinTime;                 //!< Time for current coin sprite
        private int         m_CoinSpriteOffset;         //!< Sprite offset of the coin
        private int         m_ExitGameMode;             //!< Game mode when exiting
        private bool        m_HaveToExit;               //!< Do we have to exit?

        //----------------------------------------------------------------------
        // Constructor / Destructor
        //----------------------------------------------------------------------

        public CWinner() : base()
        {
            m_pScores = null;
            m_pMatch = null;
            m_LightSpriteOffset = 0;
            m_HappyBomberSpriteOffset = 0;
            m_SadBomberSpriteOffset = 0;
            m_LightsTimer = 0;
            m_HappyBomberTimer = 0;
            m_SadBomberTimer = 0;
            m_PlayedSound = false;
            m_pMosaic = null;
            m_ModeTime = 0.0f;
            m_ExitModeTime = 0.0f;
            m_CoinTime = 0.0f;
            m_CoinSpriteOffset = 0;
            m_HaveToExit = false;
            m_ExitGameMode = (int)EGameMode.GAMEMODE_NONE;
        }

        //----------------------------------------------------------------------
        // SetScores / SetMatch
        //----------------------------------------------------------------------

        public void SetScores(CScores pScores) { m_pScores = pScores; }
        public void SetMatch(CMatch pMatch)     { m_pMatch = pMatch; }

        //----------------------------------------------------------------------
        // Create
        //----------------------------------------------------------------------

        public override void Create()
        {
            base.Create();

            Debug.Assert(m_pScores != null);
            Debug.Assert(m_pMatch != null);

            m_LightsTimer = 0.0f;
            m_HappyBomberTimer = 0.0f;
            m_SadBomberTimer = 0.0f;
            m_ModeTime = 0.0f;
            m_CoinTime = 0.0f;
            m_CoinSpriteOffset = 0;
            m_HaveToExit = false;
            m_LightSpriteOffset = 0;
            m_HappyBomberSpriteOffset = BOMBER_HAPPY_SPRITE_0;
            m_SadBomberSpriteOffset = BOMBER_SAD_SPRITE_0;

            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                if (m_pMatch.IsPlayerWinner(Player))
                    m_pScores.RaisePlayerScore(Player);
            }

            m_PlayedSound = false;

            var mosaicType = EMosaicType.MOSAICTYPE_BOMB;

            if (m_pScores.IsFinalRound())
                mosaicType = EMosaicType.MOSAICTYPE_FLAME;

            if (m_pScores.IsFirstScore())
                mosaicType = EMosaicType.MOSAICTYPE_CHAR;

            m_pMosaic = CRandomMosaic.CreateRandomMosaic(m_pDisplay,
                                                         MOSAIC_SPRITE_LAYER,
                                                         MOSAIC_SPRITE_PRIORITY_IN_LAYER,
                                                         MOSAIC_SPEED_X,
                                                         MOSAIC_SPEED_Y,
                                                         EMosaicColor.MOSAICCOLOR_GREEN,
                                                         mosaicType);
        }

        //----------------------------------------------------------------------
        // Destroy
        //----------------------------------------------------------------------

        public override void Destroy()
        {
            base.Destroy();
            m_pMosaic.Destroy();
            m_pMosaic = null;
        }

        //----------------------------------------------------------------------
        // OpenInput / CloseInput
        //----------------------------------------------------------------------

        public override void OpenInput()
        {
            m_pInput.GetMainInput().Open();
            for (int i = 0; i < Globals.MAX_PLAYERS; i++)
                m_pInput.GetPlayerInput(m_pOptions.GetPlayerInput(i)).Open();
        }

        public override void CloseInput()
        {
            m_pInput.GetMainInput().Close();
            for (int i = 0; i < Globals.MAX_PLAYERS; i++)
                m_pInput.GetPlayerInput(m_pOptions.GetPlayerInput(i)).Close();
        }

        //----------------------------------------------------------------------
        // Update
        //----------------------------------------------------------------------

        public override EGameMode Update()
        {
            m_ModeTime += m_pTimer.GetDeltaTime();

            if (m_ModeTime <= WINNER_BLACKSCREEN_DURATION)
            {
                // First black screen
            }
            else if (m_ModeTime <= WINNER_MINIMUM_DURATION || !m_HaveToExit)
            {
                m_pMosaic.Update(m_pTimer.GetDeltaTime());

                if (!m_PlayedSound)
                {
                    m_pSound.PlaySample(ESample.SAMPLE_WINNER);
                    m_PlayedSound = true;
                }

                if (m_ModeTime >= WINNER_MINIMUM_DURATION)
                {
                    bool LeaveScreen = false;

                    for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                    {
                        if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_MAN)
                        {
                            int PlayerInputNr = m_pOptions.GetPlayerInput(Player);
                            if (m_pInput.GetPlayerInput(PlayerInputNr).IsOpened())
                            {
                                m_pInput.GetPlayerInput(PlayerInputNr).Update();
                                LeaveScreen |= m_pInput.GetPlayerInput(PlayerInputNr).TestMenuNext();
                            }
                        }
                    }

                    LeaveScreen |= m_pInput.GetMainInput().TestNext();

                    if (LeaveScreen)
                    {
                        m_HaveToExit = true;
                        m_ExitModeTime = m_ModeTime;
                    }
                }

                // Animate lights
                if      (m_LightsTimer < LIGHTS_ANIMATION_TIME_0) m_LightSpriteOffset = 0;
                else if (m_LightsTimer < LIGHTS_ANIMATION_TIME_1) m_LightSpriteOffset = 1;
                else if (m_LightsTimer < LIGHTS_ANIMATION_TIME_2) m_LightSpriteOffset = 2;
                else if (m_LightsTimer < LIGHTS_ANIMATION_TIME_3) m_LightSpriteOffset = 3;
                else
                {
                    m_LightSpriteOffset = 0;
                    m_LightsTimer = 0.0f;
                }
                m_LightsTimer += m_pTimer.GetDeltaTime();

                // Animate happy bomber
                if      (m_HappyBomberTimer < BOMBER_HAPPY_ANIMATION_TIME_0) m_HappyBomberSpriteOffset = BOMBER_HAPPY_SPRITE_0;
                else if (m_HappyBomberTimer < BOMBER_HAPPY_ANIMATION_TIME_1) m_HappyBomberSpriteOffset = BOMBER_HAPPY_SPRITE_1;
                else
                {
                    m_HappyBomberSpriteOffset = BOMBER_HAPPY_SPRITE_0;
                    m_HappyBomberTimer = 0.0f;
                }
                m_HappyBomberTimer += m_pTimer.GetDeltaTime();

                // Animate sad bomber
                if      (m_SadBomberTimer < BOMBER_SAD_ANIMATION_TIME_0) m_SadBomberSpriteOffset = BOMBER_SAD_SPRITE_1;
                else if (m_SadBomberTimer < BOMBER_SAD_ANIMATION_TIME_1) m_SadBomberSpriteOffset = BOMBER_SAD_SPRITE_0;
                else if (m_SadBomberTimer < BOMBER_SAD_ANIMATION_TIME_2) m_SadBomberSpriteOffset = BOMBER_SAD_SPRITE_1;
                else if (m_SadBomberTimer < BOMBER_SAD_ANIMATION_TIME_3) m_SadBomberSpriteOffset = BOMBER_SAD_SPRITE_0;
                else
                {
                    m_SadBomberSpriteOffset = BOMBER_SAD_SPRITE_1;
                    m_SadBomberTimer = 0.0f;
                }
                m_SadBomberTimer += m_pTimer.GetDeltaTime();

                // Animate coin
                if (m_CoinSpriteOffset % COINS_SPRITE_COUNT == COINS_STATIC_SPRITE &&
                    m_CoinSpriteOffset >= COINS_ANIMATION_TURNS * COINS_SPRITE_COUNT)
                {
                    // Don't animate coin any longer
                }
                else
                {
                    m_CoinTime += m_pTimer.GetDeltaTime();
                    while (m_CoinTime >= COINS_ANIMATION_TIME)
                    {
                        m_CoinTime -= COINS_ANIMATION_TIME;
                        if (m_CoinSpriteOffset % COINS_SPRITE_COUNT != COINS_STATIC_SPRITE ||
                            m_CoinSpriteOffset < COINS_ANIMATION_TURNS * COINS_SPRITE_COUNT)
                        {
                            m_CoinSpriteOffset++;
                        }
                    }
                }
            }
            else if (m_ModeTime - m_ExitModeTime <= WINNER_BLACKSCREEN_DURATION)
            {
                // Last black screen
            }
            else
            {
                for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                {
                    if (m_pScores.GetPlayerScore(Player) == m_pOptions.GetBattleCount())
                        return EGameMode.GAMEMODE_VICTORY;
                }

                return EGameMode.GAMEMODE_MATCH;
            }

            return EGameMode.GAMEMODE_WINNER;
        }

        //----------------------------------------------------------------------
        // Display
        //----------------------------------------------------------------------

        public override void Display()
        {
            if (m_ModeTime <= WINNER_BLACKSCREEN_DURATION)
            {
                // First black screen
            }
            else if (m_ModeTime <= WINNER_MINIMUM_DURATION || !m_HaveToExit)
            {
                m_pDisplay.SetOrigin(WINNER_DISPLAY_ORIGIN_X, WINNER_DISPLAY_ORIGIN_Y);
                m_pMosaic.Display();

                int SpritesOffsetX = (Globals.VIEW_WIDTH - 240) / 2;
                int SpritesOffsetY = (Globals.VIEW_HEIGHT - 234) / 2;
                m_pDisplay.SetOrigin(WINNER_DISPLAY_ORIGIN_X + SpritesOffsetX,
                                     WINNER_DISPLAY_ORIGIN_Y + SpritesOffsetY);

                m_pDisplay.DrawSprite(SCOREBOARD_TITLE_POSITION_X,
                                      SCOREBOARD_TITLE_POSITION_Y,
                                      null, null,
                                      BmpId.BMP_WINNER_TITLE,
                                      SCOREBOARD_SPRITE,
                                      WINNER_SPRITE_LAYER,
                                      WINNER_SCOREBOARD_PRIORITY);

                int Light = 0;
                for (int Column = 0; Column < LIGHTS_FULLROW_COUNT; Column++)
                {
                    m_pDisplay.DrawSprite(LIGHTS_FULLROW1_POSITION_X + Column * LIGHTS_SPACE_X,
                                          LIGHTS_FULLROW1_POSITION_Y,
                                          null, null,
                                          BmpId.BMP_WINNER_LIGHTS,
                                          (m_LightSpriteOffset + Light) % LIGHTS_COLORS_COUNT,
                                          WINNER_SPRITE_LAYER,
                                          WINNER_LIGHTS_PRIORITY);

                    m_pDisplay.DrawSprite(LIGHTS_FULLROW2_POSITION_X + Column * LIGHTS_SPACE_X,
                                          LIGHTS_FULLROW2_POSITION_Y,
                                          null, null,
                                          BmpId.BMP_WINNER_LIGHTS,
                                          (m_LightSpriteOffset + Light) % LIGHTS_COLORS_COUNT,
                                          WINNER_SPRITE_LAYER,
                                          WINNER_LIGHTS_PRIORITY);

                    m_pDisplay.DrawSprite(LIGHTS_FULLROW3_POSITION_X + Column * LIGHTS_SPACE_X,
                                          LIGHTS_FULLROW3_POSITION_Y,
                                          null, null,
                                          BmpId.BMP_WINNER_LIGHTS,
                                          (m_LightSpriteOffset + Light) % LIGHTS_COLORS_COUNT,
                                          WINNER_SPRITE_LAYER,
                                          WINNER_LIGHTS_PRIORITY);

                    Light++;
                }

                Light = 0;
                for (int Row = 0; Row < LIGHTS_FULLCOLUMN_COUNT; Row++)
                {
                    m_pDisplay.DrawSprite(LIGHTS_FULLCOLUMN1_POSITION_X,
                                          LIGHTS_FULLCOLUMN1_POSITION_Y + Row * LIGHTS_SPACE_Y,
                                          null, null,
                                          BmpId.BMP_WINNER_LIGHTS,
                                          (m_LightSpriteOffset + Light) % LIGHTS_COLORS_COUNT,
                                          WINNER_SPRITE_LAYER,
                                          WINNER_LIGHTS_PRIORITY);

                    m_pDisplay.DrawSprite(LIGHTS_FULLCOLUMN2_POSITION_X,
                                          LIGHTS_FULLCOLUMN2_POSITION_Y + Row * LIGHTS_SPACE_Y,
                                          null, null,
                                          BmpId.BMP_WINNER_LIGHTS,
                                          (m_LightSpriteOffset + Light) % LIGHTS_COLORS_COUNT,
                                          WINNER_SPRITE_LAYER,
                                          WINNER_LIGHTS_PRIORITY);

                    Light++;
                }

                Light = 0;
                for (int Row = 0; Row < LIGHTS_SEMICOLUMN_COUNT; Row++)
                {
                    m_pDisplay.DrawSprite(LIGHTS_SEMICOLUMN_POSITION_X,
                                          LIGHTS_SEMICOLUMN_POSITION_Y + Row * LIGHTS_SPACE_Y,
                                          null, null,
                                          BmpId.BMP_WINNER_LIGHTS,
                                          (m_LightSpriteOffset + Light) % LIGHTS_COLORS_COUNT,
                                          WINNER_SPRITE_LAYER,
                                          WINNER_LIGHTS_PRIORITY);
                    Light++;
                }

                for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                {
                    if (m_pOptions.GetBomberType(Player) != EBomberType.BOMBERTYPE_OFF)
                    {
                        int BomberSprite = Player * BOMBER_SPRITES_COUNT_PER_COLOR
                                         + (m_pMatch.IsPlayerWinner(Player)
                                               ? m_HappyBomberSpriteOffset
                                               : m_SadBomberSpriteOffset);

                        m_pDisplay.DrawSprite(BOMBER_INITIAL_POSITION_X,
                                              BOMBER_INITIAL_POSITION_Y + Player * BOMBER_SPACE_Y,
                                              null, null,
                                              BmpId.BMP_WINNER_BOMBER,
                                              BomberSprite,
                                              WINNER_SPRITE_LAYER,
                                              WINNER_BOMBER_PRIORITY);

                        for (int Coin = 0; Coin < m_pScores.GetPlayerScore(Player); Coin++)
                        {
                            int currentCoinSprite = COINS_STATIC_SPRITE;
                            if (Coin + 1 == m_pScores.GetPlayerScore(Player) && m_pMatch.IsPlayerWinner(Player))
                                currentCoinSprite = m_CoinSpriteOffset % COINS_SPRITE_COUNT;

                            m_pDisplay.DrawSprite(COINS_INITIAL_POSITION_X + Coin * COINS_SPACE_X,
                                                  COINS_INITIAL_POSITION_Y + Player * COINS_SPACE_Y,
                                                  null, null,
                                                  BmpId.BMP_WINNER_COIN,
                                                  currentCoinSprite,
                                                  WINNER_SPRITE_LAYER,
                                                  WINNER_COIN_PRIORITY);
                        }
                    }
                    else
                    {
                        m_pDisplay.DrawSprite(BOMBER_INITIAL_POSITION_X + CROSS_SPACE_X,
                                              BOMBER_INITIAL_POSITION_Y + CROSS_SPACE_Y + Player * BOMBER_SPACE_Y,
                                              null, null,
                                              BmpId.BMP_WINNER_CROSS,
                                              Player,
                                              WINNER_SPRITE_LAYER,
                                              WINNER_CROSS_PRIORITY);
                    }
                }
            }
            else if (m_ModeTime - m_ExitModeTime <= WINNER_BLACKSCREEN_DURATION)
            {
                // Last black screen
            }
        }
    }
}
