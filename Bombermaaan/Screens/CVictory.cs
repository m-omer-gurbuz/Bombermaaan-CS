/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007, 2008 Bernd Arnold
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
 *  \file CVictory.cs
 *  \brief The victory screen (after a player has won all matches)
 */

using System;
using System.Diagnostics;

namespace Bombermaaan
{
    //----------------------------------------------------------------------
    // Enums / Structs
    //----------------------------------------------------------------------

    /// <summary>Describes the size of a confetti</summary>
    public enum EConfetti
    {
        CONFETTI_LARGE,     //!< Large size
        CONFETTI_MEDIUM,    //!< Medium size
        CONFETTI_SMALL      //!< Small size
    }

    /// <summary>Describes the state of a confetti</summary>
    public struct SConfetti
    {
        public EConfetti Type;          //!< Type of the confetti
        public float PositionX;         //!< Position X (in pixels) in the game view
        public float PositionY;         //!< Position Y (in pixels) in the game view
        public float SpeedX;            //!< Speed (in pixels per second) on the X-axis
        public float SpeedY;            //!< Speed (in pixels per second) on the Y-axis
        public float AnimationTimer;    //!< Timer of the loop animation of the confetti
        public int   Sprite;            //!< Sprite offset number
    }

    /// <summary>Describes the different modes of the crowd's wave</summary>
    public enum ECrowdWave
    {
        CROWDWAVE_NONE,         //!< A lazy crowd
        CROWDWAVE_CLASSIC,      //!< The wave as it was up to version 1.02
        CROWDWAVE_MEXICAN,      //!< The mexican wave
        NUMBER_CROWDWAVES       //!< The number of different crowd waves
    }

    /// <summary>
    /// The victory screen that appears after a player has won a match.
    /// </summary>
    public class CVictory : CModeScreen
    {
        //----------------------------------------------------------------------
        // Constants
        //----------------------------------------------------------------------

        private const int   NUM_CONFETTIS_LARGE    = 6;
        private const int   NUM_CONFETTIS_MEDIUM   = 10;
        private const int   NUM_CONFETTIS_SMALL    = 6;
        private const int   NUM_CONFETTIS          = NUM_CONFETTIS_LARGE + NUM_CONFETTIS_MEDIUM + NUM_CONFETTIS_SMALL;

        private const float VICTORY_BLACKSCREEN_DURATION   = 0.750f;
        private const float VICTORY_MINIMUM_DURATION       = 0.0f;
        private const float VICTORY_SCREEN_DURATION        = 10.0f;

        private const int   VICTORY_VIEW_WIDTH     = 15 * 32;
        private const int   VICTORY_VIEW_HEIGHT    = 26 + 13 * 32;

        private const int   VICTORY_DISPLAY_ORIGIN_X   = 0;
        private const int   VICTORY_DISPLAY_ORIGIN_Y   = 0;

        private const int   VICTORYWALL_TILE_SPRITE     = 0;
        private const int   VICTORYWALL_TILES_COUNT     = 16;
        private const int   VICTORYWALL_TILES_INITIAL_X = -9;
        private const int   VICTORYWALL_TILES_INITIAL_Y = 100;
        private const int   VICTORYWALL_TILE_SPACE_X    = 32;

        private const int   CROWD_TILES_COUNT_X         = 46;
        private const int   CROWD_TILES_COUNT_Y         = 8;
        private const int   CROWD_STATES_COUNT          = 2;
        private const int   CROWD_OFFSET_GETUP          = -1;
        private const int   CROWD_OFFSET_SITDOWN        = 1;
        private const int   CROWD_OFFSET_MOVING         = 0;
        private const int   CROWD_INITIAL_TILE_X        = -4;
        private const int   CROWD_INITIAL_TILE_Y        = 0;
        private const int   CROWD_TILE_SIZE_X           = 14;
        private const int   CROWD_TILE_SIZE_Y           = 16;
        private const int   CROWD_TILES_SPACE_X         = CROWD_TILE_SIZE_X - 3;
        private const int   CROWD_TILES_SPACE_Y         = CROWD_TILE_SIZE_Y - 4;
        private const float CROWD_ANIMATION_TIME_0      = 0.300f;
        private const float CROWD_ANIMATION_TIME_1      = CROWD_ANIMATION_TIME_0 * 2;
        private const int   CROWD_COLORS_COUNT          = 9;
        private const float MEXICAN_WAVE_ANIMATION_TIME = 0.07f;

