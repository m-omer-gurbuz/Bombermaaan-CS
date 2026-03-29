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
 *  \file CExplosion.cs
 *  \brief Explosions
 */

using System.Collections.Generic;
using System.Diagnostics;

namespace Bombermaaan
{
    // -----------------------------------------------------------------------
    // Supporting types
    // -----------------------------------------------------------------------

    /// <summary>Describes a single flame tile produced by an explosion.</summary>
    public struct SFlame
    {
        public int BlockX;    ///< Block position X of the flame
        public int BlockY;    ///< Block position Y of the flame
        public int FlameType; ///< Flame type used to select the sprite to draw
    }

    /// <summary>Direction in which a flame hits an element.</summary>
    public enum EBurnDirection
    {
        BURNDIRECTION_NONE,   ///< Center of explosion – no directional push
        BURNDIRECTION_UP,     ///< Element is hit from below (goes up)
        BURNDIRECTION_DOWN,   ///< Element is hit from above (goes down)
        BURNDIRECTION_LEFT,   ///< Element is hit from the right (goes left)
        BURNDIRECTION_RIGHT   ///< Element is hit from the left (goes right)
    }

    // -----------------------------------------------------------------------

    /// <summary>
    /// An element in the arena that represents an explosion (original-style,
    /// matching Super Bomberman behaviour).
    /// </summary>
    public class CExplosion : CElement
    {
        // Sprite / state constants (mirror the C++ #defines)
        private const int EXPLOSION_SPRITELAYER = 30;

        private const int STATE0 = 0;
        private const int STATE1 = 1;
        private const int STATE2 = 2;
        private const int STATE3 = 3;

        private const float ANIM_LONGER   = 1.0f;
        private const float ANIM_STATETIME1 = ANIM_LONGER * 0.060f;
        private const float ANIM_STATETIME2 = ANIM_LONGER * 0.130f;
        private const float ANIM_STATETIME3 = ANIM_LONGER * 0.210f;
        private const float ANIM_STATETIME4 = ANIM_LONGER * 0.280f;
        private const float ANIM_STATETIME5 = ANIM_LONGER * 0.340f;
        private const float ANIM_STATETIME6 = ANIM_LONGER * 0.400f;

        private const int FLAME_VERT      = 0;
        private const int FLAME_HORIZ     = 4;
        private const int FLAME_VERTUP    = 8;
        private const int FLAME_HORIZLEFT = 12;
        private const int FLAME_VERTDOWN  = 16;
        private const int FLAME_HORIZRIGHT= 20;
        private const int FLAME_CENTER    = 24;

        // -------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------

        private int   m_iX;
        private int   m_iY;
        private int   m_BlockX;
        private int   m_BlockY;
        private int   m_State;
        private float m_Timer;
        private int   m_FlameSize;
        private List<SFlame> m_Flames = new List<SFlame>();
        private bool  m_Dead;

        // Original-style per-direction flame sizes / stop flags
        private int   m_FlameSizeUp;
        private int   m_FlameSizeDown;
        private int   m_FlameSizeLeft;
        private int   m_FlameSizeRight;
        private bool  m_StopUp;
        private bool  m_StopDown;
        private bool  m_StopLeft;
        private bool  m_StopRight;

        // -------------------------------------------------------------------

        public CExplosion() : base()
        {
            m_iX = -1; m_iY = -1;
            m_BlockX = -1; m_BlockY = -1;
            m_FlameSize = 0;
            m_State = STATE2;
            m_Timer = 0.0f;
            m_Dead  = false;
            m_FlameSizeUp = m_FlameSizeDown = m_FlameSizeLeft = m_FlameSizeRight = 0;
            m_StopUp = m_StopDown = m_StopLeft = m_StopRight = false;
        }

        // -------------------------------------------------------------------

        public void CopyFrom(CExplosion other)
        {
            m_Exist          = other.m_Exist;
            m_pDisplay       = other.m_pDisplay;
            m_pSound         = other.m_pSound;
            m_iX             = other.m_iX;
            m_iY             = other.m_iY;
            m_BlockX         = other.m_BlockX;
            m_BlockY         = other.m_BlockY;
            m_State          = other.m_State;
            m_Timer          = other.m_Timer;
            m_FlameSize      = other.m_FlameSize;
            m_Flames         = new List<SFlame>(other.m_Flames);
            m_Dead           = other.m_Dead;
            m_FlameSizeUp    = other.m_FlameSizeUp;
            m_FlameSizeDown  = other.m_FlameSizeDown;
            m_FlameSizeLeft  = other.m_FlameSizeLeft;
            m_FlameSizeRight = other.m_FlameSizeRight;
            m_StopUp         = other.m_StopUp;
            m_StopDown       = other.m_StopDown;
            m_StopLeft       = other.m_StopLeft;
            m_StopRight      = other.m_StopRight;
        }

