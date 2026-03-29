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
 *  \file CAiArena.cs
 *  \brief The AI arena
 */

using System.Collections.Generic;
using System.Diagnostics;

namespace Bombermaaan
{
    //! Describe the type of danger on a block
    public enum EDanger
    {
        DANGER_MORTAL, //!< This block is currently mortal
        DANGER_SOON,   //!< This block will be mortal very soon
        DANGER_NONE    //!< This block is not dangerous for the moment
    }

    //! Maximum number of dead ends in the arena
    // MAX_DEAD_END = 50

    //! Describes the coordinates of a block (used for dead ends' exits)
    public struct SBlock
    {
        public int BlockX;
        public int BlockY;
    }

    public class CAiArena
    {
        private const int MAX_DEAD_END = 50;
        private const int SOFT_WALL_NEAR_MAX_DEPTH = 2;

        private CArena m_pArena;
        private CDisplay m_pDisplay;

        // Danger type of each block of the arena
        private EDanger[,] m_Danger = new EDanger[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];
        private float[,] m_DangerTimeLeft = new float[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];

        // If this block is not in a dead end, this contains -1.
        // Otherwise it contains the number of the dead end where this square is.
        private int[,] m_DeadEnd = new int[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];

        // Block position of the exit of each dead end (currently unused)
        private SBlock[] m_DeadEndExit = new SBlock[MAX_DEAD_END];

        // Number of soft walls near this square if there is no wall in this square
        private int[,] m_SoftWallNear = new int[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];

        // True if a soft wall is burning or is going to burn very soon in this square
        private bool[,] m_WallBurn = new bool[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];

        //**************************************************************************************