        private const float WINNER_BOMBER_ANIMATION_TIME_0  = 2.000f;
        private const float WINNER_BOMBER_ANIMATION_TIME_1  = WINNER_BOMBER_ANIMATION_TIME_0 + 0.250f;
        private const float WINNER_BOMBER_ANIMATION_TIME_2  = WINNER_BOMBER_ANIMATION_TIME_1 + 0.040f;
        private const float WINNER_BOMBER_ANIMATION_TIME_3  = WINNER_BOMBER_ANIMATION_TIME_2 + 0.030f;
        private const float WINNER_BOMBER_ANIMATION_TIME_4  = WINNER_BOMBER_ANIMATION_TIME_3 + 0.030f;
        private const float WINNER_BOMBER_ANIMATION_TIME_5  = WINNER_BOMBER_ANIMATION_TIME_4 + 0.030f;
        private const float WINNER_BOMBER_ANIMATION_TIME_6  = WINNER_BOMBER_ANIMATION_TIME_5 + 0.030f;
        private const float WINNER_BOMBER_ANIMATION_TIME_7  = WINNER_BOMBER_ANIMATION_TIME_6 + 0.030f;
        private const float WINNER_BOMBER_ANIMATION_TIME_8  = WINNER_BOMBER_ANIMATION_TIME_7 + 0.040f;
        private const float WINNER_BOMBER_ANIMATION_TIME_9  = WINNER_BOMBER_ANIMATION_TIME_8 + 0.050f;
        private const float WINNER_BOMBER_ANIMATION_TIME_10 = WINNER_BOMBER_ANIMATION_TIME_9 + 0.250f;
        private const float WINNER_BOMBER_ANIMATION_TIME_11 = WINNER_BOMBER_ANIMATION_TIME_10 + 0.300f;
        private const int   WINNER_BOMBER_SPRITE_0  = 0;
        private const int   WINNER_BOMBER_SPRITE_1  = 1;
        private const int   WINNER_BOMBER_SPRITE_2  = 2;
        private const int   WINNER_BOMBER_SPRITE_3  = 3;
        private const int   WINNER_BOMBER_SPRITE_4  = 4;
        private const int   WINNER_BOMBER_SPRITE_5  = 5;
        private const int   WINNER_BOMBER_SPRITE_6  = 6;
        private const int   WINNER_BOMBER_SPRITE_7  = 7;
        private const int   WINNER_BOMBER_SPRITE_8  = 8;
        private const int   WINNER_BOMBER_SPRITE_9  = 9;
        private const int   WINNER_BOMBER_SPRITE_10 = 10;
        private const float LOSER_BOMBER_ANIMATION_TIME_0   = 4.000f;
        private const float LOSER_BOMBER_ANIMATION_TIME_1   = 4.100f;
        private const int   LOSER_BOMBER_SPRITE_0   = 11;
        private const int   LOSER_BOMBER_SPRITE_1   = 12;
        private const int   LOSER_BOMBER_SPRITE_2   = 13;
        private const int   LOSER_BOMBER_SPACE_X    = 30;
        private const int   WINNER_BOMBER_SPACE_X   = 30;
        private const int   LOSER_BOMBER_SPACE_EDGE = 10;
        private const int   WINNER_BOMBER_POSITION_Y    = 220;
        private const int   LOSER_BOMBER_POSITION_Y     = 120;
        private const int   BOMBER_SPRITES_COUNT_PER_COLOR = 14;

