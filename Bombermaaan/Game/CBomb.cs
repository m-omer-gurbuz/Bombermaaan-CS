/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007, 2008 Bernd Arnold
    Copyright (C) 2008 Jerome Bigot
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
 *  \file CBomb.cs
 *  \brief The bomb
 */

using System;
using System.Diagnostics;

namespace Bombermaaan
{
    // -----------------------------------------------------------------------
    // Enumerations
    // -----------------------------------------------------------------------

    /// <summary>Movement direction of a kicked bomb.</summary>
    public enum EBombKick
    {
        BOMBKICK_NONE,
        BOMBKICK_UP,
        BOMBKICK_DOWN,
        BOMBKICK_LEFT,
        BOMBKICK_RIGHT
    }

    /// <summary>Movement direction of a flying bomb.</summary>
    public enum EBombFly
    {
        BOMBFLY_NONE  = 0,
        BOMBFLY_UP,
        BOMBFLY_DOWN,
        BOMBFLY_LEFT,
        BOMBFLY_RIGHT,
        NUMBER_OF_BOMBFLY_DIRECTIONS
    }

    /// <summary>Reason why a bomb is currently flying.</summary>
    public enum EBombFlightType
    {
        BOMBFLIGHTTYPE_NONE,
        BOMBFLIGHTTYPE_THROW,
        BOMBFLIGHTTYPE_PUNCH,
        BOMBFLIGHTTYPE_BOUNCE
    }

    // -----------------------------------------------------------------------

    /// <summary>An element in the arena that represents a bomb.</summary>
    public class CBomb : CElement
    {
        // -------------------------------------------------------------------
        // Constants (mirrors of C++ #defines)
        // -------------------------------------------------------------------

        private const float EXPLODE_SOON           = 0.080f;
        private const float MAX_EXPLOSION_TIME     = 8.0f;
        private const float SPEED_BOMBMOVE         = 100.0f;

        private const float ANIMBOMB_NORMAL_TIME1  = 0.200f;
        private const float ANIMBOMB_NORMAL_TIME2  = ANIMBOMB_NORMAL_TIME1 * 2;
        private const float ANIMBOMB_NORMAL_TIME3  = ANIMBOMB_NORMAL_TIME1 * 3;
        private const float ANIMBOMB_NORMAL_TIME4  = ANIMBOMB_NORMAL_TIME1 * 4;

        private const float ANIMBOMB_SLOW_TIME1    = 0.300f;
        private const float ANIMBOMB_SLOW_TIME2    = ANIMBOMB_SLOW_TIME1 * 2;
        private const float ANIMBOMB_SLOW_TIME3    = ANIMBOMB_SLOW_TIME1 * 3;
        private const float ANIMBOMB_SLOW_TIME4    = ANIMBOMB_SLOW_TIME1 * 4;

        private const float ANIMBOMB_FAST_TIME1    = 0.100f;
        private const float ANIMBOMB_FAST_TIME2    = ANIMBOMB_FAST_TIME1 * 2;
        private const float ANIMBOMB_FAST_TIME3    = ANIMBOMB_FAST_TIME1 * 3;
        private const float ANIMBOMB_FAST_TIME4    = ANIMBOMB_FAST_TIME1 * 4;

        private const int ANIMBOMB_SPRITE0         = 0;
        private const int ANIMBOMB_SPRITE1         = 1;
        private const int ANIMBOMB_SPRITE2         = 2;

        private const int BOMB_SPRITELAYER_BELOW_BOMBERS = 40;
        private const int BOMB_SPRITELAYER_ABOVE_BOMBERS = 55;

        private const float THROW_BASE_FRAME_TIME  = 0.030f;
        private const float BOUNCE_BASE_FRAME_TIME = 0.030f;
        private const float PUNCH_BASE_FRAME_TIME  = 0.030f;

        private const float TIME_BEFORE_MOVING_BOMB = 0.3f;
        private const bool  BOMB_CAN_CHANGE_DIRECTION_WHEN_KICKED = true;

        private const int HALF_BLOCK = Globals.BLOCK_SIZE / 2;

        // -------------------------------------------------------------------
        // Static flight-movement tables
        // -------------------------------------------------------------------

        private static readonly int[,] m_ThrowMoveX = new int[(int)EBombFly.NUMBER_OF_BOMBFLY_DIRECTIONS, 6]
        {
            {  0,  0,  0,  0,  0,  0 }, // BOMBFLY_NONE
            {  0,  0,  0,  0,  0,  0 }, // BOMBFLY_UP
            {  0,  0,  0,  0,  0,  0 }, // BOMBFLY_DOWN
            { -10,-11, -9, -7, -7, -4 }, // BOMBFLY_LEFT
            {  10, 11,  9,  7,  7,  4 }, // BOMBFLY_RIGHT
        };