        // -------------------------------------------------------------------
        // Accessors
        // -------------------------------------------------------------------

        public int GetBlockX() => m_BlockX;
        public int GetBlockY() => m_BlockY;
        public IReadOnlyList<SFlame> GetFlames() => m_Flames;

        // -------------------------------------------------------------------

        public void Create(int blockX, int blockY, int flameSize)
        {
            base.Create();

            m_iX = m_pArena.ToPosition(blockX);
            m_iY = m_pArena.ToPosition(blockY);
            m_BlockX    = blockX;
            m_BlockY    = blockY;
            m_FlameSize = flameSize;
            m_State     = STATE2;
            m_Timer     = 0.0f;
            m_Dead      = false;

            // ---- Original-style: compute flame extents once at creation ----

            int i;

            // Right
            m_FlameSizeRight = 0; m_StopRight = false;
            for (i = 1; i <= m_FlameSize; i++)
            {
                if (m_BlockX + i < Globals.ARENA_WIDTH)
                {
                    if (m_pArena.IsWall(m_BlockX+i, m_BlockY) ||
                        m_pArena.IsBomb(m_BlockX+i, m_BlockY) ||
                        m_pArena.IsItem(m_BlockX+i, m_BlockY) ||
                        m_pArena.IsExplosion(m_BlockX+i, m_BlockY))
                    {
                        m_FlameSizeRight = i; m_StopRight = true;
                        Burn(m_BlockX+i, m_BlockY, EBurnDirection.BURNDIRECTION_RIGHT);
                        break;
                    }
                }
                else
                {
                    m_FlameSizeRight = i; m_StopRight = true; break;
                }
                if (i == m_FlameSize) { m_FlameSizeRight = i; m_StopRight = false; }
            }

            // Left
            m_FlameSizeLeft = 0; m_StopLeft = false;
            for (i = 1; i <= m_FlameSize; i++)
            {
                if (m_BlockX - i >= 0)
                {
                    if (m_pArena.IsWall(m_BlockX-i, m_BlockY) ||
                        m_pArena.IsBomb(m_BlockX-i, m_BlockY) ||
                        m_pArena.IsItem(m_BlockX-i, m_BlockY) ||
                        m_pArena.IsExplosion(m_BlockX-i, m_BlockY))
                    {
                        m_FlameSizeLeft = i; m_StopLeft = true;
                        Burn(m_BlockX-i, m_BlockY, EBurnDirection.BURNDIRECTION_LEFT);
                        break;
                    }
                }
                else
                {
                    m_FlameSizeLeft = i; m_StopLeft = true; break;
                }
                if (i == m_FlameSize) { m_FlameSizeLeft = i; m_StopLeft = false; }
            }

            // Up
            m_FlameSizeUp = 0; m_StopUp = false;
            for (i = 1; i <= m_FlameSize; i++)
            {
                if (m_BlockY - i >= 0)
                {
                    if (m_pArena.IsWall(m_BlockX, m_BlockY-i) ||
                        m_pArena.IsBomb(m_BlockX, m_BlockY-i) ||
                        m_pArena.IsItem(m_BlockX, m_BlockY-i) ||
                        m_pArena.IsExplosion(m_BlockX, m_BlockY-i))
                    {
                        m_FlameSizeUp = i; m_StopUp = true;
                        Burn(m_BlockX, m_BlockY-i, EBurnDirection.BURNDIRECTION_UP);
                        break;
                    }
                }
                else
                {
                    m_FlameSizeUp = i; m_StopUp = true; break;
                }
                if (i == m_FlameSize) { m_FlameSizeUp = i; m_StopUp = false; }
            }

            // Down
            m_FlameSizeDown = 0; m_StopDown = false;
            for (i = 1; i <= m_FlameSize; i++)
            {
                if (m_BlockY + i < Globals.ARENA_HEIGHT)
                {
                    if (m_pArena.IsWall(m_BlockX, m_BlockY+i) ||
                        m_pArena.IsBomb(m_BlockX, m_BlockY+i) ||
                        m_pArena.IsItem(m_BlockX, m_BlockY+i) ||
                        m_pArena.IsExplosion(m_BlockX, m_BlockY+i))
                    {
                        m_FlameSizeDown = i; m_StopDown = true;
                        Burn(m_BlockX, m_BlockY+i, EBurnDirection.BURNDIRECTION_DOWN);
                        break;
                    }
                }
                else
                {
                    m_FlameSizeDown = i; m_StopDown = true; break;
                }
                if (i == m_FlameSize) { m_FlameSizeDown = i; m_StopDown = false; }
            }
        }

