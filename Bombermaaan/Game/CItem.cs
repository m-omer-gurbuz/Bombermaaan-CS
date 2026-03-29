/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2008 Jerome Bigot
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
 *  \file CItem.cs
 *  \brief Item a bomber may pick
 */

using System.Collections.Generic;
using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Describe a type of item.</summary>
    public enum EItemType
    {
        ITEM_NONE,
        ITEM_BOMB,          //!< Bomb item allows a bomber to drop more bombs
        ITEM_FLAME,         //!< Flame item allows a bomber to drop more powerful bombs
        ITEM_KICK,          //!< Kick item allows a bomber to kick bombs
        ITEM_ROLLER,        //!< Roller item allows a bomber to walk faster
        ITEM_SKULL,         //!< Skull item gives a bomber a sickness
        ITEM_THROW,         //!< Throw glove item allows a bomber to throw bombs
        ITEM_PUNCH,         //!< Boxing glove item allows a bomber to punch bombs
        ITEM_REMOTE,        //!< Remote bombs item allows a bomber to remotely control bomb fuse
        ITEM_SHIELD,        //!< Shield item allows a bomber to be resistant to flames
        ITEM_STRONGWEAK,    //!< Strong item allows a bomber to be strong or weak
        NUMBER_OF_ITEMS     //!< The number of items (this includes ITEM_NONE)
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Describes a possible kind of place for new items.</summary>
    public enum EItemPlace
    {
        ITEMPLACE_FLOOR,        //!< Create new items on the floor only
        ITEMPLACE_SOFTWALLS     //!< Create new items under soft walls only
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Describes the flying state of an item.</summary>
    public enum EItemFlying
    {
        ITEMFLYING_NONE  = -1,  //!< The item is not flying
        ITEMFLYING_UP    =  0,  //!< The item is flying and moving up
        ITEMFLYING_DOWN,        //!< The item is flying and moving down
        ITEMFLYING_LEFT,        //!< The item is flying and moving left
        ITEMFLYING_RIGHT        //!< The item is flying and moving right
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>An element in the arena which represents an item.</summary>
    public class CItem : CElement
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Item sprite layer
        private const int ITEM_SPRITELAYER = 10;

        // Fire sprite layer (when item is on fire)
        private const int FIRE_SPRITELAYER = 50;

        // Falling wall sprite layer (flying objects, bombers and item fires)
        private const int FLY_SPRITELAYER = 50;

        // Times of item animation (in seconds)
        private const float ANIMITEM_TIME1 = 0.080f;
        private const float ANIMITEM_TIME2 = ANIMITEM_TIME1 * 2;

        // Times of fumes animation (in seconds)
        private const float ANIMFUMES_TIME1 = 0.100f;
        private const float ANIMFUMES_TIME2 = ANIMFUMES_TIME1 * 2;
        private const float ANIMFUMES_TIME3 = ANIMFUMES_TIME1 * 3;

        // Duration value for the anim fire
        private const float ANIMFIRE_DURATION = 0.9f;

        // Times of item fire animation (in seconds)
        private const float ANIMFIRE_TIME1 = 0.070f * ANIMFIRE_DURATION;
        private const float ANIMFIRE_TIME2 = 0.170f * ANIMFIRE_DURATION;
        private const float ANIMFIRE_TIME3 = 0.310f * ANIMFIRE_DURATION;
        private const float ANIMFIRE_TIME4 = 0.460f * ANIMFIRE_DURATION;
        private const float ANIMFIRE_TIME5 = 0.560f * ANIMFIRE_DURATION;
        private const float ANIMFIRE_TIME6 = 0.630f * ANIMFIRE_DURATION;
        private const float ANIMFIRE_TIME7 = 0.720f * ANIMFIRE_DURATION;

        // Item sprites according to item type
        private const int SPRITE_BOMB0       = 0;
        private const int SPRITE_BOMB1       = 1;
        private const int SPRITE_FLAME0      = 2;
        private const int SPRITE_FLAME1      = 3;
        private const int SPRITE_KICK0       = 4;
        private const int SPRITE_KICK1       = 5;
        private const int SPRITE_ROLLER0     = 6;
        private const int SPRITE_ROLLER1     = 7;
        private const int SPRITE_SKULL0      = 8;
        private const int SPRITE_SKULL1      = 9;
        private const int SPRITE_THROW0      = 10;
        private const int SPRITE_THROW1      = 11;
        private const int SPRITE_PUNCH0      = 12;
        private const int SPRITE_PUNCH1      = 13;
        private const int SPRITE_REMOTE0     = 14;
        private const int SPRITE_REMOTE1     = 15;
        private const int SPRITE_SHIELD0     = 16;
        private const int SPRITE_SHIELD1     = 17;
        private const int SPRITE_STRONGWEAK0 = 18;
        private const int SPRITE_STRONGWEAK1 = 19;

        // Fume animation sprites
        private const int ANIM_FUMES_1 = 0;
        private const int ANIM_FUMES_2 = 1;
        private const int ANIM_FUMES_3 = 2;

        // Fire animation sprites
        private const int ANIM_FIRE1 = 0;
        private const int ANIM_FIRE2 = 1;
        private const int ANIM_FIRE3 = 2;
        private const int ANIM_FIRE4 = 3;
        private const int ANIM_FIRE5 = 4;
        private const int ANIM_FIRE6 = 5;
        private const int ANIM_FIRE7 = 6;

        // Offset when drawing fire sprites
        private const int FIRE_OFFSETX = -10;
        private const int FIRE_OFFSETY = -(54 - 32);

        // Flying item animation sprites
        private const int ANIM_FLYING1 = 0;
        private const int ANIM_FLYING2 = 1;
        private const int ANIM_FLYING3 = 2;
        private const int ANIM_FLYING4 = 1;

        // Flying item animation times
        private const float ANIMFLYING_TIME1 = 0.050f;
        private const float ANIMFLYING_TIME2 = ANIMFLYING_TIME1 * 2;
        private const float ANIMFLYING_TIME3 = ANIMFLYING_TIME1 * 3;
        private const float ANIMFLYING_TIME4 = ANIMFLYING_TIME1 * 4;

        // Flying speed in pixels per second
        private const float FLYING_SPEED   = 200f;
        private const float MINIMUM_FLY_TIME = (3.0f * Globals.BLOCK_SIZE) / FLYING_SPEED;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private float      m_fX;         //!< Float position X (in pixels) in the arena
        private float      m_fY;         //!< Float position Y (in pixels) in the arena
        private int        m_iX;         //!< Integer position X (in pixels) in the arena
        private int        m_iY;         //!< Integer position Y (in pixels) in the arena
        private int        m_BlockX;     //!< Position X (in blocks) in the arena grid
        private int        m_BlockY;     //!< Position Y (in blocks) in the arena grid
        private int        m_Sprite;     //!< Current item sprite to use when displaying
        private int        m_Sprite0;    //!< First sprite number of the item flash animation
        private int        m_Sprite1;    //!< Second sprite number of the item flash animation
        private float      m_Timer;      //!< Time counter for animation
        private bool       m_Dead;       //!< Should the item be deleted by the arena?
        private bool       m_Burning;    //!< Is the item burning?
        private EItemType  m_Type;       //!< Type of this item
        private bool       m_Fumes;      //!< Is the fumes animation playing?
        private int        m_FumeSprite; //!< Current fume frame number
        private EItemFlying m_Flying;    //!< Is the item currently flying and in which direction?
        private float      m_FlyTime;    //!< How long (in seconds) has the item been flying?

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void CopyFrom(CItem other)
        {
            m_Exist      = other.m_Exist;
            m_pDisplay   = other.m_pDisplay;
            m_pSound     = other.m_pSound;
            m_fX         = other.m_fX;
            m_fY         = other.m_fY;
            m_iX         = other.m_iX;
            m_iY         = other.m_iY;
            m_BlockX     = other.m_BlockX;
            m_BlockY     = other.m_BlockY;
            m_Sprite     = other.m_Sprite;
            m_Sprite0    = other.m_Sprite0;
            m_Sprite1    = other.m_Sprite1;
            m_Timer      = other.m_Timer;
            m_Dead       = other.m_Dead;
            m_Burning    = other.m_Burning;
            m_Type       = other.m_Type;
            m_Fumes      = other.m_Fumes;
            m_FumeSprite = other.m_FumeSprite;
            m_Flying     = other.m_Flying;
            m_FlyTime    = other.m_FlyTime;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Constructor (initialize the base class).</summary>
        public CItem()
        {
            m_iX        = -1;
            m_iY        = -1;
            m_fX        = (float)m_iX;
            m_fY        = (float)m_iY;
            m_BlockX    = -1;
            m_BlockY    = -1;
            m_Burning   = false;
            m_Timer     = 0.0f;
            m_Dead      = false;
            m_Type      = EItemType.ITEM_NONE;
            m_Fumes     = false;
            m_FumeSprite = ANIM_FUMES_1;
            m_Sprite    = 0;
            m_Sprite0   = 0;
            m_Sprite1   = 0;
            m_Flying    = EItemFlying.ITEMFLYING_NONE;
            m_FlyTime   = 0.0f;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Initialize the item.</summary>
        public void Create(int BlockX, int BlockY, EItemType Type, bool Fumes, bool FlyingRandom)
        {
            // The item type must be valid
            Debug.Assert(Type != EItemType.ITEM_NONE);

            base.Create();

            m_iX      = m_pArena.ToPosition(BlockX);
            m_iY      = m_pArena.ToPosition(BlockY);
            m_fX      = (float)m_iX;
            m_fY      = (float)m_iY;
            m_BlockX  = BlockX;
            m_BlockY  = BlockY;
            m_Burning = false;
            m_Timer   = 0.0f;
            m_Dead    = false;
            m_Type    = Type;
            m_Fumes   = Fumes;
            m_FumeSprite = ANIM_FUMES_1;

            if (FlyingRandom)
            {
                //! There cannot be fumes if the item must fly.
                Debug.Assert(!Fumes);

                m_Flying  = (EItemFlying)CRandom.Random(4);
                m_Sprite  = ANIM_FLYING1;
                m_FlyTime = 0.0f;
            }
            else
            {
                m_Flying = EItemFlying.ITEMFLYING_NONE;
                SetSprites();
            }
        }

        /// <summary>Uninitialize the item.</summary>
        public void Destroy()
        {
            base.Destroy();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set the sprite numbers according to the item type.</summary>
        private void SetSprites()
        {
            switch (m_Type)
            {
                case EItemType.ITEM_BOMB:       m_Sprite0 = SPRITE_BOMB0;       m_Sprite1 = SPRITE_BOMB1;       break;
                case EItemType.ITEM_FLAME:      m_Sprite0 = SPRITE_FLAME0;      m_Sprite1 = SPRITE_FLAME1;      break;
                case EItemType.ITEM_KICK:       m_Sprite0 = SPRITE_KICK0;       m_Sprite1 = SPRITE_KICK1;       break;
                case EItemType.ITEM_ROLLER:     m_Sprite0 = SPRITE_ROLLER0;     m_Sprite1 = SPRITE_ROLLER1;     break;
                case EItemType.ITEM_SKULL:      m_Sprite0 = SPRITE_SKULL0;      m_Sprite1 = SPRITE_SKULL1;      break;
                case EItemType.ITEM_THROW:      m_Sprite0 = SPRITE_THROW0;      m_Sprite1 = SPRITE_THROW1;      break;
                case EItemType.ITEM_PUNCH:      m_Sprite0 = SPRITE_PUNCH0;      m_Sprite1 = SPRITE_PUNCH1;      break;
                case EItemType.ITEM_REMOTE:     m_Sprite0 = SPRITE_REMOTE0;     m_Sprite1 = SPRITE_REMOTE1;     break;
                case EItemType.ITEM_SHIELD:     m_Sprite0 = SPRITE_SHIELD0;     m_Sprite1 = SPRITE_SHIELD1;     break;
                case EItemType.ITEM_STRONGWEAK: m_Sprite0 = SPRITE_STRONGWEAK0; m_Sprite1 = SPRITE_STRONGWEAK1; break;
                default: break;
            }

            m_Sprite = m_Sprite0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Make the item react when an explosion hits this item.</summary>
        public void Burn(EBurnDirection BurnDirection)
        {
            // If this item is not a skull item
            if (m_Type != EItemType.ITEM_SKULL)
            {
                // Set burning only if it was not burning
                if (!m_Burning)
                {
                    m_Timer   = 0.0f;
                    m_Burning = true;
                }
            }
            // If this item is a skull item and the burn direction is valid
            else if (BurnDirection != EBurnDirection.BURNDIRECTION_NONE)
            {
                switch (BurnDirection)
                {
                    case EBurnDirection.BURNDIRECTION_UP:    m_Flying = EItemFlying.ITEMFLYING_UP;    break;
                    case EBurnDirection.BURNDIRECTION_DOWN:  m_Flying = EItemFlying.ITEMFLYING_DOWN;  break;
                    case EBurnDirection.BURNDIRECTION_LEFT:  m_Flying = EItemFlying.ITEMFLYING_LEFT;  break;
                    case EBurnDirection.BURNDIRECTION_RIGHT: m_Flying = EItemFlying.ITEMFLYING_RIGHT; break;
                    default: break;
                }

                m_Sprite  = ANIM_FLYING1;
                m_FlyTime = 0.0f;
            }
        }

        /// <summary>Make the item react when crushed by a wall.</summary>
        public void Crush()
        {
            m_Dead = true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Update the item and return whether the item should be deleted by the arena.</summary>
        public override bool Update(float DeltaTime)
        {
            // If the item is not flying
            if (m_Flying == EItemFlying.ITEMFLYING_NONE)
            {
                // Don't update if it cannot be seen (under a soft wall that is not burning/falling)
                if (!m_pArena.IsWall(m_BlockX, m_BlockY) ||
                    m_pArena.IsBurningWall(m_BlockX, m_BlockY) ||
                    m_pArena.IsFallingWall(m_BlockX, m_BlockY))
                {
                    // If item is not burning
                    if (!m_Burning)
                    {
                        // Seek a bomber that is on the item block
                        if (m_pArena.IsAliveBomber(m_BlockX, m_BlockY) && !m_Burning)
                        {
                            for (int Index = 0; Index < m_pArena.MaxBombers(); Index++)
                            {
                                if (m_pArena.GetBomber(Index).Exist() &&
                                    m_pArena.GetBomber(Index).GetBlockX() == m_BlockX &&
                                    m_pArena.GetBomber(Index).GetBlockY() == m_BlockY &&
                                    m_pArena.GetBomber(Index).IsAlive())
                                {
                                    m_pArena.GetBomber(Index).ItemEffect(m_Type);
                                    m_Dead = true;
                                    break;
                                }
                            }
                        }

                        // If the fumes animation is not playing
                        if (!m_Fumes)
                        {
                            // Animate the item (flash)
                            if (m_Timer < ANIMITEM_TIME1)      m_Sprite = m_Sprite0;
                            else if (m_Timer < ANIMITEM_TIME2) m_Sprite = m_Sprite1;
                            else
                            {
                                m_Sprite = m_Sprite0;
                                m_Timer  = 0.0f;
                            }
                        }
                        else
                        {
                            // Animate the fumes
                            if (m_Timer < ANIMFUMES_TIME1)      m_FumeSprite = ANIM_FUMES_1;
                            else if (m_Timer < ANIMFUMES_TIME2) m_FumeSprite = ANIM_FUMES_2;
                            else if (m_Timer < ANIMFUMES_TIME3) m_FumeSprite = ANIM_FUMES_3;
                            else
                            {
                                // Stop the fumes animation
                                m_Fumes = false;
                                m_Timer = 0.0f;
                            }
                        }

                        m_Timer += DeltaTime;
                    }
                    // If the item is burning
                    else
                    {
                        // Animate (item fire)
                             if (m_Timer < ANIMFIRE_TIME1) m_Sprite = ANIM_FIRE1;
                        else if (m_Timer < ANIMFIRE_TIME2) m_Sprite = ANIM_FIRE2;
                        else if (m_Timer < ANIMFIRE_TIME3) m_Sprite = ANIM_FIRE3;
                        else if (m_Timer < ANIMFIRE_TIME4) m_Sprite = ANIM_FIRE4;
                        else if (m_Timer < ANIMFIRE_TIME5) m_Sprite = ANIM_FIRE5;
                        else if (m_Timer < ANIMFIRE_TIME6) m_Sprite = ANIM_FIRE6;
                        else if (m_Timer < ANIMFIRE_TIME7) m_Sprite = ANIM_FIRE7;
                        else
                        {
                            m_Dead = true; // The fire has ended, the item is burnt
                        }

                        m_Timer += DeltaTime;
                    }
                }
            }
            // If the item is flying
            else
            {
                // Animate
                     if (m_Timer < ANIMFLYING_TIME1) m_Sprite = ANIM_FLYING1;
                else if (m_Timer < ANIMFLYING_TIME2) m_Sprite = ANIM_FLYING2;
                else if (m_Timer < ANIMFLYING_TIME3) m_Sprite = ANIM_FLYING3;
                else if (m_Timer < ANIMFLYING_TIME4) m_Sprite = ANIM_FLYING4;
                else
                {
                    m_Timer = 0.0f;
                }

                m_Timer   += DeltaTime;
                m_FlyTime += DeltaTime;

                if (m_FlyTime >= MINIMUM_FLY_TIME)
                {
                    int LandBlockX = m_pArena.ToBlock(m_iX + Globals.BLOCK_SIZE / 2);
                    int LandBlockY = m_pArena.ToBlock(m_iY + Globals.BLOCK_SIZE / 2);

                    if (LandBlockX >= 0 && LandBlockX < Globals.ARENA_WIDTH &&
                        LandBlockY >= 0 && LandBlockY < Globals.ARENA_HEIGHT)
                    {
                        if (!m_pArena.IsWall(LandBlockX, LandBlockY) &&
                            !m_pArena.IsItem(LandBlockX, LandBlockY) &&
                            !m_pArena.IsBomber(LandBlockX, LandBlockY) &&
                            !m_pArena.IsBomb(LandBlockX, LandBlockY) &&
                            !m_pArena.IsFlame(LandBlockX, LandBlockY))
                        {
                            m_Flying  = EItemFlying.ITEMFLYING_NONE;
                            m_FlyTime = 0.0f;
                            m_iX      = m_pArena.ToPosition(LandBlockX);
                            m_iY      = m_pArena.ToPosition(LandBlockY);
                            m_fX      = (float)m_iX;
                            m_fY      = (float)m_iY;
                            m_BlockX  = LandBlockX;
                            m_BlockY  = LandBlockY;
                            SetSprites();
                        }
                    }
                }

                // Make the item move according to its direction
                switch (m_Flying)
                {
                    case EItemFlying.ITEMFLYING_UP:
                    {
                        m_fY -= DeltaTime * FLYING_SPEED;
                        if (m_fY < -20.0f)
                            m_fY = Globals.ARENA_HEIGHT * Globals.BLOCK_SIZE + 20.0f;
                        break;
                    }

                    case EItemFlying.ITEMFLYING_DOWN:
                    {
                        m_fY += DeltaTime * FLYING_SPEED;
                        if (m_fY > Globals.ARENA_HEIGHT * Globals.BLOCK_SIZE + 20.0f)
                            m_fY = -20.0f;
                        break;
                    }

                    case EItemFlying.ITEMFLYING_LEFT:
                    {
                        m_fX -= DeltaTime * FLYING_SPEED;
                        if (m_fX < -20.0f)
                            m_fX = Globals.ARENA_WIDTH * Globals.BLOCK_SIZE + 20.0f;
                        break;
                    }

                    case EItemFlying.ITEMFLYING_RIGHT:
                    {
                        m_fX += DeltaTime * FLYING_SPEED;
                        if (m_fX > Globals.ARENA_WIDTH * Globals.BLOCK_SIZE + 20.0f)
                            m_fX = -20.0f;
                        break;
                    }

                    default:
                        break;
                }

                m_iX = (int)m_fX;
                m_iY = (int)m_fY;
            }

            return m_Dead;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Display the item.</summary>
        public override void Display()
        {
            // If the item is not flying
            if (m_Flying == EItemFlying.ITEMFLYING_NONE)
            {
                if (!m_pArena.IsWall(m_BlockX, m_BlockY) ||
                    m_pArena.IsBurningWall(m_BlockX, m_BlockY) ||
                    m_pArena.IsFallingWall(m_BlockX, m_BlockY))
                {
                    // If item is not burning
                    if (!m_Burning)
                    {
                        // Draw the item sprite
                        m_pDisplay.DrawSprite(m_iX,
                            m_iY,
                            null,
                            null,
                            BmpId.BMP_ARENA_ITEM,
                            m_Sprite,
                            ITEM_SPRITELAYER,
                            0);

                        // If the fumes animation is playing
                        if (m_Fumes)
                        {
                            int fumesOffset = 16;

                            m_pDisplay.DrawSprite(m_iX - fumesOffset, m_iY - fumesOffset, null, null,
                                BmpId.BMP_ARENA_FUMES, m_FumeSprite + 3 * 0, 50, m_iY - 4);

                            m_pDisplay.DrawSprite(m_iX + fumesOffset, m_iY - fumesOffset, null, null,
                                BmpId.BMP_ARENA_FUMES, m_FumeSprite + 3 * 1, 50, m_iY - 4);

                            m_pDisplay.DrawSprite(m_iX + fumesOffset, m_iY + fumesOffset, null, null,
                                BmpId.BMP_ARENA_FUMES, m_FumeSprite + 3 * 2, 50, m_iY + 4);

                            m_pDisplay.DrawSprite(m_iX - fumesOffset, m_iY + fumesOffset, null, null,
                                BmpId.BMP_ARENA_FUMES, m_FumeSprite + 3 * 3, 50, m_iY + 4);
                        }
                    }
                    // If item is burning
                    else
                    {
                        // Draw the fire sprite
                        m_pDisplay.DrawSprite(m_iX + FIRE_OFFSETX,
                            m_iY + FIRE_OFFSETY,
                            null,
                            null,
                            BmpId.BMP_ARENA_FIRE,
                            m_Sprite,
                            FIRE_SPRITELAYER,
                            m_iY);

                        // Draw the item sprite underneath
                        m_pDisplay.DrawSprite(m_iX,
                            m_iY,
                            null,
                            null,
                            BmpId.BMP_ARENA_ITEM,
                            m_Sprite0,
                            ITEM_SPRITELAYER,
                            CDisplay.PRIORITY_UNUSED);
                    }
                }
            }
            // If the item is flying
            else
            {
                RECT Clip;
                Clip.left   = 0;
                Clip.top    = 0;
                Clip.right  = CDisplay.VIEW_WIDTH;
                Clip.bottom = CDisplay.VIEW_HEIGHT - 26;

                m_pDisplay.DrawSprite(m_iX,
                    m_iY,
                    null,
                    Clip,
                    BmpId.BMP_ARENA_FLY,
                    m_Sprite,
                    FLY_SPRITELAYER,
                    m_iY);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected override void OnWriteSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.WriteFloat(m_fX);
            Snapshot.WriteFloat(m_fY);
            Snapshot.WriteInteger(m_iX);
            Snapshot.WriteInteger(m_iY);
            Snapshot.WriteInteger(m_BlockX);
            Snapshot.WriteInteger(m_BlockY);
            Snapshot.WriteInteger(m_Sprite);
            Snapshot.WriteInteger(m_Sprite0);
            Snapshot.WriteInteger(m_Sprite1);
            Snapshot.WriteFloat(m_Timer);
            Snapshot.WriteBoolean(m_Dead);
            Snapshot.WriteBoolean(m_Burning);
            Snapshot.WriteInteger((int)m_Type);
            Snapshot.WriteBoolean(m_Fumes);
            Snapshot.WriteInteger(m_FumeSprite);
            Snapshot.WriteInteger((int)m_Flying);
            Snapshot.WriteFloat(m_FlyTime);
        }

        protected override void OnReadSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.ReadFloat(out m_fX);
            Snapshot.ReadFloat(out m_fY);
            Snapshot.ReadInteger(out m_iX);
            Snapshot.ReadInteger(out m_iY);
            Snapshot.ReadInteger(out m_BlockX);
            Snapshot.ReadInteger(out m_BlockY);
            Snapshot.ReadInteger(out m_Sprite);
            Snapshot.ReadInteger(out m_Sprite0);
            Snapshot.ReadInteger(out m_Sprite1);
            Snapshot.ReadFloat(out m_Timer);
            Snapshot.ReadBoolean(out m_Dead);
            Snapshot.ReadBoolean(out m_Burning);
            int itemType = 0;
            Snapshot.ReadInteger(out itemType);
            m_Type = (EItemType)itemType;
            Snapshot.ReadBoolean(out m_Fumes);
            Snapshot.ReadInteger(out m_FumeSprite);
            int flying = 0;
            Snapshot.ReadInteger(out flying);
            m_Flying = (EItemFlying)flying;
            Snapshot.ReadFloat(out m_FlyTime);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Create the specified amount of items in an arena in the specified EItemPlace.
        /// Returns whether at least one item was created.
        /// </summary>
        public static bool CreateItems(
            CArena pArena,
            EItemPlace ItemPlace,
            int NumberOfItemBombs,
            int NumberOfItemFlames,
            int NumberOfItemRollers,
            int NumberOfItemKicks,
            int NumberOfItemSkulls,
            int NumberOfItemThrow,
            int NumberOfItemPunch,
            int NumberOfItemRemote,
            int NumberOfItemShield,
            int NumberOfItemStrongWeak)
        {
            bool Fumes   = false;
            bool Created = false;

            // This array tells if it is allowed to create an item at this place
            bool[,] PossibleGrid = new bool[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];

            if (ItemPlace == EItemPlace.ITEMPLACE_SOFTWALLS)
            {
                for (int X = 0; X < Globals.ARENA_WIDTH; X++)
                    for (int Y = 0; Y < Globals.ARENA_HEIGHT; Y++)
                        PossibleGrid[X, Y] = pArena.IsSoftWall(X, Y);

                Fumes = false;
            }
            else if (ItemPlace == EItemPlace.ITEMPLACE_FLOOR)
            {
                for (int X = 0; X < Globals.ARENA_WIDTH; X++)
                {
                    for (int Y = 0; Y < Globals.ARENA_HEIGHT; Y++)
                    {
                        PossibleGrid[X, Y] = !pArena.IsWall(X, Y) &&
                                             !pArena.IsItem(X, Y) &&
                                             !pArena.IsAliveBomber(X, Y) &&
                                             !pArena.IsBomb(X, Y) &&
                                             !pArena.IsFlame(X, Y);
                    }
                }

                Fumes = true;
            }

            // Build list of possible positions
            var Possible = new List<(int X, int Y)>();

            for (int X = 0; X < Globals.ARENA_WIDTH; X++)
                for (int Y = 0; Y < Globals.ARENA_HEIGHT; Y++)
                    if (PossibleGrid[X, Y])
                        Possible.Add((X, Y));

            int CountPossible = Possible.Count;

            if (CountPossible > 0)
            {
                // Reduce number of items to create until it's possible to create them all
                while (NumberOfItemBombs +
                       NumberOfItemFlames +
                       NumberOfItemRollers +
                       NumberOfItemKicks +
                       NumberOfItemSkulls +
                       NumberOfItemThrow +
                       NumberOfItemPunch +
                       NumberOfItemRemote +
                       NumberOfItemShield +
                       NumberOfItemStrongWeak > CountPossible)
                {
                    switch (CRandom.Random((int)EItemType.NUMBER_OF_ITEMS))
                    {
                        case 0: if (NumberOfItemBombs      > 0) NumberOfItemBombs--;      break;
                        case 1: if (NumberOfItemFlames     > 0) NumberOfItemFlames--;     break;
                        case 2: if (NumberOfItemRollers    > 0) NumberOfItemRollers--;    break;
                        case 3: if (NumberOfItemKicks      > 0) NumberOfItemKicks--;      break;
                        case 4: if (NumberOfItemSkulls     > 0) NumberOfItemSkulls--;     break;
                        case 5: if (NumberOfItemThrow      > 0) NumberOfItemThrow--;      break;
                        case 6: if (NumberOfItemPunch      > 0) NumberOfItemPunch--;      break;
                        case 7: if (NumberOfItemRemote     > 0) NumberOfItemRemote--;     break;
                        case 8: if (NumberOfItemShield     > 0) NumberOfItemShield--;     break;
                        case 9: if (NumberOfItemStrongWeak > 0) NumberOfItemStrongWeak--; break;
                    }
                }

                // While there are still items to create
                while (NumberOfItemBombs      > 0 ||
                       NumberOfItemFlames     > 0 ||
                       NumberOfItemRollers    > 0 ||
                       NumberOfItemKicks      > 0 ||
                       NumberOfItemSkulls     > 0 ||
                       NumberOfItemThrow      > 0 ||
                       NumberOfItemPunch      > 0 ||
                       NumberOfItemRemote     > 0 ||
                       NumberOfItemShield     > 0 ||
                       NumberOfItemStrongWeak > 0)
                {
                    EItemType Type = EItemType.ITEM_NONE;

                    if      (NumberOfItemBombs      > 0) { Type = EItemType.ITEM_BOMB;       NumberOfItemBombs--;      }
                    else if (NumberOfItemFlames     > 0) { Type = EItemType.ITEM_FLAME;      NumberOfItemFlames--;     }
                    else if (NumberOfItemRollers    > 0) { Type = EItemType.ITEM_ROLLER;     NumberOfItemRollers--;    }
                    else if (NumberOfItemKicks      > 0) { Type = EItemType.ITEM_KICK;       NumberOfItemKicks--;      }
                    else if (NumberOfItemSkulls     > 0) { Type = EItemType.ITEM_SKULL;      NumberOfItemSkulls--;     }
                    else if (NumberOfItemThrow      > 0) { Type = EItemType.ITEM_THROW;      NumberOfItemThrow--;      }
                    else if (NumberOfItemPunch      > 0) { Type = EItemType.ITEM_PUNCH;      NumberOfItemPunch--;      }
                    else if (NumberOfItemRemote     > 0) { Type = EItemType.ITEM_REMOTE;     NumberOfItemRemote--;     }
                    else if (NumberOfItemShield     > 0) { Type = EItemType.ITEM_SHIELD;     NumberOfItemShield--;     }
                    else if (NumberOfItemStrongWeak > 0) { Type = EItemType.ITEM_STRONGWEAK; NumberOfItemStrongWeak--; }

                    // Try a random index in the possible places array
                    int Index = CRandom.Random(CountPossible);

                    Debug.Assert(Type != EItemType.ITEM_NONE);

                    pArena.NewItem(Possible[Index].X, Possible[Index].Y, Type, Fumes, false);

                    Created = true;

                    // Remove used position by overwriting it with the last entry
                    Possible[Index] = Possible[CountPossible - 1];
                    CountPossible--;
                }
            }

            return Created;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Return the block position X of the item.</summary>
        public int GetBlockX() { return m_BlockX; }

        /// <summary>Return the block position Y of the item.</summary>
        public int GetBlockY() { return m_BlockY; }

        /// <summary>Return whether the item is burning.</summary>
        public bool IsBurning() { return m_Burning; }

        /// <summary>Return the type of the item.</summary>
        public EItemType GetType() { return m_Type; }

        /// <summary>Return whether the item is currently flying.</summary>
        public bool IsFlying() { return m_Flying != EItemFlying.ITEMFLYING_NONE; }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