        private static readonly int[,] m_ThrowMoveY = new int[(int)EBombFly.NUMBER_OF_BOMBFLY_DIRECTIONS, 6]
        {
            {  0,  0,  0,  0,  0,  0 }, // BOMBFLY_NONE
            { -5, -7, -8, -8, -5, -5 }, // BOMBFLY_UP
            {  7,  9, 14, 15, 11,  9 }, // BOMBFLY_DOWN
            { -4, -1,  2,  4,  6, 10 }, // BOMBFLY_LEFT
            { -4, -1,  2,  4,  6, 10 }, // BOMBFLY_RIGHT
        };

        private static readonly int[,] m_PunchMoveX = new int[(int)EBombFly.NUMBER_OF_BOMBFLY_DIRECTIONS, 6]
        {
            {  0,  0,  0,  0,  0,  0 }, // BOMBFLY_NONE
            {  0,  0,  0,  0,  0,  0 }, // BOMBFLY_UP
            {  0,  0,  0,  0,  0,  0 }, // BOMBFLY_DOWN
            { -8,-10, -9, -9, -6, -6 }, // BOMBFLY_LEFT
            {  8, 10,  9,  9,  6,  6 }, // BOMBFLY_RIGHT
        };

        private static readonly int[,] m_PunchMoveY = new int[(int)EBombFly.NUMBER_OF_BOMBFLY_DIRECTIONS, 6]
        {
            {  0,  0,  0,  0,  0,  0 }, // BOMBFLY_NONE
            { -6, -8,-10,-10, -8, -6 }, // BOMBFLY_UP
            {  6,  8, 10, 10,  8,  6 }, // BOMBFLY_DOWN
            { -9, -5,  0,  3,  4,  7 }, // BOMBFLY_LEFT
            { -9, -5,  0,  3,  4,  7 }, // BOMBFLY_RIGHT
        };

        private static readonly int[,] m_BounceMoveX = new int[(int)EBombFly.NUMBER_OF_BOMBFLY_DIRECTIONS, 3]
        {
            {  0,  0,  0 }, // BOMBFLY_NONE
            {  0,  0,  0 }, // BOMBFLY_UP
            {  0,  0,  0 }, // BOMBFLY_DOWN
            { -5, -6, -5 }, // BOMBFLY_LEFT
            {  5,  6,  5 }, // BOMBFLY_RIGHT
        };

        private static readonly int[,] m_BounceMoveY = new int[(int)EBombFly.NUMBER_OF_BOMBFLY_DIRECTIONS, 3]
        {
            {  0,  0,  0 }, // BOMBFLY_NONE
            { -5, -7, -4 }, // BOMBFLY_UP
            {  4,  7,  5 }, // BOMBFLY_DOWN
            { -4,  0,  4 }, // BOMBFLY_LEFT
            { -4,  0,  4 }, // BOMBFLY_RIGHT
        };

        // -------------------------------------------------------------------
        // Instance fields
        // -------------------------------------------------------------------

        private int          m_iX;
        private int          m_iY;
        private float        m_X;
        private float        m_Y;
        private int          m_BlockX;
        private int          m_BlockY;
        private int          m_Sprite;
        private float        m_Timer;
        private EBombKick    m_BombKick;
        private bool         m_HasToStopMoving;
        private int          m_OwnerPlayer;
        private int          m_KickerPlayer;
        private bool         m_Checked;
        private bool         m_Dead;
        private int          m_FlameSize;
        private float        m_ElapsedTime;
        private float        m_TimeLeft;
        private float[]      m_AnimationTimes = new float[4];
        private bool         m_BeingHeld;
        private bool         m_BeingLifted;
        private bool         m_BeingPunched;
        private EBombFly     m_BombFly;
        private float        m_FlightTimer;
        private int          m_FlightFrame;
        private EBombFlightType m_FlightType;
        private bool         m_Warping;
        private bool         m_Remote;

        private Random       m_Random = new Random();

        // -------------------------------------------------------------------

        public CBomb() : base()
        {
            m_iX = 0; m_iY = 0;
            m_X  = 0.0f; m_Y = 0.0f;
            m_BlockX = 0; m_BlockY = 0;
            m_Sprite = 0;
            m_Timer  = 0.0f;
            m_HasToStopMoving = false;
            m_OwnerPlayer  = 0;
            m_KickerPlayer = 0;
            m_Checked  = false;
            m_Dead     = false;
            m_FlameSize = 0;
            m_ElapsedTime = 0.0f;
            m_TimeLeft    = 0.0f;
            m_BeingHeld    = false;
            m_BeingLifted  = false;
            m_BeingPunched = false;
            m_FlightTimer  = 0.0f;
            m_FlightFrame  = 0;
            m_Warping = false;
            m_Remote  = false;
            m_BombKick  = EBombKick.BOMBKICK_NONE;
            m_BombFly   = EBombFly.BOMBFLY_NONE;
            m_FlightType = EBombFlightType.BOMBFLIGHTTYPE_NONE;
            for (int i = 0; i < 4; i++) m_AnimationTimes[i] = 0.0f;
        }