        private const float CONFETTI_LIMIT_LEFT     = -20.0f;
        // CONFETTI_LIMIT_RIGHT and CONFETTI_LIMIT_BOTTOM are runtime values using VIEW_WIDTH/VIEW_HEIGHT
        private const float CONFETTI_RESET_POSITION_Y   = -20.0f;
        private const float CONFETTI_ANIMATION_TIME_0   = 1 * 0.200f;
        private const float CONFETTI_ANIMATION_TIME_1   = 2 * 0.200f;
        private const float CONFETTI_ANIMATION_TIME_2   = 3 * 0.200f;
        private const float CONFETTI_ANIMATION_TIME_3   = 4 * 0.200f;
        private const float CONFETTI_ANIMATION_TIME_4   = 5 * 0.200f;
        private const int   CONFETTI_ANIMATION_SPRITE_0 = 0;
        private const int   CONFETTI_ANIMATION_SPRITE_1 = 1;
        private const int   CONFETTI_ANIMATION_SPRITE_2 = 2;
        private const int   CONFETTI_ANIMATION_SPRITE_3 = 3;
        private const int   CONFETTI_ANIMATION_SPRITE_4 = 4;
        private const int   CONFETTIS_COUNT_PER_COLOR   = 5;

        private const int   VICTORY_TITLE_SPRITE        = 0;
        private const int   VICTORY_TITLE_POSITION_X    = 25;
        private const int   VICTORY_TITLE_POSITION_Y    = 11;

        private const int   VICTORY_CROWD_LAYER         = 0;
        private const int   VICTORY_WALL_LAYER          = 1;
        private const int   VICTORY_BOMBER_LAYER        = 2;
        private const int   VICTORY_CONFETTIS_LAYER     = 3;
        private const int   VICTORY_TITLE_LAYER         = 4;

        //----------------------------------------------------------------------
        // Private members
        //----------------------------------------------------------------------

        private CScores     m_pScores;                      //!< Link to the scores object
        private float       m_ModeTime;                     //!< Time since mode started
        private float       m_CrowdTimer;                   //!< Crowd animation timer
        private float       m_WinnerBomberTimer;            //!< Victorious bomber animation timer
        private float       m_LoserBomberTimer;             //!< Loser bomber animation timer
        private SConfetti[] m_Confettis;                    //!< The confettis to manage
        private bool        m_HaveToExit;                   //!< Do we have to exit?
        private float       m_ExitModeTime;                 //!< Mode time when we decided to exit
        private bool        m_PlayedSound;                  //!< Did we start playing the victory sound?
        private bool        m_CrowdFlag;                    //!< Crowd state flag
        private int         m_WinnerBomberSprite;           //!< Current winner bomber sprite offset
        private int         m_LoserBomberSprite;            //!< Current loser bomber sprite offset
        private float       m_MexicanWaveTimer;             //!< Timer for Mexican wave
        private int         m_MexicanWavePosition;          //!< Current position of Mexican wave
        private ECrowdWave  m_CrowdWaveMode;                //!< Which wave the crowd is doing

        private static readonly Random s_Random = new Random();

        //----------------------------------------------------------------------
        // Constructor / Destructor
        //----------------------------------------------------------------------

        public CVictory() : base()
        {
            m_pScores = null;
            m_ModeTime = 0.0f;
            m_CrowdTimer = 0.0f;
            m_WinnerBomberTimer = 0.0f;
            m_LoserBomberTimer = 0.0f;
            m_MexicanWaveTimer = 0.0f;
            m_MexicanWavePosition = -1;
            m_CrowdWaveMode = (ECrowdWave)(CRandom.Random((int)ECrowdWave.NUMBER_CROWDWAVES));
            m_HaveToExit = false;
            m_ExitModeTime = 0.0f;
            m_PlayedSound = false;
            m_CrowdFlag = false;
            m_WinnerBomberSprite = 0;
            m_LoserBomberSprite = 0;
            m_Confettis = new SConfetti[NUM_CONFETTIS];
        }

        //----------------------------------------------------------------------
        // SetScores
        //----------------------------------------------------------------------

        public void SetScores(CScores pScores)
        {
            m_pScores = pScores;
        }

        //----------------------------------------------------------------------
        // Create
        //----------------------------------------------------------------------

