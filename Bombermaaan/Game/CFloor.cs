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
 *  \file CFloor.cs
 *  \brief Floor in the arena
 */

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Describes actions on floors (moving bombs so far).</summary>
    public enum EFloorAction
    {
        FLOORACTION_NONE,               //!< There is no special action.
        FLOORACTION_MOVEBOMB_RIGHT,     //!< Bombs start moving right
        FLOORACTION_MOVEBOMB_DOWN,      //!< Bombs start moving down
        FLOORACTION_MOVEBOMB_LEFT,      //!< Bombs start moving left
        FLOORACTION_MOVEBOMB_UP         //!< Bombs start moving up
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    // Floor sprite layers
    internal static class FloorConstants
    {
        public const int FLOOR_SPRITELAYER  = 0;
        public const int ACTION_SPRITELAYER = 1;

        // Floor sprites
        public const int FLOORSPRITE_NOSHADOW = 0;
        public const int FLOORSPRITE_SHADOW   = 1;

        // Arrow sprites
        public const int ARENA_FLOOR_ARROW_RIGHT = 0;
        public const int ARENA_FLOOR_ARROW_DOWN  = 1;
        public const int ARENA_FLOOR_ARROW_LEFT  = 2;
        public const int ARENA_FLOOR_ARROW_UP    = 3;
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>An element in the arena which represents a floor tile.</summary>
    public class CFloor : CElement
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private int         m_iX;           //!< Integer position X (in pixels) in the arena
        private int         m_iY;           //!< Integer position Y (in pixels) in the arena
        private int         m_BlockX;       //!< Position X (in blocks) in the arena grid
        private int         m_BlockY;       //!< Position Y (in blocks) in the arena grid
        private bool        m_Dead;         //!< Should the floor be deleted by the arena?
        private EFloorAction m_FloorAction; //!< Action the floor does to objects touching it

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Constructor. Initialize the base class.</summary>
        public CFloor()
        {
            m_iX         = -1;
            m_iY         = -1;
            m_BlockX     = -1;
            m_BlockY     = -1;
            m_Dead       = false;
            m_FloorAction = EFloorAction.FLOORACTION_NONE;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Initialize the floor.</summary>
        public void CopyFrom(CFloor other)
        {
            m_Exist       = other.m_Exist;
            m_pDisplay    = other.m_pDisplay;
            m_pSound      = other.m_pSound;
            m_iX          = other.m_iX;
            m_iY          = other.m_iY;
            m_BlockX      = other.m_BlockX;
            m_BlockY      = other.m_BlockY;
            m_Dead        = other.m_Dead;
            m_FloorAction = other.m_FloorAction;
        }

        public void Create(int BlockX, int BlockY, EFloorAction floorAction)
        {
            base.Create();

            m_iX         = m_pArena.ToPosition(BlockX);
            m_iY         = m_pArena.ToPosition(BlockY);
            m_BlockX     = BlockX;
            m_BlockY     = BlockY;
            m_Dead       = false;
            m_FloorAction = floorAction;
        }

        /// <summary>Uninitialize the floor.</summary>
        public void Destroy()
        {
            base.Destroy();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Update the element. Return whether the element should be deleted by the arena.</summary>
        public override bool Update(float DeltaTime)
        {
            // The arena can destroy this floor if it is dead
            return m_Dead;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Display the floor.</summary>
        public override void Display()
        {
            // If there is no item and (no wall or a burning wall or a falling wall)
            if (!m_pArena.IsItem(m_BlockX, m_BlockY) &&
                (
                    !m_pArena.IsWall(m_BlockX, m_BlockY) ||
                    m_pArena.IsBurningWall(m_BlockX, m_BlockY) ||
                    m_pArena.IsFallingWall(m_BlockX, m_BlockY)
                ))
            {
                // Try to determine if there is a shadow on this floor.
                int Sprite;

                // If there can be a block above and if there is a wall above and it's not a falling wall
                if (m_BlockY - 1 >= 0 &&
                    m_pArena.IsWall(m_BlockX, m_BlockY - 1) &&
                    !m_pArena.IsFallingWall(m_BlockX, m_BlockY - 1))
                {
                    // Then there is a shadow
                    Sprite = FloorConstants.FLOORSPRITE_SHADOW;
                }
                else
                {
                    // Then there is no shadow
                    Sprite = FloorConstants.FLOORSPRITE_NOSHADOW;
                }

                // Add the sprite in the layer. Priority is not used.
                m_pDisplay.DrawSprite(m_iX,
                                      m_iY,
                                      null,
                                      null,
                                      BmpId.BMP_ARENA_FLOOR,
                                      Sprite,
                                      FloorConstants.FLOOR_SPRITELAYER,
                                      CDisplay.PRIORITY_UNUSED);

                Sprite = -1;

                switch (m_FloorAction)
                {
                    case EFloorAction.FLOORACTION_MOVEBOMB_RIGHT: Sprite = FloorConstants.ARENA_FLOOR_ARROW_RIGHT; break;
                    case EFloorAction.FLOORACTION_MOVEBOMB_DOWN:  Sprite = FloorConstants.ARENA_FLOOR_ARROW_DOWN;  break;
                    case EFloorAction.FLOORACTION_MOVEBOMB_LEFT:  Sprite = FloorConstants.ARENA_FLOOR_ARROW_LEFT;  break;
                    case EFloorAction.FLOORACTION_MOVEBOMB_UP:    Sprite = FloorConstants.ARENA_FLOOR_ARROW_UP;    break;
                    default: break;
                }

                if (Sprite != -1)
                {
                    m_pDisplay.DrawSprite(m_iX,
                                          m_iY,
                                          null,
                                          null,
                                          BmpId.BMP_ARENA_ARROWS,
                                          Sprite,
                                          FloorConstants.ACTION_SPRITELAYER,
                                          CDisplay.PRIORITY_UNUSED);
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected override void OnWriteSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.WriteInteger(m_iX);
            Snapshot.WriteInteger(m_iY);
            Snapshot.WriteInteger(m_BlockX);
            Snapshot.WriteInteger(m_BlockY);
            Snapshot.WriteBoolean(m_Dead);
            Snapshot.WriteInteger((int)m_FloorAction);
        }

        protected override void OnReadSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.ReadInteger(out m_iX);
            Snapshot.ReadInteger(out m_iY);
            Snapshot.ReadInteger(out m_BlockX);
            Snapshot.ReadInteger(out m_BlockY);
            Snapshot.ReadBoolean(out m_Dead);
            int floorAction = 0;
            Snapshot.ReadInteger(out floorAction);
            m_FloorAction = (EFloorAction)floorAction;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Make the floor react when it is crushed by a wall.</summary>
        public void Crush()
        {
            m_Dead = true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Return the block position X of the floor.</summary>
        public int GetBlockX() { return m_BlockX; }

        /// <summary>Return the block position Y of the floor.</summary>
        public int GetBlockY() { return m_BlockY; }

        /// <summary>Return the action of the floor.</summary>
        public EFloorAction GetFloorAction() { return m_FloorAction; }

        /// <summary>Return if the block has an action.</summary>
        public bool HasAction() { return GetFloorAction() != EFloorAction.FLOORACTION_NONE; }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