        // -------------------------------------------------------------------

        public void CopyFrom(CBomb other)
        {
            m_Exist            = other.m_Exist;
            m_pDisplay         = other.m_pDisplay;
            m_pSound           = other.m_pSound;
            m_iX               = other.m_iX;
            m_iY               = other.m_iY;
            m_X                = other.m_X;
            m_Y                = other.m_Y;
            m_BlockX           = other.m_BlockX;
            m_BlockY           = other.m_BlockY;
            m_Sprite           = other.m_Sprite;
            m_Timer            = other.m_Timer;
            m_BombKick         = other.m_BombKick;
            m_HasToStopMoving  = other.m_HasToStopMoving;
            m_OwnerPlayer      = other.m_OwnerPlayer;
            m_KickerPlayer     = other.m_KickerPlayer;
            m_Checked          = other.m_Checked;
            m_Dead             = other.m_Dead;
            m_FlameSize        = other.m_FlameSize;
            m_ElapsedTime      = other.m_ElapsedTime;
            m_TimeLeft         = other.m_TimeLeft;
            for (int i = 0; i < 4; i++) m_AnimationTimes[i] = other.m_AnimationTimes[i];
            m_BeingHeld        = other.m_BeingHeld;
            m_BeingLifted      = other.m_BeingLifted;
            m_BeingPunched     = other.m_BeingPunched;
            m_BombFly          = other.m_BombFly;
            m_FlightTimer      = other.m_FlightTimer;
            m_FlightFrame      = other.m_FlightFrame;
            m_FlightType       = other.m_FlightType;
            m_Warping          = other.m_Warping;
            m_Remote           = other.m_Remote;
        }

        // -------------------------------------------------------------------
        // Accessors
        // -------------------------------------------------------------------

        public void   SetChecked()                         { m_Checked = true; }
        public int    GetOwnerPlayer()                     => m_OwnerPlayer;
        public int    GetKickerPlayer()                    => m_KickerPlayer;
        public bool   IsDead()                             => m_Dead;
        public int    GetBlockX()                          => m_BlockX;
        public int    GetBlockY()                          => m_BlockY;
        public float  GetTimeLeft()                        => m_TimeLeft;
        public float  GetElapsedTime()                     => m_ElapsedTime;
        public int    GetFlameSize()                       => m_FlameSize;
        public bool   IsRemote()                           => m_Remote;
        public bool   IsBeingLifted()                      => m_BeingLifted;
        public bool   IsBeingHeld()                        => m_BeingHeld;
        public bool   IsBeingPunched()                     => m_BeingPunched;

        public bool IsOnFloor() =>
            !m_BeingLifted && !m_BeingHeld && !m_BeingPunched &&
            m_FlightType == EBombFlightType.BOMBFLIGHTTYPE_NONE;

        public void SetPosition(int x, int y)
        {
            Debug.Assert(m_BombFly == EBombFly.BOMBFLY_NONE);
            m_iX = x; m_iY = y;
            m_X = (float)x; m_Y = (float)y;
        }

        public void SetBlock(int blockX, int blockY)
        {
            Debug.Assert(m_BombFly == EBombFly.BOMBFLY_NONE);
            Debug.Assert(blockX >= 0 && blockX < Globals.ARENA_WIDTH);
            Debug.Assert(blockY >= 0 && blockY < Globals.ARENA_HEIGHT);
            m_BlockX = blockX;
            m_BlockY = blockY;
        }

        public void SetBeingLifted()
        {
            Debug.Assert(!m_Dead);
            Debug.Assert(!m_BeingHeld);
            Debug.Assert(!m_BeingLifted);
            Debug.Assert(!m_BeingPunched);
            AbortKick();
            m_BeingPunched = false;
            m_BeingLifted  = true;
            m_BeingHeld    = false;
        }

        public void SetBeingHeld()
        {
            Debug.Assert(m_BeingLifted);
            Debug.Assert(m_BombKick == EBombKick.BOMBKICK_NONE);
            Debug.Assert(!m_HasToStopMoving);
            Debug.Assert(!m_Dead);
            Debug.Assert(!m_BeingPunched);
            Debug.Assert(!m_BeingHeld);
            m_BeingPunched = false;
            m_BeingLifted  = false;
            m_BeingHeld    = true;
        }

        public void SetBeingPunched()
        {
            Debug.Assert(!m_Dead);
            Debug.Assert(!m_BeingLifted);
            Debug.Assert(!m_BeingHeld);
            Debug.Assert(!m_BeingPunched);
            AbortKick();
            m_BeingPunched = true;
            m_BeingLifted  = false;
            m_BeingHeld    = false;
        }

        private void AbortKick()
        {
            if (m_BombKick != EBombKick.BOMBKICK_NONE)
            {
                m_BombKick = EBombKick.BOMBKICK_NONE;
                m_KickerPlayer = -1;
                m_HasToStopMoving = false;
                CenterOnBlock();
            }
        }