        public CAiArena()
        {
            m_pArena = null;
            m_pDisplay = null;

            for (int i = 0; i < MAX_DEAD_END; i++)
            {
                m_DeadEndExit[i].BlockX = -1;
                m_DeadEndExit[i].BlockY = -1;
            }

            for (int BlockX = 0; BlockX < Globals.ARENA_WIDTH; BlockX++)
            {
                for (int BlockY = 0; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                {
                    m_Danger[BlockX, BlockY] = EDanger.DANGER_NONE;
                    m_DangerTimeLeft[BlockX, BlockY] = 0.0f;
                    m_DeadEnd[BlockX, BlockY] = -1;
                    m_SoftWallNear[BlockX, BlockY] = 0;
                    m_WallBurn[BlockX, BlockY] = false;
                }
            }
        }

        //**************************************************************************************

        public void SetArena(CArena pArena)
        {
            Debug.Assert(pArena != null);
            m_pArena = pArena;
        }

        public CArena GetArena()
        {
            Debug.Assert(m_pArena != null);
            return m_pArena;
        }

        public void SetDisplay(CDisplay pDisplay)
        {
            Debug.Assert(pDisplay != null);
            // Save the display object pointer to pass to elements
            m_pDisplay = pDisplay;
        }

        public EDanger GetDanger(int BlockX, int BlockY)
        {
            return m_Danger[BlockX, BlockY];
        }

        public float GetDangerTimeLeft(int BlockX, int BlockY)
        {
            return m_DangerTimeLeft[BlockX, BlockY];
        }

        public int GetDeadEnd(int BlockX, int BlockY)
        {
            return m_DeadEnd[BlockX, BlockY];
        }

        public ref SBlock GetDeadEndExit(int Exit)
        {
            return ref m_DeadEndExit[Exit];
        }

        public int GetSoftWallNear(int BlockX, int BlockY)
        {
            return m_SoftWallNear[BlockX, BlockY];
        }

        public bool GetWallBurn(int BlockX, int BlockY)
        {
            return m_WallBurn[BlockX, BlockY];
        }

        //**************************************************************************************

        public void Create()
        {
            Debug.Assert(m_pArena != null);
            // Debug font setup omitted (debug-only in original)
        }

        //**************************************************************************************

        public void Destroy()
        {
            // Debug rectangle removal omitted (debug-only in original)
        }

        //**************************************************************************************

        public void Update(float DeltaTime)
        {
            // Block coordinates used in many places
            int BlockX;
            int BlockY;

            // Auxiliary variables for code readability : is there a wall around the current block?
            bool IsWallHere;
            bool IsWallUp;
            bool IsWallDown;
            bool IsWallLeft;
            bool IsWallRight;

            // Current dead end number. Incremented each time there is a new dead end.
            int CurrentDeadEnd = 0;

            //*************
            // DEAD END
            //*************

            // Scan each block of the dead end array
            for (BlockX = 0; BlockX < Globals.ARENA_WIDTH; BlockX++)
            {
                for (BlockY = 0; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                {
                    // Set undefined dead end (is there one? we don't know yet)
                    m_DeadEnd[BlockX, BlockY] = -2;
                }
            }

            // Scan each block of the arena
            for (BlockX = 0; BlockX < Globals.ARENA_WIDTH; BlockX++)
            {
                for (BlockY = 0; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                {
                    // If the dead end on this block is currently undefined
                    if (m_DeadEnd[BlockX, BlockY] == -2)
                    {
                        // If the block is on the edges of the arena grid
                        if (BlockX == 0 || BlockX == Globals.ARENA_WIDTH - 1 ||
                            BlockY == 0 || BlockY == Globals.ARENA_HEIGHT - 1)
                        {
                            // There is definitely no dead end here
                            m_DeadEnd[BlockX, BlockY] = -1;
                        }
                        // If the block is not on the edges of the arena grid
                        else
                        {
                            // Is there a wall on this block?
                            IsWallHere = m_pArena.IsWall(BlockX, BlockY);

                            // If there is no wall on this block
                            if (!IsWallHere)
                            {
                                // Is there a wall around this block?
                                IsWallUp    = m_pArena.IsWall(BlockX, BlockY - 1);
                                IsWallDown  = m_pArena.IsWall(BlockX, BlockY + 1);
                                IsWallLeft  = m_pArena.IsWall(BlockX - 1, BlockY);
                                IsWallRight = m_pArena.IsWall(BlockX + 1, BlockY);

                                // If this block is the back of a dead end ("[")
                                if (IsWallLeft && IsWallUp && IsWallDown)
                                {
                                    // Start scanning the dead end on this block
                                    int DeadEndBlockX = BlockX;
                                    int DeadEndBlockY = BlockY;

                                    // While we are still in this dead end and there is no wall blocking the way
                                    while (IsWallUp && IsWallDown && !IsWallHere)
                                    {
                                        // Set the dead end number of the current block
                                        m_DeadEnd[DeadEndBlockX, DeadEndBlockY] = CurrentDeadEnd;

                                        // Continue scanning (go right)
                                        DeadEndBlockX++;

                                        // Update the auxiliary variables value.
                                        IsWallHere = m_pArena.IsWall(DeadEndBlockX, DeadEndBlockY);
                                        IsWallUp   = m_pArena.IsWall(DeadEndBlockX, DeadEndBlockY - 1);
                                        IsWallDown = m_pArena.IsWall(DeadEndBlockX, DeadEndBlockY + 1);
                                    }

                                    // If there is no wall blocking the way and the dead end was entirely scanned
                                    if (!IsWallHere)
                                    {
                                        // Then this dead end has an exit. Record the block coordinates of this exit.
                                        m_DeadEndExit[CurrentDeadEnd].BlockX = DeadEndBlockX;
                                        m_DeadEndExit[CurrentDeadEnd].BlockY = DeadEndBlockY;
                                    }

                                    // Next dead end number
                                    CurrentDeadEnd++;
                                }
                                // If this block is the back of a dead end (top-open U shape)
                                else if (IsWallUp && IsWallLeft && IsWallRight)
                                {
                                    // Start scanning the dead end on this block
                                    int DeadEndBlockX = BlockX;
                                    int DeadEndBlockY = BlockY;

                                    // While we are still in this dead end and there is no wall blocking the way
                                    while (IsWallLeft && IsWallRight && !IsWallHere)
                                    {
                                        // Set the dead end number of the current block
                                        m_DeadEnd[DeadEndBlockX, DeadEndBlockY] = CurrentDeadEnd;

                                        // Continue scanning (go down)
                                        DeadEndBlockY++;

                                        // Update the auxiliary variables value.
                                        IsWallHere  = m_pArena.IsWall(DeadEndBlockX, DeadEndBlockY);
                                        IsWallLeft  = m_pArena.IsWall(DeadEndBlockX - 1, DeadEndBlockY);
                                        IsWallRight = m_pArena.IsWall(DeadEndBlockX + 1, DeadEndBlockY);
                                    }

                                    // If there is no wall blocking the way and the dead end was entirely scanned
                                    if (!IsWallHere)
                                    {
                                        // Then this dead end has an exit. Record the block coordinates of this exit.
                                        m_DeadEndExit[CurrentDeadEnd].BlockX = DeadEndBlockX;
                                        m_DeadEndExit[CurrentDeadEnd].BlockY = DeadEndBlockY;
                                    }

                                    // Next dead end number
                                    CurrentDeadEnd++;
                                }
                                // If this block is the back of a dead end ("]")
                                else if (IsWallRight && IsWallUp && IsWallDown)
                                {
                                    // Start scanning the dead end on this block
                                    int DeadEndBlockX = BlockX;
                                    int DeadEndBlockY = BlockY;

                                    // While we are still in this dead end and there is no wall blocking the way
                                    while (IsWallUp && IsWallDown && !IsWallHere)
                                    {
                                        // Set the dead end number of the current block
                                        m_DeadEnd[DeadEndBlockX, DeadEndBlockY] = CurrentDeadEnd;

                                        // Continue scanning (go left)
                                        DeadEndBlockX--;

                                        // Update the auxiliary variables value.
                                        IsWallHere = m_pArena.IsWall(DeadEndBlockX, DeadEndBlockY);
                                        IsWallUp   = m_pArena.IsWall(DeadEndBlockX, DeadEndBlockY - 1);
                                        IsWallDown = m_pArena.IsWall(DeadEndBlockX, DeadEndBlockY + 1);
                                    }

                                    // If there is no wall blocking the way and the dead end was entirely scanned
                                    if (!IsWallHere)
                                    {
                                        // Then this dead end has an exit. Record the block coordinates of this exit.
                                        m_DeadEndExit[CurrentDeadEnd].BlockX = DeadEndBlockX;
                                        m_DeadEndExit[CurrentDeadEnd].BlockY = DeadEndBlockY;
                                    }

                                    // Next dead end number
                                    CurrentDeadEnd++;
                                }
                                // If this block is the back of a dead end (bottom-open U shape)
                                else if (IsWallDown && IsWallLeft && IsWallRight)
                                {
                                    // Start scanning the dead end on this block
                                    int DeadEndBlockX = BlockX;
                                    int DeadEndBlockY = BlockY;

                                    // While we are still in this dead end and there is no wall blocking the way
                                    while (IsWallLeft && IsWallRight && !IsWallHere)
                                    {
                                        // Set the dead end number of the current block
                                        m_DeadEnd[DeadEndBlockX, DeadEndBlockY] = CurrentDeadEnd;

                                        // Continue scanning (go up)
                                        DeadEndBlockY--;

                                        // Update the auxiliary variables value.
                                        IsWallHere  = m_pArena.IsWall(DeadEndBlockX, DeadEndBlockY);
                                        IsWallLeft  = m_pArena.IsWall(DeadEndBlockX - 1, DeadEndBlockY);
                                        IsWallRight = m_pArena.IsWall(DeadEndBlockX + 1, DeadEndBlockY);
                                    }

                                    // If there is no wall blocking the way and the dead end was entirely scanned
                                    if (!IsWallHere)
                                    {
                                        // Then this dead end has an exit. Record the block coordinates of this exit.
                                        m_DeadEndExit[CurrentDeadEnd].BlockX = DeadEndBlockX;
                                        m_DeadEndExit[CurrentDeadEnd].BlockY = DeadEndBlockY;
                                    }

                                    // Next dead end number
                                    CurrentDeadEnd++;
                                }
                                // If this block is the back of NO dead end
                                else
                                {
                                    // Set there is no dead end on this block.
                                    m_DeadEnd[BlockX, BlockY] = -1;
                                }
                            }
                            // If there is a wall on this block
                            else
                            {
                                // There is definitely no dead end here
                                m_DeadEnd[BlockX, BlockY] = -1;
                            }
                        } // if
                    } // if
                } // for
            } // for

            //*****************
            // SOFT WALL NEAR
            //*****************

            // Auxiliary variables for code readability : is there a soft wall around the current block?
            bool IsSoftWallUp;
            bool IsSoftWallDown;
            bool IsSoftWallLeft;
            bool IsSoftWallRight;

            // Current distance of the scanned block from the start block
            int Depth;

            // Scan the blocks of the arena
            for (BlockX = 0; BlockX < Globals.ARENA_WIDTH; BlockX++)
            {
                for (BlockY = 0; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                {
                    // If the block is on the edges of the arena grid
                    if (BlockX == 0 || BlockX == Globals.ARENA_WIDTH - 1 ||
                        BlockY == 0 || BlockY == Globals.ARENA_HEIGHT - 1)
                    {
                        // There is definitely no soft wall near here
                        m_SoftWallNear[BlockX, BlockY] = -1;
                    }
                    // If the block is not on the edges of the arena grid
                    else
                    {
                        // Is there a wall on this block?
                        IsWallHere = m_pArena.IsWall(BlockX, BlockY);

                        // If there is no wall on this block
                        if (!IsWallHere)
                        {
                            //----------------------
                            // Scan above the block
                            //----------------------

                            // Start scanning (distance 1 from the block)
                            Depth = 1;

                            // Scan until there is a wall or until maximum depth has been reached
                            do
                            {
                                // Is there a wall on the scanned block?
                                IsWallUp = m_pArena.IsWall(BlockX, BlockY - Depth);

                                // Is there a soft wall on the scanned block?
                                IsSoftWallUp = m_pArena.IsSoftWall(BlockX, BlockY - Depth);

                                // Continue scanning
                                Depth++;
                            }
                            while (Depth <= SOFT_WALL_NEAR_MAX_DEPTH && !IsWallUp && !m_pArena.IsItem(BlockX, BlockY - Depth));

                            //----------------------
                            // Scan below the block
                            //----------------------

                            // Start scanning (distance 1 from the block)
                            Depth = 1;

                            // Scan until there is a wall or until maximum depth has been reached
                            do
                            {
                                // Is there a wall on the scanned block?
                                IsWallDown = m_pArena.IsWall(BlockX, BlockY + Depth);

                                // Is there a soft wall on the scanned block?
                                IsSoftWallDown = m_pArena.IsSoftWall(BlockX, BlockY + Depth);

                                // Continue scanning
                                Depth++;
                            }
                            while (Depth <= SOFT_WALL_NEAR_MAX_DEPTH && !IsWallDown && !m_pArena.IsItem(BlockX, BlockY + Depth));

                            //-------------------------------
                            // Scan to the left of the block
                            //-------------------------------

                            // Start scanning (distance 1 from the block)
                            Depth = 1;

                            // Scan until there is a wall or until maximum depth has been reached
                            do
                            {
                                // Is there a wall on the scanned block?
                                IsWallLeft = m_pArena.IsWall(BlockX - Depth, BlockY);

                                // Is there a soft wall on the scanned block?
                                IsSoftWallLeft = m_pArena.IsSoftWall(BlockX - Depth, BlockY);

                                // Continue scanning
                                Depth++;
                            }
                            while (Depth <= SOFT_WALL_NEAR_MAX_DEPTH && !IsWallLeft && !m_pArena.IsItem(BlockX - Depth, BlockY));

                            //--------------------------------
                            // Scan to the right of the block
                            //--------------------------------

                            // Start scanning (distance 1 from the block)
                            Depth = 1;

                            // Scan until there is a wall or until maximum depth has been reached
                            do
                            {
                                // Is there a wall on the scanned block?
                                IsWallRight = m_pArena.IsWall(BlockX + Depth, BlockY);

                                // Is there a soft wall on the scanned block?
                                IsSoftWallRight = m_pArena.IsSoftWall(BlockX + Depth, BlockY);

                                // Continue scanning
                                Depth++;
                            }
                            while (Depth <= SOFT_WALL_NEAR_MAX_DEPTH && !IsWallRight && !m_pArena.IsItem(BlockX + Depth, BlockY));

                            //--------------------------------------------------
                            // Count total number of soft walls near this block
                            //--------------------------------------------------

                            // No soft walls near this block yet
                            int NumSoftWallsNear = 0;

                            // Increase this number for each direction if there is a soft wall in this direction
                            if (IsSoftWallUp)    NumSoftWallsNear++;
                            if (IsSoftWallDown)  NumSoftWallsNear++;
                            if (IsSoftWallLeft)  NumSoftWallsNear++;
                            if (IsSoftWallRight) NumSoftWallsNear++;

                            // Set the number of soft walls near this block
                            m_SoftWallNear[BlockX, BlockY] = NumSoftWallsNear;
                        }
                        // If there is a wall on this block
                        else
                        {
                            // There is definitely no soft wall near here
                            m_SoftWallNear[BlockX, BlockY] = -1;
                        }
                    } // if
                } // for
            } // for

            //*************
            // DANGER AND BURNING WALL AND BURNING SOON WALL
            //*************

            // Scan each block of the danger array and wallburn array
            for (BlockX = 0; BlockX < Globals.ARENA_WIDTH; BlockX++)
            {
                for (BlockY = 0; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                {
                    // Set no danger for the moment
                    m_Danger[BlockX, BlockY] = EDanger.DANGER_NONE;

                    m_DangerTimeLeft[BlockX, BlockY] = 999.0f;

                    // Set true if there is a burning wall
                    m_WallBurn[BlockX, BlockY] = m_pArena.IsBurningWall(BlockX, BlockY);
                }
            }

            // Create an index array for the bombs.
            // Each element represents a bomb; its value is the index to the bomb which
            // will ignite this bomb.
            List<int> BombIndex = new List<int>(m_pArena.MaxBombs());
            for (int Index = 0; Index < m_pArena.MaxBombs(); Index++)
            {
                BombIndex.Add(Index); // start with the bomb itself
            }

            // Was there an update?
            bool Updated;

            // Scan each block of the arena
            do
            {
                Updated = false;

                for (BlockX = 0; BlockX < Globals.ARENA_WIDTH; BlockX++)
                {
                    for (BlockY = 0; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                    {
                        // If there is a flame or a wall on this block
                        if (m_pArena.IsFlame(BlockX, BlockY) || m_pArena.IsWall(BlockX, BlockY))
                        {
                            // This block is mortal
                            m_Danger[BlockX, BlockY] = EDanger.DANGER_MORTAL;
                            m_DangerTimeLeft[BlockX, BlockY] = 0.0f;
                        }
                        // If there is a bomb on this block
                        else if (m_pArena.IsBomb(BlockX, BlockY))
                        {
                            // This block will at least soon be mortal (but it can already be mortal)
                            if (m_Danger[BlockX, BlockY] == EDanger.DANGER_NONE)
                                m_Danger[BlockX, BlockY] = EDanger.DANGER_SOON;

                            float TimeLeft = -1.0f;
                            int FlameSize = -1;
                            bool IgnoreBomb = false;
                            int ThisBombIndex = -1;

                            for (int Index = 0; Index < m_pArena.MaxBombs(); Index++)
                            {
                                if (m_pArena.GetBomb(Index).GetBlockX() == BlockX &&
                                    m_pArena.GetBomb(Index).GetBlockY() == BlockY)
                                {
                                    FlameSize = m_pArena.GetBomb(Index).GetFlameSize();
                                    // Time left will be propagated to all bombs being ignited by this bomb
                                    TimeLeft = m_pArena.GetBomb(BombIndex[Index]).GetTimeLeft();
                                    // ignore bomb: when we don't know when a bomb explodes
                                    // we can't know when the soft walls around will disappear
                                    ThisBombIndex = BombIndex[Index];
                                    IgnoreBomb = m_pArena.GetBomb(Index).IsRemote() &&
                                        !m_pArena.GetBomber(m_pArena.GetBomb(Index).GetOwnerPlayer()).IsAlive();
                                    break;
                                }
                            }

                            Debug.Assert(TimeLeft != -1.0f);
                            Debug.Assert(FlameSize != -1);
                            Debug.Assert(ThisBombIndex != -1);

                            if (FlameSize >= 4)
                            {
                                switch (FlameSize)
                                {
                                    case 4:  FlameSize =  5; break;
                                    case 5:  FlameSize =  7; break;
                                    case 6:  FlameSize =  8; break;
                                    default: FlameSize = 99; break;
                                }
                            }

                            int ScanDepth;

                            // Block coordinates used to scan the blocks where the bomb creates danger
                            int DangerBlockX;
                            int DangerBlockY;

                            // Auxiliary variables for updating the timeleft when another bomb
                            // was found which may ignite the bomb we're currently dealing with
                            int UpdateX;
                            int UpdateY;

                            // Auxiliary variable for the time left on the current block
                            float TimeLeftHere;

                            // Auxiliary variables for code readability : is there a wall/bomb on the current block?
                            bool IsWall;
                            bool IsBomb;

                            //----------- Scan RIGHT -----------
                            // Start scanning on this block
                            DangerBlockX = BlockX;
                            DangerBlockY = BlockY;
                            ScanDepth = 0;

                            // No wall and no bomb on the current block
                            IsWall = false;
                            IsBomb = false;

                            // While there is no wall or bomb that could stop the explosion flames
                            while (true)
                            {
                                // If there is a bomb where we are scanning or if we scanned deep enough
                                if (IsBomb || ScanDepth > FlameSize)
                                {
                                    // Update time left if this bomb might ignite the other
                                    for (int Index = 0; IsBomb && Index < m_pArena.MaxBombs(); Index++)
                                    {
                                        if (m_pArena.GetBomb(Index).GetBlockX() == DangerBlockX &&
                                            m_pArena.GetBomb(Index).GetBlockY() == DangerBlockY)
                                        {
                                            TimeLeftHere = m_pArena.GetBomb(BombIndex[Index]).GetTimeLeft();
                                            if (TimeLeft > TimeLeftHere)
                                            {
                                                TimeLeft = TimeLeftHere;
                                                Updated = true;

                                                // update timeleft on the blocks which have been passed
                                                for (UpdateX = BlockX; UpdateX <= DangerBlockX; UpdateX++)
                                                    m_DangerTimeLeft[UpdateX, DangerBlockY] = TimeLeft;

                                                // update BombIndex array elements
                                                for (int i = 0; i < m_pArena.MaxBombs(); i++)
                                                    if (BombIndex[i] == ThisBombIndex)
                                                        BombIndex[i] = BombIndex[Index];
                                            }

                                            break;
                                        }
                                    }

                                    // Stop scanning.
                                    break;
                                }
                                // If there is a wall where we are scanning
                                else if (IsWall)
                                {
                                    // If this is a soft wall
                                    if (m_pArena.IsSoftWall(DangerBlockX, DangerBlockY))
                                    {
                                        // Then this wall will soon burn (unless we think we can ignore the bomb)
                                        if (!IgnoreBomb)
                                            m_WallBurn[DangerBlockX, DangerBlockY] = true;
                                    }

                                    // Stop scanning.
                                    break;
                                }
                                // If there is no bomb and no wall on the block where we are scanning
                                else if (!IgnoreBomb)
                                {
                                    // If no danger was recorded on this block
                                    if (m_Danger[DangerBlockX, DangerBlockY] == EDanger.DANGER_NONE)
                                    {
                                        // This block will soon be mortal
                                        m_Danger[DangerBlockX, DangerBlockY] = EDanger.DANGER_SOON;
                                    }

                                    if (m_DangerTimeLeft[DangerBlockX, DangerBlockY] > TimeLeft)
                                    {
                                        m_DangerTimeLeft[DangerBlockX, DangerBlockY] = TimeLeft;
                                    }
                                }

                                // Continue scanning (go right)
                                DangerBlockX++;
                                ScanDepth++; // Go deeper

                                // Update auxiliary variables
                                IsWall = m_pArena.IsWall(DangerBlockX, DangerBlockY);
                                IsBomb = m_pArena.IsBomb(DangerBlockX, DangerBlockY);
                            } // while

                            //----------- Scan LEFT -----------
                            // Start scanning on this block
                            DangerBlockX = BlockX;
                            DangerBlockY = BlockY;
                            ScanDepth = 0;

                            // No wall and no bomb on the current block
                            IsWall = false;
                            IsBomb = false;

                            // While there is no wall or bomb that could stop the explosion flames
                            while (true)
                            {
                                // If there is a bomb where we are scanning or if we scanned deep enough
                                if (IsBomb || ScanDepth > FlameSize)
                                {
                                    // Update time left if this bomb might ignite the other
                                    for (int Index = 0; IsBomb && Index < m_pArena.MaxBombs(); Index++)
                                    {
                                        if (m_pArena.GetBomb(Index).GetBlockX() == DangerBlockX &&
                                            m_pArena.GetBomb(Index).GetBlockY() == DangerBlockY)
                                        {
                                            TimeLeftHere = m_pArena.GetBomb(BombIndex[Index]).GetTimeLeft();
                                            if (TimeLeft > TimeLeftHere)
                                            {
                                                TimeLeft = TimeLeftHere;
                                                Updated = true;

                                                // update timeleft on the blocks which have been passed
                                                for (UpdateX = DangerBlockX; UpdateX <= BlockX; UpdateX++)
                                                    m_DangerTimeLeft[UpdateX, DangerBlockY] = TimeLeft;

                                                // update BombIndex array elements
                                                for (int i = 0; i < m_pArena.MaxBombs(); i++)
                                                    if (BombIndex[i] == ThisBombIndex)
                                                        BombIndex[i] = BombIndex[Index];
                                            }

                                            break;
                                        }
                                    }

                                    // Stop scanning.
                                    break;
                                }
                                // If there is a wall where we are scanning
                                else if (IsWall)
                                {
                                    // If this is a soft wall
                                    if (m_pArena.IsSoftWall(DangerBlockX, DangerBlockY))
                                    {
                                        // Then this wall will soon burn (unless we think we can ignore the bomb)
                                        if (!IgnoreBomb)
                                            m_WallBurn[DangerBlockX, DangerBlockY] = true;
                                    }

                                    // Stop scanning.
                                    break;
                                }
                                // If there is no bomb and no wall on the block where we are scanning
                                else if (!IgnoreBomb)
                                {
                                    // If no danger was recorded on this block
                                    if (m_Danger[DangerBlockX, DangerBlockY] == EDanger.DANGER_NONE)
                                    {
                                        // This block will soon be mortal
                                        m_Danger[DangerBlockX, DangerBlockY] = EDanger.DANGER_SOON;
                                    }

                                    if (m_DangerTimeLeft[DangerBlockX, DangerBlockY] > TimeLeft)
                                    {
                                        m_DangerTimeLeft[DangerBlockX, DangerBlockY] = TimeLeft;
                                    }
                                }

                                // Continue scanning (go left)
                                DangerBlockX--;
                                ScanDepth++; // Go deeper

                                // Update auxiliary variables
                                IsWall = m_pArena.IsWall(DangerBlockX, DangerBlockY);
                                IsBomb = m_pArena.IsBomb(DangerBlockX, DangerBlockY);
                            } // while

                            //----------- Scan UP -----------
                            // Start scanning on this block
                            DangerBlockX = BlockX;
                            DangerBlockY = BlockY;
                            ScanDepth = 0;

                            // No wall and no bomb on the current block
                            IsWall = false;
                            IsBomb = false;

                            // While there is no wall or bomb that could stop the explosion flames
                            while (true)
                            {
                                // If there is a bomb where we are scanning or if we scanned deep enough
                                if (IsBomb || ScanDepth > FlameSize)
                                {
                                    // Update time left if this bomb might ignite the other
                                    for (int Index = 0; IsBomb && Index < m_pArena.MaxBombs(); Index++)
                                    {
                                        if (m_pArena.GetBomb(Index).GetBlockX() == DangerBlockX &&
                                            m_pArena.GetBomb(Index).GetBlockY() == DangerBlockY)
                                        {
                                            TimeLeftHere = m_pArena.GetBomb(BombIndex[Index]).GetTimeLeft();
                                            if (TimeLeft > TimeLeftHere)
                                            {
                                                TimeLeft = TimeLeftHere;
                                                Updated = true;

                                                // update timeleft on the blocks which have been passed
                                                for (UpdateY = DangerBlockY; UpdateY <= BlockY; UpdateY++)
                                                    m_DangerTimeLeft[DangerBlockX, UpdateY] = TimeLeft;

                                                // update BombIndex array elements
                                                for (int i = 0; i < m_pArena.MaxBombs(); i++)
                                                    if (BombIndex[i] == ThisBombIndex)
                                                        BombIndex[i] = BombIndex[Index];
                                            }

                                            break;
                                        }
                                    }

                                    // Stop scanning.
                                    break;
                                }
                                // If there is a wall where we are scanning
                                else if (IsWall)
                                {
                                    // If this is a soft wall
                                    if (m_pArena.IsSoftWall(DangerBlockX, DangerBlockY))
                                    {
                                        // Then this wall will soon burn (unless we think we can ignore the bomb)
                                        if (!IgnoreBomb)
                                            m_WallBurn[DangerBlockX, DangerBlockY] = true;
                                    }

                                    // Stop scanning.
                                    break;
                                }
                                // If there is no bomb and no wall on the block where we are scanning
                                else if (!IgnoreBomb)
                                {
                                    // If no danger was recorded on this block
                                    if (m_Danger[DangerBlockX, DangerBlockY] == EDanger.DANGER_NONE)
                                    {
                                        // This block will soon be mortal
                                        m_Danger[DangerBlockX, DangerBlockY] = EDanger.DANGER_SOON;
                                    }

                                    if (m_DangerTimeLeft[DangerBlockX, DangerBlockY] > TimeLeft)
                                    {
                                        m_DangerTimeLeft[DangerBlockX, DangerBlockY] = TimeLeft;
                                    }
                                }

                                // Continue scanning (go up)
                                DangerBlockY--;
                                ScanDepth++; // Go deeper

                                // Update auxiliary variables
                                IsWall = m_pArena.IsWall(DangerBlockX, DangerBlockY);
                                IsBomb = m_pArena.IsBomb(DangerBlockX, DangerBlockY);
                            } // while

                            //----------- Scan DOWN -----------
                            // Start scanning on this block
                            DangerBlockX = BlockX;
                            DangerBlockY = BlockY;
                            ScanDepth = 0;

                            // No wall and no bomb on the current block
                            IsWall = false;
                            IsBomb = false;

                            // While there is no wall or bomb that could stop the explosion flames
                            while (true)
                            {
                                // If there is a bomb where we are scanning or if we scanned deep enough
                                if (IsBomb || ScanDepth > FlameSize)
                                {
                                    // Update time left if this bomb might ignite the other
                                    for (int Index = 0; IsBomb && Index < m_pArena.MaxBombs(); Index++)
                                    {
                                        if (m_pArena.GetBomb(Index).GetBlockX() == DangerBlockX &&
                                            m_pArena.GetBomb(Index).GetBlockY() == DangerBlockY)
                                        {
                                            TimeLeftHere = m_pArena.GetBomb(BombIndex[Index]).GetTimeLeft();
                                            if (TimeLeft > TimeLeftHere)
                                            {
                                                TimeLeft = TimeLeftHere;
                                                Updated = true;

                                                // update timeleft on the blocks which have been passed
                                                for (UpdateY = BlockY; UpdateY <= DangerBlockY; UpdateY++)
                                                    m_DangerTimeLeft[DangerBlockX, UpdateY] = TimeLeft;

                                                // update BombIndex array elements
                                                for (int i = 0; i < m_pArena.MaxBombs(); i++)
                                                    if (BombIndex[i] == ThisBombIndex)
                                                        BombIndex[i] = BombIndex[Index];
                                            }

                                            break;
                                        }
                                    }

                                    // Stop scanning.
                                    break;
                                }
                                // If there is a wall where we are scanning
                                else if (IsWall)
                                {
                                    // If this is a soft wall
                                    if (m_pArena.IsSoftWall(DangerBlockX, DangerBlockY))
                                    {
                                        // Then this wall will soon burn (unless we think we can ignore the bomb)
                                        if (!IgnoreBomb)
                                            m_WallBurn[DangerBlockX, DangerBlockY] = true;
                                    }

                                    // Stop scanning.
                                    break;
                                }
                                // If there is no bomb and no wall on the block where we are scanning
                                else if (!IgnoreBomb)
                                {
                                    // If no danger was recorded on this block
                                    if (m_Danger[DangerBlockX, DangerBlockY] == EDanger.DANGER_NONE)
                                    {
                                        // This block will soon be mortal
                                        m_Danger[DangerBlockX, DangerBlockY] = EDanger.DANGER_SOON;
                                    }

                                    if (m_DangerTimeLeft[DangerBlockX, DangerBlockY] > TimeLeft)
                                    {
                                        m_DangerTimeLeft[DangerBlockX, DangerBlockY] = TimeLeft;
                                    }
                                }

                                // Continue scanning (go down)
                                DangerBlockY++;
                                ScanDepth++; // Go deeper

                                // Update auxiliary variables
                                IsWall = m_pArena.IsWall(DangerBlockX, DangerBlockY);
                                IsBomb = m_pArena.IsBomb(DangerBlockX, DangerBlockY);
                            } // while
                        } // if
                    } // for
                } // for
            } // do
            while (Updated);

            // If the arena is closing right now
            if (m_pArena.GetArenaCloser().IsClosing())
            {
                // Save in how many seconds the next falling wall will start falling
                float DangerTimeLeft = m_pArena.GetArenaCloser().GetTimeLeftBeforeClosingNextBlock();

                // Scan the next X blocks that will be closed
                for (int Index = 0; Index < m_pArena.GetArenaCloser().GetNumberOfBlocksLeft() && Index < 10; Index++)
                {
                    // Save the block position of the block that will soon be closed
                    BlockX = m_pArena.GetArenaCloser().GetNextBlockPositionX(Index);
                    BlockY = m_pArena.GetArenaCloser().GetNextBlockPositionY(Index);

                    // If there is no danger on this block
                    if (m_Danger[BlockX, BlockY] == EDanger.DANGER_NONE)
                    {
                        // Now this block will soon be dangerous
                        m_Danger[BlockX, BlockY] = EDanger.DANGER_SOON;
                    }

                    // If the danger of this falling wall will happen earlier
                    // than the current recorded danger on this block
                    if (m_DangerTimeLeft[BlockX, BlockY] > DangerTimeLeft)
                    {
                        // The danger that will happen earlier has a higher priority
                        m_DangerTimeLeft[BlockX, BlockY] = DangerTimeLeft;
                    }

                    // Get the time left in seconds before the next falling wall will start falling
                    DangerTimeLeft += m_pArena.GetArenaCloser().GetTimeBetweenTwoBlockClosures();
                }
            }
        } // Update

        //**************************************************************************************
    }
}