        // -------------------------------------------------------------------

        public void Destroy()
        {
            m_Flames.Clear();
            base.Destroy();
        }

        // -------------------------------------------------------------------

        private void PutFlame(int blockX, int blockY, int flameType)
        {
            m_Flames.Add(new SFlame { BlockX = blockX, BlockY = blockY, FlameType = flameType });
        }

        // -------------------------------------------------------------------

        /// <summary>
        /// Burn whatever is at (blockX, blockY): walls, items, bombs, bombers.
        /// </summary>
        private void Burn(int blockX, int blockY, EBurnDirection dir)
        {
            Debug.Assert(blockX >= 0 && blockX < Globals.ARENA_WIDTH);
            Debug.Assert(blockY >= 0 && blockY < Globals.ARENA_HEIGHT);

            if (m_pArena.IsWall(blockX, blockY))
            {
                for (int idx = 0; idx < m_pArena.MaxWalls(); idx++)
                {
                    if (m_pArena.GetWall(idx).Exist() &&
                        m_pArena.GetWall(idx).GetBlockX() == blockX &&
                        m_pArena.GetWall(idx).GetBlockY() == blockY)
                    {
                        m_pArena.GetWall(idx).Burn();
                        break;
                    }
                }
            }
            else if (m_pArena.IsItem(blockX, blockY))
            {
                for (int idx = 0; idx < m_pArena.MaxItems(); idx++)
                {
                    if (m_pArena.GetItem(idx).Exist() &&
                        m_pArena.GetItem(idx).GetBlockX() == blockX &&
                        m_pArena.GetItem(idx).GetBlockY() == blockY)
                    {
                        m_pArena.GetItem(idx).Burn(dir);
                        break;
                    }
                }
            }

            if (m_pArena.IsBomb(blockX, blockY))
            {
                for (int idx = 0; idx < m_pArena.MaxBombs(); idx++)
                {
                    if (m_pArena.GetBomb(idx).Exist() &&
                        m_pArena.GetBomb(idx).GetBlockX() == blockX &&
                        m_pArena.GetBomb(idx).GetBlockY() == blockY)
                    {
                        m_pArena.GetBomb(idx).Burn();
                        break;
                    }
                }
            }

            if (m_pArena.IsAliveBomber(blockX, blockY))
            {
                for (int idx = 0; idx < m_pArena.MaxBombers(); idx++)
                {
                    if (m_pArena.GetBomber(idx).Exist() &&
                        m_pArena.GetBomber(idx).GetBlockX() == blockX &&
                        m_pArena.GetBomber(idx).GetBlockY() == blockY)
                    {
                        m_pArena.GetBomber(idx).Burn();
                        // no break – burn every bomber on the block
                    }
                }
            }
        }

        // -------------------------------------------------------------------