        // -------------------------------------------------------------------

        public void Create(int blockX, int blockY, int flameSize, float timeLeft, int ownerPlayer)
        {
            base.Create();

            m_iX = m_pArena.ToPosition(blockX);
            m_iY = m_pArena.ToPosition(blockY);
            m_X  = (float)m_iX;
            m_Y  = (float)m_iY;
            m_BlockX   = blockX;
            m_BlockY   = blockY;
            m_TimeLeft = timeLeft;
            m_FlameSize = flameSize;
            m_Sprite    = ANIMBOMB_SPRITE2;
            m_BombKick  = EBombKick.BOMBKICK_NONE;
            m_BombFly   = EBombFly.BOMBFLY_NONE;
            m_ElapsedTime = 0.0f;
            m_Timer    = 0.0f;
            m_Dead     = false;
            m_HasToStopMoving = false;
            m_Checked  = false;
            m_OwnerPlayer  = ownerPlayer;
            m_KickerPlayer = -1;
            m_BeingLifted  = false;
            m_BeingHeld    = false;
            m_BeingPunched = false;
            m_FlightTimer  = 0.0f;
            m_FlightFrame  = -1;
            m_FlightType   = EBombFlightType.BOMBFLIGHTTYPE_NONE;
            m_Warping = false;
            m_Remote  = m_pArena.GetBomber(ownerPlayer).CanRemoteFuseBombs();

            if (timeLeft <= 1.0f)
            {
                m_AnimationTimes[0] = ANIMBOMB_FAST_TIME1;
                m_AnimationTimes[1] = ANIMBOMB_FAST_TIME2;
                m_AnimationTimes[2] = ANIMBOMB_FAST_TIME3;
                m_AnimationTimes[3] = ANIMBOMB_FAST_TIME4;
            }
            else if (timeLeft >= 4.0f)
            {
                m_AnimationTimes[0] = ANIMBOMB_SLOW_TIME1;
                m_AnimationTimes[1] = ANIMBOMB_SLOW_TIME2;
                m_AnimationTimes[2] = ANIMBOMB_SLOW_TIME3;
                m_AnimationTimes[3] = ANIMBOMB_SLOW_TIME4;
            }
            else
            {
                m_AnimationTimes[0] = ANIMBOMB_NORMAL_TIME1;
                m_AnimationTimes[1] = ANIMBOMB_NORMAL_TIME2;
                m_AnimationTimes[2] = ANIMBOMB_NORMAL_TIME3;
                m_AnimationTimes[3] = ANIMBOMB_NORMAL_TIME4;
            }
        }

        public new void Destroy()
        {
            base.Destroy();
        }

        // -------------------------------------------------------------------

        private void Explode()
        {
            m_pArena.NewExplosion(m_BlockX, m_BlockY, m_FlameSize);

            // Play a random explosion sound according to flame size
            int sampleIndex = (m_Random.Next(100) >= 50) ? 1 : 0;
            int clampedSize = Math.Clamp(m_FlameSize, 1, 10);
            m_pSound.PlaySample((ESample)((int)ESample.SAMPLE_EXPLOSION_01_1 + (clampedSize - 1) * 2 + sampleIndex));

            m_Dead     = true;
            m_BombKick = EBombKick.BOMBKICK_NONE;
        }

        public void Crush() { Explode(); }

        public void Burn()
        {
            if (m_TimeLeft > EXPLODE_SOON)
                m_TimeLeft = EXPLODE_SOON;
        }

        // -------------------------------------------------------------------

        public void StartMoving(EBombKick bombKick, int kickerPlayer)
        {
            if (m_BeingHeld || m_BeingLifted || m_BeingPunched) return;

            switch (m_BombKick)
            {
                case EBombKick.BOMBKICK_LEFT:
                case EBombKick.BOMBKICK_RIGHT:
                    if (bombKick == EBombKick.BOMBKICK_UP || bombKick == EBombKick.BOMBKICK_DOWN)
                        CenterOnBlock();
                    break;
                case EBombKick.BOMBKICK_UP:
                case EBombKick.BOMBKICK_DOWN:
                    if (bombKick == EBombKick.BOMBKICK_LEFT || bombKick == EBombKick.BOMBKICK_RIGHT)
                        CenterOnBlock();
                    break;
            }

            m_BombKick    = bombKick;
            m_KickerPlayer = kickerPlayer;
            m_HasToStopMoving = false;
        }

        public void StopMoving()
        {
            m_HasToStopMoving = true;
            m_KickerPlayer    = -1;
        }

        public void StartFlying(EBombFly bombFly, EBombFlightType flightType)
        {
            Debug.Assert(bombFly   != EBombFly.BOMBFLY_NONE);
            Debug.Assert(flightType != EBombFlightType.BOMBFLIGHTTYPE_NONE);
            m_BombFly    = bombFly;
            m_FlightType = flightType;
            m_FlightTimer = 0.0f;
            m_FlightFrame = -1;
            m_BeingPunched = false;
            m_BeingLifted  = false;
            m_BeingHeld    = false;
        }