        public override void Create()
        {
            base.Create();

            Debug.Assert(m_pScores != null);

            m_ModeTime = 0.0f;
            m_CrowdTimer = 0.0f;
            m_WinnerBomberTimer = 0.0f;
            m_LoserBomberTimer = 0.0f;
            m_MexicanWaveTimer = 0.0f;
            m_MexicanWavePosition = -1;
            m_CrowdWaveMode = (ECrowdWave)(CRandom.Random((int)ECrowdWave.NUMBER_CROWDWAVES));
            m_HaveToExit = false;
            m_ExitModeTime = 0.0f;
            m_PlayedSound = false;

            int Confetti = 0;
            while (Confetti < NUM_CONFETTIS_LARGE)
            {
                m_Confettis[Confetti].Type = EConfetti.CONFETTI_LARGE;
                Confetti++;
            }
            while (Confetti < NUM_CONFETTIS_LARGE + NUM_CONFETTIS_MEDIUM)
            {
                m_Confettis[Confetti].Type = EConfetti.CONFETTI_MEDIUM;
                Confetti++;
            }
            while (Confetti < NUM_CONFETTIS_LARGE + NUM_CONFETTIS_MEDIUM + NUM_CONFETTIS_SMALL)
            {
                m_Confettis[Confetti].Type = EConfetti.CONFETTI_MEDIUM;
                Confetti++;
            }

            for (Confetti = 0; Confetti < NUM_CONFETTIS; Confetti++)
            {
                ResetConfetti(ref m_Confettis[Confetti]);
            }
        }

        //----------------------------------------------------------------------
        // Destroy
        //----------------------------------------------------------------------

        public override void Destroy()
        {
            base.Destroy();
            StopSong();
        }

        //----------------------------------------------------------------------
        // OpenInput / CloseInput
        //----------------------------------------------------------------------

        public override void OpenInput()
        {
            m_pInput.GetMainInput().Open();
        }

        public override void CloseInput()
        {
            m_pInput.GetMainInput().Close();
        }

        //----------------------------------------------------------------------
        // Update
        //----------------------------------------------------------------------