        /// <summary>
        /// Update the explosion for one frame. Returns true when the explosion
        /// is dead and should be removed by the arena.
        /// </summary>
        public override bool Update(float deltaTime)
        {
            int i;
            m_Flames.Clear();

            // Center
            if (!m_pArena.IsWall(m_BlockX, m_BlockY) ||
                 m_pArena.IsFallingWall(m_BlockX, m_BlockY))
            {
                Burn(m_BlockX, m_BlockY, EBurnDirection.BURNDIRECTION_NONE);
                PutFlame(m_BlockX, m_BlockY, FLAME_CENTER);
            }

            // ---- Original-style rays ----

            // Right
            for (i = 1; i <= (m_StopRight ? m_FlameSizeRight - 1 : m_FlameSizeRight); i++)
            {
                if (m_BlockX + i < Globals.ARENA_WIDTH)
                {
                    if (m_pArena.IsWall(m_BlockX+i, m_BlockY) ||
                        m_pArena.IsBomb(m_BlockX+i, m_BlockY) ||
                        m_pArena.IsItem(m_BlockX+i, m_BlockY))
                    {
                        Burn(m_BlockX+i, m_BlockY, EBurnDirection.BURNDIRECTION_RIGHT);
                        m_FlameSizeRight = i;
                        break;
                    }
                    else if (m_pArena.IsExplosion(m_BlockX+i, m_BlockY))
                    {
                        m_FlameSizeRight = i;
                        break;
                    }
                    else
                    {
                        Burn(m_BlockX+i, m_BlockY, EBurnDirection.BURNDIRECTION_RIGHT);
                        if (i != m_FlameSizeRight || !m_StopRight)
                            PutFlame(m_BlockX+i, m_BlockY, (i != m_FlameSize ? FLAME_HORIZ : FLAME_HORIZRIGHT));
                    }
                }
                else break;
            }

            // Left
            for (i = 1; i <= (m_StopLeft ? m_FlameSizeLeft - 1 : m_FlameSizeLeft); i++)
            {
                if (m_BlockX - i >= 0)
                {
                    if (m_pArena.IsWall(m_BlockX-i, m_BlockY) ||
                        m_pArena.IsBomb(m_BlockX-i, m_BlockY) ||
                        m_pArena.IsItem(m_BlockX-i, m_BlockY))
                    {
                        Burn(m_BlockX-i, m_BlockY, EBurnDirection.BURNDIRECTION_LEFT);
                        m_FlameSizeLeft = i;
                        break;
                    }
                    else if (m_pArena.IsExplosion(m_BlockX-i, m_BlockY))
                    {
                        m_FlameSizeLeft = i;
                        break;
                    }
                    else
                    {
                        Burn(m_BlockX-i, m_BlockY, EBurnDirection.BURNDIRECTION_LEFT);
                        if (i != m_FlameSizeLeft || !m_StopLeft)
                            PutFlame(m_BlockX-i, m_BlockY, (i != m_FlameSize ? FLAME_HORIZ : FLAME_HORIZLEFT));
                    }
                }
                else break;
            }

            // Up
            for (i = 1; i <= (m_StopUp ? m_FlameSizeUp - 1 : m_FlameSizeUp); i++)
            {
                if (m_BlockY - i >= 0)
                {
                    if (m_pArena.IsWall(m_BlockX, m_BlockY-i) ||
                        m_pArena.IsBomb(m_BlockX, m_BlockY-i) ||
                        m_pArena.IsItem(m_BlockX, m_BlockY-i))
                    {
                        Burn(m_BlockX, m_BlockY-i, EBurnDirection.BURNDIRECTION_UP);
                        m_FlameSizeUp = i;
                        break;
                    }
                    else if (m_pArena.IsExplosion(m_BlockX, m_BlockY-i))
                    {
                        m_FlameSizeUp = i;
                        break;
                    }
                    else
                    {
                        Burn(m_BlockX, m_BlockY-i, EBurnDirection.BURNDIRECTION_UP);
                        if (i != m_FlameSizeUp || !m_StopUp)
                            PutFlame(m_BlockX, m_BlockY-i, (i != m_FlameSize ? FLAME_VERT : FLAME_VERTUP));
                    }
                }
                else break;
            }

            // Down
            for (i = 1; i <= (m_StopDown ? m_FlameSizeDown - 1 : m_FlameSizeDown); i++)
            {
                if (m_BlockY + i < Globals.ARENA_HEIGHT)
                {
                    if (m_pArena.IsWall(m_BlockX, m_BlockY+i) ||
                        m_pArena.IsBomb(m_BlockX, m_BlockY+i) ||
                        m_pArena.IsItem(m_BlockX, m_BlockY+i))
                    {
                        Burn(m_BlockX, m_BlockY+i, EBurnDirection.BURNDIRECTION_DOWN);
                        m_FlameSizeDown = i;
                        break;
                    }
                    else if (m_pArena.IsExplosion(m_BlockX, m_BlockY+i))
                    {
                        m_FlameSizeDown = i;
                        break;
                    }
                    else
                    {
                        Burn(m_BlockX, m_BlockY+i, EBurnDirection.BURNDIRECTION_DOWN);
                        if (i != m_FlameSizeDown || !m_StopDown)
                            PutFlame(m_BlockX, m_BlockY+i, (i != m_FlameSize ? FLAME_VERT : FLAME_VERTDOWN));
                    }
                }
                else break;
            }

            // ---- Animate state ----
            if      (m_Timer < ANIM_STATETIME1) m_State = STATE2;
            else if (m_Timer < ANIM_STATETIME2) m_State = STATE1;
            else if (m_Timer < ANIM_STATETIME3) m_State = STATE0;
            else if (m_Timer < ANIM_STATETIME4) m_State = STATE1;
            else if (m_Timer < ANIM_STATETIME5) m_State = STATE2;
            else if (m_Timer < ANIM_STATETIME6) m_State = STATE3;
            else m_Dead = true;

            m_Timer += deltaTime;
            return m_Dead;
        }

