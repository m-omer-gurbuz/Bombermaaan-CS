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
 *  \file CWall.cs
 *  \brief Wall element of the arena
 */

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Describe a wall type.</summary>
    public enum EWallType
    {
        WALL_HARD,      //!< Hard walls cannot be burnt by flames
        WALL_SOFT,      //!< Soft walls can be burnt by flames
        WALL_FALLING    //!< Falling walls come from the sky and crush what's on the ground
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>An element in the arena which represents a wall.</summary>
    public class CWall : CElement
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Wall sprites
        private const int SPRITE_WALLHARD = 0;
        private const int SPRITE_WALLSOFT = 1;

        // Burning wall animation sprites
        private const int ANIM_BURNING1 = 2;
        private const int ANIM_BURNING2 = 3;
        private const int ANIM_BURNING3 = 4;
        private const int ANIM_BURNING4 = 5;
        private const int ANIM_BURNING5 = 6;

        // Burning wall animation times
        private const float ANIMBURNING_TIME1 = 0.120f;
        private const float ANIMBURNING_TIME2 = 0.230f;
        private const float ANIMBURNING_TIME3 = 0.330f;
        private const float ANIMBURNING_TIME4 = 0.420f;
        private const float ANIMBURNING_TIME5 = 0.500f;

        // Wall sprite layer
        private const int WALL_SPRITELAYER = 20;

        // Falling wall sprite layer (flying objects, bombers and item fires)
        private const int FLY_SPRITELAYER = 50;

        // Falling wall animation sprites
        private const int ANIM_FALLING1 = 0;
        private const int ANIM_FALLING2 = 1;
        private const int ANIM_FALLING3 = 2;
        private const int ANIM_FALLING4 = 1;

        // Falling wall animation times
        private const float ANIMFALLING_TIME1 = 0.050f;
        private const float ANIMFALLING_TIME2 = ANIMFALLING_TIME1 * 2;
        private const float ANIMFALLING_TIME3 = ANIMFALLING_TIME1 * 3;
        private const float ANIMFALLING_TIME4 = ANIMFALLING_TIME1 * 4;

        // Shadow sprite for falling wall
        private const int FALLING_SHADOW = 3;

        // Falling speed in pixels per second
        private const float FALLING_SPEED = 500f;

        // Priority in layer to use when drawing wall sprites
        private const int WALL_PRIORITY = 0;

        // The shadow must be on the top of any soft/hard wall
        private const int FLYSHADOW_PRIORITY = 1;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private int      m_iX;       //!< Integer position X (in pixels) in the arena
        private int      m_iY;       //!< Integer position Y (in pixels) in the arena
        private float    m_fX;       //!< Float position X (in pixels) in the arena (only used for falling walls)
        private float    m_fY;       //!< Float position Y (in pixels) in the arena (only used for falling walls)
        private int      m_BlockX;   //!< Position X (in blocks) in the arena grid
        private int      m_BlockY;   //!< Position Y (in blocks) in the arena grid
        private int      m_Sprite;   //!< Current sprite to use when displaying
        private float    m_Timer;    //!< Time counter for animation
        private bool     m_Dead;     //!< Is the wall dead?
        private bool     m_Burning;  //!< Is the wall burning?
        private EWallType m_Type;    //!< Type of the wall

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Constructor (initialize the base class).</summary>
        public CWall()
        {
            m_iX     = 0;
            m_iY     = 0;
            m_fX     = 0.0f;
            m_fY     = 0.0f;
            m_BlockX = 0;
            m_BlockY = 0;
            m_Timer  = 0.0f;
            m_Burning = false;
            m_Dead   = false;
            m_Type   = EWallType.WALL_HARD;
            m_Sprite = 0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void CopyFrom(CWall other)
        {
            m_Exist    = other.m_Exist;
            m_pDisplay = other.m_pDisplay;
            m_pSound   = other.m_pSound;
            m_iX       = other.m_iX;
            m_iY       = other.m_iY;
            m_fX       = other.m_fX;
            m_fY       = other.m_fY;
            m_BlockX   = other.m_BlockX;
            m_BlockY   = other.m_BlockY;
            m_Sprite   = other.m_Sprite;
            m_Timer    = other.m_Timer;
            m_Dead     = other.m_Dead;
            m_Burning  = other.m_Burning;
            m_Type     = other.m_Type;
        }

        /// <summary>Initialize the wall.</summary>
        public void Create(int BlockX, int BlockY, EWallType Type)
        {
            base.Create();

            switch (Type)
            {
                case EWallType.WALL_HARD:
                {
                    m_Sprite = SPRITE_WALLHARD;
                    break;
                }

                case EWallType.WALL_SOFT:
                {
                    m_Sprite = SPRITE_WALLSOFT;
                    break;
                }

                case EWallType.WALL_FALLING:
                {
                    m_Sprite = ANIM_FALLING1;
                    m_fX = (float)m_pArena.ToPosition(BlockX);
                    m_fY = (float)(m_pArena.ToPosition(BlockY) - Globals.BLOCK_SIZE * Globals.ARENA_HEIGHT);
                    break;
                }
            }

            m_iX     = m_pArena.ToPosition(BlockX);
            m_iY     = m_pArena.ToPosition(BlockY);
            m_BlockX = BlockX;
            m_BlockY = BlockY;
            m_Type   = Type;
            m_Burning = false;
            m_Timer  = 0.0f;
            m_Dead   = false;
        }

        /// <summary>Uninitialize the wall.</summary>
        public void Destroy()
        {
            base.Destroy();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Make the wall react when it is burnt by an explosion.</summary>
        public void Burn()
        {
            // Flames can only burn a soft wall
            if (m_Type == EWallType.WALL_SOFT)
                m_Burning = true;
        }

        /// <summary>Make the wall react when it is crushed by another wall.</summary>
        public void Crush()
        {
            // Die at next update
            m_Dead = true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Update the wall. Return whether the element should be deleted by the arena.</summary>
        public override bool Update(float DeltaTime)
        {
            // For hard and falling walls, crush the bombers and
            // make the bombs explode if they touch the wall
            if (m_Type == EWallType.WALL_HARD || m_Type == EWallType.WALL_FALLING)
            {
                // Crush the bombers
                if (m_pArena.IsAliveBomber(m_BlockX, m_BlockY))
                {
                    for (int Index = 0; Index < m_pArena.MaxBombers(); Index++)
                    {
                        if (m_pArena.GetBomber(Index).Exist() &&
                            m_pArena.GetBomber(Index).GetBlockX() == m_BlockX &&
                            m_pArena.GetBomber(Index).GetBlockY() == m_BlockY &&
                            m_pArena.GetBomber(Index).IsAlive())
                        {
                            m_pArena.GetBomber(Index).Crush();
                        }
                    }
                }

                // Make the bombs explode
                if (m_pArena.IsBomb(m_BlockX, m_BlockY))
                {
                    for (int Index = 0; Index < m_pArena.MaxBombs(); Index++)
                    {
                        if (m_pArena.GetBomb(Index).Exist() &&
                            m_pArena.GetBomb(Index).IsOnFloor() &&
                            m_pArena.GetBomb(Index).GetBlockX() == m_BlockX &&
                            m_pArena.GetBomb(Index).GetBlockY() == m_BlockY)
                        {
                            m_pArena.GetBomb(Index).Crush();
                            break;
                        }
                    }
                }
            }

            // If it's a hard wall
            if (m_Type == EWallType.WALL_HARD)
            {
                // Crush the floor under the wall
                if (m_pArena.IsFloor(m_BlockX, m_BlockY))
                {
                    for (int Index = 0; Index < m_pArena.MaxFloors(); Index++)
                    {
                        if (m_pArena.GetFloor(Index).Exist() &&
                            m_pArena.GetFloor(Index).GetBlockX() == m_BlockX &&
                            m_pArena.GetFloor(Index).GetBlockY() == m_BlockY)
                        {
                            m_pArena.GetFloor(Index).Crush();
                            break;
                        }
                    }
                }
            }
            // If it's a soft wall
            else if (m_Type == EWallType.WALL_SOFT)
            {
                // If burning, play the burning animation
                if (m_Burning)
                {
                         if (m_Timer < ANIMBURNING_TIME1) m_Sprite = ANIM_BURNING1;
                    else if (m_Timer < ANIMBURNING_TIME2) m_Sprite = ANIM_BURNING2;
                    else if (m_Timer < ANIMBURNING_TIME3) m_Sprite = ANIM_BURNING3;
                    else if (m_Timer < ANIMBURNING_TIME4) m_Sprite = ANIM_BURNING4;
                    else if (m_Timer < ANIMBURNING_TIME5) m_Sprite = ANIM_BURNING5;
                    else
                    {
                        // Dead
                        m_Dead = true;
                    }

                    m_Timer += DeltaTime;
                }
            }
            // If it's a falling wall
            else if (m_Type == EWallType.WALL_FALLING)
            {
                // Animate
                     if (m_Timer < ANIMFALLING_TIME1) m_Sprite = ANIM_FALLING1;
                else if (m_Timer < ANIMFALLING_TIME2) m_Sprite = ANIM_FALLING2;
                else if (m_Timer < ANIMFALLING_TIME3) m_Sprite = ANIM_FALLING3;
                else if (m_Timer < ANIMFALLING_TIME4) m_Sprite = ANIM_FALLING4;
                else
                {
                    m_Timer = 0.0f;
                }

                m_Timer += DeltaTime;

                // Make the falling wall move
                m_fY += DeltaTime * FALLING_SPEED;

                // If the wall has landed
                if (m_fY >= (float)m_iY)
                {
                    // Play a random sound (clap!)
                    m_pSound.PlaySample(CRandom.Random(100) >= 50 ? ESample.SAMPLE_WALL_CLAP_1 : ESample.SAMPLE_WALL_CLAP_2);

                    // If there is a non falling wall at (BlockX,BlockY), crush it
                    if (m_pArena.IsWall(m_BlockX, m_BlockY))
                    {
                        for (int Index = 0; Index < m_pArena.MaxWalls(); Index++)
                        {
                            if (m_pArena.GetWall(Index).Exist() &&
                                !object.ReferenceEquals(m_pArena.GetWall(Index), this) &&
                                m_pArena.GetWall(Index).GetBlockX() == m_BlockX &&
                                m_pArena.GetWall(Index).GetBlockY() == m_BlockY)
                            {
                                m_pArena.GetWall(Index).Crush();
                                break;
                            }
                        }
                    }

                    // Crush any item at (BlockX, BlockY)
                    if (m_pArena.IsItem(m_BlockX, m_BlockY))
                    {
                        for (int Index = 0; Index < m_pArena.MaxItems(); Index++)
                        {
                            if (m_pArena.GetItem(Index).Exist() &&
                                m_pArena.GetItem(Index).GetBlockX() == m_BlockX &&
                                m_pArena.GetItem(Index).GetBlockY() == m_BlockY)
                            {
                                m_pArena.GetItem(Index).Crush();
                                break;
                            }
                        }
                    }

                    // Type transformation: the falling wall becomes a hard wall.
                    m_Type   = EWallType.WALL_HARD;
                    m_Timer  = 0.0f;
                    m_Sprite = SPRITE_WALLHARD;
                }
            }

            return m_Dead;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Display the wall.</summary>
        public override void Display()
        {
            // If it's a hard wall
            if (m_Type == EWallType.WALL_HARD)
            {
                m_pDisplay.DrawSprite(m_iX,
                                      m_iY,
                                      null,
                                      null,
                                      BmpId.BMP_ARENA_WALL,
                                      m_Sprite,
                                      WALL_SPRITELAYER,
                                      WALL_PRIORITY);
            }
            // If it's a soft wall
            else if (m_Type == EWallType.WALL_SOFT)
            {
                m_pDisplay.DrawSprite(m_iX,
                                      m_iY,
                                      null,
                                      null,
                                      BmpId.BMP_ARENA_WALL,
                                      m_Sprite,
                                      WALL_SPRITELAYER,
                                      WALL_PRIORITY);
            }
            // If it's a falling wall
            else if (m_Type == EWallType.WALL_FALLING)
            {
                // Prepare a clipping rect since the sprite can be out of the arena view.
                RECT Clip;
                Clip.left   = 0;
                Clip.top    = 0;
                Clip.right  = CDisplay.VIEW_WIDTH;
                Clip.bottom = CDisplay.VIEW_HEIGHT - 26;

                // Put the falling wall sprite in the fly layer.
                m_pDisplay.DrawSprite((int)m_fX,
                                      (int)m_fY,
                                      null,
                                      Clip,
                                      BmpId.BMP_ARENA_FLY,
                                      m_Sprite,
                                      FLY_SPRITELAYER,
                                      m_iY);

                // Put the shadow sprite.
                m_pDisplay.DrawSprite(m_iX,
                                      m_iY,
                                      null,
                                      null,
                                      BmpId.BMP_ARENA_FLY,
                                      FALLING_SHADOW,
                                      WALL_SPRITELAYER,
                                      FLYSHADOW_PRIORITY);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected override void OnWriteSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.WriteInteger(m_iX);
            Snapshot.WriteInteger(m_iY);
            Snapshot.WriteFloat(m_fX);
            Snapshot.WriteFloat(m_fY);
            Snapshot.WriteInteger(m_BlockX);
            Snapshot.WriteInteger(m_BlockY);
            Snapshot.WriteInteger(m_Sprite);
            Snapshot.WriteFloat(m_Timer);
            Snapshot.WriteBoolean(m_Dead);
            Snapshot.WriteBoolean(m_Burning);
            Snapshot.WriteInteger((int)m_Type);
        }

        protected override void OnReadSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.ReadInteger(out m_iX);
            Snapshot.ReadInteger(out m_iY);
            Snapshot.ReadFloat(out m_fX);
            Snapshot.ReadFloat(out m_fY);
            Snapshot.ReadInteger(out m_BlockX);
            Snapshot.ReadInteger(out m_BlockY);
            Snapshot.ReadInteger(out m_Sprite);
            Snapshot.ReadFloat(out m_Timer);
            Snapshot.ReadBoolean(out m_Dead);
            Snapshot.ReadBoolean(out m_Burning);
            int wallType = 0;
            Snapshot.ReadInteger(out wallType);
            m_Type = (EWallType)wallType;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Return the block position X of the wall.</summary>
        public int GetBlockX() { return m_BlockX; }

        /// <summary>Return the block position Y of the wall.</summary>
        public int GetBlockY() { return m_BlockY; }

        /// <summary>Return whether the wall is burning.</summary>
        public bool IsBurning() { return m_Burning; }

        /// <summary>Return the type of the wall.</summary>
        public EWallType GetType() { return m_Type; }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
