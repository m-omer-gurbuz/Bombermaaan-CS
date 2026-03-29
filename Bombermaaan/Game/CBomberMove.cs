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
 *  \file CBomberMove.cs
 *  \brief Bomber moves
 */

using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    //! Describes a bomber move command
    public enum EBomberMove
    {
        BOMBERMOVE_NONE,                //!< None
        BOMBERMOVE_UP,                  //!< Up only
        BOMBERMOVE_DOWN,                //!< Down only
        BOMBERMOVE_LEFT,                //!< Left only
        BOMBERMOVE_RIGHT,               //!< Right only
        BOMBERMOVE_UPLEFT,              //!< Up and left together
        BOMBERMOVE_UPRIGHT,             //!< Up and right together
        BOMBERMOVE_DOWNLEFT,            //!< Down and left together
        BOMBERMOVE_DOWNRIGHT            //!< Down and right together
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    //! Describes the type of turning of the bomber
    public enum ETurning
    {
        TURNING_NOTTURNING,                 //!< Is not turning for the moment
        TURNING_UPLEFT_UP,                  //!< Turning up/left, up direction was blocked before turning
        TURNING_UPLEFT_LEFT,                //!< Turning up/left, left direction was blocked before turning
        TURNING_UPRIGHT_UP,
        TURNING_UPRIGHT_RIGHT,
        TURNING_DOWNLEFT_DOWN,
        TURNING_DOWNLEFT_LEFT,
        TURNING_DOWNRIGHT_DOWN,
        TURNING_DOWNRIGHT_RIGHT
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    //! Return type of the CanMove method
    public enum ECanMove
    {
        CANMOVE_CANNOT,             //!< The bomber just cannot go in the desired direction
        CANMOVE_FREEWAY,            //!< The way is totally free
        CANMOVE_AVOID,              //!< The bomber can move but has to avoid an obstacle
        CANMOVE_TURN                //!< The bomber can move but has to turn around an obstacle
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    // Add these values (in pixels) to the bomber position in order
    // to get the position of the bomb the bomber is holding
    // BOMBER_TO_HELD_BOMB_POSITION_X = 0
    // BOMBER_TO_HELD_BOMB_POSITION_Y = -17

    //! This class manages the moves of a bomber
    public class CBomberMove
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private const int BOMBER_TO_HELD_BOMB_POSITION_X = 0;
        private const int BOMBER_TO_HELD_BOMB_POSITION_Y = -17;

        // MAX_ITER prevents infinite loops in TryMove/CanMove/TurnTest
        private const int MAX_ITER = 64;

        private CArena      m_pArena;               //!< Arena where to check for obstacles
        private int         m_Player;               //!< Player number of the parent bomber. -1 when not initialized.
        private float       m_X;                    //!< Float position X in arena (in pixels) from the top left corner of the arena.
        private float       m_Y;                    //!< Float position Y in arena (in pixels) from the top left corner of the arena.
        private int         m_iX;                   //!< Integer position X in arena (in pixels) from the top left corner of the arena.
        private int         m_iY;                   //!< Integer position Y in arena (in pixels) from the top left corner of the arena.
        private int         m_BlockX;               //!< Position X in the arena grid (in blocks).
        private int         m_BlockY;               //!< Position Y in the arena grid (in blocks).
        private EBomberMove m_BomberMove;            //!< Describes the move that the owner player currently wants the bomber to perform.
        private EBomberMove m_LastRealBomberMove;   //!< Describes the last "real" (ie. not BOMBERMOVE_NONE) move the owner player wanted the bomber to perform.
        private ETurning    m_Turning;              //!< Is the bomber turning around a wall and how
        private bool        m_CouldMove;            //!< Could the bomber move the last time he tried? (used for Inertia sickness)

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        //! Constructor
        public CBomberMove()
        {
            m_pArena = null;
            m_Player = -1;

            m_X = -1;
            m_Y = -1;

            m_iX = -1;
            m_iY = -1;

            m_BlockX = -1;
            m_BlockY = -1;

            m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
            m_LastRealBomberMove = EBomberMove.BOMBERMOVE_NONE;
            m_Turning = ETurning.TURNING_NOTTURNING;
            m_CouldMove = false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        //! Initialize the object
        public void Create(int BlockX, int BlockY, int Player)
        {
            Debug.Assert(m_pArena != null);

            Debug.Assert(BlockX >= 0);
            Debug.Assert(BlockX < Globals.ARENA_WIDTH);

            Debug.Assert(BlockY >= 0);
            Debug.Assert(BlockY < Globals.ARENA_HEIGHT);

            Debug.Assert(Player >= 0);
            Debug.Assert(Player < Globals.MAX_PLAYERS);

            m_Player = Player;
            m_BlockX = BlockX;
            m_BlockY = BlockY;
            m_iX = m_pArena.ToPosition(BlockX);
            m_iY = m_pArena.ToPosition(BlockY);
            m_X = (float)m_iX;
            m_Y = (float)m_iY;
            m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
            m_LastRealBomberMove = EBomberMove.BOMBERMOVE_DOWN;
            m_Turning = ETurning.TURNING_NOTTURNING;
            m_CouldMove = false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        //! Free any memory allocated by this object
        public void Destroy()
        {
        }

        //! Deep-copy state from another CBomberMove (arena reference is NOT copied — caller keeps its own arena)
        public void CopyFrom(CBomberMove other)
        {
            // m_pArena is intentionally not copied — the owning CBomber sets it separately
            m_Player             = other.m_Player;
            m_X                  = other.m_X;
            m_Y                  = other.m_Y;
            m_iX                 = other.m_iX;
            m_iY                 = other.m_iY;
            m_BlockX             = other.m_BlockX;
            m_BlockY             = other.m_BlockY;
            m_BomberMove         = other.m_BomberMove;
            m_LastRealBomberMove = other.m_LastRealBomberMove;
            m_Turning            = other.m_Turning;
            m_CouldMove          = other.m_CouldMove;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        //! Give a move order to the bomber for next update.
        public void Command(EBomberMove BomberMove)
        {
            // If the bomber is currently lifting or throwing or punching a bomb
            if (m_pArena.GetBomber(m_Player).GetState() == EBomberState.BOMBERSTATE_LIFT ||
                m_pArena.GetBomber(m_Player).GetState() == EBomberState.BOMBERSTATE_THROW ||
                m_pArena.GetBomber(m_Player).GetState() == EBomberState.BOMBERSTATE_PUNCH)
            {
                // Then he cannot move.
                m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
            }
            else
            {
                // The given BomberMove will be modified
                // according to the bomber's sickness. When updating,
                // these modified BomberMove will be used.
                switch (m_pArena.GetBomber(m_Player).GetSickness())
                {
                    // Sicknesses that don't affect the move
                    case ESick.SICK_NOTSICK:
                    case ESick.SICK_SLOW:
                    case ESick.SICK_FAST:
                    case ESick.SICK_SMALLFLAME:
                    case ESick.SICK_LONGBOMB:
                    case ESick.SICK_SHORTBOMB:
                    case ESick.SICK_CONSTIPATED:
                    case ESick.SICK_COLIC:
                    case ESick.SICK_INVISIBILITY:
                    case ESick.SICK_FLAMEPROOF:
                    {
                        m_BomberMove = BomberMove;
                        break;
                    }

                    case ESick.SICK_INVERTION:
                    {
                        // Invert move
                        switch (BomberMove)
                        {
                            case EBomberMove.BOMBERMOVE_NONE:      m_BomberMove = EBomberMove.BOMBERMOVE_NONE;         break;
                            case EBomberMove.BOMBERMOVE_UP:        m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;         break;
                            case EBomberMove.BOMBERMOVE_DOWN:      m_BomberMove = EBomberMove.BOMBERMOVE_UP;           break;
                            case EBomberMove.BOMBERMOVE_LEFT:      m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;        break;
                            case EBomberMove.BOMBERMOVE_RIGHT:     m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;         break;
                            case EBomberMove.BOMBERMOVE_UPLEFT:    m_BomberMove = EBomberMove.BOMBERMOVE_DOWNRIGHT;    break;
                            case EBomberMove.BOMBERMOVE_UPRIGHT:   m_BomberMove = EBomberMove.BOMBERMOVE_DOWNLEFT;     break;
                            case EBomberMove.BOMBERMOVE_DOWNLEFT:  m_BomberMove = EBomberMove.BOMBERMOVE_UPRIGHT;      break;
                            case EBomberMove.BOMBERMOVE_DOWNRIGHT: m_BomberMove = EBomberMove.BOMBERMOVE_UPLEFT;       break;
                        }

                        break;
                    }

                    case ESick.SICK_INERTIA:
                    {
                        // If bomber is asked to go somewhere
                        if (BomberMove != EBomberMove.BOMBERMOVE_NONE)
                        {
                            bool Can = false;

                            switch (BomberMove)
                            {
                                // BomberMove with two directions
                                case EBomberMove.BOMBERMOVE_UPLEFT:
                                case EBomberMove.BOMBERMOVE_UPRIGHT:
                                case EBomberMove.BOMBERMOVE_DOWNLEFT:
                                case EBomberMove.BOMBERMOVE_DOWNRIGHT:
                                    // Always OK
                                    Can = true;
                                    break;

                                // BomberMove with single direction
                                case EBomberMove.BOMBERMOVE_UP:
                                case EBomberMove.BOMBERMOVE_DOWN:
                                case EBomberMove.BOMBERMOVE_LEFT:
                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // OK if you won't be blocked (Otherwise the player could be able to stop his bomber)
                                    Can = (CanMove(BomberMove) != ECanMove.CANMOVE_CANNOT);
                                    break;
                                default:
                                    break;
                            }

                            // If he can, go in that direction
                            if (Can)
                                m_BomberMove = BomberMove;
                        }
                        // Else if he could not move when he last tried
                        else if (!m_CouldMove)
                        {
                            // Stop trying
                            m_BomberMove = EBomberMove.BOMBERMOVE_NONE;
                        }

                        break;
                    }
                    case ESick.NUMBER_SICKNESSES:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        //! Make the bomber move according to the bomber move command it received.
        public void Update(float DeltaTime)
        {
            // If the bomber is not alive (dead or dying) then he cannot move
            if (!m_pArena.GetBomber(m_Player).IsAlive())
                return;

            // If the bomber is stunt then he cannot move
            if (m_pArena.GetBomber(m_Player).GetState() == EBomberState.BOMBERSTATE_STUNT)
                return;

            // If bomber has to move
            if (m_BomberMove != EBomberMove.BOMBERMOVE_NONE)
            {
                // Number of pixels the bomber has to move
                float fPixels = m_pArena.GetBomber(m_Player).GetPixelsPerSecond() * DeltaTime;

                // Convert the move with multiple directions to a move with a single direction
                // For the player's comfort, if the bomber is asked to go up and left, one
                // of the two directions will be choosen using tests. The direction making
                // the bomber turn (not any turning type!) will be choosen, and if no
                // choice could be made, the direction where the way is free will be
                // choosen, else the direction making the move possible will be choosen.
                // Priorities :
                // 1- BomberMove in a direction that will make you turn (not any turning type)
                // 2- BomberMove in a direction where the way is free
                // 3- BomberMove in a direction that will make a move possible
                // 4- Else... go in a direction, even if the move is impossible

                switch (m_BomberMove)
                {
                    case EBomberMove.BOMBERMOVE_UPLEFT:
                        if (TurnTest(EBomberMove.BOMBERMOVE_UP) == ETurning.TURNING_UPLEFT_UP)                  m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                        else if (TurnTest(EBomberMove.BOMBERMOVE_LEFT) == ETurning.TURNING_UPLEFT_LEFT)         m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                        else if (CanMove(EBomberMove.BOMBERMOVE_UP) == ECanMove.CANMOVE_FREEWAY)                m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                        else if (CanMove(EBomberMove.BOMBERMOVE_LEFT) == ECanMove.CANMOVE_FREEWAY)              m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                        else if (CanMove(EBomberMove.BOMBERMOVE_UP) != ECanMove.CANMOVE_CANNOT)                 m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                        else                                                                                     m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                        break;

                    case EBomberMove.BOMBERMOVE_UPRIGHT:
                        if (TurnTest(EBomberMove.BOMBERMOVE_UP) == ETurning.TURNING_UPRIGHT_UP)                 m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                        else if (TurnTest(EBomberMove.BOMBERMOVE_RIGHT) == ETurning.TURNING_UPRIGHT_RIGHT)      m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                        else if (CanMove(EBomberMove.BOMBERMOVE_UP) == ECanMove.CANMOVE_FREEWAY)                m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                        else if (CanMove(EBomberMove.BOMBERMOVE_RIGHT) == ECanMove.CANMOVE_FREEWAY)             m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                        else if (CanMove(EBomberMove.BOMBERMOVE_UP) != ECanMove.CANMOVE_CANNOT)                 m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                        else                                                                                     m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                        break;

                    case EBomberMove.BOMBERMOVE_DOWNLEFT:
                        if (TurnTest(EBomberMove.BOMBERMOVE_DOWN) == ETurning.TURNING_DOWNLEFT_DOWN)            m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                        else if (TurnTest(EBomberMove.BOMBERMOVE_LEFT) == ETurning.TURNING_DOWNLEFT_LEFT)       m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                        else if (CanMove(EBomberMove.BOMBERMOVE_DOWN) == ECanMove.CANMOVE_FREEWAY)              m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                        else if (CanMove(EBomberMove.BOMBERMOVE_LEFT) == ECanMove.CANMOVE_FREEWAY)              m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                        else if (CanMove(EBomberMove.BOMBERMOVE_DOWN) != ECanMove.CANMOVE_CANNOT)               m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                        else                                                                                     m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                        break;

                    case EBomberMove.BOMBERMOVE_DOWNRIGHT:
                        if (TurnTest(EBomberMove.BOMBERMOVE_DOWN) == ETurning.TURNING_DOWNRIGHT_DOWN)           m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                        else if (TurnTest(EBomberMove.BOMBERMOVE_RIGHT) == ETurning.TURNING_DOWNRIGHT_RIGHT)    m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                        else if (CanMove(EBomberMove.BOMBERMOVE_DOWN) == ECanMove.CANMOVE_FREEWAY)              m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                        else if (CanMove(EBomberMove.BOMBERMOVE_RIGHT) == ECanMove.CANMOVE_FREEWAY)             m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                        else if (CanMove(EBomberMove.BOMBERMOVE_DOWN) != ECanMove.CANMOVE_CANNOT)               m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                        else                                                                                     m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                        break;

                    default:
                        break;
                }

                // If the move is no more than one pixel then there is no problem.
                // Otherwise move pixel by pixel in order not to avoid any collision.
                float fPixelsLeft = fPixels;    // How many pixels left. Used to set m_CouldMove

                while (true)
                {
                    if (fPixelsLeft >= 1.0f)
                    {
                        // If you can't move by one pixel
                        // then you can't move at all
                        if (!TryMove(1.0f))
                            break;

                        // You moved
                        fPixelsLeft -= 1.0f;
                    }
                    else
                    {
                        // If you can move by one pixel then
                        // you can move by less than one pixel
                        if (TryMove(fPixelsLeft))
                            fPixelsLeft = 0.0f;     // You moved

                        // Finished moving
                        break;
                    }
                }

                // If the bomber could move by any number of pixels
                // or part of pixels then he could move
                m_CouldMove = (fPixelsLeft < fPixels);

                // Update integer position
                m_iX = (int)m_X;
                m_iY = (int)m_Y;

                // Update Block coordinates
                m_BlockX = m_pArena.ToBlock(m_iX + Globals.BLOCK_SIZE / 2);
                m_BlockY = m_pArena.ToBlock(m_iY + Globals.BLOCK_SIZE / 2);

                Debug.Assert(m_BlockX >= 0);
                Debug.Assert(m_BlockX < Globals.ARENA_WIDTH);

                Debug.Assert(m_BlockY >= 0);
                Debug.Assert(m_BlockY < Globals.ARENA_HEIGHT);
            }

            // If the bomber is holding a bomb
            if (m_pArena.GetBomber(m_Player).GetState() == EBomberState.BOMBERSTATE_WALK_HOLD)
            {
                // Get the bomb the bomber is holding
                CBomb Bomb = m_pArena.GetBomb(m_pArena.GetBomber(m_Player).GetBombIndex());

                // Make this bomb follow the bomber
                Bomb.SetBlock(m_BlockX, m_BlockY);
                Bomb.SetPosition(m_iX + BOMBER_TO_HELD_BOMB_POSITION_X, m_iY + BOMBER_TO_HELD_BOMB_POSITION_Y);
            }

            // If the bomber had to move
            if (m_BomberMove != EBomberMove.BOMBERMOVE_NONE)
            {
                // Remember this move, which is a move in a valid direction.
                m_LastRealBomberMove = m_BomberMove;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void WriteSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.WriteInteger(m_Player);
            Snapshot.WriteFloat(m_X);
            Snapshot.WriteFloat(m_Y);
            Snapshot.WriteInteger(m_iX);
            Snapshot.WriteInteger(m_iY);
            Snapshot.WriteInteger(m_BlockX);
            Snapshot.WriteInteger(m_BlockY);
            Snapshot.WriteInteger((int)m_BomberMove);
            Snapshot.WriteInteger((int)m_LastRealBomberMove);
            Snapshot.WriteInteger((int)m_Turning);
            Snapshot.WriteBoolean(m_CouldMove);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void ReadSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.ReadInteger(out m_Player);
            Snapshot.ReadFloat(out m_X);
            Snapshot.ReadFloat(out m_Y);
            Snapshot.ReadInteger(out m_iX);
            Snapshot.ReadInteger(out m_iY);
            Snapshot.ReadInteger(out m_BlockX);
            Snapshot.ReadInteger(out m_BlockY);
            int tmp;
            tmp = 0; Snapshot.ReadInteger(out tmp); m_BomberMove = (EBomberMove)tmp;
            tmp = 0; Snapshot.ReadInteger(out tmp); m_LastRealBomberMove = (EBomberMove)tmp;
            tmp = 0; Snapshot.ReadInteger(out tmp); m_Turning = (ETurning)tmp;
            Snapshot.ReadBoolean(out m_CouldMove);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        //! Set the arena to use
        public void SetArena(CArena pArena)
        {
            Debug.Assert(pArena != null);
            m_pArena = pArena;
        }

        //! Return the block position X of the bomber
        public int GetBlockX()
        {
            Debug.Assert(m_BlockX >= 0);
            Debug.Assert(m_BlockX < Globals.ARENA_WIDTH);

            Debug.Assert(m_BlockY >= 0);
            Debug.Assert(m_BlockY < Globals.ARENA_HEIGHT);

            return m_BlockX;
        }

        //! Return the block position Y of the bomber
        public int GetBlockY()
        {
            Debug.Assert(m_BlockX >= 0);
            Debug.Assert(m_BlockX < Globals.ARENA_WIDTH);

            Debug.Assert(m_BlockY >= 0);
            Debug.Assert(m_BlockY < Globals.ARENA_HEIGHT);

            return m_BlockY;
        }

        //! Get the integer X position (in pixels) of the bomber in the arena
        public int GetX()
        {
            return m_iX;
        }

        //! Get the integer Y position (in pixels) of the bomber in the arena
        public int GetY()
        {
            return m_iY;
        }

        //! Return whether the bomber could move the last time he tried
        public bool CouldMove()
        {
            return m_CouldMove;
        }

        //! Return the current move order the bomber has to perform
        public EBomberMove GetMove()
        {
            return m_BomberMove;
        }

        //! Return the direction where the bomber was going the last time he really moved.
        public EBomberMove GetLastRealMove()
        {
            return m_LastRealBomberMove;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // These helper properties simplify the code in TryMove/CanMove/TurnTest.
        private int ToBlock(int a) { return m_pArena.ToBlock(a); }
        private int HalfBlock { get { return Globals.BLOCK_SIZE / 2; } }
        private int TurnLimit { get { return HalfBlock / 4; } }
        private int BlockedLimit { get { return -HalfBlock + (HalfBlock / 4); } }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // TryMove tries to go by fPixels pixels in the current direction and turning state.
        // Bombs which are in the bomber's way will be kicked if the bomber can kick them.
        // Important : the tests are made for a move of one single pixel or less and do not
        // depend on fPixels. If the move was judged possible, position is modified by fPixels.
        // The move is tested in the current BomberMove or turning state :
        // - This can make direction or turning state change : then try again
        // - This can modify the coordinates because the way is free : then return true
        // - Or maybe it is impossible to go further : then return false

        private bool TryMove(float fPixels)
        {
            // The m_BomberMove must describe a SINGLE DIRECTION. If not, infinite loop.
            Debug.Assert(m_BomberMove != EBomberMove.BOMBERMOVE_UPLEFT   &&
                m_BomberMove != EBomberMove.BOMBERMOVE_UPRIGHT  &&
                m_BomberMove != EBomberMove.BOMBERMOVE_DOWNLEFT &&
                m_BomberMove != EBomberMove.BOMBERMOVE_DOWNRIGHT);

            CBomber pBomber = m_pArena.GetBomber(m_Player);

            // Compute coordinates
            int X = (int)m_X + HalfBlock;   // Modified integer current coordinates (x+hb and y+hb to point to center)
            int Y = (int)m_Y + HalfBlock;   // Note : we have to compute int(m_X) and int(m_Y) because they change

            int iter = 0;

            // Try until bomber coordinates are modified or bomber is blocked
            while (iter++ < MAX_ITER)
            {
                // Not turning
                if (m_Turning == ETurning.TURNING_NOTTURNING)
                {
                    switch (m_BomberMove)
                    {
                        case EBomberMove.BOMBERMOVE_UP:
                            // If obstacle above
                            if (pBomber.IsObstacle(m_BlockX, ToBlock(Y - HalfBlock - 1)))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockY == ToBlock(Y - HalfBlock - 1))
                                {
                                    // Can move
                                    m_Y -= fPixels;
                                    return true;
                                }

                                // Else try to kick a bomb here
                                pBomber.TryKickBomb(m_BlockX, ToBlock(Y - HalfBlock - 1), EBombKick.BOMBKICK_UP);

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + BlockedLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // No obstacle up left, avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - BlockedLimit - 1), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // No obstacle up right, avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return false;
                            }
                            // No wall or bomb above, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + TurnLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Turn
                                    m_Turning = ETurning.TURNING_UPLEFT_UP;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - TurnLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    continue;
                                }
                                // If wall up left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Turn
                                    m_Turning = ETurning.TURNING_UPRIGHT_UP;
                                    continue;
                                }

                                // Else the way is free, modify the coordinates
                                m_Y -= fPixels;
                                return true;
                            }

                        case EBomberMove.BOMBERMOVE_DOWN:
                            // If obstacle below
                            if (pBomber.IsObstacle(m_BlockX, ToBlock(Y + HalfBlock)))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockY == ToBlock(Y + HalfBlock))
                                {
                                    // Can move
                                    m_Y += fPixels;
                                    return true;
                                }

                                // Else try to kick a bomb here
                                pBomber.TryKickBomb(m_BlockX, ToBlock(Y + HalfBlock), EBombKick.BOMBKICK_DOWN);

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + BlockedLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // No obstacle down left, avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - BlockedLimit - 1), ToBlock(Y + HalfBlock)))
                                {
                                    // No obstacle down right, avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return false;
                            }
                            // No wall or bomb below, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + TurnLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // Avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    continue;
                                }
                                // If wall down right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                {
                                    // Turn
                                    m_Turning = ETurning.TURNING_DOWNLEFT_DOWN;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - TurnLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // Avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    continue;
                                }
                                // If wall down left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock), ToBlock(Y + HalfBlock)))
                                {
                                    // Turn
                                    m_Turning = ETurning.TURNING_DOWNRIGHT_DOWN;
                                    continue;
                                }

                                // Else the way is free, modify the coordinates
                                m_Y += fPixels;
                                return true;
                            }

                        case EBomberMove.BOMBERMOVE_LEFT:
                            // If obstacle to the left
                            if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), m_BlockY))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockX == ToBlock(X - HalfBlock - 1))
                                {
                                    // Can move
                                    m_X -= fPixels;
                                    return true;
                                }

                                // Else try to kick a bomb here
                                pBomber.TryKickBomb(ToBlock(X - HalfBlock - 1), m_BlockY, EBombKick.BOMBKICK_LEFT);

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + BlockedLimit)))
                                {
                                    // No obstacle up left, avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - BlockedLimit - 1)))
                                {
                                    // No obstacle down left, avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return false;
                            }
                            // No wall or bomb to the left, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y + TurnLimit)))
                                {
                                    // Avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    continue;
                                }
                                // If wall down left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock - 1)))
                                {
                                    // Turn
                                    m_Turning = ETurning.TURNING_UPLEFT_LEFT;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y - TurnLimit)))
                                {
                                    // Avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock)))
                                {
                                    // Turn
                                    m_Turning = ETurning.TURNING_DOWNLEFT_LEFT;
                                    continue;
                                }

                                // Else the way is free, modify the coordinates
                                m_X -= fPixels;
                                return true;
                            }

                        case EBomberMove.BOMBERMOVE_RIGHT:
                            // If obstacle to the right
                            if (pBomber.IsObstacle(ToBlock(X + HalfBlock), m_BlockY))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockX == ToBlock(X + HalfBlock))
                                {
                                    // Can move
                                    m_X += fPixels;
                                    return true;
                                }

                                // Else try to kick a bomb here
                                pBomber.TryKickBomb(ToBlock(X + HalfBlock), m_BlockY, EBombKick.BOMBKICK_RIGHT);

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + BlockedLimit)))
                                {
                                    // No obstacle up right, avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - BlockedLimit - 1)))
                                {
                                    // No obstacle down right, avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return false;
                            }
                            // No wall or bomb to the right, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y + TurnLimit)))
                                {
                                    // Avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    continue;
                                }
                                // If wall down right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock - 1)))
                                {
                                    // Turn
                                    m_Turning = ETurning.TURNING_UPRIGHT_RIGHT;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y - TurnLimit)))
                                {
                                    // Avoid
                                    m_BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock)))
                                {
                                    // Turn
                                    m_Turning = ETurning.TURNING_DOWNRIGHT_RIGHT;
                                    continue;
                                }

                                // Else the way is free, modify the coordinates
                                m_X += fPixels;
                                return true;
                            }

                        default:
                            break;
                    }
                }
                // Turning
                else
                {
                    switch (m_Turning)
                    {
                        case ETurning.TURNING_UPLEFT_UP:
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_UP:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok, modify the coordinates
                                        m_X -= fPixels;
                                        m_Y -= fPixels;
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        return true;
                                    }

                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Try turning in the opposite direction
                                    m_Turning = ETurning.TURNING_DOWNRIGHT_RIGHT;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    m_Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPLEFT_LEFT:
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok, modify the coordinates
                                        m_X -= fPixels;
                                        m_Y -= fPixels;
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        return true;
                                    }

                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Try turning in the opposite direction
                                    m_Turning = ETurning.TURNING_DOWNRIGHT_DOWN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    m_Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPRIGHT_UP:
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_UP:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok, modify the coordinates
                                        m_X += fPixels;
                                        m_Y -= fPixels;
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        return true;
                                    }

                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Try turning in the opposite direction
                                    m_Turning = ETurning.TURNING_DOWNLEFT_LEFT;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    m_Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPRIGHT_RIGHT:
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok, modify the coordinates
                                        m_X += fPixels;
                                        m_Y -= fPixels;
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        return true;
                                    }

                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Try turning in the opposite direction
                                    m_Turning = ETurning.TURNING_DOWNLEFT_DOWN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    m_Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNLEFT_DOWN:
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                    {
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok, modify the coordinates
                                        m_X -= fPixels;
                                        m_Y += fPixels;
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        return true;
                                    }

                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Try turning in the opposite direction
                                    m_Turning = ETurning.TURNING_UPRIGHT_RIGHT;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    m_Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNLEFT_LEFT:
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                    {
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok, modify the coordinates
                                        m_X -= fPixels;
                                        m_Y += fPixels;
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        return true;
                                    }

                                case EBomberMove.BOMBERMOVE_UP:
                                    // Try turning in the opposite direction
                                    m_Turning = ETurning.TURNING_UPRIGHT_UP;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    m_Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNRIGHT_DOWN:
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock)))
                                    {
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok, modify the coordinates
                                        m_X += fPixels;
                                        m_Y += fPixels;
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        return true;
                                    }

                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Try turning in the opposite direction
                                    m_Turning = ETurning.TURNING_UPLEFT_LEFT;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    m_Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNRIGHT_RIGHT:
                            switch (m_BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock)))
                                    {
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok, modify the coordinates
                                        m_X += fPixels;
                                        m_Y += fPixels;
                                        m_Turning = ETurning.TURNING_NOTTURNING;
                                        return true;
                                    }

                                case EBomberMove.BOMBERMOVE_UP:
                                    // Try turning in the opposite direction
                                    m_Turning = ETurning.TURNING_UPLEFT_UP;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    m_Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        default:
                            break;
                    }
                }
            }

            // Can't happen
            return false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // CanMove is very similar to TryMove. CanMove tries to go in the desired direction, but
        // contrary to TryMove, the test are made with a virtual bomber, the real states won't
        // be modified. CanMove returns an information about the test : could he move, did he
        // have to avoid or turn around an obstacle?

        private ECanMove CanMove(EBomberMove TestMove)
        {
            // The TestMove must describe a SINGLE DIRECTION. If not, infinite loop.
            Debug.Assert(TestMove != EBomberMove.BOMBERMOVE_NONE && TestMove != EBomberMove.BOMBERMOVE_UPLEFT && TestMove != EBomberMove.BOMBERMOVE_UPRIGHT && TestMove != EBomberMove.BOMBERMOVE_DOWNLEFT && TestMove != EBomberMove.BOMBERMOVE_DOWNRIGHT);

            CBomber pBomber = m_pArena.GetBomber(m_Player);
            EBomberMove BomberMove = TestMove;      // Temporary move variable in order not to modify the real one
            ETurning Turning = m_Turning;           // Temporary turning variable in order not to modify the real one
            ECanMove canMove = ECanMove.CANMOVE_FREEWAY;  // The variable that will be returned. Assume the way is free.

            // Compute coordinates
            int X = m_iX + HalfBlock;   // Modified integer current coordinates (x+hb and y+hb to point to center)
            int Y = m_iY + HalfBlock;

            int iter = 0;

            // Try until bomber coordinates are modified or bomber is blocked
            while (iter++ < MAX_ITER)
            {
                // Not turning
                if (Turning == ETurning.TURNING_NOTTURNING)
                {
                    switch (BomberMove)
                    {
                        case EBomberMove.BOMBERMOVE_UP:
                            // If obstacle above
                            if (pBomber.IsObstacle(m_BlockX, ToBlock(Y - HalfBlock - 1)))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockY == ToBlock(Y - HalfBlock - 1))
                                {
                                    // Can move
                                    return canMove;
                                }

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + BlockedLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // No obstacle up left, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - BlockedLimit - 1), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // No obstacle up right, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return ECanMove.CANMOVE_CANNOT;
                            }
                            // No wall or bomb above, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + TurnLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_UPLEFT_UP;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - TurnLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                // If wall up left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_UPRIGHT_UP;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;
                                }

                                // Else the way is free, but perhaps he had to avoid or turn so return the variable
                                return canMove;
                            }

                        case EBomberMove.BOMBERMOVE_DOWN:
                            // If obstacle below
                            if (pBomber.IsObstacle(m_BlockX, ToBlock(Y + HalfBlock)))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockY == ToBlock(Y + HalfBlock))
                                {
                                    // Can move
                                    return canMove;
                                }

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + BlockedLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // No obstacle down left, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - BlockedLimit - 1), ToBlock(Y + HalfBlock)))
                                {
                                    // No obstacle down right, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return ECanMove.CANMOVE_CANNOT;
                            }
                            // No wall or bomb below, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + TurnLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                // If wall down right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_DOWNLEFT_DOWN;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - TurnLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                // If wall down left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock), ToBlock(Y + HalfBlock)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_DOWNRIGHT_DOWN;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;
                                }

                                // Else the way is free, but perhaps he had to avoid or turn so return the variable
                                return canMove;
                            }

                        case EBomberMove.BOMBERMOVE_LEFT:
                            // If obstacle to the left
                            if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), m_BlockY))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockX == ToBlock(X - HalfBlock - 1))
                                {
                                    // Can move
                                    return canMove;
                                }

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + BlockedLimit)))
                                {
                                    // No obstacle up left, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - BlockedLimit - 1)))
                                {
                                    // No obstacle down left, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return ECanMove.CANMOVE_CANNOT;
                            }
                            // No wall or bomb to the left, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y + TurnLimit)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                // If wall down left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock - 1)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_UPLEFT_LEFT;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y - TurnLimit)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_DOWNLEFT_LEFT;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;
                                }

                                // Else the way is free, but perhaps he had to avoid or turn so return the variable
                                return canMove;
                            }

                        case EBomberMove.BOMBERMOVE_RIGHT:
                            // If obstacle to the right
                            if (pBomber.IsObstacle(ToBlock(X + HalfBlock), m_BlockY))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockX == ToBlock(X + HalfBlock))
                                {
                                    // Can move
                                    return canMove;
                                }

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + BlockedLimit)))
                                {
                                    // No obstacle up right, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - BlockedLimit - 1)))
                                {
                                    // No obstacle down right, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return ECanMove.CANMOVE_CANNOT;
                            }
                            // No wall or bomb to the right, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y + TurnLimit)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                // If wall down right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock - 1)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_UPRIGHT_RIGHT;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y - TurnLimit)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    canMove = ECanMove.CANMOVE_AVOID;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_DOWNRIGHT_RIGHT;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;
                                }

                                // Else the way is free, but perhaps he had to avoid or turn so return the variable
                                return canMove;
                            }

                        default:
                            break;
                    }
                }
                // Turning
                else
                {
                    switch (Turning)
                    {
                        case ETurning.TURNING_UPLEFT_UP:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_UP:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return canMove;
                                    }

                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_DOWNRIGHT_RIGHT;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPLEFT_LEFT:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return canMove;
                                    }

                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_DOWNRIGHT_DOWN;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPRIGHT_UP:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_UP:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return canMove;
                                    }

                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_DOWNLEFT_LEFT;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPRIGHT_RIGHT:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return canMove;
                                    }

                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_DOWNLEFT_DOWN;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNLEFT_DOWN:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return canMove;
                                    }

                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_UPRIGHT_RIGHT;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNLEFT_LEFT:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return canMove;
                                    }

                                case EBomberMove.BOMBERMOVE_UP:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_UPRIGHT_UP;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNRIGHT_DOWN:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return canMove;
                                    }

                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_UPLEFT_LEFT;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNRIGHT_RIGHT:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return canMove;
                                    }

                                case EBomberMove.BOMBERMOVE_UP:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_UPLEFT_UP;
                                    canMove = ECanMove.CANMOVE_TURN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        default:
                            break;
                    }
                }
            }

            // Can't happen
            return ECanMove.CANMOVE_CANNOT;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // TurnTest is very similar to CanMove. TurnTest makes a virtual bomber move, then when
        // the move is finished (ie the virtual bomber has moved or is blocked), it returns
        // the virtual turning state.

        private ETurning TurnTest(EBomberMove TestMove)
        {
            // The TestMove must describe a SINGLE DIRECTION. If not, infinite loop.
            Debug.Assert(TestMove != EBomberMove.BOMBERMOVE_UPLEFT && TestMove != EBomberMove.BOMBERMOVE_UPRIGHT && TestMove != EBomberMove.BOMBERMOVE_DOWNLEFT && TestMove != EBomberMove.BOMBERMOVE_DOWNRIGHT);

            CBomber pBomber = m_pArena.GetBomber(m_Player);
            EBomberMove BomberMove = TestMove;  // Temporary go variable in order not to modify the real one
            ETurning Turning = m_Turning;       // Temporary turning variable in order not to modify the real one

            // Compute coordinates
            int X = m_iX + HalfBlock;       // Modified integer current coordinates (x+hb and y+hb to point to center)
            int Y = m_iY + HalfBlock;

            int iter = 0;

            // Try until bomber coordinates are modified or bomber is blocked
            while (iter++ < MAX_ITER)
            {
                // Not turning
                if (Turning == ETurning.TURNING_NOTTURNING)
                {
                    switch (BomberMove)
                    {
                        case EBomberMove.BOMBERMOVE_UP:
                            // If obstacle above
                            if (pBomber.IsObstacle(m_BlockX, ToBlock(Y - HalfBlock - 1)))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockY == ToBlock(Y - HalfBlock - 1))
                                {
                                    // Can move
                                    return Turning;
                                }

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + BlockedLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // No obstacle up left, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - BlockedLimit - 1), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // No obstacle up right, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return Turning;
                            }
                            // No wall or bomb above, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + TurnLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_UPLEFT_UP;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - TurnLimit), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    continue;
                                }
                                // If wall up left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_UPRIGHT_UP;
                                    continue;
                                }

                                // Else the way is free
                                return Turning;
                            }

                        case EBomberMove.BOMBERMOVE_DOWN:
                            // If obstacle below
                            if (pBomber.IsObstacle(m_BlockX, ToBlock(Y + HalfBlock)))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockY == ToBlock(Y + HalfBlock))
                                {
                                    // Can move
                                    return Turning;
                                }

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + BlockedLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // No obstacle down left, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - BlockedLimit - 1), ToBlock(Y + HalfBlock)))
                                {
                                    // No obstacle down right, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return Turning;
                            }
                            // No wall or bomb below, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + TurnLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                    continue;
                                }
                                // If wall down right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_DOWNLEFT_DOWN;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - TurnLimit), ToBlock(Y + HalfBlock)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                    continue;
                                }
                                // If wall down left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock), ToBlock(Y + HalfBlock)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_DOWNRIGHT_DOWN;
                                    continue;
                                }

                                // Else the way is free
                                return Turning;
                            }

                        case EBomberMove.BOMBERMOVE_LEFT:
                            // If obstacle to the left
                            if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), m_BlockY))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockX == ToBlock(X - HalfBlock - 1))
                                {
                                    // Can move
                                    return Turning;
                                }

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + BlockedLimit)))
                                {
                                    // No obstacle up left, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - BlockedLimit - 1)))
                                {
                                    // No obstacle down left, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return Turning;
                            }
                            // No wall or bomb to the left, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y + TurnLimit)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    continue;
                                }
                                // If wall down left
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock - 1)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_UPLEFT_LEFT;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y - TurnLimit)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_DOWNLEFT_LEFT;
                                    continue;
                                }

                                // Else the way is free
                                return Turning;
                            }

                        case EBomberMove.BOMBERMOVE_RIGHT:
                            // If obstacle to the right
                            if (pBomber.IsObstacle(ToBlock(X + HalfBlock), m_BlockY))
                            {
                                // For walls and bombs : if the tested square is the same as the bomber's square
                                if (m_BlockX == ToBlock(X + HalfBlock))
                                {
                                    // Can move
                                    return Turning;
                                }

                                // Can he avoid the obstacle?
                                if (!pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + BlockedLimit)))
                                {
                                    // No obstacle up right, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    continue;
                                }
                                else if (!pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - BlockedLimit - 1)))
                                {
                                    // No obstacle down right, avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    continue;
                                }

                                // Can't avoid the obstacle
                                return Turning;
                            }
                            // No wall or bomb to the right, should he turn around a wall (or avoid it) or be blocked?
                            else
                            {
                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y + TurnLimit)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_UP;
                                    continue;
                                }
                                // If wall down right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock - 1)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_UPRIGHT_RIGHT;
                                    continue;
                                }

                                // To avoid turning too early
                                if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y - TurnLimit)))
                                {
                                    // Avoid
                                    BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                    continue;
                                }
                                // If wall up right
                                else if (m_pArena.IsWall(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock)))
                                {
                                    // Turn
                                    Turning = ETurning.TURNING_DOWNRIGHT_RIGHT;
                                    continue;
                                }

                                // Else the way is free
                                return Turning;
                            }

                        default:
                            break;
                    }
                }
                // Turning
                else
                {
                    switch (Turning)
                    {
                        case ETurning.TURNING_UPLEFT_UP:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_UP:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return Turning;
                                    }

                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_DOWNRIGHT_RIGHT;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPLEFT_LEFT:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return Turning;
                                    }

                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_DOWNRIGHT_DOWN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPRIGHT_UP:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_UP:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return Turning;
                                    }

                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_DOWNLEFT_LEFT;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_UPRIGHT_RIGHT:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y - HalfBlock - 1)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return Turning;
                                    }

                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_DOWNLEFT_DOWN;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNLEFT_DOWN:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return Turning;
                                    }

                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_UPRIGHT_RIGHT;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNLEFT_LEFT:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X - HalfBlock - 1), ToBlock(Y + HalfBlock)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return Turning;
                                    }

                                case EBomberMove.BOMBERMOVE_UP:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_UPRIGHT_UP;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNRIGHT_DOWN:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_DOWN:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return Turning;
                                    }

                                case EBomberMove.BOMBERMOVE_LEFT:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_UPLEFT_LEFT;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        case ETurning.TURNING_DOWNRIGHT_RIGHT:
                            switch (BomberMove)
                            {
                                case EBomberMove.BOMBERMOVE_RIGHT:
                                    // Stop turning if obstacle
                                    if (pBomber.IsObstacle(ToBlock(X + HalfBlock), ToBlock(Y + HalfBlock)))
                                    {
                                        Turning = ETurning.TURNING_NOTTURNING;
                                        continue;
                                    }
                                    else
                                    {
                                        // Else it's ok
                                        return Turning;
                                    }

                                case EBomberMove.BOMBERMOVE_UP:
                                    // Try turning in the opposite direction
                                    Turning = ETurning.TURNING_UPLEFT_UP;
                                    continue;

                                default:
                                    // Stop turning to try this direction
                                    Turning = ETurning.TURNING_NOTTURNING;
                                    continue;
                            }

                        default:
                            break;
                    }
                }
            }

            // Can't happen
            return Turning;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