        public override EGameMode Update()
        {
            m_ModeTime += m_pTimer.GetDeltaTime();

            if (m_ModeTime <= VICTORY_BLACKSCREEN_DURATION)
            {
                // First black screen
            }
            else if (!m_HaveToExit)
            {
                if (!m_PlayedSound)
                {
                    m_pSound.PlaySample(ESample.SAMPLE_VICTORY);
                    m_pSound.PlaySample(ESample.SAMPLE_VICTORY_VOICE);
                    m_PlayedSound = true;
                }

                if (m_ModeTime >= VICTORY_MINIMUM_DURATION)
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

                if (m_ModeTime >= VICTORY_BLACKSCREEN_DURATION + VICTORY_SCREEN_DURATION)
                {
                    m_HaveToExit = true;
                    m_ExitModeTime = m_ModeTime;
                }

                // Animate crowd (classic style)
                if (m_CrowdWaveMode == ECrowdWave.CROWDWAVE_CLASSIC)
                {
                    if      (m_CrowdTimer < CROWD_ANIMATION_TIME_0) m_CrowdFlag = true;
                    else if (m_CrowdTimer < CROWD_ANIMATION_TIME_1) m_CrowdFlag = false;
                    else
                    {
                        m_CrowdTimer = 0.0f;
                        m_CrowdFlag = true;
                    }
                    m_CrowdTimer += m_pTimer.GetDeltaTime();
                }
                else if (m_CrowdWaveMode == ECrowdWave.CROWDWAVE_MEXICAN)
                {
                    if (m_MexicanWaveTimer > MEXICAN_WAVE_ANIMATION_TIME)
                    {
                        m_MexicanWavePosition++;
                        if (m_MexicanWavePosition > CROWD_TILES_COUNT_X)
                            m_MexicanWavePosition = -5;
                        m_MexicanWaveTimer = 0.0f;
                    }
                    m_MexicanWaveTimer += m_pTimer.GetDeltaTime();
                }

                // Animate victorious bomber
                if      (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_0)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_0;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_1)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_1;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_2)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_2;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_3)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_3;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_4)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_4;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_5)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_5;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_6)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_6;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_7)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_7;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_8)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_8;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_9)  m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_9;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_10) m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_1;
                else if (m_WinnerBomberTimer < WINNER_BOMBER_ANIMATION_TIME_11) m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_0;
                else
                    m_WinnerBomberSprite = WINNER_BOMBER_SPRITE_10;

                m_WinnerBomberTimer += m_pTimer.GetDeltaTime();

                // Animate losers
                if      (m_LoserBomberTimer < LOSER_BOMBER_ANIMATION_TIME_0) m_LoserBomberSprite = LOSER_BOMBER_SPRITE_0;
                else if (m_LoserBomberTimer < LOSER_BOMBER_ANIMATION_TIME_1) m_LoserBomberSprite = LOSER_BOMBER_SPRITE_1;
                else
                    m_LoserBomberSprite = LOSER_BOMBER_SPRITE_2;

                m_LoserBomberTimer += m_pTimer.GetDeltaTime();

                // Update confettis
                for (int Confetti = 0; Confetti < NUM_CONFETTIS; Confetti++)
                {
                    m_Confettis[Confetti].PositionX += m_Confettis[Confetti].SpeedX * m_pTimer.GetDeltaTime();
                    m_Confettis[Confetti].PositionY += m_Confettis[Confetti].SpeedY * m_pTimer.GetDeltaTime();

                    if (m_Confettis[Confetti].PositionX < CONFETTI_LIMIT_LEFT ||
                        m_Confettis[Confetti].PositionX > (float)(Globals.VIEW_WIDTH + 10) ||
                        m_Confettis[Confetti].PositionY > (float)(Globals.VIEW_HEIGHT + 10))
                    {
                        ResetConfetti(ref m_Confettis[Confetti]);
                    }
                    else
                    {
                        if      (m_Confettis[Confetti].AnimationTimer < CONFETTI_ANIMATION_TIME_0) m_Confettis[Confetti].Sprite = CONFETTI_ANIMATION_SPRITE_0;
                        else if (m_Confettis[Confetti].AnimationTimer < CONFETTI_ANIMATION_TIME_1) m_Confettis[Confetti].Sprite = CONFETTI_ANIMATION_SPRITE_1;
                        else if (m_Confettis[Confetti].AnimationTimer < CONFETTI_ANIMATION_TIME_2) m_Confettis[Confetti].Sprite = CONFETTI_ANIMATION_SPRITE_2;
                        else if (m_Confettis[Confetti].AnimationTimer < CONFETTI_ANIMATION_TIME_3) m_Confettis[Confetti].Sprite = CONFETTI_ANIMATION_SPRITE_3;
                        else if (m_Confettis[Confetti].AnimationTimer < CONFETTI_ANIMATION_TIME_4) m_Confettis[Confetti].Sprite = CONFETTI_ANIMATION_SPRITE_4;
                        else
                        {
                            m_Confettis[Confetti].AnimationTimer = 0.0f;
                            m_Confettis[Confetti].Sprite = CONFETTI_ANIMATION_SPRITE_0;
                        }

                        m_Confettis[Confetti].AnimationTimer += m_pTimer.GetDeltaTime();
                    }
                }
            }
            else if (m_ModeTime - m_ExitModeTime <= VICTORY_BLACKSCREEN_DURATION)
            {
                // Last black screen
            }
            else
            {
                m_pScores.Reset();
                return EGameMode.GAMEMODE_MENU;
            }

            return EGameMode.GAMEMODE_VICTORY;
        }

        //----------------------------------------------------------------------
        // Display
        //----------------------------------------------------------------------

        public override void Display()
        {
            if (m_ModeTime <= VICTORY_BLACKSCREEN_DURATION)
            {
                // First black screen
            }
            else if (!m_HaveToExit)
            {
                m_pDisplay.SetOrigin(VICTORY_DISPLAY_ORIGIN_X, VICTORY_DISPLAY_ORIGIN_Y);

                RECT Clip;
                Clip.left   = 0;
                Clip.top    = 0;
                Clip.right  = VICTORY_VIEW_WIDTH;
                Clip.bottom = VICTORY_VIEW_HEIGHT;

                int Color = 0;

                for (int TileX = 0; TileX < CROWD_TILES_COUNT_X; TileX++)
                {
                    for (int TileY = 0; TileY < CROWD_TILES_COUNT_Y; TileY++)
                    {
                        int OffsetY;

                        if (m_CrowdWaveMode == ECrowdWave.CROWDWAVE_CLASSIC)
                        {
                            if (((TileX + TileY) % CROWD_STATES_COUNT) == 0)
                                OffsetY = (m_CrowdFlag ? CROWD_OFFSET_GETUP : CROWD_OFFSET_SITDOWN);
                            else
                                OffsetY = (m_CrowdFlag ? CROWD_OFFSET_SITDOWN : CROWD_OFFSET_GETUP);
                        }
                        else if (m_CrowdWaveMode == ECrowdWave.CROWDWAVE_MEXICAN)
                        {
                            if (m_MexicanWavePosition == TileX)
                                OffsetY = CROWD_OFFSET_GETUP;
                            else if (TileX == m_MexicanWavePosition - 1 || TileX == m_MexicanWavePosition + 1)
                                OffsetY = CROWD_OFFSET_MOVING;
                            else
                                OffsetY = CROWD_OFFSET_SITDOWN;
                        }
                        else if (m_CrowdWaveMode == ECrowdWave.CROWDWAVE_NONE)
                        {
                            OffsetY = CROWD_OFFSET_SITDOWN;
                        }
                        else
                        {
                            OffsetY = CROWD_OFFSET_SITDOWN;
                            Debug.Assert(false);
                        }

                        m_pDisplay.DrawSprite(CROWD_INITIAL_TILE_X + TileX * CROWD_TILES_SPACE_X,
                                              CROWD_INITIAL_TILE_Y + TileY * CROWD_TILES_SPACE_Y + OffsetY,
                                              null, Clip,
                                              BmpId.BMP_VICTORY_CROWD,
                                              Color % CROWD_COLORS_COUNT,
                                              VICTORY_CROWD_LAYER,
                                              TileY);
                    }

                    if ((TileX + 1) % 2 == 0) Color++;
                }

                for (int TileX = 0; TileX < VICTORYWALL_TILES_COUNT; TileX++)
                {
                    m_pDisplay.DrawSprite(VICTORYWALL_TILES_INITIAL_X + TileX * VICTORYWALL_TILE_SPACE_X,
                                          VICTORYWALL_TILES_INITIAL_Y,
                                          null, Clip,
                                          BmpId.BMP_VICTORY_WALL,
                                          VICTORYWALL_TILE_SPRITE,
                                          VICTORY_WALL_LAYER,
                                          CDisplay.PRIORITY_UNUSED);
                }

                int WinnerBombersCount = 0;
                int LoserBombersCount  = 0;

                for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                {
                    if (m_pOptions.GetBomberType(Player) != EBomberType.BOMBERTYPE_OFF)
                    {
                        if (m_pScores.GetPlayerScore(Player) == m_pOptions.GetBattleCount())
                            WinnerBombersCount++;
                        else
                            LoserBombersCount++;
                    }
                }

                int LoserInitialX  = VICTORY_VIEW_WIDTH - (LoserBombersCount * LOSER_BOMBER_SPACE_X + LOSER_BOMBER_SPACE_EDGE);
                int WinnerInitialX = (LoserInitialX - WinnerBombersCount * WINNER_BOMBER_SPACE_X) / 2;

                WinnerBombersCount = 0;
                LoserBombersCount  = 0;

                for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                {
                    if (m_pOptions.GetBomberType(Player) != EBomberType.BOMBERTYPE_OFF)
                    {
                        if (m_pScores.GetPlayerScore(Player) == m_pOptions.GetBattleCount())
                        {
                            m_pDisplay.DrawSprite(WinnerInitialX + WinnerBombersCount * WINNER_BOMBER_SPACE_X,
                                                  WINNER_BOMBER_POSITION_Y,
                                                  null, null,
                                                  BmpId.BMP_VICTORY_BOMBER,
                                                  m_WinnerBomberSprite + Player * BOMBER_SPRITES_COUNT_PER_COLOR,
                                                  VICTORY_BOMBER_LAYER,
                                                  CDisplay.PRIORITY_UNUSED);
                            WinnerBombersCount++;
                        }
                        else
                        {
                            m_pDisplay.DrawSprite(LoserInitialX + LoserBombersCount * LOSER_BOMBER_SPACE_X,
                                                  LOSER_BOMBER_POSITION_Y,
                                                  null, null,
                                                  BmpId.BMP_VICTORY_BOMBER,
                                                  m_LoserBomberSprite + Player * BOMBER_SPRITES_COUNT_PER_COLOR,
                                                  VICTORY_BOMBER_LAYER,
                                                  CDisplay.PRIORITY_UNUSED);
                            LoserBombersCount++;
                        }
                    }
                }

                int WinnerPlayer = 0;
                for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                {
                    if (m_pOptions.GetBomberType(Player) != EBomberType.BOMBERTYPE_OFF)
                    {
                        if (m_pScores.GetPlayerScore(Player) == m_pOptions.GetBattleCount())
                        {
                            WinnerPlayer = Player;
                            break;
                        }
                    }
                }

                for (int Confetti = 0; Confetti < NUM_CONFETTIS; Confetti++)
                {
                    m_Confettis[Confetti].PositionX += m_Confettis[Confetti].SpeedX * m_pTimer.GetDeltaTime();
                    m_Confettis[Confetti].PositionY += m_Confettis[Confetti].SpeedY * m_pTimer.GetDeltaTime();

                    int ConfettiSpriteTable = -1;
                    switch (m_Confettis[Confetti].Type)
                    {
                        case EConfetti.CONFETTI_LARGE  : ConfettiSpriteTable = (int)BmpId.BMP_VICTORY_CONFETTIS_LARGE;  break;
                        case EConfetti.CONFETTI_MEDIUM : ConfettiSpriteTable = (int)BmpId.BMP_VICTORY_CONFETTIS_MEDIUM; break;
                        case EConfetti.CONFETTI_SMALL  : ConfettiSpriteTable = (int)BmpId.BMP_VICTORY_CONFETTIS_SMALL;  break;
                    }

                    Debug.Assert(ConfettiSpriteTable != -1);

                    m_pDisplay.DrawSprite((int)m_Confettis[Confetti].PositionX,
                                          (int)m_Confettis[Confetti].PositionY,
                                          null, Clip,
                                          ConfettiSpriteTable,
                                          WinnerPlayer * CONFETTIS_COUNT_PER_COLOR + m_Confettis[Confetti].Sprite,
                                          VICTORY_CONFETTIS_LAYER,
                                          CDisplay.PRIORITY_UNUSED);
                }

                m_pDisplay.DrawSprite(VICTORY_TITLE_POSITION_X,
                                      VICTORY_TITLE_POSITION_Y,
                                      null, Clip,
                                      BmpId.BMP_VICTORY_TITLE,
                                      VICTORY_TITLE_SPRITE,
                                      VICTORY_TITLE_LAYER,
                                      CDisplay.PRIORITY_UNUSED);
            }
            else if (m_ModeTime - m_ExitModeTime <= VICTORY_BLACKSCREEN_DURATION)
            {
                // Last black screen
            }
        }

        //----------------------------------------------------------------------
        // StopSong
        //----------------------------------------------------------------------

        public void StopSong()
        {
            m_pSound.StopAllSamples();
        }

        //----------------------------------------------------------------------
        // Private helpers
        //----------------------------------------------------------------------

        private void ResetConfetti(ref SConfetti pConfetti)
        {
            pConfetti.AnimationTimer = 0.0f;
            pConfetti.PositionX = (float)(CRandom.Random(Globals.VIEW_WIDTH));
            pConfetti.PositionY = CONFETTI_RESET_POSITION_Y;
            pConfetti.SpeedX    = (float)(CRandom.Random(110) - 70);
            pConfetti.SpeedY    = (float)(CRandom.Random(100) + 40);
            pConfetti.Sprite    = CONFETTI_ANIMATION_SPRITE_0;
        }
    }
}
