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
 *  \file CBoard.cs
 *  \brief The board during a match showing time and bombers' health and wins
 *         (C# port of CBoard.h / CBoard.cpp)
 */

namespace Bombermaaan
{
    //! The board that is above the arena in the match screen. It shows the scores and the time left.
    public class CBoard
    {
        //******************************************************************************************************************************
        // Constants
        //******************************************************************************************************************************

        // Sprites stuff
        private const int SPRITE_SEMICOLON            = 10;  // Sprite number of semicolon (:)
        private const int SPRITE_DASH                 = 11;  // Sprite number of dash (-)
        private const int SPRITE_BOARD_BACKGROUND     = 0;   // Sprite number of board background

        // Sprite layers & priority stuff
        private const int BOARD_SPRITELAYER           = 100; // Sprite layer of all board sprites (this layer must be above any layer)
        private const int BOARD_BACKGROUND_PRIORITY   = 0;   // Background sprite priority in board sprite layer
        private const int BOARD_OBJECTS_PRIORITY      = 1;   // Other board objects sprite priority in board sprite layer

        // Clock animation sprites
        private const int ANIMCLOCKBOTTOM_SPRITE0     = 0;
        private const int ANIMCLOCKBOTTOM_SPRITE1     = 1;
        private const int ANIMCLOCKBOTTOM_SPRITE2     = 2;
        private const int ANIMCLOCKBOTTOM_SPRITE3     = 3;
        private const int ANIMCLOCKBOTTOM_SPRITE4     = 4;
        private const int ANIMCLOCKBOTTOM_SPRITE5     = 5;
        private const int ANIMCLOCKBOTTOM_SPRITE6     = 6;
        private const int ANIMCLOCKBOTTOM_SPRITE7     = 7;
        private const int ANIMCLOCKTOP_SPRITE0        = 0;
        private const int ANIMCLOCKTOP_SPRITE1        = 1;

        // Clock animation time
        private const float ANIMCLOCKBOTTOM_NORMAL    = 0.090f; // How many seconds should elapse between two frames in normal clockbottom animation
        private const float ANIMCLOCKBOTTOM_FAST      = 0.045f; // How many seconds should elapse between two frames in fast clockbottom animation
        private const float ANIMCLOCKTOP_TIME         = 0.040f; // How many seconds should elapse between two frames in clocktop animation

        // Pixel positions stuff
        private const int BOARD_POSITION_X            = 0;   // Position of the board from origin
        private const int BOARD_POSITION_Y            = 0;
        private const int CLOCKTOP_POSITION_X         = 4;   // Position of the board clock from board origin
        private const int CLOCKTOP_POSITION_Y         = 2;
        private const int CLOCKBOTTOM_POSITION_X      = 4;   // Position of the board clock from board origin
        private const int CLOCKBOTTOM_POSITION_Y      = 9;
        private const int TIME_DIGIT_SPACE            = 8;   // X offset to add for next digit when drawing board time
        private const int SCORE_INITIAL_POSITION_X    = 79;  // Position of the first bomber head to draw in the board
        private const int SCORE_INITIAL_POSITION_Y    = 5;
        private const int SCORE_NEXT_X_OFFSET         = 35;  // X offset to add for next bomber head position in the board
        private const int HEAD_TO_SCORE_X_OFFSET      = 17;  // Offset to add to go from the bomber head position to the corresponding score
        private const int HEAD_TO_SCORE_Y_OFFSET      = 4;
        private const int BOARD_BACKGROUND_POSITION_X = 0;   // Position of the board background from board origin
        private const int BOARD_BACKGROUND_POSITION_Y = 0;
        private const int TIME_POSITION_X             = 23;  // Position of the board time from board origin
        private const int TIME_POSITION_Y             = 8;

        //******************************************************************************************************************************
        // Private fields
        //******************************************************************************************************************************

        private CDisplay  m_pDisplay;           //!< Link to the display object to use
        private COptions  m_pOptions;           //!< Link to the options object to use
        private CScores   m_pScores;            //!< Link to the scores object to use
        private CClock    m_pClock;             //!< Link to the clock object to use
        private CTimer    m_pTimer;             //!< Link to the timer object to use
        private CArena    m_pArena;             //!< Link to the arena object to use

        private float m_ClockBottomTimer;       //!< Timer for clockbottom animation
        private float m_ClockTopTimer;          //!< Timer for clocktop animation
        private int   m_ClockBottomSprite;      //!< Current clockbottom sprite to draw
        private int   m_ClockTopSprite;         //!< Current clocktop sprite to draw
        private bool  m_AnimateClock;           //!< Should the clock animate?

        //******************************************************************************************************************************
        // Constructor / Destructor
        //******************************************************************************************************************************

        //! Constructor. Initialize some members.
        public CBoard()
        {
            // Initialize the pointers to null so that we
            // can easily detect the ones we forgot to set.
            m_pClock   = null;
            m_pDisplay = null;
            m_pOptions = null;
            m_pTimer   = null;
            m_pScores  = null;
            m_pArena   = null;

            m_ClockBottomTimer   = 0.0f;
            m_ClockTopTimer      = 0.0f;
            m_ClockBottomSprite  = 0;
            m_ClockTopSprite     = 0;
            m_AnimateClock       = false;
        }

        // Destructor - nothing to do.

        //******************************************************************************************************************************
        // Setters (inline in original)
        //******************************************************************************************************************************

        //! Set link to the display object to use
        public void SetDisplay(CDisplay pDisplay)
        {
            m_pDisplay = pDisplay;
        }

        //! Set link to the options object to use
        public void SetOptions(COptions pOptions)
        {
            m_pOptions = pOptions;
        }

        //! Set link to the scores object to use
        public void SetScores(CScores pScores)
        {
            m_pScores = pScores;
        }

        //! Set link to the clock object to use
        public void SetClock(CClock pClock)
        {
            m_pClock = pClock;
        }

        //! Set link to the timer object to use
        public void SetTimer(CTimer pTimer)
        {
            m_pTimer = pTimer;
        }

        //! Make the board's clock animation active or inactive
        public void SetClockAnimation(bool AnimateClock)
        {
            m_AnimateClock = AnimateClock;
        }

        //! Set link to the arena object to use
        public void SetArena(CArena pArena)
        {
            m_pArena = pArena;
        }

        //******************************************************************************************************************************
        // Create / Destroy
        //******************************************************************************************************************************

        //! Initialize the object. Before using a CBoard, it must be created.
        public void Create()
        {
            // Check if all the objects to communicate with are set
            System.Diagnostics.Debug.Assert(m_pDisplay != null);
            System.Diagnostics.Debug.Assert(m_pClock   != null);
            System.Diagnostics.Debug.Assert(m_pOptions != null);
            System.Diagnostics.Debug.Assert(m_pTimer   != null);
            System.Diagnostics.Debug.Assert(m_pScores  != null);
            System.Diagnostics.Debug.Assert(m_pArena   != null);

            // Reset clock animation timers
            m_ClockBottomTimer = 0.0f;
            m_ClockTopTimer    = 0.0f;

            // Set first clock top/bottom sprites in case the board is displayed and has never been updated
            m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE0;
            m_ClockTopSprite    = ANIMCLOCKTOP_SPRITE0;

            // Animate the clock
            m_AnimateClock = true;
        }

        //! Uninitialize the object. When a CBoard is not needed anymore, it should be destroyed.
        public void Destroy()
        {
            // Nothing to do!
        }

        //******************************************************************************************************************************
        // Update
        //******************************************************************************************************************************

        //! Update the board - simply animates the clock.
        public void Update()
        {
            //----------------------------
            // Clock sprite update
            //----------------------------

            // If the clock should animate
            if (m_AnimateClock)
            {
                // If there is not an infinite time for the battle
                if (m_pOptions.GetTimeStartMinutes() > 0 || m_pOptions.GetTimeStartSeconds() > 0)
                {
                    // If the clock's current time is less than (or equal to) the timeup's time
                    if (m_pClock.GetMinutes() <  m_pOptions.GetTimeUpMinutes() ||
                        (m_pClock.GetMinutes() == m_pOptions.GetTimeUpMinutes() &&
                         m_pClock.GetSeconds() <= m_pOptions.GetTimeUpSeconds()))
                    {
                        // Animate the bottom of the clock (fast)
                             if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_FAST)      m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE0;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_FAST * 2)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE1;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_FAST * 3)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE2;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_FAST * 4)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE3;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_FAST * 5)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE4;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_FAST * 6)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE5;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_FAST * 7)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE6;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_FAST * 8)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE7;
                        else
                        {
                            m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE0;
                            m_ClockBottomTimer  = 0.0f;
                        }

                        // Play clock bottom animation
                        m_ClockBottomTimer += m_pTimer.GetDeltaTime();

                        // Animate the top of the clock
                             if (m_ClockTopTimer < ANIMCLOCKTOP_TIME)      m_ClockTopSprite = ANIMCLOCKTOP_SPRITE0;
                        else if (m_ClockTopTimer < ANIMCLOCKTOP_TIME * 2)  m_ClockTopSprite = ANIMCLOCKTOP_SPRITE1;
                        else
                        {
                            m_ClockTopSprite  = ANIMCLOCKTOP_SPRITE0;
                            m_ClockTopTimer   = 0.0f;
                        }

                        // Play clock top animation
                        m_ClockTopTimer += m_pTimer.GetDeltaTime();
                    }
                    // Time is not up
                    else
                    {
                        // Animate the bottom of the clock (normal speed)
                             if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_NORMAL)      m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE0;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_NORMAL * 2)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE1;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_NORMAL * 3)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE2;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_NORMAL * 4)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE3;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_NORMAL * 5)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE4;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_NORMAL * 6)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE5;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_NORMAL * 7)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE6;
                        else if (m_ClockBottomTimer < ANIMCLOCKBOTTOM_NORMAL * 8)  m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE7;
                        else
                        {
                            m_ClockBottomSprite = ANIMCLOCKBOTTOM_SPRITE0;
                            m_ClockBottomTimer  = 0.0f;
                        }

                        // Play clock bottom animation
                        m_ClockBottomTimer += m_pTimer.GetDeltaTime();
                    }
                }
            }
        }

        //******************************************************************************************************************************
        // Display
        //******************************************************************************************************************************

        //! Display the board: background, clock, current time left, current player scores with the bomber heads.
        public void Display()
        {
            // Set the origin where to draw
            m_pDisplay.SetOrigin(BOARD_POSITION_X, BOARD_POSITION_Y);

            //-----------------------------------
            // Draw background
            //-----------------------------------

            m_pDisplay.DrawSprite(
                BOARD_BACKGROUND_POSITION_X,
                BOARD_BACKGROUND_POSITION_Y,
                null,                            // Draw entire background
                null,                            // No need to clip
                BmpId.BMP_BOARD_BACKGROUND,
                SPRITE_BOARD_BACKGROUND,
                BOARD_SPRITELAYER,
                BOARD_BACKGROUND_PRIORITY);

            //-----------------------------------
            // Draw the clock
            //-----------------------------------

            // Draw the clock bottom part
            m_pDisplay.DrawSprite(
                CLOCKBOTTOM_POSITION_X,
                CLOCKBOTTOM_POSITION_Y,
                null,                            // Draw entire sprite
                null,                            // No need to clip
                BmpId.BMP_BOARD_CLOCK_BOTTOM,
                m_ClockBottomSprite,
                BOARD_SPRITELAYER,
                BOARD_OBJECTS_PRIORITY);

            // Draw the clock top part
            m_pDisplay.DrawSprite(
                CLOCKTOP_POSITION_X,
                CLOCKTOP_POSITION_Y,
                null,                            // Draw entire sprite
                null,                            // No need to clip
                BmpId.BMP_BOARD_CLOCK_TOP,
                m_ClockTopSprite,
                BOARD_SPRITELAYER,
                BOARD_OBJECTS_PRIORITY);

            //-----------------------------------
            // Draw the current clock time
            //-----------------------------------

            // If there is not an infinite time for the battle
            if (m_pOptions.GetTimeStartMinutes() > 0 || m_pOptions.GetTimeStartSeconds() > 0)
            {
                int Minutes = m_pClock.GetMinutes();
                int Seconds = m_pClock.GetSeconds();

                // Assert one character to draw only
                System.Diagnostics.Debug.Assert(Minutes >= 0 && Minutes < 10);

                // Draw the number of minutes left
                m_pDisplay.DrawSprite(
                    TIME_POSITION_X,
                    TIME_POSITION_Y,
                    null,                            // Draw entire sprite
                    null,                            // No need to clip
                    BmpId.BMP_BOARD_TIME,
                    Minutes,
                    BOARD_SPRITELAYER,
                    BOARD_OBJECTS_PRIORITY);

                // Draw the ":" symbol
                m_pDisplay.DrawSprite(
                    TIME_POSITION_X + TIME_DIGIT_SPACE,
                    TIME_POSITION_Y,
                    null,                            // Draw entire sprite
                    null,                            // No need to clip
                    BmpId.BMP_BOARD_TIME,
                    SPRITE_SEMICOLON,
                    BOARD_SPRITELAYER,
                    BOARD_OBJECTS_PRIORITY);

                // Get each digit of the two-digit seconds number
                int Seconds10 = 0;  // Number of seconds 10 (seconds = 25 --> seconds10 = 2)
                int Seconds1  = 0;  // Number of seconds 1  (seconds = 25 --> seconds1  = 5)

                while (Seconds >= 10)
                {
                    Seconds -= 10;
                    Seconds10++;
                }

                Seconds1 = Seconds;

                // Assert one character to draw only for each
                System.Diagnostics.Debug.Assert(Seconds10 >= 0 && Seconds10 < 10);
                System.Diagnostics.Debug.Assert(Seconds1  >= 0 && Seconds1  < 10);

                // Draw the two characters to draw the number of seconds
                m_pDisplay.DrawSprite(
                    TIME_POSITION_X + TIME_DIGIT_SPACE * 2,
                    TIME_POSITION_Y,
                    null,                            // Draw entire sprite
                    null,                            // No need to clip
                    BmpId.BMP_BOARD_TIME,
                    Seconds10,
                    BOARD_SPRITELAYER,
                    BOARD_OBJECTS_PRIORITY);

                m_pDisplay.DrawSprite(
                    TIME_POSITION_X + TIME_DIGIT_SPACE * 3,
                    TIME_POSITION_Y,
                    null,                            // Draw entire sprite
                    null,                            // No need to clip
                    BmpId.BMP_BOARD_TIME,
                    Seconds1,
                    BOARD_SPRITELAYER,
                    BOARD_OBJECTS_PRIORITY);
            }
            // If there is an infinite time for the battle
            else
            {
                // Draw the first dash "-"
                m_pDisplay.DrawSprite(
                    TIME_POSITION_X,
                    TIME_POSITION_Y,
                    null,                            // Draw entire sprite
                    null,                            // No need to clip
                    BmpId.BMP_BOARD_TIME,
                    SPRITE_DASH,
                    BOARD_SPRITELAYER,
                    BOARD_OBJECTS_PRIORITY);

                // Draw the ":" symbol
                m_pDisplay.DrawSprite(
                    TIME_POSITION_X + TIME_DIGIT_SPACE,
                    TIME_POSITION_Y,
                    null,                            // Draw entire sprite
                    null,                            // No need to clip
                    BmpId.BMP_BOARD_TIME,
                    SPRITE_SEMICOLON,
                    BOARD_SPRITELAYER,
                    BOARD_OBJECTS_PRIORITY);

                // Draw the second dash "-"
                m_pDisplay.DrawSprite(
                    TIME_POSITION_X + TIME_DIGIT_SPACE * 2,
                    TIME_POSITION_Y,
                    null,                            // Draw entire sprite
                    null,                            // No need to clip
                    BmpId.BMP_BOARD_TIME,
                    SPRITE_DASH,
                    BOARD_SPRITELAYER,
                    BOARD_OBJECTS_PRIORITY);

                // Draw the third dash "-"
                m_pDisplay.DrawSprite(
                    TIME_POSITION_X + TIME_DIGIT_SPACE * 3 + 1, // +1 for the look
                    TIME_POSITION_Y,
                    null,                            // Draw entire sprite
                    null,                            // No need to clip
                    BmpId.BMP_BOARD_TIME,
                    SPRITE_DASH,
                    BOARD_SPRITELAYER,
                    BOARD_OBJECTS_PRIORITY);
            }

            //-----------------------------------
            // Draw scores and draw games count
            //-----------------------------------

            // Begin to draw at (ScoreX,ScoreY) from the
            // top left corner of the board background
            int ScoreX = SCORE_INITIAL_POSITION_X;
            int ScoreY = SCORE_INITIAL_POSITION_Y;

            // Draw the score of each player
            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                // If this player plays then draw its score
                if (m_pOptions.GetBomberType(Player) != EBomberType.BOMBERTYPE_OFF
                    && m_pArena.GetBomber(Player).HasExisted())
                {
                    int DeadHeadOffset = (m_pArena.GetBomber(Player).IsDead() ? 5 : 0);

                    // Draw the player's bomber head
                    m_pDisplay.DrawSprite(
                        ScoreX,
                        ScoreY,
                        null,                            // Draw entire sprite
                        null,                            // No need to clip
                        BmpId.BMP_BOARD_HEADS,
                        DeadHeadOffset + Player,
                        BOARD_SPRITELAYER,
                        BOARD_OBJECTS_PRIORITY);

                    // Draw the score
                    m_pDisplay.DrawSprite(
                        ScoreX + HEAD_TO_SCORE_X_OFFSET,
                        ScoreY + HEAD_TO_SCORE_Y_OFFSET,
                        null,                            // Draw entire sprite
                        null,                            // No need to clip
                        BmpId.BMP_BOARD_SCORE,
                        m_pScores.GetPlayerScore(Player),
                        BOARD_SPRITELAYER,
                        BOARD_OBJECTS_PRIORITY);

                    // Next score to draw on the right
                    ScoreX += SCORE_NEXT_X_OFFSET;
                }
            }

            // Display flag
            m_pDisplay.DrawSprite(
                ScoreX,
                ScoreY,
                null,                            // Draw entire sprite
                null,                            // No need to clip
                BmpId.BMP_BOARD_DRAWGAME,
                0,
                BOARD_SPRITELAYER,
                BOARD_OBJECTS_PRIORITY);

            // Draw the number of draw games
            m_pDisplay.DrawSprite(
                ScoreX + HEAD_TO_SCORE_X_OFFSET,
                ScoreY + HEAD_TO_SCORE_Y_OFFSET,
                null,                            // Draw entire sprite
                null,                            // No need to clip
                BmpId.BMP_BOARD_SCORE,
                m_pScores.GetDrawGamesCount(),
                BOARD_SPRITELAYER,
                BOARD_OBJECTS_PRIORITY);
        }
    }
}