        // -------------------------------------------------------------------

        /// <summary>Update the bomb for one frame. Returns true when the arena should delete it.</summary>
        public override bool Update(float deltaTime)
        {
            m_ElapsedTime += deltaTime;

            if (!m_Dead)
            {
                if (!m_BeingLifted && !m_BeingHeld && !m_BeingPunched && m_BombFly == EBombFly.BOMBFLY_NONE)
                {
                    if (!m_Remote || (m_TimeLeft <= EXPLODE_SOON))
                    {
                        if (m_TimeLeft > 0.0f)
                        {
                            m_TimeLeft -= deltaTime;
                            if (m_TimeLeft <= 0.0f)
                            {
                                Explode();
                                m_TimeLeft = 0.0f;
                            }
                        }
                    }
                }

                // Kick bomb by special floor blocks
                if (m_BombKick == EBombKick.BOMBKICK_NONE && m_ElapsedTime >= TIME_BEFORE_MOVING_BOMB &&
                    !m_Dead && !m_BeingLifted && !m_BeingHeld && !m_BeingPunched &&
                    m_BombFly == EBombFly.BOMBFLY_NONE)
                {
                    Debug.Assert(m_pArena != null);
                    if (m_pArena.IsFloorWithMoveEffect(m_BlockX, m_BlockY))
                    {
                        EFloorAction action = m_pArena.GetFloorAction(m_BlockX, m_BlockY);
                        EBombKick kickDir = EBombKick.BOMBKICK_NONE;
                        switch (action)
                        {
                            case EFloorAction.FLOORACTION_MOVEBOMB_RIGHT: kickDir = EBombKick.BOMBKICK_RIGHT; break;
                            case EFloorAction.FLOORACTION_MOVEBOMB_DOWN:  kickDir = EBombKick.BOMBKICK_DOWN;  break;
                            case EFloorAction.FLOORACTION_MOVEBOMB_LEFT:  kickDir = EBombKick.BOMBKICK_LEFT;  break;
                            case EFloorAction.FLOORACTION_MOVEBOMB_UP:    kickDir = EBombKick.BOMBKICK_UP;    break;
                        }
                        Debug.Assert(kickDir != EBombKick.BOMBKICK_NONE);
                        StartMoving(kickDir, -1);
                    }
                }

                ManageMove(deltaTime);
                ManageFlight(deltaTime);

                // Animate
                if      (m_Timer < m_AnimationTimes[0]) m_Sprite = ANIMBOMB_SPRITE2;
                else if (m_Timer < m_AnimationTimes[1]) m_Sprite = ANIMBOMB_SPRITE1;
                else if (m_Timer < m_AnimationTimes[2]) m_Sprite = ANIMBOMB_SPRITE0;
                else if (m_Timer < m_AnimationTimes[3]) m_Sprite = ANIMBOMB_SPRITE1;
                else { m_Sprite = ANIMBOMB_SPRITE2; m_Timer = 0.0f; }

                m_Timer += deltaTime;
            }

            // HACK: reset held/lifted if bomber has moved away
            if (m_BeingHeld || m_BeingLifted)
            {
                if (m_pArena.GetBomber(GetOwnerPlayer()).GetBlockX() != m_BlockX ||
                    m_pArena.GetBomber(GetOwnerPlayer()).GetBlockY() != m_BlockY)
                {
                    m_BeingHeld   = false;
                    m_BeingLifted = false;
                }
            }

            // HACK: reset punched if no longer flying
            if (m_BeingPunched && m_BombFly == EBombFly.BOMBFLY_NONE)
                m_BeingPunched = false;

            if (!m_pArena.GetBomber(GetOwnerPlayer()).IsAlive())
            {
                m_Checked      = true;
                m_BeingHeld    = false;
                m_BeingLifted  = false;
                m_BeingPunched = false;

                if (m_Remote && m_BombFly == EBombFly.BOMBFLY_NONE)
                    Explode();
            }

            return m_Dead && m_Checked;
        }

        // -------------------------------------------------------------------

        public override void Display()
        {
            int spriteLayer = (m_BeingHeld || m_BombFly != EBombFly.BOMBFLY_NONE)
                ? BOMB_SPRITELAYER_ABOVE_BOMBERS
                : BOMB_SPRITELAYER_BELOW_BOMBERS;

            // Clip to the arena view
            RECT clip; clip.left = 0; clip.top = 0; clip.right = Globals.VIEW_WIDTH; clip.bottom = Globals.VIEW_HEIGHT - 26;

            int bmp = m_Remote ? BmpId.BMP_ARENA_REMOTE_BOMB : BmpId.BMP_ARENA_BOMB;

            m_pDisplay.DrawSprite(m_iX, m_iY, null, clip, bmp, m_Sprite, spriteLayer, 0 /* PRIORITY_UNUSED */);
        }

