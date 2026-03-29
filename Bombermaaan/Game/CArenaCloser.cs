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
 *  \file CArenaCloser.cs
 *  \brief Arena closer (walls falling down)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bombermaaan
{
    /// <summary>Describes a block position to close when the arena is closing.</summary>
    public struct SClosePosition
    {
        public int X;  ///< Block X position
        public int Y;  ///< Block Y position
    }

    /// <summary>This class manages the closure of an arena.</summary>
    public class CArenaCloser
    {
        private CArena   m_pArena;                         ///< Arena to close
        private COptions m_pOptions;                       ///< Options object with arena closure settings
        private Queue<SClosePosition> m_ClosureData        = new Queue<SClosePosition>(); ///< Ordered sequence of blocks to close
        private float    m_TimeBetweenTwoBlockClosures;    ///< Seconds between consecutive block closures
        private float    m_TimeLeftBeforeClosingNextBlock; ///< Time left before the next block starts closing
        private bool     m_IsClosing;                      ///< Is the arena currently closing?

        private Random   m_Random = new Random();

        // -------------------------------------------------------------------
        // Properties / accessors
        // -------------------------------------------------------------------

        public void SetArena(CArena pArena)   { m_pArena   = pArena;   }
        public void SetOptions(COptions pOptions) { m_pOptions = pOptions; }

        public bool  IsClosing()                       => m_IsClosing;
        public int   GetNumberOfBlocksLeft()           => m_ClosureData.Count;
        public float GetTimeBetweenTwoBlockClosures()  => m_TimeBetweenTwoBlockClosures;
        public float GetTimeLeftBeforeClosingNextBlock() => m_TimeLeftBeforeClosingNextBlock;

        public int GetNextBlockPositionX(int index)
        {
            int i = 0;
            foreach (SClosePosition pos in m_ClosureData)
            {
                if (i == index) return pos.X;
                i++;
            }
            return -1;
        }

        public int GetNextBlockPositionY(int index)
        {
            int i = 0;
            foreach (SClosePosition pos in m_ClosureData)
            {
                if (i == index) return pos.Y;
                i++;
            }
            return -1;
        }

        // -------------------------------------------------------------------

        public CArenaCloser()
        {
            m_pArena   = null;
            m_pOptions = null;
            m_TimeBetweenTwoBlockClosures     = 0.0f;
            m_TimeLeftBeforeClosingNextBlock  = 0.0f;
            m_ClosureData.Clear();
            m_IsClosing = false;
        }

        // -------------------------------------------------------------------

        public void Create()
        {
            switch (m_Random.Next(3))
            {
                case 0: CreateSpiralClosing();     break;
                case 1: CreateHorizontalClosing(); break;
                case 2: CreateVerticalClosing();   break;
            }
            m_IsClosing = false;
        }

        public void Destroy()
        {
            Stop();
        }

        //! Deep-copy state from another CArenaCloser (arena/options references are NOT copied — caller keeps its own)
        public void CopyFrom(CArenaCloser other)
        {
            // m_pArena and m_pOptions are intentionally not copied — set by the owning CArena
            m_TimeBetweenTwoBlockClosures    = other.m_TimeBetweenTwoBlockClosures;
            m_TimeLeftBeforeClosingNextBlock = other.m_TimeLeftBeforeClosingNextBlock;
            m_IsClosing                      = other.m_IsClosing;
            m_ClosureData.Clear();
            foreach (SClosePosition pos in other.m_ClosureData)
                m_ClosureData.Enqueue(pos);
        }

        // -------------------------------------------------------------------

        public void Start()
        {
            Debug.Assert(m_pArena   != null);
            Debug.Assert(m_pOptions != null);

            int totalSeconds      = m_pOptions.GetTimeUpMinutes() * 60 + m_pOptions.GetTimeUpSeconds();
            int totalBlocksToClose = m_ClosureData.Count;

            if (totalBlocksToClose > 0)
                m_TimeBetweenTwoBlockClosures = (float)totalSeconds / totalBlocksToClose;
            else
                m_TimeBetweenTwoBlockClosures = (float)totalSeconds;

            m_TimeLeftBeforeClosingNextBlock = 0.0f;
            m_IsClosing = true;
        }

        public void Stop()
        {
            m_ClosureData.Clear();
            m_IsClosing = false;
        }

        // -------------------------------------------------------------------

        public void Update(float deltaTime)
        {
            if (!m_IsClosing) return;

            if (m_ClosureData.Count > 0)
            {
                m_TimeLeftBeforeClosingNextBlock -= deltaTime;

                if (m_TimeLeftBeforeClosingNextBlock <= 0.0f)
                {
                    m_TimeLeftBeforeClosingNextBlock += m_TimeBetweenTwoBlockClosures;

                    SClosePosition front = m_ClosureData.Peek();
                    int blockX = front.X;
                    int blockY = front.Y;

                    bool closeIt = true;

                    // If there is already a hard wall here, no need to add a falling wall
                    for (int index = 0; index < m_pArena.MaxWalls(); index++)
                    {
                        if (m_pArena.GetWall(index).Exist() &&
                            m_pArena.GetWall(index).GetType() == EWallType.WALL_HARD &&
                            m_pArena.GetWall(index).GetBlockX() == blockX &&
                            m_pArena.GetWall(index).GetBlockY() == blockY)
                        {
                            closeIt = false;
                            break;
                        }
                    }

                    if (closeIt)
                        m_pArena.NewWall(blockX, blockY, EWallType.WALL_FALLING);

                    m_ClosureData.Dequeue();
                }
            }
            else
            {
                Stop();
            }
        }

        // -------------------------------------------------------------------
        // Snapshot
        // -------------------------------------------------------------------

        public void WriteSnapshot(CArenaSnapshot snapshot)
        {
            snapshot.WriteBoolean(m_IsClosing);
            snapshot.WriteBoolean(m_ClosureData.Count == 0);

            if (m_ClosureData.Count > 0)
            {
                snapshot.WriteFloat(m_TimeLeftBeforeClosingNextBlock);
                SClosePosition front = m_ClosureData.Peek();
                snapshot.WriteInteger(front.X);
                snapshot.WriteInteger(front.Y);
            }
        }

        public void ReadSnapshot(CArenaSnapshot snapshot)
        {
            snapshot.ReadBoolean(out m_IsClosing);

            bool closureDataEmpty;
            snapshot.ReadBoolean(out closureDataEmpty);

            if (!closureDataEmpty)
            {
                snapshot.ReadFloat(out m_TimeLeftBeforeClosingNextBlock);
                snapshot.ReadInteger(out int blockX);
                snapshot.ReadInteger(out int blockY);
                // Commented-out calibration logic preserved from C++ source
            }
            else
            {
                m_ClosureData.Clear();
            }
        }

        // -------------------------------------------------------------------
        // Closure shape generators
        // -------------------------------------------------------------------

        private void CreateSpiralClosing()
        {
            int I = 0;
            int X = 1;
            int Y = 1;
            int K = 1;

            while (K < 6 && I < 143)
            {
                while (X < Globals.ARENA_WIDTH - K)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    X++; I++;
                }
                X = Globals.ARENA_WIDTH - 1 - K;
                Y++;

                while (Y < Globals.ARENA_HEIGHT - K)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    Y++; I++;
                }
                Y = Globals.ARENA_HEIGHT - 1 - K;
                X--;

                while (X >= 0 + K)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    X--; I++;
                }
                X = 0 + K;
                Y--;

                while (Y >= 2 + K)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    Y--; I++;
                }
                Y = 2 + K;
                Y--;
                K++;
            }

            while (X <= 8)
            {
                m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                X++; I++;
            }
        }

        private void CreateHorizontalClosing()
        {
            int I = 0;
            int X = -1;
            int Y = -1;

            while (I <= 5)
            {
                X = Globals.ARENA_WIDTH - 2;
                Y = Globals.ARENA_HEIGHT - 2 - I;

                while (X >= 1)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    X--;
                }

                X = 1;
                Y = 1 + I;

                while (X <= Globals.ARENA_WIDTH - 2)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    X++;
                }

                I++;
            }

            Y = Globals.ARENA_HEIGHT - 2 - I;

            while (X >= 1)
            {
                m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                X--;
            }
        }

        private void CreateVerticalClosing()
        {
            int I = 0;

            while (I <= 5)
            {
                int X = 1 + I;
                int Y = 1;

                while (Y <= Globals.ARENA_HEIGHT - 2)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    Y++;
                }

                X = Globals.ARENA_WIDTH - 2 - I;
                Y = Globals.ARENA_HEIGHT - 2;

                while (Y >= 1)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    Y--;
                }

                I++;
            }

            {
                int X = 1 + I;
                int Y = 1;

                while (Y <= Globals.ARENA_HEIGHT - 2)
                {
                    m_ClosureData.Enqueue(new SClosePosition { X = X, Y = Y });
                    Y++;
                }
            }
        }
    }
}
