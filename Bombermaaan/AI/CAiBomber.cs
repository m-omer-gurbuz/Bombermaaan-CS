// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2008 Markus Drescher
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
 *  \file CAiBomber.cs
 *  \brief Bomber bot (C# port)
 */

using System;
using System.Diagnostics;
namespace Bombermaaan 
{

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Describes the mode of the computer player</summary>
    public enum EComputerMode
    {
        COMPUTERMODE_THINK,      //!< Deciding what to do and set the new computer mode
        COMPUTERMODE_ITEM,       //!< Picking up items or dropping bombs in order to burn walls
        COMPUTERMODE_ATTACK,     //!< Attacking a bomber
        COMPUTERMODE_THROW,      //!< Attacking a bomber (throw the bomb afterwards with COMPUTERMODE_ATTACK)
        COMPUTERMODE_2NDACTION,  //!< Use punch/stop bomb while being kicked/remote detonate
        COMPUTERMODE_DEFENCE,    //!< Trying to be in a safe place
        COMPUTERMODE_WALK        //!< Walking in random directions until there is some activity around the bomber
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Describes the direction to the next enemy</summary>
    public enum EEnemyDirection
    {
        ENEMYDIRECTION_UNKNOWN, //!< No enemy near us (at least we think so)
        ENEMYDIRECTION_HERE,    //!< Enemy is at the same position as we are
        ENEMYDIRECTION_ABOVE,   //!< Enemy is above us
        ENEMYDIRECTION_BELOW,   //!< Enemy is below us
        ENEMYDIRECTION_LEFT,
        ENEMYDIRECTION_RIGHT
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public class CAiBomber
    {
        //---------------------------------------------------------------------------
        // Constants (mirror the C++ #defines)
        //---------------------------------------------------------------------------
        private const int AI_VIEW_SIZE = 6;

        private const int MAX_NEAR_DISTANCE = 3;
        private const int MAX_CALLS_MODEITEM = 5;
        private const int MAX_CALLS_MODEWALK = 5;

        //---------------------------------------------------------------------------
        // Random number generator (shared across all instances, equivalent to C rand)
        //---------------------------------------------------------------------------
        private static readonly Random _rng = new Random();

        //---------------------------------------------------------------------------
        // Static burn-mark table
        //---------------------------------------------------------------------------
        private static readonly int[,] m_BurnMark = new int[4, 6]
        {
            {  0,  0,  0,  0,  0,  0 },
            { 10,  8,  5,  3,  2,  1 },
            { 20, 17, 15, 12, 10,  5 },
            { 30, 26, 24, 22,  5, 10 }
        };

        //---------------------------------------------------------------------------
        // Instance fields
        //---------------------------------------------------------------------------
        private CAiArena    m_pArena;
        private CDisplay    m_pDisplay;
        private int         m_Player;

        // Accessibility maps indexed [x, y]
        private int[,]      m_Accessible;        // -1 = inaccessible, else BFS distance
        private int[,]      m_PseudoAccessible;  // Same but bombs are NOT obstacles

        private int         m_NumAccessible;
        private float       m_StopTimeLeft;      // Seconds left before sending commands
        private int         m_ItemGoalBlockX;
        private int         m_ItemGoalBlockY;
        private bool        m_ItemDropBomb;
        private int         m_BlockWalk;
        private float       m_WalkTime;
        private EComputerMode  m_ComputerMode;
        private EBomberMove    m_BomberMove;
        private EBomberAction  m_BomberAction;
        private float          m_BomberMoveTimeLeft;
        private CBomber        m_pBomber;

        // Block coordinates around the bomber (for readability)
        private int m_BlockHereX;
        private int m_BlockHereY;
        private int m_BlockUpX;
        private int m_BlockUpY;
        private int m_BlockDownX;
        private int m_BlockDownY;
        private int m_BlockLeftX;
        private int m_BlockLeftY;
        private int m_BlockRightX;
        private int m_BlockRightY;

        // Per-method call counters that were C++ static locals
        private int _callsOfModeItem = 0;
        private uint _callsModeWalk  = 0;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CAiBomber()
        {
            m_pDisplay   = null;
            m_pArena     = null;
            m_pBomber    = null;
            m_Player     = -1;

            m_NumAccessible      = 0;
            m_StopTimeLeft       = 0.0f;
            m_ItemGoalBlockX     = 0;
            m_ItemGoalBlockY     = 0;
            m_ItemDropBomb       = false;
            m_BlockWalk          = 0;
            m_WalkTime           = 0.0f;
            m_ComputerMode       = EComputerMode.COMPUTERMODE_THINK;
            m_BomberMove         = EBomberMove.BOMBERMOVE_NONE;
            m_BomberAction       = EBomberAction.BOMBERACTION_NONE;
            m_BomberMoveTimeLeft = 0.0f;
            m_BlockHereX  = 0;
            m_BlockHereY  = 0;
            m_BlockUpX    = 0;
            m_BlockUpY    = 0;
            m_BlockDownX  = 0;
            m_BlockDownY  = 0;
            m_BlockLeftX  = 0;
            m_BlockLeftY  = 0;
            m_BlockRightX = 0;
            m_BlockRightY = 0;

            m_Accessible       = new int[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];
            m_PseudoAccessible = new int[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];

            for (int bx = 0; bx < Globals.ARENA_WIDTH; bx++)
            {
                for (int by = 0; by < Globals.ARENA_HEIGHT; by++)
                {
                    m_Accessible[bx, by]       = -1;
                    m_PseudoAccessible[bx, by] = -1;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void SetArena(CAiArena pArena)
        {
            Debug.Assert(pArena != null);
            m_pArena = pArena;
        }

        public void SetDisplay(CDisplay pDisplay)
        {
            Debug.Assert(pDisplay != null);
            m_pDisplay = pDisplay;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Create(int Player)
        {
            Debug.Assert(m_pArena != null);
            m_Player = Player;

            // Wait a little before thinking for the first time
            m_StopTimeLeft = 0.1f;

            // Reset commands variables
            m_BomberMove         = EBomberMove.BOMBERMOVE_NONE;
            m_BomberAction       = EBomberAction.BOMBERACTION_NONE;
            m_BomberMoveTimeLeft = 0.0f;

            m_BlockWalk = 0;

            // Think next time
            m_ComputerMode = EComputerMode.COMPUTERMODE_THINK; // added due to a valgrind warning
            SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Destroy()
        {
            // Debug display cleanup omitted (compile-time flags DEBUG_DRAW_* not ported)
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Update(float DeltaTime)
        {
            // Pointer to bomber
            m_pBomber = m_pArena.GetArena().GetBomber(m_Player);

            // Think and send commands to the bomber only if the bomber is alive
            if (m_pBomber.IsAlive())
            {
                // If the player does not have to stop commanding his bomber
                if (m_StopTimeLeft <= 0.0f)
                {
                    // If the current bombermove's duration has elapsed
                    if (m_BomberMoveTimeLeft <= 0.0f)
                    {
                        m_BlockWalk++;

                        // Bomber block coordinates
                        m_BlockHereX = m_pBomber.GetBlockX();
                        m_BlockHereY = m_pBomber.GetBlockY();

                        // Coordinates of blocks around the bomber
                        m_BlockUpX    = m_BlockHereX;
                        m_BlockUpY    = m_BlockHereY - 1;
                        m_BlockDownX  = m_BlockHereX;
                        m_BlockDownY  = m_BlockHereY + 1;
                        m_BlockLeftX  = m_BlockHereX - 1;
                        m_BlockLeftY  = m_BlockHereY;
                        m_BlockRightX = m_BlockHereX + 1;
                        m_BlockRightY = m_BlockHereY;

                        UpdateAccessibility();

                        // If the AI has to think
                        if (m_ComputerMode == EComputerMode.COMPUTERMODE_THINK)
                        {
                            // Think right now. We do this because the AI should ACT
                            // just after deciding what to do.
                            ModeThink();
                        }

                        // Update the computer player according to its mode
                        switch (m_ComputerMode)
                        {
                            case EComputerMode.COMPUTERMODE_ITEM:      ModeItem(DeltaTime);    break;
                            case EComputerMode.COMPUTERMODE_ATTACK:    ModeAttack();           break;
                            case EComputerMode.COMPUTERMODE_THROW:     ModeThrow();            break;
                            case EComputerMode.COMPUTERMODE_2NDACTION: ModeSecondAction();     break;
                            case EComputerMode.COMPUTERMODE_DEFENCE:   ModeDefence(DeltaTime); break;
                            case EComputerMode.COMPUTERMODE_WALK:      ModeWalk(DeltaTime);    break;
                            default: break;
                        }

                        if (m_pBomber.GetSickness() == ESick.SICK_INVERTION)
                        {
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_UP:    m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;  break;
                                case EBomberMove.BOMBERMOVE_DOWN:  m_BomberMove = EBomberMove.BOMBERMOVE_UP;    break;
                                case EBomberMove.BOMBERMOVE_LEFT:  m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT; break;
                                case EBomberMove.BOMBERMOVE_RIGHT: m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;  break;
                                default: break;
                            }
                        }
                    }

                    // Send commands to the bomber
                    m_pBomber.Command(m_BomberMove, m_BomberAction);

                    // Decrease time left before the bombermove has to be updated
                    m_BomberMoveTimeLeft -= DeltaTime;
                }
                // If the player has to stop commanding his bomber
                else
                {
                    // Send no command to the bomber
                    m_pBomber.Command(EBomberMove.BOMBERMOVE_NONE, EBomberAction.BOMBERACTION_NONE);

                    // Decrease time left before sending commands to the bomber
                    m_StopTimeLeft -= DeltaTime;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Determines if there is a bomber near us, i.e. at our position or
        /// max. MAX_NEAR_DISTANCE blocks away and in front of us.
        /// </summary>
        /// <param name="direction">Out parameter set to the direction of our enemy (may be null).</param>
        /// <param name="BeyondArenaFrontiers">
        /// Should we also look beyond the arena frontiers,
        /// i.e. start at the top if we are at the lowest position, etc.
        /// </param>
        /// <returns>True if there is an enemy near and in front of us, false otherwise.</returns>
        private bool EnemyNearAndFront(ref EEnemyDirection direction, bool BeyondArenaFrontiers = false)
        {
            int BlockX;
            int BlockY;

            // variables to keep loop assertion semantics (they may be out of bounds)
            int FakeBlockX;
            int FakeBlockY;

            bool beyondTheFrontier;

            if (m_pArena.GetArena().IsBomb(m_BlockHereX, m_BlockHereY))
            {
                direction = EEnemyDirection.ENEMYDIRECTION_UNKNOWN;
                return false;
            }

            //-----------------------------------------
            // Check if there is an enemy where we are
            //-----------------------------------------

            for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
            {
                if (!m_pArena.GetArena().GetBomber(Index).Exist() ||
                    !m_pArena.GetArena().GetBomber(Index).IsAlive())
                    continue;

                if (Index != m_Player &&
                    m_pArena.GetArena().GetBomber(Index).GetTeam().GetTeamId() !=
                        m_pArena.GetArena().GetBomber(m_Player).GetTeam().GetTeamId() &&
                    m_pArena.GetArena().GetBomber(Index).GetBlockX() == m_BlockHereX &&
                    m_pArena.GetArena().GetBomber(Index).GetBlockY() == m_BlockHereY)
                {
                    direction = EEnemyDirection.ENEMYDIRECTION_HERE;
                    return true;
                }
            }

            //---------------------------------------------------------
            // Check if there is an enemy near our bomber to the right
            //---------------------------------------------------------

            BlockX = m_BlockHereX + 1;
            BlockY = m_BlockHereY;
            FakeBlockX = BlockX;
            FakeBlockY = BlockY;
            beyondTheFrontier = false;

            while (FakeBlockX <= m_BlockHereX + MAX_NEAR_DISTANCE)
            {
                if (!BeyondArenaFrontiers && (BlockX >= Globals.ARENA_WIDTH ||
                    m_pArena.GetArena().IsWall(BlockX, BlockY) ||
                    m_pArena.GetArena().IsBomb(BlockX, BlockY)))
                {
                    break;
                }
                else if (BeyondArenaFrontiers && BlockX >= Globals.ARENA_WIDTH)
                {
                    BlockX = 0;
                    beyondTheFrontier = true;
                }

                if (BeyondArenaFrontiers && beyondTheFrontier &&
                    BlockX >= m_BlockHereX - MAX_NEAR_DISTANCE)
                    break;

                if (m_pArena.GetArena().IsBomber(BlockX, BlockY))
                {
                    bool EnemyBomber = false;
                    for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
                    {
                        if (!m_pArena.GetArena().GetBomber(Index).IsAlive() ||
                            !m_pArena.GetArena().GetBomber(Index).Exist())
                            continue;

                        if (m_pArena.GetArena().GetBomber(Index).GetTeam().GetTeamId() !=
                                m_pArena.GetArena().GetBomber(m_Player).GetTeam().GetTeamId() &&
                            m_pArena.GetArena().GetBomber(Index).GetBlockX() == BlockX &&
                            m_pArena.GetArena().GetBomber(Index).GetBlockY() == BlockY)
                        {
                            EnemyBomber = true;
                            break;
                        }
                    }

                    if (EnemyBomber)
                    {
                        direction = EEnemyDirection.ENEMYDIRECTION_RIGHT;
                        return true;
                    }
                }

                if (BeyondArenaFrontiers)
                {
                    if (!m_pArena.GetArena().IsBomb(BlockX, BlockY) &&
                        !m_pArena.GetArena().IsWall(BlockX, BlockY))
                        FakeBlockX++;
                }
                else
                {
                    FakeBlockX++;
                }
                BlockX++;
            }

            //---------------------------------------------------------
            // Check if there is an enemy near our bomber to the left
            //---------------------------------------------------------

            BlockX = m_BlockHereX - 1;
            BlockY = m_BlockHereY;
            FakeBlockX = BlockX;
            FakeBlockY = BlockY;
            beyondTheFrontier = false;

            while (FakeBlockX >= m_BlockHereX - MAX_NEAR_DISTANCE)
            {
                if (!BeyondArenaFrontiers && (BlockX < 0 ||
                    m_pArena.GetArena().IsWall(BlockX, BlockY) ||
                    m_pArena.GetArena().IsBomb(BlockX, BlockY)))
                {
                    break;
                }
                else if (BeyondArenaFrontiers && BlockX < 0)
                {
                    BlockX = Globals.ARENA_WIDTH - 1;
                    beyondTheFrontier = true;
                }

                if (BeyondArenaFrontiers && beyondTheFrontier &&
                    BlockX <= m_BlockHereX + MAX_NEAR_DISTANCE)
                    break;

                if (m_pArena.GetArena().IsBomber(BlockX, BlockY))
                {
                    bool EnemyBomber = false;
                    for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
                    {
                        if (!m_pArena.GetArena().GetBomber(Index).IsAlive() ||
                            !m_pArena.GetArena().GetBomber(Index).Exist())
                            continue;

                        if (m_pArena.GetArena().GetBomber(Index).GetTeam().GetTeamId() !=
                                m_pArena.GetArena().GetBomber(m_Player).GetTeam().GetTeamId() &&
                            m_pArena.GetArena().GetBomber(Index).GetBlockX() == BlockX &&
                            m_pArena.GetArena().GetBomber(Index).GetBlockY() == BlockY)
                        {
                            EnemyBomber = true;
                            break;
                        }
                    }

                    if (EnemyBomber)
                    {
                        direction = EEnemyDirection.ENEMYDIRECTION_LEFT;
                        return true;
                    }
                }

                if (BeyondArenaFrontiers)
                {
                    if (!m_pArena.GetArena().IsBomb(BlockX, BlockY) &&
                        !m_pArena.GetArena().IsWall(BlockX, BlockY))
                        FakeBlockX--;
                }
                else
                {
                    FakeBlockX--;
                }
                BlockX--;
            }

            //---------------------------------------------------------
            // Check if there is an enemy near our bomber above
            //---------------------------------------------------------

            BlockX = m_BlockHereX;
            BlockY = m_BlockHereY - 1;
            FakeBlockX = BlockX;
            FakeBlockY = BlockY;
            beyondTheFrontier = false;

            while (FakeBlockY >= m_BlockHereY - MAX_NEAR_DISTANCE)
            {
                if (!BeyondArenaFrontiers && (BlockY < 0 ||
                    m_pArena.GetArena().IsWall(BlockX, BlockY) ||
                    m_pArena.GetArena().IsBomb(BlockX, BlockY)))
                {
                    break;
                }
                else if (BeyondArenaFrontiers && BlockY < 0)
                {
                    BlockY = Globals.ARENA_HEIGHT - 1;
                    beyondTheFrontier = true;
                }

                if (BeyondArenaFrontiers && beyondTheFrontier &&
                    BlockY <= m_BlockHereY + MAX_NEAR_DISTANCE)
                    break;

                if (m_pArena.GetArena().IsBomber(BlockX, BlockY))
                {
                    bool EnemyBomber = false;
                    for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
                    {
                        if (!m_pArena.GetArena().GetBomber(Index).IsAlive() ||
                            !m_pArena.GetArena().GetBomber(Index).Exist())
                            continue;

                        if (m_pArena.GetArena().GetBomber(Index).GetTeam().GetTeamId() !=
                                m_pArena.GetArena().GetBomber(m_Player).GetTeam().GetTeamId() &&
                            m_pArena.GetArena().GetBomber(Index).GetBlockX() == BlockX &&
                            m_pArena.GetArena().GetBomber(Index).GetBlockY() == BlockY)
                        {
                            EnemyBomber = true;
                            break;
                        }
                    }

                    if (EnemyBomber)
                    {
                        direction = EEnemyDirection.ENEMYDIRECTION_ABOVE;
                        return true;
                    }
                }

                if (BeyondArenaFrontiers)
                {
                    if (!m_pArena.GetArena().IsBomb(BlockX, BlockY) &&
                        !m_pArena.GetArena().IsWall(BlockX, BlockY))
                        FakeBlockY--;
                }
                else
                {
                    FakeBlockY--;
                }
                BlockY--;
            }

            //---------------------------------------------------------
            // Check if there is an enemy near our bomber below
            //---------------------------------------------------------

            BlockX = m_BlockHereX;
            BlockY = m_BlockHereY + 1;
            FakeBlockX = BlockX;
            FakeBlockY = BlockY;
            beyondTheFrontier = false;

            while (FakeBlockY <= m_BlockHereY + MAX_NEAR_DISTANCE)
            {
                if (!BeyondArenaFrontiers && (BlockY >= Globals.ARENA_HEIGHT ||
                    m_pArena.GetArena().IsWall(BlockX, BlockY) ||
                    m_pArena.GetArena().IsBomb(BlockX, BlockY)))
                {
                    break;
                }
                else if (BeyondArenaFrontiers && BlockY >= Globals.ARENA_HEIGHT)
                {
                    BlockY = 0;
                    beyondTheFrontier = true;
                }

                if (BeyondArenaFrontiers && beyondTheFrontier &&
                    BlockY >= m_BlockHereY - MAX_NEAR_DISTANCE)
                    break;

                if (m_pArena.GetArena().IsBomber(BlockX, BlockY))
                {
                    bool EnemyBomber = false;
                    for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
                    {
                        if (!m_pArena.GetArena().GetBomber(Index).IsAlive() ||
                            !m_pArena.GetArena().GetBomber(Index).Exist())
                            continue;

                        if (m_pArena.GetArena().GetBomber(Index).GetTeam().GetTeamId() !=
                                m_pArena.GetArena().GetBomber(m_Player).GetTeam().GetTeamId() &&
                            m_pArena.GetArena().GetBomber(Index).GetBlockX() == BlockX &&
                            m_pArena.GetArena().GetBomber(Index).GetBlockY() == BlockY)
                        {
                            EnemyBomber = true;
                            break;
                        }
                    }

                    if (EnemyBomber)
                    {
                        direction = EEnemyDirection.ENEMYDIRECTION_BELOW;
                        return true;
                    }
                }

                if (BeyondArenaFrontiers)
                {
                    if (!m_pArena.GetArena().IsBomb(BlockX, BlockY) &&
                        !m_pArena.GetArena().IsWall(BlockX, BlockY))
                        FakeBlockY++;
                }
                else
                {
                    FakeBlockY++;
                }
                BlockY++;
            }

            // We scanned in every direction from the bomber.
            // There is no enemy bomber near and in front of our bomber.
            return false;
        }

        /// <summary>
        /// Convenience overload that discards the direction out parameter.
        /// </summary>
        private bool EnemyNearAndFront()
        {
            EEnemyDirection dir = EEnemyDirection.ENEMYDIRECTION_UNKNOWN;
            return EnemyNearAndFront(ref dir, false);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private bool EnemyNear(int BlockX, int BlockY)
        {
            // Scan the players
            for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
            {
                if (!m_pArena.GetArena().GetBomber(Index).Exist() ||
                    !m_pArena.GetArena().GetBomber(Index).IsAlive())
                    continue;

                // If the current player is not the one we are controlling
                // and the bomber of this player exists and is alive
                // and the manhattan distance between him and the tested block is not too big
                // and with big probability
                if (Index != m_Player &&
                    m_pArena.GetArena().GetBomber(Index).GetTeam().GetTeamId() !=
                        m_pArena.GetArena().GetBomber(m_Player).GetTeam().GetTeamId() &&
                    Math.Abs(m_pArena.GetArena().GetBomber(Index).GetBlockX() - BlockX) +
                    Math.Abs(m_pArena.GetArena().GetBomber(Index).GetBlockY() - BlockY) <= 3 &&
                    _rng.Next(100) < 90 + Index * 2)
                {
                    return true;
                }
            }

            return false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private bool EnemyNearRemoteFuseBomb(CBomb bomb)
        {
            // TODO: simulate explosion properly (like in CExplosion)
            int BombX = bomb.GetBlockX();
            int BombY = bomb.GetBlockY();

            for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
            {
                if (!m_pArena.GetArena().GetBomber(Index).Exist() ||
                    !m_pArena.GetArena().GetBomber(Index).IsAlive())
                    continue;

                int BomberX = m_pArena.GetArena().GetBomber(Index).GetBlockX();
                int BomberY = m_pArena.GetArena().GetBomber(Index).GetBlockY();

                if (Index != m_Player &&
                    m_pArena.GetArena().GetBomber(Index).GetTeam().GetTeamId() !=
                        m_pArena.GetArena().GetBomber(m_Player).GetTeam().GetTeamId() &&
                    ((BomberX == BombX && Math.Abs(BomberY - BombY) <= bomb.GetFlameSize()) ||
                     (BomberY == BombY && Math.Abs(BomberX - BombX) <= bomb.GetFlameSize())) &&
                    _rng.Next(100) < 70 + Index * 2)
                {
                    return true;
                }
            }

            return false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private bool TeamMateNearRemoteFuseBomb(CBomb bomb)
        {
            // TODO: simulate explosion properly (like in CExplosion)
            int BombX = bomb.GetBlockX();
            int BombY = bomb.GetBlockY();

            for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
            {
                if (!m_pArena.GetArena().GetBomber(Index).Exist() ||
                    !m_pArena.GetArena().GetBomber(Index).IsAlive())
                    continue;

                int BomberX = m_pArena.GetArena().GetBomber(Index).GetBlockX();
                int BomberY = m_pArena.GetArena().GetBomber(Index).GetBlockY();

                if (m_pArena.GetArena().GetBomber(Index).GetTeam().GetTeamId() ==
                        m_pArena.GetArena().GetBomber(m_Player).GetTeam().GetTeamId() &&
                    ((BomberX == BombX && Math.Abs(BomberY - BombY) <= bomb.GetFlameSize()) ||
                     (BomberY == BombY && Math.Abs(BomberX - BombX) <= bomb.GetFlameSize())))
                {
                    return true;
                }
            }

            return false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private bool DropBombOK(int BlockX, int BlockY)
        {
            int Depth;
            int DangerBlockX;
            int DangerBlockY;

            // If the tested block is NOT accessible to our bomber
            if (m_Accessible[BlockX, BlockY] == -1)
                return false;

            if (m_pArena.GetArena().GetArenaCloser().GetNumberOfBlocksLeft() <= 15)
                return false;

            if (m_pArena.GetArena().IsBomb(BlockX, BlockY))
                return false;

            if (m_pArena.GetDanger(BlockX, BlockY) == EDanger.DANGER_NONE)
            {
                if ((BlockX - 1 < 0 || m_pArena.GetDanger(BlockX - 1, BlockY) != EDanger.DANGER_NONE) &&
                    (BlockX + 1 >= Globals.ARENA_WIDTH  || m_pArena.GetDanger(BlockX + 1, BlockY) != EDanger.DANGER_NONE) &&
                    (BlockY - 1 < 0 || m_pArena.GetDanger(BlockX, BlockY - 1) != EDanger.DANGER_NONE) &&
                    (BlockX + 1 >= Globals.ARENA_HEIGHT || m_pArena.GetDanger(BlockX, BlockY + 1) != EDanger.DANGER_NONE))
                {
                    return false;
                }
            }

            if (m_pBomber.GetSickness() == ESick.SICK_COLIC)
                return false;

            // If a bomb is dropped on the tested block then of course one
            // accessible block will be endangered since the tested block
            // is accessible to our bomber.
            int AccessibleEndangered = 1;

            //-------------------------------------------------------------------------------------------
            // Make a fuzzy estimation of the flame size of our bombs (more human than exact flame size)
            //-------------------------------------------------------------------------------------------

            int FlameSize = m_pArena.GetArena().GetBomber(m_Player).GetFlameSize();

            if (FlameSize >= 4)
            {
                switch (FlameSize)
                {
                    case 4:  FlameSize = 5;  break; // Flame size estimation error is low
                    case 5:  FlameSize = 7;  break; // Flame size estimation error is medium
                    case 6:  FlameSize = 8;  break; // Flame size estimation error is medium
                    default: FlameSize = 99; break; // Flame size estimation error is high
                }
            }

            //----------------------------------------------------------------------------------------
            // Simulate the flame ray (right)
            //----------------------------------------------------------------------------------------

            DangerBlockX = BlockX + 1;
            DangerBlockY = BlockY;
            Depth = 0;

            while (!m_pArena.GetArena().IsBomb(DangerBlockX, DangerBlockY) &&
                   !m_pArena.GetArena().IsWall(DangerBlockX, DangerBlockY) &&
                   Depth <= FlameSize)
            {
                AccessibleEndangered++;

                if (m_pArena.GetArena().IsItem(DangerBlockX, DangerBlockY) &&
                    ItemMark(DangerBlockX, DangerBlockY) > 0)
                    return false;

                DangerBlockX++;
                Depth++;
            }

            if (m_pArena.GetArena().IsWall(DangerBlockX, DangerBlockY) &&
                m_pArena.GetWallBurn(DangerBlockX, DangerBlockY))
                return false;

            //----------------------------------------------------------------------------------------
            // Simulate the flame ray (left)
            //----------------------------------------------------------------------------------------

            DangerBlockX = BlockX - 1;
            DangerBlockY = BlockY;
            Depth = 0;

            while (!m_pArena.GetArena().IsBomb(DangerBlockX, DangerBlockY) &&
                   !m_pArena.GetArena().IsWall(DangerBlockX, DangerBlockY) &&
                   Depth <= FlameSize)
            {
                AccessibleEndangered++;

                if (m_pArena.GetArena().IsItem(DangerBlockX, DangerBlockY) &&
                    ItemMark(DangerBlockX, DangerBlockY) > 0)
                    return false;

                DangerBlockX--;
                Depth++;
            }

            if (m_pArena.GetArena().IsWall(DangerBlockX, DangerBlockY) &&
                m_pArena.GetWallBurn(DangerBlockX, DangerBlockY))
                return false;

            //----------------------------------------------------------------------------------------
            // Simulate the flame ray (up)
            //----------------------------------------------------------------------------------------

            DangerBlockX = BlockX;
            DangerBlockY = BlockY - 1;
            Depth = 0;

            while (!m_pArena.GetArena().IsBomb(DangerBlockX, DangerBlockY) &&
                   !m_pArena.GetArena().IsWall(DangerBlockX, DangerBlockY) &&
                   Depth <= FlameSize)
            {
                AccessibleEndangered++;

                if (m_pArena.GetArena().IsItem(DangerBlockX, DangerBlockY) &&
                    ItemMark(DangerBlockX, DangerBlockY) > 0)
                    return false;

                DangerBlockY--;
                Depth++;
            }

            if (m_pArena.GetArena().IsWall(DangerBlockX, DangerBlockY) &&
                m_pArena.GetWallBurn(DangerBlockX, DangerBlockY))
                return false;

            //----------------------------------------------------------------------------------------
            // Simulate the flame ray (down)
            //----------------------------------------------------------------------------------------

            DangerBlockX = BlockX;
            DangerBlockY = BlockY + 1;
            Depth = 0;

            while (!m_pArena.GetArena().IsBomb(DangerBlockX, DangerBlockY) &&
                   !m_pArena.GetArena().IsWall(DangerBlockX, DangerBlockY) &&
                   Depth <= FlameSize)
            {
                AccessibleEndangered++;

                if (m_pArena.GetArena().IsItem(DangerBlockX, DangerBlockY) &&
                    ItemMark(DangerBlockX, DangerBlockY) > 0)
                    return false;

                DangerBlockY++;
                Depth++;
            }

            if (m_pArena.GetArena().IsWall(DangerBlockX, DangerBlockY) &&
                m_pArena.GetWallBurn(DangerBlockX, DangerBlockY))
                return false;

            return (m_NumAccessible > AccessibleEndangered);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private int ItemMark(int BlockX, int BlockY)
        {
            // If there is no item on the tested block
            // or if this is a skull or burning item
            // or if the item is not accessible to our bomber
            // or if the item will die in less than one second
            // or if the item is in danger and our bomber is too far away from this item
            if (!m_pArena.GetArena().IsItem(BlockX, BlockY) ||
                m_pArena.GetArena().IsSkullItem(BlockX, BlockY) ||
                m_pArena.GetArena().IsBurningItem(BlockX, BlockY) ||
                m_Accessible[BlockX, BlockY] == -1 ||
                m_pArena.GetDangerTimeLeft(BlockX, BlockY) < 1.0f ||
                (m_pArena.GetDanger(BlockX, BlockY) != EDanger.DANGER_NONE && m_Accessible[BlockX, BlockY] >= 3))
            {
                return 0;
            }

            int Mark = 0;

            //----------------------------------------
            // Take the type of the item into account
            //----------------------------------------

            EItemType ItemType = EItemType.ITEM_NONE;

            for (int Index = 0; Index < m_pArena.GetArena().MaxItems(); Index++)
            {
                if (m_pArena.GetArena().GetItem(Index).GetBlockX() == BlockX &&
                    m_pArena.GetArena().GetItem(Index).GetBlockY() == BlockY)
                {
                    ItemType = m_pArena.GetArena().GetItem(Index).GetType();
                    break;
                }
            }

            Debug.Assert(ItemType != EItemType.ITEM_NONE);

            switch (ItemType)
            {
                case EItemType.ITEM_SKULL:       Mark -= 100; break;
                case EItemType.ITEM_BOMB:        Mark += 10;  break;
                case EItemType.ITEM_FLAME:       Mark += 10;  break;
                case EItemType.ITEM_ROLLER:      Mark += 20;  break;
                case EItemType.ITEM_KICK:        Mark += 30;  break;
                case EItemType.ITEM_THROW:       Mark += 50;  break;
                case EItemType.ITEM_PUNCH:       Mark += 60;  break;
                case EItemType.ITEM_REMOTE:      Mark += 60;  break;
                case EItemType.ITEM_SHIELD:      Mark += 80;  break;
                case EItemType.ITEM_STRONGWEAK:  Mark += 40;  break;
                default: break;
            }

            //--------------------------------------------------------------
            // Take other details of this item into account (distance, etc)
            //--------------------------------------------------------------

            if (m_Accessible[BlockX, BlockY] <= 3)
                Mark += 5;
            else if (m_Accessible[BlockX, BlockY] <= 6)
                Mark += 3;

            if (m_pArena.GetDeadEnd(BlockX, BlockY) == -1)
                Mark += 10;

            return Mark;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /*
        ALGO :

        si on est en danger ou l'arene est presque completement fermee
        mode defense

        si ca semble ok d'attaquer (ou bien attaquer en jetant des bombes)
        mode attaque

        si on a oublie de detoner une bombe a tele-detonation et il y a un ennemi proche
        detoner cette bombe la

        chercher un item

        si on a trouve un item
        mode item

        si je suis malade et que je peux aller toucher un bomber ennemi
        pathfinding goto pour l'atteindre

        chercher des murs qui sont entrain de bruler

        si on en trouve
        mode item pour s'y rendre

        chercher le bloc avec le plus de murs cassables adjacents
        mode item pour s'y rendre et y poser une bombe

        si on a pas pu trouver quelque chose d'interessant a faire...
        mode balade
        */

        private void ModeThink()
        {
            int BlockX;
            int BlockY;
            bool FoundItem;
            int BestGoalBlockX = -1;
            int BestGoalBlockY = -1;
            int BestMark;
            bool FoundSoftWallBurn;
            int BestDistance;
            EEnemyDirection EnemyDirection = EEnemyDirection.ENEMYDIRECTION_UNKNOWN;

            //--------------------------------------------------
            // Check if we should defend.
            //--------------------------------------------------

            if (m_pArena.GetDanger(m_BlockHereX, m_BlockHereY) != EDanger.DANGER_NONE ||
                m_pArena.GetArena().GetArenaCloser().GetNumberOfBlocksLeft() <= 10)
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_DEFENCE);
                return;
            }

            //---------------------------------------------------------
            // Check if we should attack.
            //---------------------------------------------------------

            if ((EnemyNearAndFront(ref EnemyDirection, false) &&
                 DropBombOK(m_BlockHereX, m_BlockHereY) &&
                 _rng.Next(100) < (60 + (m_pBomber.HasShield() ? 35 : 0))))
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_ATTACK);
                return;
            }
            else if (m_pBomber.CanThrowBombs() &&
                     EnemyNearAndFront(ref EnemyDirection, true) &&
                     (DropBombOK(m_BlockHereX, m_BlockHereY) ||
                      m_pArena.GetArena().IsBomb(m_BlockHereX, m_BlockHereY)) &&
                     _rng.Next(100) < 50)
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_THROW);
                return;
            }

            //----------------------------------------------------------------------
            // Check if we accidentally forgot some bombs which have a remote trigger.
            //----------------------------------------------------------------------

            if (m_pBomber.CanRemoteFuseBombs())
            {
                for (int Index = 0; Index < m_pArena.GetArena().MaxBombs(); Index++)
                {
                    if (m_pArena.GetArena().GetBomb(Index).Exist() &&
                        m_pArena.GetArena().GetBomb(Index).IsRemote() &&
                        m_pArena.GetArena().GetBomb(Index).GetOwnerPlayer() == m_Player)
                    {
                        if (EnemyNearRemoteFuseBomb(m_pArena.GetArena().GetBomb(Index))
                            || _rng.Next(100) < 50)
                        {
                            if (!TeamMateNearRemoteFuseBomb(m_pArena.GetArena().GetBomb(Index))
                                || _rng.Next(100) > 96)
                            {
                                m_BomberAction = EBomberAction.BOMBERACTION_ACTION2; // detonate the bomb
                                m_pArena.GetArena().GetBomb(Index).Burn();
                                return;
                            }
                        }

                        // there is no enemy
                        break;
                    }
                }
            }

            //----------------------------------------------
            // Check if there is a cool item to pick up
            //----------------------------------------------

            FoundItem = false;
            BestMark  = 0;

            for (BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
            {
                for (BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                {
                    if (BlockX < 0 || BlockX > Globals.ARENA_WIDTH  - 1 ||
                        BlockY < 0 || BlockY > Globals.ARENA_HEIGHT - 1)
                        continue;

                    int Mark = ItemMark(BlockX, BlockY);

                    if (Mark > 0 && (Mark > BestMark || (Mark == BestMark && _rng.Next(100) >= 50)))
                    {
                        FoundItem      = true;
                        BestGoalBlockX = BlockX;
                        BestGoalBlockY = BlockY;
                        BestMark       = Mark;
                    }
                }
            }

            if (FoundItem)
            {
                Debug.Assert(BestGoalBlockX != -1);
                Debug.Assert(BestGoalBlockY != -1);

                m_ItemGoalBlockX = BestGoalBlockX;
                m_ItemGoalBlockY = BestGoalBlockY;
                m_ItemDropBomb   = false;
                SetComputerMode(EComputerMode.COMPUTERMODE_ITEM);
                return;
            }

            //*********
            // SICKNESS
            //*********

            if (m_pBomber.GetSickness() != ESick.SICK_NOTSICK)
            {
                for (int Index = 0; Index < Globals.MAX_PLAYERS; Index++)
                {
                    if (Index != m_Player &&
                        m_pArena.GetArena().GetBomber(Index).Exist() &&
                        m_pArena.GetArena().GetBomber(Index).IsAlive())
                    {
                        BlockX = m_pArena.GetArena().GetBomber(Index).GetBlockX();
                        BlockY = m_pArena.GetArena().GetBomber(Index).GetBlockY();

                        if (m_Accessible[BlockX, BlockY] != -1)
                        {
                            GoTo(BlockX, BlockY);
                            return;
                        }
                    }
                }
            }

            //------------------------------------------------------------------------
            // Check if there are soft walls that will burn soon or that are burning.
            //------------------------------------------------------------------------

            FoundSoftWallBurn  = false;
            BestGoalBlockX     = m_BlockHereX;
            BestGoalBlockY     = m_BlockHereY;
            BestDistance       = 999;

            for (BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
            {
                for (BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                {
                    if (BlockX < 0 || BlockX > Globals.ARENA_WIDTH  - 1 ||
                        BlockY < 0 || BlockY > Globals.ARENA_HEIGHT - 1)
                        continue;

                    if (m_PseudoAccessible[BlockX, BlockY] != -1 &&
                        m_PseudoAccessible[BlockX, BlockY] <= 5 &&
                        (BestDistance > m_PseudoAccessible[BlockX, BlockY] ||
                         (BestDistance == m_PseudoAccessible[BlockX, BlockY] && _rng.Next(100) >= 50)) &&
                        (m_pArena.GetDeadEnd(BlockX, BlockY) == -1 || !EnemyNear(BlockX, BlockY)) &&
                        ((BlockX > 0                        && m_pArena.GetWallBurn(BlockX - 1, BlockY)) ||
                         (BlockX < Globals.ARENA_WIDTH  - 1 && m_pArena.GetWallBurn(BlockX + 1, BlockY)) ||
                         (BlockY > 0                        && m_pArena.GetWallBurn(BlockX, BlockY - 1)) ||
                         (BlockY < Globals.ARENA_HEIGHT - 1 && m_pArena.GetWallBurn(BlockX, BlockY + 1))))
                    {
                        FoundSoftWallBurn  = true;
                        BestGoalBlockX     = BlockX;
                        BestGoalBlockY     = BlockY;
                        BestDistance       = m_PseudoAccessible[BlockX, BlockY];
                    }
                }
            }

            if (FoundSoftWallBurn)
            {
                if (m_Accessible[BestGoalBlockX, BestGoalBlockY] != -1 &&
                    (m_BlockHereX != BestGoalBlockX || m_BlockHereY != BestGoalBlockY))
                {
                    m_ItemGoalBlockX = BestGoalBlockX;
                    m_ItemGoalBlockY = BestGoalBlockY;
                    m_ItemDropBomb   = false;
                    SetComputerMode(EComputerMode.COMPUTERMODE_ITEM);
                    return;
                }
            }

            //----------------------------------------------------------
            // Find the block close to the highest number of soft walls
            //----------------------------------------------------------

            BestMark       = 0;
            BestGoalBlockX = m_BlockHereX;
            BestGoalBlockY = m_BlockHereY;

            for (BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
            {
                for (BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                {
                    if (BlockX < 0 || BlockX > Globals.ARENA_WIDTH  - 1 ||
                        BlockY < 0 || BlockY > Globals.ARENA_HEIGHT - 1)
                        continue;

                    if (m_pArena.GetSoftWallNear(BlockX, BlockY) != -1 &&
                        m_pArena.GetSoftWallNear(BlockX, BlockY) > 0 &&
                        m_Accessible[BlockX, BlockY] != -1 &&
                        m_Accessible[BlockX, BlockY] <= 5 &&
                        (m_pArena.GetDeadEnd(BlockX, BlockY) == -1 || !EnemyNear(BlockX, BlockY)) &&
                        m_pArena.GetDanger(BlockX, BlockY) == EDanger.DANGER_NONE &&
                        (
                            BestMark < m_BurnMark[m_pArena.GetSoftWallNear(BlockX, BlockY), m_Accessible[BlockX, BlockY]]
                            ||
                            (
                                BestMark == m_BurnMark[m_pArena.GetSoftWallNear(BlockX, BlockY), m_Accessible[BlockX, BlockY]]
                                && _rng.Next(100) >= 50
                            )
                        ) &&
                        DropBombOK(BlockX, BlockY))
                    {
                        BestGoalBlockX = BlockX;
                        BestGoalBlockY = BlockY;
                        BestMark = m_BurnMark[m_pArena.GetSoftWallNear(BlockX, BlockY), m_Accessible[BlockX, BlockY]];
                    }
                }
            }

            if (BestMark > 0)
            {
                m_ItemGoalBlockX = BestGoalBlockX;
                m_ItemGoalBlockY = BestGoalBlockY;
                m_ItemDropBomb   = true;
                SetComputerMode(EComputerMode.COMPUTERMODE_ITEM);
                return;
            }

            // Nothing better to do than walking in random directions.
            SetComputerMode(EComputerMode.COMPUTERMODE_WALK);
            m_WalkTime = 0.0f;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /*
        ALGO :

        si il y a un ennemi devant moi et que je peux poser une bombe sans danger
        mode reflechir

        si mon but est d'aller poser une bombe pour bruler du mur, et que je ne peux pas poser une bombe sans danger
        mode reflechir

        si mon but est de ramasser un item a tel bloc
        actualiser le bloc de but, ou se trouve l'item le plus interessant

        se diriger vers le bloc de but en cours

        si le bloc de but est accessible
        y aller
        si j'ai l'essayer MAX_CALLS_MODEITEM sans success
        mode reflechir
        sinon
        mode reflechir

        si j'ai atteint le bloc de but
        si je devais y poser une bombe
        poser une bombe
        sinon
        fini, j'ai pris l'item, mode reflechir
        sinon
        si je ne sais pas d'autres strategies il y a MAX_CALLS_MODEITEM fois, mode reflechir
        */

        private void ModeItem(float DeltaTime)
        {
            // Reset the commands to send to the bomber
            m_BomberMove   = EBomberMove.BOMBERMOVE_NONE;
            m_BomberAction = EBomberAction.BOMBERACTION_NONE;

            if ((EnemyNearAndFront() &&
                 DropBombOK(m_BlockHereX, m_BlockHereY) &&
                 _rng.Next(100) < 70)
                ||
                (m_ItemDropBomb && !DropBombOK(m_ItemGoalBlockX, m_ItemGoalBlockY)))
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                _callsOfModeItem = 0;
                return;
            }

            // If we are trying to pick up an item
            if (m_pArena.GetArena().IsItem(m_ItemGoalBlockX, m_ItemGoalBlockY))
            {
                bool FoundItem = false;
                int BestMark   = 0;

                for (int BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
                {
                    for (int BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                    {
                        if (BlockX < 0 || BlockX > Globals.ARENA_WIDTH  - 1 ||
                            BlockY < 0 || BlockY > Globals.ARENA_HEIGHT - 1)
                            continue;

                        int Mark = ItemMark(BlockX, BlockY);

                        if (Mark > 0 && Mark > BestMark)
                        {
                            FoundItem        = true;
                            m_ItemGoalBlockX = BlockX;
                            m_ItemGoalBlockY = BlockY;
                            BestMark         = Mark;
                        }
                    }
                }

                if (FoundItem && BestMark == 0)
                {
                    SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                    _callsOfModeItem = 0;
                    return;
                }
            }

            // Assume the goal has not been reached yet
            bool GoalReach = false;

            if (m_Accessible[m_ItemGoalBlockX, m_ItemGoalBlockY] != -1)
            {
                GoalReach = GoTo(m_ItemGoalBlockX, m_ItemGoalBlockY);
                _callsOfModeItem++;
            }
            else
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                _callsOfModeItem = 0;
                return;
            }

            // If the goal was reached and the bomber has to drop a bomb and has bombs available
            if (GoalReach && m_ItemDropBomb &&
                m_pBomber.GetTotalBombs() - m_pBomber.GetUsedBombsCount() > 0)
            {
                m_BomberAction   = EBomberAction.BOMBERACTION_ACTION1;
                m_ItemGoalBlockX = m_BlockHereX;
                m_ItemGoalBlockY = m_BlockHereY;
                m_ItemDropBomb   = false;
            }
            else if (GoalReach && !m_ItemDropBomb)
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                _callsOfModeItem = 0;
            }
            else if (_callsOfModeItem > MAX_CALLS_MODEITEM)
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                _callsOfModeItem = 0;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /*
        ALGO :

        si on veut jeter une bombe
        poser la bombe en attendant la prochaine entree du code ici
        sinon
        poser la bombe
        Passer en mode reflechir
        */

        private void ModeAttack()
        {
            m_BomberMove   = EBomberMove.BOMBERMOVE_NONE;
            m_BomberAction = EBomberAction.BOMBERACTION_ACTION1;

            SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /*
        ALGO :

        je voudrais aller jeter ma bombe que je vais poser maintenant.
        donc je la pose et je tourne en bonne direction.
        puis je serai en mode attaque et je vais jeter la bombe.
        */

        private void ModeThrow()
        {
            EEnemyDirection direction = EEnemyDirection.ENEMYDIRECTION_UNKNOWN;
            bool enemyNearFront = EnemyNearAndFront(ref direction, true);

            switch (direction)
            {
                case EEnemyDirection.ENEMYDIRECTION_ABOVE: m_BomberMove = EBomberMove.BOMBERMOVE_UP;    break;
                case EEnemyDirection.ENEMYDIRECTION_BELOW: m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;  break;
                case EEnemyDirection.ENEMYDIRECTION_LEFT:  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;  break;
                case EEnemyDirection.ENEMYDIRECTION_RIGHT: m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT; break;
                default:                                   m_BomberMove = EBomberMove.BOMBERMOVE_NONE;  break;
            }

            // TODO (check if is always enemyNearFront == true)
            // if (!enemyNearFront) { /* printf("enemyNearFront == false"); */ }

            // plant my bomb
            m_BomberAction = EBomberAction.BOMBERACTION_ACTION1;

            SetComputerMode(EComputerMode.COMPUTERMODE_ATTACK);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /*
        ALGO :

        utiliser l'action deux
        Passer en mode reflechir
        */

        private void ModeSecondAction()
        {
            // TODO determine direction to look (for a bomb to punch for instance)
            // right now this method is only being used for remote fuse bombs
            m_BomberMove   = EBomberMove.BOMBERMOVE_NONE;
            m_BomberAction = EBomberAction.BOMBERACTION_ACTION2;

            SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /*
        ALGO :

        si le bomber n'est pas en danger
        mode reflechir
        sinon
        chercher une case non dangereuse (qu'on peut atteindre sans avoir d'items speciaux)
        si aucune case trouvee
        chercher une case non dangereuse qu'on peut atteindre avec le kick item
        si case trouvee
        enregistrer le mouvement (simple mouvement horizontal ou vertical, pas besoin de goto)
        sinon
        chercher la case accessible la moins dangereuse possible
        enregistrer le mouvement (pathfinding avec goto)
        */

        private void ModeDefence(float DeltaTime)
        {
            m_BomberAction = EBomberAction.BOMBERACTION_NONE;

            // If the bomber is not in danger
            if (m_pArena.GetDanger(m_BlockHereX, m_BlockHereY) == EDanger.DANGER_NONE &&
                m_pArena.GetArena().GetArenaCloser().GetNumberOfBlocksLeft() > 10)
            {
                m_BomberMove = EBomberMove.BOMBERMOVE_NONE;

                // Did we plant a bomb with a remote trigger?
                if (m_pBomber.CanRemoteFuseBombs())
                {
                    for (int Index = 0; Index < m_pArena.GetArena().MaxBombs(); Index++)
                    {
                        if (m_pArena.GetArena().GetBomb(Index).Exist() &&
                            m_pArena.GetArena().GetBomb(Index).IsRemote() &&
                            m_pArena.GetArena().GetBomb(Index).GetOwnerPlayer() == m_Player)
                        {
                            if (TeamMateNearRemoteFuseBomb(m_pArena.GetArena().GetBomb(Index)) &&
                                _rng.Next(100) < 95)
                                break;

                            m_BomberAction = EBomberAction.BOMBERACTION_ACTION2; // detonate the bomb
                            m_pArena.GetArena().GetBomb(Index).Burn();
                            break;
                        }
                    }
                }

                SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                return;
            }

            bool  Found        = false;
            int   BestBlockX   = -1;
            int   BestBlockY   = -1;
            int   BlockX;
            int   BlockY;
            int   NextBlockX   = -1;
            int   NextBlockY   = -1;
            int   BestDistance = 999;
            bool  DeadEnd      = true;
            bool  twoBombs     = false;

            // Scan the blocks of the AI view
            for (BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
            {
                for (BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                {
                    if (BlockX < 0 || BlockX > Globals.ARENA_WIDTH  - 1 ||
                        BlockY < 0 || BlockY > Globals.ARENA_HEIGHT - 1)
                        continue;

                    if (m_Accessible[BlockX, BlockY] != -1 &&
                        (m_pArena.GetDeadEnd(BlockX, BlockY) == -1 || !DeadEnd) &&
                        m_pArena.GetDanger(BlockX, BlockY) == EDanger.DANGER_NONE &&
                        !m_pArena.GetArena().IsSkullItem(BlockX, BlockY) &&
                        (m_Accessible[BlockX, BlockY] < BestDistance ||
                         (m_Accessible[BlockX, BlockY] == BestDistance)))
                    {
                        Found        = true;
                        BestBlockX   = BlockX;
                        BestBlockY   = BlockY;
                        BestDistance = m_Accessible[BlockX, BlockY];
                        DeadEnd      = (m_pArena.GetDeadEnd(BlockX, BlockY) != -1);
                    }
                }
            }

            float BestDangerTimeLeft;

            if (!Found)
            {
                // kick only bomb with 5% of probability (avoid kicking against walls)
                // punch bomb with 25% of probability
                if ((m_pBomber.CanKickBombs() && _rng.Next(100) < 5) ||
                    (m_pBomber.CanPunchBombs() && _rng.Next(100) < 25))
                {
                    BestDangerTimeLeft = 0.0f;

                    for (BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
                    {
                        for (BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                        {
                            if (BlockX < 0 || BlockX > Globals.ARENA_WIDTH  - 1 ||
                                BlockY < 0 || BlockY > Globals.ARENA_HEIGHT - 1)
                                continue;

                            if (m_PseudoAccessible[BlockX, BlockY] != -1 &&
                                m_PseudoAccessible[BlockX, BlockY] <= 4 &&
                                m_pArena.GetArena().IsBomb(BlockX, BlockY) &&
                                (BlockX == m_BlockHereX || BlockY == m_BlockHereY) &&
                                (BlockX != m_BlockHereX || BlockY != m_BlockHereY) &&
                                m_pArena.GetDangerTimeLeft(BlockX, BlockY) > BestDangerTimeLeft)
                            {
                                Found              = true;
                                BestBlockX         = BlockX;
                                BestBlockY         = BlockY;
                                BestDangerTimeLeft = m_pArena.GetDangerTimeLeft(BlockX, BlockY);
                            }
                        }
                    }
                }

                if (Found)
                {
                    twoBombs    = false;
                    NextBlockX  = -1;
                    NextBlockY  = -1;

                    if (m_BlockHereX - BestBlockX > 0)
                    {
                        m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;

                        if (m_BlockHereX > 0)
                        {
                            NextBlockX = m_BlockHereX - 1;
                            NextBlockY = m_BlockHereY;

                            if (m_BlockHereX > 1)
                            {
                                if (m_pArena.GetArena().IsBomb(NextBlockX, NextBlockY) &&
                                    m_pArena.GetArena().IsBomb(NextBlockX - 1, NextBlockY))
                                    twoBombs = true;
                            }
                        }
                    }
                    else if (m_BlockHereX - BestBlockX < 0)
                    {
                        m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;

                        if (m_BlockHereX < Globals.ARENA_WIDTH - 1)
                        {
                            NextBlockX = m_BlockHereX + 1;
                            NextBlockY = m_BlockHereY;

                            if (m_BlockHereX < Globals.ARENA_WIDTH - 2)
                            {
                                if (m_pArena.GetArena().IsBomb(NextBlockX, NextBlockY) &&
                                    m_pArena.GetArena().IsBomb(NextBlockX + 1, NextBlockY))
                                    twoBombs = true;
                            }
                        }
                    }
                    else if (m_BlockHereY - BestBlockY > 0)
                    {
                        m_BomberMove = EBomberMove.BOMBERMOVE_UP;

                        if (m_BlockHereY > 0)
                        {
                            NextBlockX = m_BlockHereX;
                            NextBlockY = m_BlockHereY - 1;

                            if (m_BlockHereY > 1)
                            {
                                if (m_pArena.GetArena().IsBomb(NextBlockX, NextBlockY) &&
                                    m_pArena.GetArena().IsBomb(NextBlockX, NextBlockY - 1))
                                    twoBombs = true;
                            }
                        }
                    }
                    else if (m_BlockHereY - BestBlockY < 0)
                    {
                        m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;

                        if (m_BlockHereY < Globals.ARENA_HEIGHT - 1)
                        {
                            NextBlockX = m_BlockHereX;
                            NextBlockY = m_BlockHereY + 1;

                            if (m_BlockHereY < Globals.ARENA_HEIGHT - 2)
                            {
                                if (m_pArena.GetArena().IsBomb(NextBlockX, NextBlockY) &&
                                    m_pArena.GetArena().IsBomb(NextBlockX, NextBlockY + 1))
                                    twoBombs = true;
                            }
                        }
                    }
                    else
                    {
                        m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                    }

                    if (m_BomberMove != EBomberMove.BOMBERMOVE_NONE)
                    {
                        // if we want to punch a bomb away, this is the time!
                        if (NextBlockX != -1 && NextBlockY != -1)
                        {
                            if ((!m_pBomber.CanKickBombs() || twoBombs) &&
                                m_pBomber.CanPunchBombs() &&
                                m_pArena.GetArena().IsBomb(NextBlockX, NextBlockY))
                            {
                                m_BomberAction = EBomberAction.BOMBERACTION_ACTION2;
                            }
                        }

                        int PixelsPerSecond = m_pBomber.GetPixelsPerSecond();
                        if (PixelsPerSecond != 0)
                            m_BomberMoveTimeLeft = Globals.BLOCK_SIZE * 1.0f / PixelsPerSecond;
                    }
                    else
                    {
                        m_BomberMoveTimeLeft = 0.0f;
                    }

                    return;
                }
            } // if (!Found) first block

            if (!Found)
            {
                //------------------------------------
                // Determine the less dangerous block
                //------------------------------------

                BestDangerTimeLeft = 0.0f;

                for (BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
                {
                    for (BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                    {
                        if (BlockX < 0 || BlockX > Globals.ARENA_WIDTH  - 1 ||
                            BlockY < 0 || BlockY > Globals.ARENA_HEIGHT - 1)
                            continue;

                        if (m_Accessible[BlockX, BlockY] != -1 &&
                            m_pArena.GetDangerTimeLeft(BlockX, BlockY) > BestDangerTimeLeft)
                        {
                            Found              = true;
                            BestBlockX         = BlockX;
                            BestBlockY         = BlockY;
                            BestDistance       = m_Accessible[BlockX, BlockY];
                            BestDangerTimeLeft = m_pArena.GetDangerTimeLeft(BlockX, BlockY);
                        }
                    }
                }
            }

            if (Found)
            {
                Debug.Assert(BestBlockX != -1);
                Debug.Assert(BestBlockY != -1);

                GoTo(BestBlockX, BestBlockY);
            }
            else
            {
                m_BomberMove         = EBomberMove.BOMBERMOVE_NONE;
                m_BomberMoveTimeLeft = 0.0f;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /*
        ALGO :

        si on est en danger
        mode reflechir

        si on a un ennemi devant soi et qu'on pourrait poser une bombe
        mode reflechir

        si ca fait une seconde ou bien 50 fois qu'on se balade
        mode reflechir

        donner une note d'interet a chaque quart de l'arene (le centre etant notre position)
        choisir un mouvement selon l'interet, le danger, notre precedent mouvement, etc
        enregistrer le mouvement
        */

        private void ModeWalk(float DeltaTime)
        {
            m_BomberAction = EBomberAction.BOMBERACTION_NONE;

            _callsModeWalk++;

            // If the bomber is in danger
            if (m_pArena.GetDanger(m_BlockHereX, m_BlockHereY) != EDanger.DANGER_NONE)
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                _callsModeWalk = 0;
                return;
            }

            if (EnemyNearAndFront() && DropBombOK(m_BlockHereX, m_BlockHereY))
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                _callsModeWalk = 0;
                return;
            }

            int BlockX;
            int BlockY;

            if (m_WalkTime >= 1.0f || _callsModeWalk > MAX_CALLS_MODEWALK)
            {
                SetComputerMode(EComputerMode.COMPUTERMODE_THINK);
                _callsModeWalk = 0;
                return;
            }

            int MarkDownRight = 0;
            int MarkDownLeft  = 0;
            int MarkUpLeft    = 0;
            int MarkUpRight   = 0;

            for (BlockX = m_BlockHereX; BlockX < Globals.ARENA_WIDTH; BlockX++)
            {
                for (BlockY = m_BlockHereY; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                {
                    if (BlockX == m_BlockHereX && BlockY == m_BlockHereY)
                        continue;

                    if (m_pArena.GetArena().IsSoftWall(BlockX, BlockY))
                        MarkDownRight += 2;
                    else if (m_pArena.GetArena().IsItem(BlockX, BlockY) &&
                             !m_pArena.GetArena().IsSkullItem(BlockX, BlockY))
                        MarkDownRight += 10;
                    else if (m_pArena.GetArena().IsBomber(BlockX, BlockY))
                        MarkDownRight += 5;
                }
            }

            for (BlockX = m_BlockHereX; BlockX >= 0; BlockX--)
            {
                for (BlockY = m_BlockHereY; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                {
                    if (BlockX == m_BlockHereX && BlockY == m_BlockHereY)
                        continue;

                    if (m_pArena.GetArena().IsSoftWall(BlockX, BlockY))
                        MarkDownLeft += 2;
                    else if (m_pArena.GetArena().IsItem(BlockX, BlockY) &&
                             !m_pArena.GetArena().IsSkullItem(BlockX, BlockY))
                        MarkDownLeft += 10;
                    else if (m_pArena.GetArena().IsBomber(BlockX, BlockY))
                        MarkDownLeft += 5;
                }
            }

            for (BlockX = m_BlockHereX; BlockX >= 0; BlockX--)
            {
                for (BlockY = m_BlockHereY; BlockY >= 0; BlockY--)
                {
                    if (BlockX == m_BlockHereX && BlockY == m_BlockHereY)
                        continue;

                    if (m_pArena.GetArena().IsSoftWall(BlockX, BlockY))
                        MarkUpLeft += 2;
                    else if (m_pArena.GetArena().IsItem(BlockX, BlockY) &&
                             !m_pArena.GetArena().IsSkullItem(BlockX, BlockY))
                        MarkUpLeft += 10;
                    else if (m_pArena.GetArena().IsBomber(BlockX, BlockY))
                        MarkUpLeft += 5;
                }
            }

            for (BlockX = m_BlockHereX; BlockX < Globals.ARENA_WIDTH; BlockX++)
            {
                for (BlockY = m_BlockHereY; BlockY >= 0; BlockY--)
                {
                    if (BlockX == m_BlockHereX && BlockY == m_BlockHereY)
                        continue;

                    if (m_pArena.GetArena().IsSoftWall(BlockX, BlockY))
                        MarkUpRight += 2;
                    else if (m_pArena.GetArena().IsItem(BlockX, BlockY) &&
                             !m_pArena.GetArena().IsSkullItem(BlockX, BlockY))
                        MarkUpRight += 10;
                    else if (m_pArena.GetArena().IsBomber(BlockX, BlockY))
                        MarkUpRight += 5;
                }
            }

            if (MarkDownRight == 0 && MarkDownLeft == 0 && MarkUpLeft == 0 && MarkUpRight == 0)
                return;

            bool CanMoveUp    = !m_pArena.GetArena().IsWall(m_BlockUpX,    m_BlockUpY)    && !m_pArena.GetArena().IsBomb(m_BlockUpX,    m_BlockUpY);
            bool CanMoveDown  = !m_pArena.GetArena().IsWall(m_BlockDownX,  m_BlockDownY)  && !m_pArena.GetArena().IsBomb(m_BlockDownX,  m_BlockDownY);
            bool CanMoveLeft  = !m_pArena.GetArena().IsWall(m_BlockLeftX,  m_BlockLeftY)  && !m_pArena.GetArena().IsBomb(m_BlockLeftX,  m_BlockLeftY);
            bool CanMoveRight = !m_pArena.GetArena().IsWall(m_BlockRightX, m_BlockRightY) && !m_pArena.GetArena().IsBomb(m_BlockRightX, m_BlockRightY);

            EDanger DangerUp    = m_pArena.GetDanger(m_BlockUpX,    m_BlockUpY);
            EDanger DangerDown  = m_pArena.GetDanger(m_BlockDownX,  m_BlockDownY);
            EDanger DangerLeft  = m_pArena.GetDanger(m_BlockLeftX,  m_BlockLeftY);
            EDanger DangerRight = m_pArena.GetDanger(m_BlockRightX, m_BlockRightY);

            if (MarkDownRight >= MarkDownLeft &&
                MarkDownRight >= MarkUpLeft   &&
                MarkDownRight >= MarkUpRight)
            {
                if (_rng.Next(100) >= 50)
                {
                    if      (DangerDown  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_UP    && CanMoveDown)  m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                    else if (DangerRight == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_LEFT  && CanMoveRight) m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                    else if (DangerUp    == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_DOWN  && CanMoveUp)    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                    else if (DangerLeft  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_RIGHT && CanMoveLeft)  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                    else                                                                                                          m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
                else
                {
                    if      (DangerRight == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_LEFT  && CanMoveRight) m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                    else if (DangerDown  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_UP    && CanMoveDown)  m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                    else if (DangerLeft  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_RIGHT && CanMoveLeft)  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                    else if (DangerUp    == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_DOWN  && CanMoveUp)    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                    else                                                                                                          m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
            }
            else if (MarkDownLeft >= MarkDownRight &&
                     MarkDownLeft >= MarkUpLeft    &&
                     MarkDownLeft >= MarkUpRight)
            {
                if (_rng.Next(100) >= 50)
                {
                    if      (DangerDown  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_UP    && CanMoveDown)  m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                    else if (DangerLeft  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_RIGHT && CanMoveLeft)  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                    else if (DangerUp    == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_DOWN  && CanMoveUp)    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                    else if (DangerRight == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_LEFT  && CanMoveRight) m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                    else                                                                                                          m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
                else
                {
                    if      (DangerLeft  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_RIGHT && CanMoveLeft)  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                    else if (DangerDown  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_UP    && CanMoveDown)  m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                    else if (DangerRight == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_LEFT  && CanMoveRight) m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                    else if (DangerUp    == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_DOWN  && CanMoveUp)    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                    else                                                                                                          m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
            }
            else if (MarkUpLeft >= MarkDownRight &&
                     MarkUpLeft >= MarkDownLeft  &&
                     MarkUpLeft >= MarkUpRight)
            {
                if (_rng.Next(100) >= 50)
                {
                    if      (DangerUp    == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_DOWN  && CanMoveUp)    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                    else if (DangerLeft  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_RIGHT && CanMoveLeft)  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                    else if (DangerDown  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_UP    && CanMoveDown)  m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                    else if (DangerRight == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_LEFT  && CanMoveRight) m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                    else                                                                                                          m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
                else
                {
                    if      (DangerLeft  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_RIGHT && CanMoveLeft)  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                    else if (DangerUp    == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_DOWN  && CanMoveUp)    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                    else if (DangerRight == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_LEFT  && CanMoveRight) m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                    else if (DangerDown  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_UP    && CanMoveDown)  m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                    else                                                                                                          m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
            }
            else  // MarkUpRight is best (or tied)
            {
                if (_rng.Next(100) >= 50)
                {
                    if      (DangerUp    == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_DOWN  && CanMoveUp)    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                    else if (DangerRight == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_LEFT  && CanMoveRight) m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                    else if (DangerDown  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_UP    && CanMoveDown)  m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                    else if (DangerLeft  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_RIGHT && CanMoveLeft)  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                    else                                                                                                          m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
                else
                {
                    if      (DangerRight == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_LEFT  && CanMoveRight) m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                    else if (DangerUp    == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_DOWN  && CanMoveUp)    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                    else if (DangerLeft  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_RIGHT && CanMoveLeft)  m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                    else if (DangerDown  == EDanger.DANGER_NONE && m_BomberMove != EBomberMove.BOMBERMOVE_UP    && CanMoveDown)  m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                    else                                                                                                          m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
            }

            // If the bomber move is a real move
            if (m_BomberMove != EBomberMove.BOMBERMOVE_NONE)
            {
                int PixelsPerSecond = m_pBomber.GetPixelsPerSecond();
                if (PixelsPerSecond != 0)
                    m_BomberMoveTimeLeft = Globals.BLOCK_SIZE * 1.0f / PixelsPerSecond;
            }
            else
            {
                m_BomberMoveTimeLeft = 0.0f;
            }

            m_WalkTime += DeltaTime;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Modify the commands to send to the bomber so that it moves to the specified goal.
        /// Returns true if the bomber is already on the goal block.
        /// </summary>
        private bool GoTo(int GoalBlockX, int GoalBlockY)
        {
            // If the block to go to is not accessible
            // or the bomber is already on this block
            if (m_Accessible[GoalBlockX, GoalBlockY] == -1 ||
                m_Accessible[GoalBlockX, GoalBlockY] == 0)
            {
                m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
            }
            else
            {
                // Block coordinates used to go from the goal to the bomber using the accessible array.
                int BlockX = GoalBlockX;
                int BlockY = GoalBlockY;

                while (true)
                {
                    if (m_Accessible[BlockX, BlockY - 1] == m_Accessible[BlockX, BlockY] - 1)
                    {
                        if (m_Accessible[BlockX, BlockY - 1] == 0)
                        {
                            m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                            break;
                        }
                        BlockY--;
                    }
                    else if (m_Accessible[BlockX, BlockY + 1] == m_Accessible[BlockX, BlockY] - 1)
                    {
                        if (m_Accessible[BlockX, BlockY + 1] == 0)
                        {
                            m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                            break;
                        }
                        BlockY++;
                    }
                    else if (m_Accessible[BlockX - 1, BlockY] == m_Accessible[BlockX, BlockY] - 1)
                    {
                        if (m_Accessible[BlockX - 1, BlockY] == 0)
                        {
                            m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                            break;
                        }
                        BlockX--;
                    }
                    else if (m_Accessible[BlockX + 1, BlockY] == m_Accessible[BlockX, BlockY] - 1)
                    {
                        if (m_Accessible[BlockX + 1, BlockY] == 0)
                        {
                            m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                            break;
                        }
                        BlockX++;
                    }
                }

                if (m_pArena.GetArena().GetArenaCloser().GetNumberOfBlocksLeft() > 10)
                {
                    float DangerTimeLeftHere = m_pArena.GetDangerTimeLeft(m_BlockHereX, m_BlockHereY);
                    float DangerTimeLeftNext = -1.0f;

                    switch (m_BomberMove)
                    {
                        case EBomberMove.BOMBERMOVE_UP:    DangerTimeLeftNext = m_pArena.GetDangerTimeLeft(m_BlockUpX,    m_BlockUpY);    break;
                        case EBomberMove.BOMBERMOVE_DOWN:  DangerTimeLeftNext = m_pArena.GetDangerTimeLeft(m_BlockDownX,  m_BlockDownY);  break;
                        case EBomberMove.BOMBERMOVE_LEFT:  DangerTimeLeftNext = m_pArena.GetDangerTimeLeft(m_BlockLeftX,  m_BlockLeftY);  break;
                        case EBomberMove.BOMBERMOVE_RIGHT: DangerTimeLeftNext = m_pArena.GetDangerTimeLeft(m_BlockRightX, m_BlockRightY); break;
                        default: break;
                    }

                    Debug.Assert(DangerTimeLeftNext != -1.0f);

                    if (DangerTimeLeftHere > DangerTimeLeftNext)
                        m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                }
            }

            // If the bomber move is a real move
            if (m_BomberMove != EBomberMove.BOMBERMOVE_NONE)
            {
                int PixelsPerSecond = m_pBomber.GetPixelsPerSecond();
                if (PixelsPerSecond != 0)
                    m_BomberMoveTimeLeft = Globals.BLOCK_SIZE * 1.0f / PixelsPerSecond;
            }
            else
            {
                m_BomberMoveTimeLeft = 0.0f;
            }

            return m_Accessible[GoalBlockX, GoalBlockY] == 0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void SetComputerMode(EComputerMode ComputerMode)
        {
            if (ComputerMode == EComputerMode.COMPUTERMODE_THINK ||
                ComputerMode == EComputerMode.COMPUTERMODE_THROW)
            {
                m_StopTimeLeft = 0.180f;

                switch (m_ComputerMode)
                {
                    case EComputerMode.COMPUTERMODE_ITEM:      m_StopTimeLeft = 0.080f + _rng.Next(40) / 1000.0f; break;
                    case EComputerMode.COMPUTERMODE_ATTACK:    m_StopTimeLeft = 0.200f + _rng.Next(40) / 1000.0f; break;
                    case EComputerMode.COMPUTERMODE_THROW:     m_StopTimeLeft = 0.200f + _rng.Next(40) / 1000.0f; break;
                    case EComputerMode.COMPUTERMODE_2NDACTION: m_StopTimeLeft = 0.200f + _rng.Next(40) / 1000.0f; break;
                    case EComputerMode.COMPUTERMODE_DEFENCE:   m_StopTimeLeft = 0.120f + _rng.Next(40) / 1000.0f; break;
                    case EComputerMode.COMPUTERMODE_WALK:      m_StopTimeLeft = 0.220f + _rng.Next(40) / 1000.0f; break;
                    default: break;
                }
            }

            m_ComputerMode = ComputerMode;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void UpdateAccessibility()
        {
            int BlockX;
            int BlockY;
            bool Updated;

            //****************
            // ACCESSIBLE
            //****************

            for (BlockX = 0; BlockX < Globals.ARENA_WIDTH; BlockX++)
                for (BlockY = 0; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                    m_Accessible[BlockX, BlockY] = -1;

            m_Accessible[m_BlockHereX, m_BlockHereY] = 0;
            m_NumAccessible = 1;

            do
            {
                Updated = false;

                for (BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
                {
                    for (BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                    {
                        if (BlockX <= 0 || BlockX >= Globals.ARENA_WIDTH  - 1 ||
                            BlockY <= 0 || BlockY >= Globals.ARENA_HEIGHT - 1)
                            continue;

                        if (m_Accessible[BlockX, BlockY] != -1)
                        {
                            // ABOVE
                            if (m_Accessible[BlockX, BlockY - 1] == -1 &&
                                !m_pArena.GetArena().IsWall(BlockX, BlockY - 1) &&
                                !m_pArena.GetArena().IsBomb(BlockX, BlockY - 1))
                            {
                                m_Accessible[BlockX, BlockY - 1] = m_Accessible[BlockX, BlockY] + 1;
                                Updated = true;
                                m_NumAccessible++;
                            }

                            // BELOW
                            if (m_Accessible[BlockX, BlockY + 1] == -1 &&
                                !m_pArena.GetArena().IsWall(BlockX, BlockY + 1) &&
                                !m_pArena.GetArena().IsBomb(BlockX, BlockY + 1))
                            {
                                m_Accessible[BlockX, BlockY + 1] = m_Accessible[BlockX, BlockY] + 1;
                                Updated = true;
                                m_NumAccessible++;
                            }

                            // LEFT
                            if (m_Accessible[BlockX - 1, BlockY] == -1 &&
                                !m_pArena.GetArena().IsWall(BlockX - 1, BlockY) &&
                                !m_pArena.GetArena().IsBomb(BlockX - 1, BlockY))
                            {
                                m_Accessible[BlockX - 1, BlockY] = m_Accessible[BlockX, BlockY] + 1;
                                Updated = true;
                                m_NumAccessible++;
                            }

                            // RIGHT
                            if (m_Accessible[BlockX + 1, BlockY] == -1 &&
                                !m_pArena.GetArena().IsWall(BlockX + 1, BlockY) &&
                                !m_pArena.GetArena().IsBomb(BlockX + 1, BlockY))
                            {
                                m_Accessible[BlockX + 1, BlockY] = m_Accessible[BlockX, BlockY] + 1;
                                Updated = true;
                                m_NumAccessible++;
                            }
                        }
                    }
                }
            } while (Updated);

            //****************
            // PSEUDO ACCESSIBLE  (bombs are NOT obstacles)
            //****************

            for (BlockX = 0; BlockX < Globals.ARENA_WIDTH; BlockX++)
                for (BlockY = 0; BlockY < Globals.ARENA_HEIGHT; BlockY++)
                    m_PseudoAccessible[BlockX, BlockY] = -1;

            m_PseudoAccessible[m_BlockHereX, m_BlockHereY] = 0;
            m_NumAccessible = 1;

            do
            {
                Updated = false;

                for (BlockX = m_BlockHereX - AI_VIEW_SIZE; BlockX < m_BlockHereX + AI_VIEW_SIZE; BlockX++)
                {
                    for (BlockY = m_BlockHereY - AI_VIEW_SIZE; BlockY < m_BlockHereY + AI_VIEW_SIZE; BlockY++)
                    {
                        if (BlockX <= 0 || BlockX >= Globals.ARENA_WIDTH  - 1 ||
                            BlockY <= 0 || BlockY >= Globals.ARENA_HEIGHT - 1)
                            continue;

                        if (m_PseudoAccessible[BlockX, BlockY] != -1)
                        {
                            // ABOVE
                            if (m_PseudoAccessible[BlockX, BlockY - 1] == -1 &&
                                !m_pArena.GetArena().IsWall(BlockX, BlockY - 1))
                            {
                                m_PseudoAccessible[BlockX, BlockY - 1] = m_PseudoAccessible[BlockX, BlockY] + 1;
                                Updated = true;
                                m_NumAccessible++;
                            }

                            // BELOW
                            if (m_PseudoAccessible[BlockX, BlockY + 1] == -1 &&
                                !m_pArena.GetArena().IsWall(BlockX, BlockY + 1))
                            {
                                m_PseudoAccessible[BlockX, BlockY + 1] = m_PseudoAccessible[BlockX, BlockY] + 1;
                                Updated = true;
                                m_NumAccessible++;
                            }

                            // LEFT
                            if (m_PseudoAccessible[BlockX - 1, BlockY] == -1 &&
                                !m_pArena.GetArena().IsWall(BlockX - 1, BlockY))
                            {
                                m_PseudoAccessible[BlockX - 1, BlockY] = m_PseudoAccessible[BlockX, BlockY] + 1;
                                Updated = true;
                                m_NumAccessible++;
                            }

                            // RIGHT
                            if (m_PseudoAccessible[BlockX + 1, BlockY] == -1 &&
                                !m_pArena.GetArena().IsWall(BlockX + 1, BlockY))
                            {
                                m_PseudoAccessible[BlockX + 1, BlockY] = m_PseudoAccessible[BlockX, BlockY] + 1;
                                Updated = true;
                                m_NumAccessible++;
                            }
                        }
                    }
                }
            } while (Updated);
        }

    } // class CAiBomber

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

} // namespace Bombermaaan