        // -------------------------------------------------------------------
        // Snapshot
        // -------------------------------------------------------------------

        protected override void OnWriteSnapshot(CArenaSnapshot snapshot)
        {
            snapshot.WriteInteger(m_iX);
            snapshot.WriteInteger(m_iY);
            snapshot.WriteFloat(m_X);
            snapshot.WriteFloat(m_Y);
            snapshot.WriteInteger(m_BlockX);
            snapshot.WriteInteger(m_BlockY);
            snapshot.WriteInteger(m_Sprite);
            snapshot.WriteFloat(m_Timer);
            snapshot.WriteInteger((int)m_BombKick);
            snapshot.WriteBoolean(m_HasToStopMoving);
            snapshot.WriteInteger(m_OwnerPlayer);
            snapshot.WriteInteger(m_KickerPlayer);
            snapshot.WriteBoolean(m_Checked);
            snapshot.WriteBoolean(m_Dead);
            snapshot.WriteInteger(m_FlameSize);
            snapshot.WriteFloat(m_TimeLeft);
            for (int i = 0; i < 4; i++) snapshot.WriteFloat(m_AnimationTimes[i]);
            snapshot.WriteBoolean(m_BeingHeld);
            snapshot.WriteBoolean(m_BeingLifted);
            snapshot.WriteBoolean(m_BeingPunched);
            snapshot.WriteInteger((int)m_BombFly);
            snapshot.WriteFloat(m_FlightTimer);
            snapshot.WriteInteger(m_FlightFrame);
            snapshot.WriteInteger((int)m_FlightType);
            snapshot.WriteBoolean(m_Warping);
            snapshot.WriteBoolean(m_Remote);
        }

        protected override void OnReadSnapshot(CArenaSnapshot snapshot)
        {
            snapshot.ReadInteger(out m_iX);
            snapshot.ReadInteger(out m_iY);
            snapshot.ReadFloat(out m_X);
            snapshot.ReadFloat(out m_Y);
            snapshot.ReadInteger(out m_BlockX);
            snapshot.ReadInteger(out m_BlockY);
            snapshot.ReadInteger(out m_Sprite);
            snapshot.ReadFloat(out m_Timer);
            snapshot.ReadInteger(out int bombKick);       m_BombKick   = (EBombKick)bombKick;
            snapshot.ReadBoolean(out m_HasToStopMoving);
            snapshot.ReadInteger(out m_OwnerPlayer);
            snapshot.ReadInteger(out m_KickerPlayer);
            snapshot.ReadBoolean(out m_Checked);
            snapshot.ReadBoolean(out m_Dead);
            snapshot.ReadInteger(out m_FlameSize);
            snapshot.ReadFloat(out m_TimeLeft);
            for (int i = 0; i < 4; i++) snapshot.ReadFloat(out m_AnimationTimes[i]);
            snapshot.ReadBoolean(out m_BeingHeld);
            snapshot.ReadBoolean(out m_BeingLifted);
            snapshot.ReadBoolean(out m_BeingPunched);
            snapshot.ReadInteger(out int bombFly);        m_BombFly    = (EBombFly)bombFly;
            snapshot.ReadFloat(out m_FlightTimer);
            snapshot.ReadInteger(out m_FlightFrame);
            snapshot.ReadInteger(out int flightType);    m_FlightType = (EBombFlightType)flightType;
            snapshot.ReadBoolean(out m_Warping);
            snapshot.ReadBoolean(out m_Remote);
        }

        // -------------------------------------------------------------------
        // Private movement helpers
        // -------------------------------------------------------------------