        // -------------------------------------------------------------------

        public override void Display()
        {
            for (int i = 0; i < m_Flames.Count; i++)
            {
                m_pDisplay.DrawSprite(
                    m_pArena.ToPosition(m_Flames[i].BlockX),
                    m_pArena.ToPosition(m_Flames[i].BlockY),
                    null,
                    null,
                    BmpId.BMP_ARENA_FLAME,
                    m_Flames[i].FlameType + m_State,
                    EXPLOSION_SPRITELAYER,
                    0 /* PRIORITY_UNUSED */);
            }
        }

        // -------------------------------------------------------------------
        // Snapshot
        // -------------------------------------------------------------------

        protected override void OnWriteSnapshot(CArenaSnapshot snapshot)
        {
            snapshot.WriteInteger(m_iX);
            snapshot.WriteInteger(m_iY);
            snapshot.WriteInteger(m_BlockX);
            snapshot.WriteInteger(m_BlockY);
            snapshot.WriteInteger(m_State);
            snapshot.WriteFloat(m_Timer);
            snapshot.WriteInteger(m_FlameSize);

            snapshot.WriteInteger(m_Flames.Count);
            for (int i = 0; i < m_Flames.Count; i++)
            {
                snapshot.WriteInteger(m_Flames[i].BlockX);
                snapshot.WriteInteger(m_Flames[i].BlockY);
                snapshot.WriteInteger(m_Flames[i].FlameType);
            }

            snapshot.WriteBoolean(m_Dead);

            snapshot.WriteInteger(m_FlameSizeUp);
            snapshot.WriteInteger(m_FlameSizeDown);
            snapshot.WriteInteger(m_FlameSizeLeft);
            snapshot.WriteInteger(m_FlameSizeRight);
            snapshot.WriteBoolean(m_StopUp);
            snapshot.WriteBoolean(m_StopDown);
            snapshot.WriteBoolean(m_StopLeft);
            snapshot.WriteBoolean(m_StopRight);
        }

        protected override void OnReadSnapshot(CArenaSnapshot snapshot)
        {
            snapshot.ReadInteger(out m_iX);
            snapshot.ReadInteger(out m_iY);
            snapshot.ReadInteger(out m_BlockX);
            snapshot.ReadInteger(out m_BlockY);
            snapshot.ReadInteger(out m_State);
            snapshot.ReadFloat(out m_Timer);
            snapshot.ReadInteger(out m_FlameSize);

            snapshot.ReadInteger(out int numberOfFlames);
            m_Flames.Clear();
            for (int i = 0; i < numberOfFlames; i++)
            {
                snapshot.ReadInteger(out int bx);
                snapshot.ReadInteger(out int by);
                snapshot.ReadInteger(out int ft);
                m_Flames.Add(new SFlame { BlockX = bx, BlockY = by, FlameType = ft });
            }

            snapshot.ReadBoolean(out m_Dead);

            snapshot.ReadInteger(out m_FlameSizeUp);
            snapshot.ReadInteger(out m_FlameSizeDown);
            snapshot.ReadInteger(out m_FlameSizeLeft);
            snapshot.ReadInteger(out m_FlameSizeRight);
            snapshot.ReadBoolean(out m_StopUp);
            snapshot.ReadBoolean(out m_StopDown);
            snapshot.ReadBoolean(out m_StopLeft);
            snapshot.ReadBoolean(out m_StopRight);
        }
    }
}