        private void ManageMove(float deltaTime)
        {
            if (m_BombKick == EBombKick.BOMBKICK_NONE || m_Dead) return;

            float fPixels = SPEED_BOMBMOVE * deltaTime;

            while (true)
            {
                // Check for a direction-changing floor block
                if (m_BombFly == EBombFly.BOMBFLY_NONE)
                {
                    Debug.Assert(m_pArena != null);
                    if (m_pArena.IsFloorWithMoveEffect(m_BlockX, m_BlockY) &&
                        BOMB_CAN_CHANGE_DIRECTION_WHEN_KICKED &&
                        ((m_iX & (Globals.BLOCK_SIZE - 1)) == 0) &&
                        ((m_iY & (Globals.BLOCK_SIZE - 1)) == 0) &&
                        m_BombKick != EBombKick.BOMBKICK_NONE)
                    {
                        EFloorAction action = m_pArena.GetFloorAction(m_BlockX, m_BlockY);
                        switch (action)
                        {
                            case EFloorAction.FLOORACTION_MOVEBOMB_RIGHT: m_BombKick = EBombKick.BOMBKICK_RIGHT; break;
                            case EFloorAction.FLOORACTION_MOVEBOMB_DOWN:  m_BombKick = EBombKick.BOMBKICK_DOWN;  break;
                            case EFloorAction.FLOORACTION_MOVEBOMB_LEFT:  m_BombKick = EBombKick.BOMBKICK_LEFT;  break;
                            case EFloorAction.FLOORACTION_MOVEBOMB_UP:    m_BombKick = EBombKick.BOMBKICK_UP;    break;
                        }
                        Debug.Assert(m_BombKick != EBombKick.BOMBKICK_NONE);
                    }
                }

                if (fPixels >= 1.0f)
                {
                    if (!TryMove(1.0f)) break;
                    fPixels -= 1.0f;

                    if (m_HasToStopMoving &&
                        (m_iX & (Globals.BLOCK_SIZE - 1)) == 0 &&
                        (m_iY & (Globals.BLOCK_SIZE - 1)) == 0)
                    {
                        m_BombKick = EBombKick.BOMBKICK_NONE;
                        m_HasToStopMoving = false;
                        break;
                    }
                }
                else
                {
                    if (TryMove(fPixels)) fPixels = 0.0f;

                    if (m_HasToStopMoving &&
                        (m_iX & (Globals.BLOCK_SIZE - 1)) == 0 &&
                        (m_iY & (Globals.BLOCK_SIZE - 1)) == 0)
                    {
                        m_BombKick = EBombKick.BOMBKICK_NONE;
                        m_HasToStopMoving = false;
                    }
                    break;
                }
            }

            CrushItem(m_BlockX, m_BlockY);
        }

        private bool TryMove(float fPixels)
        {
            Debug.Assert(m_BombKick != EBombKick.BOMBKICK_NONE);

            int X = m_iX + HALF_BLOCK;
            int Y = m_iY + HALF_BLOCK;

            switch (m_BombKick)
            {
                case EBombKick.BOMBKICK_UP:
                    if (!IsObstacle(m_BlockX, ToBlock(Y - HALF_BLOCK - 1)) || ToBlock(Y - HALF_BLOCK - 1) == m_BlockY)
                    {
                        m_Y -= fPixels; m_iY = (int)m_Y;
                        m_BlockY = ToBlock(m_iY + HALF_BLOCK);
                        return true;
                    }
                    m_BombKick = EBombKick.BOMBKICK_NONE; CenterOnBlock(); return false;

                case EBombKick.BOMBKICK_DOWN:
                    if (!IsObstacle(m_BlockX, ToBlock(Y + HALF_BLOCK)) || ToBlock(Y + HALF_BLOCK) == m_BlockY)
                    {
                        m_Y += fPixels; m_iY = (int)m_Y;
                        m_BlockY = ToBlock(m_iY + HALF_BLOCK);
                        return true;
                    }
                    m_BombKick = EBombKick.BOMBKICK_NONE; CenterOnBlock(); return false;

                case EBombKick.BOMBKICK_LEFT:
                    if (!IsObstacle(ToBlock(X - HALF_BLOCK - 1), m_BlockY) || ToBlock(X - HALF_BLOCK - 1) == m_BlockX)
                    {
                        m_X -= fPixels; m_iX = (int)m_X;
                        m_BlockX = ToBlock(m_iX + HALF_BLOCK);
                        return true;
                    }
                    m_BombKick = EBombKick.BOMBKICK_NONE; CenterOnBlock(); return false;

                case EBombKick.BOMBKICK_RIGHT:
                    if (!IsObstacle(ToBlock(X + HALF_BLOCK), m_BlockY) || ToBlock(X + HALF_BLOCK) == m_BlockX)
                    {
                        m_X += fPixels; m_iX = (int)m_X;
                        m_BlockX = ToBlock(m_iX + HALF_BLOCK);
                        return true;
                    }
                    m_BombKick = EBombKick.BOMBKICK_NONE; CenterOnBlock(); return false;
            }
            return false;
        }

        private bool IsObstacle(int blockX, int blockY) =>
            m_pArena.IsWall(blockX, blockY) ||
            m_pArena.IsBomb(blockX, blockY) ||
            m_pArena.IsBomber(blockX, blockY);

        private int ToBlock(int position) => m_pArena.ToBlock(position);

        private void CrushItem(int blockX, int blockY)
        {
            if (m_pArena.IsItem(blockX, blockY))
            {
                for (int idx = 0; idx < m_pArena.MaxItems(); idx++)
                {
                    if (m_pArena.GetItem(idx).Exist() &&
                        m_pArena.GetItem(idx).GetBlockX() == blockX &&
                        m_pArena.GetItem(idx).GetBlockY() == blockY)
                    {
                        m_pArena.GetItem(idx).Crush();
                        break;
                    }
                }
            }
        }

        private void CenterOnBlock()
        {
            m_iX = m_pArena.ToPosition(m_BlockX);
            m_iY = m_pArena.ToPosition(m_BlockY);
            m_X  = (float)m_iX;
            m_Y  = (float)m_iY;
        }

        // -------------------------------------------------------------------
        // Flight management
        // -------------------------------------------------------------------

        private void ManageFlight(float deltaTime)
        {
            if (m_BombFly == EBombFly.BOMBFLY_NONE) return;

            switch (m_FlightType)
            {
                case EBombFlightType.BOMBFLIGHTTYPE_THROW:
                    HandleFlightType(deltaTime, THROW_BASE_FRAME_TIME, 6, m_ThrowMoveX, m_ThrowMoveY, 3, true);
                    break;

                case EBombFlightType.BOMBFLIGHTTYPE_BOUNCE:
                    HandleFlightType(deltaTime, BOUNCE_BASE_FRAME_TIME, 3, m_BounceMoveX, m_BounceMoveY, 1, false);
                    break;

                case EBombFlightType.BOMBFLIGHTTYPE_PUNCH:
                    HandleFlightType(deltaTime, PUNCH_BASE_FRAME_TIME, 6, m_PunchMoveX, m_PunchMoveY, 3, true);
                    break;
            }
        }

        /// <summary>
        /// Unified helper that handles throw / punch (6 frames, advance 3 blocks) and
        /// bounce (3 frames, advance 1 block).
        /// </summary>
        private void HandleFlightType(float deltaTime, float baseFrameTime, int numFrames,
                                       int[,] moveX, int[,] moveY,
                                       int blockAdvance, bool timerBeforeCalc)
        {
            if (timerBeforeCalc) m_FlightTimer += deltaTime;

            int currentFrame = Math.Min(numFrames - 1, (int)(m_FlightTimer / baseFrameTime));
            bool lastFrame   = m_FlightTimer >= baseFrameTime * numFrames;

            if (lastFrame)
            {
                currentFrame  = numFrames - 1;
                m_FlightTimer = 0.0f;

                // Advance block position
                AdvanceBlock(blockAdvance);

                // Snap pixel position, then decide land vs bounce
                CenterOnBlock();

                if (!m_Warping && !IsObstacle(m_BlockX, m_BlockY))
                {
                    m_BombFly    = EBombFly.BOMBFLY_NONE;
                    m_FlightType = EBombFlightType.BOMBFLIGHTTYPE_NONE;
                    CrushItem(m_BlockX, m_BlockY);
                }
                else
                {
                    Bounce(deltaTime);
                }
            }

            if (!timerBeforeCalc) m_FlightTimer += deltaTime;

            if (currentFrame != m_FlightFrame)
            {
                m_FlightFrame = currentFrame;
                m_iX += moveX[(int)m_BombFly, m_FlightFrame];
                m_iY += moveY[(int)m_BombFly, m_FlightFrame];
                m_X   = (float)m_iX;
                m_Y   = (float)m_iY;
            }
        }

        private void AdvanceBlock(int steps)
        {
            switch (m_BombFly)
            {
                case EBombFly.BOMBFLY_UP:
                    m_BlockY -= steps;
                    if (m_BlockY < 0)                         { m_BlockY = Globals.ARENA_HEIGHT; m_Warping = true; }
                    else                                      { m_Warping = false; }
                    break;
                case EBombFly.BOMBFLY_DOWN:
                    m_BlockY += steps;
                    if (m_BlockY >= Globals.ARENA_HEIGHT)     { m_BlockY = -1; m_Warping = true; }
                    else                                      { m_Warping = false; }
                    break;
                case EBombFly.BOMBFLY_LEFT:
                    m_BlockX -= steps;
                    if (m_BlockX < 0)                         { m_BlockX = Globals.ARENA_WIDTH; m_Warping = true; }
                    else                                      { m_Warping = false; }
                    break;
                case EBombFly.BOMBFLY_RIGHT:
                    m_BlockX += steps;
                    if (m_BlockX >= Globals.ARENA_WIDTH)      { m_BlockX = -1; m_Warping = true; }
                    else                                      { m_Warping = false; }
                    break;
            }
        }

        private void Bounce(float deltaTime)
        {
            if (!m_Warping)
            {
                if (m_pArena.IsBomber(m_BlockX, m_BlockY))
                {
                    int iPseudoRandom = (int)(deltaTime * 100000.0f);
                    int startIndex    = iPseudoRandom % m_pArena.MaxBombers();
                    int index         = startIndex;

                    while (true)
                    {
                        Debug.Assert(index >= 0 && index < m_pArena.MaxBombers());
                        if (m_pArena.GetBomber(index).Exist() &&
                            m_pArena.GetBomber(index).GetBlockX() == m_BlockX &&
                            m_pArena.GetBomber(index).GetBlockY() == m_BlockY)
                        {
                            m_pArena.GetBomber(index).Stunt();
                            break;
                        }
                        index++;
                        if (index >= m_pArena.MaxBombers()) index = 0;
                        if (index == startIndex) break;
                    }
                }
            }

            m_pSound.PlaySample(ESample.SAMPLE_BOMB_BOUNCE);
            m_FlightType = EBombFlightType.BOMBFLIGHTTYPE_BOUNCE;
        }
    }
}
