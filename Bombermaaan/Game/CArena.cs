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
 *  \file CArena.cs
 *  \brief Arena during a match
 */

using System.Diagnostics;

namespace Bombermaaan
{
    // BlockHas flags: describe exactly what can be seen on a specific block.
    // Example: soft wall with bomb item below -> FLOOR | WALL | WALLSOFT | ITEM | ITEMBOMB.
    public static class BlockHas
    {
        public const int NONE                = (1 <<  0);
        public const int FLOOR               = (1 <<  1);
        public const int WALL                = (1 <<  2);
        public const int WALLSOFT            = (1 <<  3);
        public const int WALLHARD            = (1 <<  4);
        public const int WALLFALLING         = (1 <<  5);
        public const int WALLBURNING         = (1 <<  6);
        public const int ITEM                = (1 <<  7);
        public const int ITEMBOMB            = (1 <<  8);
        public const int ITEMFLAME           = (1 <<  9);
        public const int ITEMROLLER          = (1 << 10);
        public const int ITEMSKULL           = (1 << 11);
        public const int ITEMKICK            = (1 << 12);
        public const int ITEMBURNING         = (1 << 13);
        public const int BOMB                = (1 << 14);
        public const int FLAME               = (1 << 15);
        public const int EXPLOSION           = (1 << 16);
        public const int BOMBER              = (1 << 17);
        public const int BOMBERALIVE         = (1 << 18);
        public const int BOMBERDYING         = (1 << 19);
        public const int FLOORWITHMOVEEFFECT = (1 << 20);
        public const int ITEMTHROW           = (1 << 21);
        public const int ITEMPUNCH           = (1 << 22);
        public const int ITEMREMOTE          = (1 << 23);
        public const int ITEMSHIELD          = (1 << 24);
        public const int ITEMSTRONGWEAK      = (1 << 25);
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public class CArena
    {
        public const int MAX_FLOORS     = Globals.ARENA_WIDTH * Globals.ARENA_HEIGHT;
        public const int MAX_WALLS      = 2 * Globals.ARENA_WIDTH * Globals.ARENA_HEIGHT;
        public const int MAX_BOMBS      = 50;
        public const int MAX_ITEMS      = 50;
        public const int MAX_EXPLOSIONS = 50;
        public const int MAX_BOMBERS    = Globals.MAX_PLAYERS;
        public const int MAX_TEAMS      = Globals.MAX_PLAYERS;

        private const int CHANCE_SOFTWALL  = 50;
        private const int ARENA_POSITION_X = 0;
        private const int ARENA_POSITION_Y = 26;

        private CDisplay?    m_pDisplay;
        private CSound?      m_pSound;
        private COptions?    m_pOptions;
        private CFloor[]     m_Floors;
        private CWall[]      m_Walls;
        private CBomb[]      m_Bombs;
        private CItem[]      m_Items;
        private CExplosion[] m_Explosions;
        private CBomber[]    m_Bombers;
        private CArenaCloser m_ArenaCloser;
        private int[,]       m_BlockHas;
        private int          m_BombsInUse;
        private bool         m_Prediction;

        //******************************************************************************************************************************

        public CArena()
        {
            m_pDisplay   = null;
            m_pSound     = null;
            m_pOptions   = null;
            m_BombsInUse = 0;

            m_Floors     = new CFloor[MAX_FLOORS];
            m_Walls      = new CWall[MAX_WALLS];
            m_Bombs      = new CBomb[MAX_BOMBS];
            m_Items      = new CItem[MAX_ITEMS];
            m_Explosions = new CExplosion[MAX_EXPLOSIONS];
            m_Bombers    = new CBomber[MAX_BOMBERS];
            m_ArenaCloser = new CArenaCloser();
            m_BlockHas   = new int[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];

            for (int i = 0; i < MAX_FLOORS;     i++) m_Floors[i]     = new CFloor();
            for (int i = 0; i < MAX_WALLS;      i++) m_Walls[i]      = new CWall();
            for (int i = 0; i < MAX_BOMBS;      i++) m_Bombs[i]      = new CBomb();
            for (int i = 0; i < MAX_ITEMS;      i++) m_Items[i]      = new CItem();
            for (int i = 0; i < MAX_EXPLOSIONS; i++) m_Explosions[i] = new CExplosion();
            for (int i = 0; i < MAX_BOMBERS;    i++) m_Bombers[i]    = new CBomber();

            m_ArenaCloser.SetArena(this);

            for (int i = 0; i < MaxExplosions(); i++) GetExplosion(i).SetArena(this);
            for (int i = 0; i < MaxBombs();      i++) GetBomb(i).SetArena(this);
            for (int i = 0; i < MaxBombers();    i++) GetBomber(i).SetArena(this);
            for (int i = 0; i < MaxWalls();      i++) GetWall(i).SetArena(this);
            for (int i = 0; i < MaxFloors();     i++) GetFloor(i).SetArena(this);
            for (int i = 0; i < MaxItems();      i++) GetItem(i).SetArena(this);
        }

        //******************************************************************************************************************************

        public void CopyFrom(CArena Arena)
        {
            m_pDisplay = Arena.m_pDisplay;
            m_pSound   = Arena.m_pSound;
            m_pOptions = Arena.m_pOptions;

            for (int i = 0; i < MaxFloors();     i++) { m_Floors[i].CopyFrom(Arena.m_Floors[i]);         m_Floors[i].SetArena(this); }
            for (int i = 0; i < MaxWalls();      i++) { m_Walls[i].CopyFrom(Arena.m_Walls[i]);           m_Walls[i].SetArena(this); }
            for (int i = 0; i < MaxBombs();      i++) { m_Bombs[i].CopyFrom(Arena.m_Bombs[i]);           m_Bombs[i].SetArena(this); }
            for (int i = 0; i < MaxItems();      i++) { m_Items[i].CopyFrom(Arena.m_Items[i]);           m_Items[i].SetArena(this); }
            for (int i = 0; i < MaxExplosions(); i++) { m_Explosions[i].CopyFrom(Arena.m_Explosions[i]); m_Explosions[i].SetArena(this); }
            for (int i = 0; i < MaxBombers();    i++) { m_Bombers[i].CopyFrom(Arena.m_Bombers[i]);       m_Bombers[i].SetArena(this); }

            m_ArenaCloser.CopyFrom(Arena.m_ArenaCloser);
            m_ArenaCloser.SetArena(this);

            for (int bx = 0; bx < Globals.ARENA_WIDTH; bx++)
                for (int by = 0; by < Globals.ARENA_HEIGHT; by++)
                    m_BlockHas[bx, by] = Arena.m_BlockHas[bx, by];

            m_BombsInUse = Arena.m_BombsInUse;
            m_Prediction = Arena.m_Prediction;
        }

        //******************************************************************************************************************************

        public void Create()
        {
            m_Prediction = false;
            m_ArenaCloser.Create();

            for (int X = 0; X < Globals.ARENA_WIDTH; X++)
            {
                for (int Y = 0; Y < Globals.ARENA_HEIGHT; Y++)
                {
                    NewFloor(X, Y, m_pOptions!.GetBlockType(X, Y));

                    switch (m_pOptions.GetBlockType(X, Y))
                    {
                        case EBlockType.BLOCKTYPE_HARDWALL:     NewWall(X, Y, EWallType.WALL_HARD);  break;
                        case EBlockType.BLOCKTYPE_SOFTWALL:     NewWall(X, Y, EWallType.WALL_SOFT);  break;
                        case EBlockType.BLOCKTYPE_RANDOM:
                            if (CRandom.Random(100) < CHANCE_SOFTWALL) NewWall(X, Y, EWallType.WALL_SOFT);
                            break;
                        case EBlockType.BLOCKTYPE_WHITEBOMBER:
                            if (m_pOptions.GetBomberType(Globals.PLAYER_WHITE) != EBomberType.BOMBERTYPE_OFF) NewBomber(X, Y, Globals.PLAYER_WHITE); break;
                        case EBlockType.BLOCKTYPE_BLACKBOMBER:
                            if (m_pOptions.GetBomberType(Globals.PLAYER_BLACK) != EBomberType.BOMBERTYPE_OFF) NewBomber(X, Y, Globals.PLAYER_BLACK); break;
                        case EBlockType.BLOCKTYPE_REDBOMBER:
                            if (m_pOptions.GetBomberType(Globals.PLAYER_RED) != EBomberType.BOMBERTYPE_OFF) NewBomber(X, Y, Globals.PLAYER_RED);     break;
                        case EBlockType.BLOCKTYPE_BLUEBOMBER:
                            if (m_pOptions.GetBomberType(Globals.PLAYER_BLUE) != EBomberType.BOMBERTYPE_OFF) NewBomber(X, Y, Globals.PLAYER_BLUE);   break;
                        case EBlockType.BLOCKTYPE_GREENBOMBER:
                            if (m_pOptions.GetBomberType(Globals.PLAYER_GREEN) != EBomberType.BOMBERTYPE_OFF) NewBomber(X, Y, Globals.PLAYER_GREEN); break;
                        case EBlockType.BLOCKTYPE_ITEM_BOMB:       NewItem(X, Y, EItemType.ITEM_BOMB,      false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_FLAME:      NewItem(X, Y, EItemType.ITEM_FLAME,     false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_KICK:       NewItem(X, Y, EItemType.ITEM_KICK,      false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_PUNCH:      NewItem(X, Y, EItemType.ITEM_PUNCH,     false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_ROLLER:     NewItem(X, Y, EItemType.ITEM_ROLLER,    false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_THROW:      NewItem(X, Y, EItemType.ITEM_THROW,     false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_REMOTES:    NewItem(X, Y, EItemType.ITEM_REMOTE,    false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_SKULL:      NewItem(X, Y, EItemType.ITEM_SKULL,     false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_SHIELD:     NewItem(X, Y, EItemType.ITEM_SHIELD,    false, false); break;
                        case EBlockType.BLOCKTYPE_ITEM_STRONGWEAK: NewItem(X, Y, EItemType.ITEM_STRONGWEAK,false, false); break;
                        default: break;
                    }
                }
            }

            UpdateView();

            CItem.CreateItems(this, EItemPlace.ITEMPLACE_SOFTWALLS,
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_BOMB),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_FLAME),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_ROLLER),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_KICK),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_SKULL),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_THROW),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_PUNCH),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_REMOTE),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_SHIELD),
                m_pOptions.GetNumberOfItemsInWalls(EItemType.ITEM_STRONGWEAK));

            UpdateView();
        }

        //******************************************************************************************************************************

        public void Destroy()
        {
            m_ArenaCloser.Destroy();
            for (int i = 0; i < MaxFloors();     i++) m_Floors[i].Destroy();
            for (int i = 0; i < MaxWalls();      i++) m_Walls[i].Destroy();
            for (int i = 0; i < MaxBombs();      i++) m_Bombs[i].Destroy();
            for (int i = 0; i < MaxItems();      i++) m_Items[i].Destroy();
            for (int i = 0; i < MaxExplosions(); i++) m_Explosions[i].Destroy();
            for (int i = 0; i < MaxBombers();    i++)
            {
                m_Bombers[i].Destroy();
                m_Bombers[i].ResetHasExisted();
            }
        }

        //******************************************************************************************************************************

        private void ClearBlock(int X, int Y)
        {
            Debug.Assert(X >= 0 && X < Globals.ARENA_WIDTH);
            Debug.Assert(Y >= 0 && Y < Globals.ARENA_HEIGHT);

            for (int Index = 0; Index < MaxWalls(); Index++)
                if (GetWall(Index).GetBlockX() == X && GetWall(Index).GetBlockY() == Y) { DeleteWall(Index); break; }

            for (int Index = 0; Index < MaxItems(); Index++)
                if (GetItem(Index).GetBlockX() == X && GetItem(Index).GetBlockY() == Y) { DeleteItem(Index); break; }
        }

        //******************************************************************************************************************************

        private void UpdateElements(float DeltaTime)
        {
            for (int Index = 0; Index < MaxExplosions(); Index++)
                if (GetExplosion(Index).Exist() && GetExplosion(Index).Update(DeltaTime) && !m_Prediction) DeleteExplosion(Index);

            for (int Index = 0; Index < MaxBombs(); Index++)
                if (GetBomb(Index).Exist() && GetBomb(Index).Update(DeltaTime) && !m_Prediction) DeleteBomb(Index);

            for (int Index = 0; Index < MaxBombers(); Index++)
                if (GetBomber(Index).Exist() && GetBomber(Index).Update(DeltaTime) && !m_Prediction) DeleteBomber(Index);

            for (int Index = 0; Index < MaxWalls(); Index++)
                if (GetWall(Index).Exist() && GetWall(Index).Update(DeltaTime) && !m_Prediction) DeleteWall(Index);

            for (int Index = 0; Index < MaxFloors(); Index++)
                if (GetFloor(Index).Exist() && GetFloor(Index).Update(DeltaTime) && !m_Prediction) DeleteFloor(Index);

            for (int Index = 0; Index < MaxItems(); Index++)
                if (GetItem(Index).Exist() && GetItem(Index).Update(DeltaTime) && !m_Prediction) DeleteItem(Index);
        }

        //******************************************************************************************************************************

        public void UpdateSingleBomber(int Player, float DeltaTime)
        {
            if (GetBomber(Player).Exist() && GetBomber(Player).Update(DeltaTime)) DeleteBomber(Player);
            UpdateView();
        }

        //******************************************************************************************************************************

        private void UpdateView()
        {
            for (int bx = 0; bx < Globals.ARENA_WIDTH; bx++)
                for (int by = 0; by < Globals.ARENA_HEIGHT; by++)
                    m_BlockHas[bx, by] = 0;

            // Explosions & flames
            for (int i = 0; i < MaxExplosions(); i++)
            {
                if (GetExplosion(i).Exist())
                {
                    CExplosion ex = GetExplosion(i);
                    SetBlockHas(ex.GetBlockX(), ex.GetBlockY(), BlockHas.EXPLOSION);
                    for (int f = 0; f < ex.GetFlames().Count; f++)
                        SetBlockHas(ex.GetFlames()[f].BlockX, ex.GetFlames()[f].BlockY, BlockHas.FLAME);
                }
            }

            // Bombs
            for (int i = 0; i < MaxBombs(); i++)
                if (GetBomb(i).Exist() && GetBomb(i).IsOnFloor())
                    SetBlockHas(GetBomb(i).GetBlockX(), GetBomb(i).GetBlockY(), BlockHas.BOMB);

            // Bombers
            for (int i = 0; i < MaxBombers(); i++)
            {
                if (GetBomber(i).Exist() && !GetBomber(i).IsDead())
                {
                    int bx = GetBomber(i).GetBlockX(), by = GetBomber(i).GetBlockY();
                    SetBlockHas(bx, by, BlockHas.BOMBER);
                    if (GetBomber(i).IsAlive())     SetBlockHas(bx, by, BlockHas.BOMBERALIVE);
                    else if (GetBomber(i).IsDying()) SetBlockHas(bx, by, BlockHas.BOMBERDYING);
                }
            }

            // Walls
            for (int i = 0; i < MaxWalls(); i++)
            {
                if (GetWall(i).Exist())
                {
                    int bx = GetWall(i).GetBlockX(), by = GetWall(i).GetBlockY();
                    SetBlockHas(bx, by, BlockHas.WALL);
                    if (!GetWall(i).IsBurning())
                    {
                        switch (GetWall(i).GetType())
                        {
                            case EWallType.WALL_HARD:    SetBlockHas(bx, by, BlockHas.WALLHARD);    break;
                            case EWallType.WALL_SOFT:    SetBlockHas(bx, by, BlockHas.WALLSOFT);    break;
                            case EWallType.WALL_FALLING: SetBlockHas(bx, by, BlockHas.WALLFALLING); break;
                        }
                    }
                    else SetBlockHas(bx, by, BlockHas.WALLBURNING);
                }
            }

            // Floors
            for (int i = 0; i < MaxFloors(); i++)
            {
                if (GetFloor(i).Exist())
                {
                    SetBlockHas(GetFloor(i).GetBlockX(), GetFloor(i).GetBlockY(), BlockHas.FLOOR);
                    if (GetFloor(i).HasAction())
                        SetBlockHas(GetFloor(i).GetBlockX(), GetFloor(i).GetBlockY(), BlockHas.FLOORWITHMOVEEFFECT);
                }
            }

            // Items
            for (int i = 0; i < MaxItems(); i++)
            {
                if (GetItem(i).Exist() && !GetItem(i).IsFlying())
                {
                    int bx = GetItem(i).GetBlockX(), by = GetItem(i).GetBlockY();
                    SetBlockHas(bx, by, BlockHas.ITEM);
                    if (!GetItem(i).IsBurning())
                    {
                        switch (GetItem(i).GetType())
                        {
                            case EItemType.ITEM_BOMB:       SetBlockHas(bx, by, BlockHas.ITEMBOMB);       break;
                            case EItemType.ITEM_FLAME:      SetBlockHas(bx, by, BlockHas.ITEMFLAME);      break;
                            case EItemType.ITEM_ROLLER:     SetBlockHas(bx, by, BlockHas.ITEMROLLER);     break;
                            case EItemType.ITEM_KICK:       SetBlockHas(bx, by, BlockHas.ITEMKICK);       break;
                            case EItemType.ITEM_SKULL:      SetBlockHas(bx, by, BlockHas.ITEMSKULL);      break;
                            case EItemType.ITEM_THROW:      SetBlockHas(bx, by, BlockHas.ITEMTHROW);      break;
                            case EItemType.ITEM_PUNCH:      SetBlockHas(bx, by, BlockHas.ITEMPUNCH);      break;
                            case EItemType.ITEM_REMOTE:     SetBlockHas(bx, by, BlockHas.ITEMREMOTE);     break;
                            case EItemType.ITEM_SHIELD:     SetBlockHas(bx, by, BlockHas.ITEMSHIELD);     break;
                            case EItemType.ITEM_STRONGWEAK: SetBlockHas(bx, by, BlockHas.ITEMSTRONGWEAK); break;
                            default: break;
                        }
                    }
                    else SetBlockHas(bx, by, BlockHas.ITEMBURNING);
                }
            }
        }

        //******************************************************************************************************************************

        public void Update(float DeltaTime)
        {
            m_ArenaCloser.Update(DeltaTime);
            UpdateElements(DeltaTime);
            UpdateView();
        }

        //******************************************************************************************************************************

        public void Display()
        {
            if (m_pDisplay != null)
                m_pDisplay.SetOrigin(ARENA_POSITION_X, ARENA_POSITION_Y);

            for (int i = 0; i < MaxExplosions(); i++) if (GetExplosion(i).Exist()) GetExplosion(i).Display();
            for (int i = 0; i < MaxBombs();      i++) if (GetBomb(i).Exist())      GetBomb(i).Display();
            for (int i = 0; i < MaxBombers();    i++) if (GetBomber(i).Exist())    GetBomber(i).Display();
            for (int i = 0; i < MaxWalls();      i++) if (GetWall(i).Exist())      GetWall(i).Display();
            for (int i = 0; i < MaxFloors();     i++) if (GetFloor(i).Exist())     GetFloor(i).Display();
            for (int i = 0; i < MaxItems();      i++) if (GetItem(i).Exist())      GetItem(i).Display();
        }

        //******************************************************************************************************************************

        public void WriteSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.Begin();
            for (int i = 0; i < MaxFloors();     i++) GetFloor(i).WriteSnapshot(Snapshot);
            for (int i = 0; i < MaxWalls();      i++) GetWall(i).WriteSnapshot(Snapshot);
            for (int i = 0; i < MaxBombs();      i++) GetBomb(i).WriteSnapshot(Snapshot);
            for (int i = 0; i < MaxItems();      i++) GetItem(i).WriteSnapshot(Snapshot);
            for (int i = 0; i < MaxExplosions(); i++) GetExplosion(i).WriteSnapshot(Snapshot);
            for (int i = 0; i < MaxBombers();    i++) GetBomber(i).WriteSnapshot(Snapshot);
            m_ArenaCloser.WriteSnapshot(Snapshot);
            for (int bx = 0; bx < Globals.ARENA_WIDTH; bx++)
                for (int by = 0; by < Globals.ARENA_HEIGHT; by++)
                    Snapshot.WriteInteger(m_BlockHas[bx, by]);
            Snapshot.WriteBoolean(m_Prediction);
            Snapshot.WriteInteger(m_BombsInUse);
        }

        public void ReadSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.Begin();
            for (int i = 0; i < MaxFloors();     i++) GetFloor(i).ReadSnapshot(Snapshot);
            for (int i = 0; i < MaxWalls();      i++) GetWall(i).ReadSnapshot(Snapshot);
            for (int i = 0; i < MaxBombs();      i++) GetBomb(i).ReadSnapshot(Snapshot);
            for (int i = 0; i < MaxItems();      i++) GetItem(i).ReadSnapshot(Snapshot);
            for (int i = 0; i < MaxExplosions(); i++) GetExplosion(i).ReadSnapshot(Snapshot);
            for (int i = 0; i < MaxBombers();    i++) GetBomber(i).ReadSnapshot(Snapshot);
            m_ArenaCloser.ReadSnapshot(Snapshot);
            for (int bx = 0; bx < Globals.ARENA_WIDTH; bx++)
                for (int by = 0; by < Globals.ARENA_HEIGHT; by++)
                    Snapshot.ReadInteger(out m_BlockHas[bx, by]);
            Snapshot.ReadBoolean(out m_Prediction);
            Snapshot.ReadInteger(out m_BombsInUse);
            UpdateView();
        }

        //******************************************************************************************************************************
        // New* factories

        public void NewFloor(int BlockX, int BlockY, EBlockType BlockType)
        {
            Debug.Assert(!m_Prediction);
            EFloorAction action = EFloorAction.FLOORACTION_NONE;
            switch (BlockType)
            {
                case EBlockType.BLOCKTYPE_MOVEBOMB_RIGHT: action = EFloorAction.FLOORACTION_MOVEBOMB_RIGHT; break;
                case EBlockType.BLOCKTYPE_MOVEBOMB_DOWN:  action = EFloorAction.FLOORACTION_MOVEBOMB_DOWN;  break;
                case EBlockType.BLOCKTYPE_MOVEBOMB_LEFT:  action = EFloorAction.FLOORACTION_MOVEBOMB_LEFT;  break;
                case EBlockType.BLOCKTYPE_MOVEBOMB_UP:    action = EFloorAction.FLOORACTION_MOVEBOMB_UP;    break;
                default: break;
            }
            for (int i = 0; i < MaxFloors(); i++)
            {
                if (!m_Floors[i].Exist())
                {
                    m_Floors[i].SetArena(this); m_Floors[i].SetDisplay(m_pDisplay); m_Floors[i].SetSound(m_pSound);
                    m_Floors[i].Create(BlockX, BlockY, action); return;
                }
            }
            Debug.Assert(false);
        }

        public void NewWall(int BlockX, int BlockY, EWallType Type)
        {
            Debug.Assert(!m_Prediction);
            for (int i = 0; i < MaxWalls(); i++)
            {
                if (!m_Walls[i].Exist())
                {
                    m_Walls[i].SetArena(this); m_Walls[i].SetDisplay(m_pDisplay); m_Walls[i].SetSound(m_pSound);
                    m_Walls[i].Create(BlockX, BlockY, Type); return;
                }
            }
            Debug.Assert(false);
        }

        public void NewBomb(int BlockX, int BlockY, int FlameSize, float TimeLeft, int OwnerPlayer)
        {
            Debug.Assert(!m_Prediction);
            Debug.Assert(m_BombsInUse < MaxBombs());
            for (int i = 0; i < MaxBombs(); i++)
            {
                if (!m_Bombs[i].Exist())
                {
                    m_Bombs[i].SetArena(this); m_Bombs[i].SetDisplay(m_pDisplay); m_Bombs[i].SetSound(m_pSound);
                    m_Bombs[i].Create(BlockX, BlockY, FlameSize, TimeLeft, OwnerPlayer); m_BombsInUse++; return;
                }
            }
            Debug.Assert(false);
        }

        public void NewExplosion(int BlockX, int BlockY, int FlameSize)
        {
            Debug.Assert(!m_Prediction);
            for (int i = 0; i < MaxExplosions(); i++)
            {
                if (!m_Explosions[i].Exist())
                {
                    m_Explosions[i].SetArena(this); m_Explosions[i].SetDisplay(m_pDisplay); m_Explosions[i].SetSound(m_pSound);
                    m_Explosions[i].Create(BlockX, BlockY, FlameSize); return;
                }
            }
            Debug.Assert(false);
        }

        public void NewItem(int BlockX, int BlockY, EItemType Type, bool Fumes, bool FlyingRandom)
        {
            Debug.Assert(!m_Prediction);
            for (int i = 0; i < MaxItems(); i++)
            {
                if (!m_Items[i].Exist())
                {
                    m_Items[i].SetArena(this); m_Items[i].SetDisplay(m_pDisplay); m_Items[i].SetSound(m_pSound);
                    m_Items[i].Create(BlockX, BlockY, Type, Fumes, FlyingRandom);
                    SetBlockHas(BlockX, BlockY, BlockHas.ITEM);
                    switch (Type)
                    {
                        case EItemType.ITEM_BOMB:       SetBlockHas(BlockX, BlockY, BlockHas.ITEMBOMB);       break;
                        case EItemType.ITEM_FLAME:      SetBlockHas(BlockX, BlockY, BlockHas.ITEMFLAME);      break;
                        case EItemType.ITEM_ROLLER:     SetBlockHas(BlockX, BlockY, BlockHas.ITEMROLLER);     break;
                        case EItemType.ITEM_KICK:       SetBlockHas(BlockX, BlockY, BlockHas.ITEMKICK);       break;
                        case EItemType.ITEM_SKULL:      SetBlockHas(BlockX, BlockY, BlockHas.ITEMSKULL);      break;
                        case EItemType.ITEM_REMOTE:     SetBlockHas(BlockX, BlockY, BlockHas.ITEMREMOTE);     break;
                        case EItemType.ITEM_SHIELD:     SetBlockHas(BlockX, BlockY, BlockHas.ITEMSHIELD);     break;
                        case EItemType.ITEM_PUNCH:      SetBlockHas(BlockX, BlockY, BlockHas.ITEMPUNCH);      break;
                        case EItemType.ITEM_THROW:      SetBlockHas(BlockX, BlockY, BlockHas.ITEMTHROW);      break;
                        case EItemType.ITEM_STRONGWEAK: SetBlockHas(BlockX, BlockY, BlockHas.ITEMSTRONGWEAK); break;
                        default: break;
                    }
                    return;
                }
            }
            Debug.Assert(false);
        }

        public void NewBomber(int BlockX, int BlockY, int Player)
        {
            Debug.Assert(!m_Prediction);
            Debug.Assert(Player >= 0 && Player < Globals.MAX_PLAYERS);
            Debug.Assert(!m_Bombers[Player].Exist());
            Debug.Assert(m_pOptions != null);
            m_Bombers[Player].SetArena(this); m_Bombers[Player].SetDisplay(m_pDisplay); m_Bombers[Player].SetSound(m_pSound);
            m_Bombers[Player].Create(BlockX, BlockY, Player, m_pOptions);
        }

        //******************************************************************************************************************************

        public EFloorAction GetFloorAction(int BlockX, int BlockY)
        {
            for (int i = 0; i < MaxFloors(); i++)
            {
                CFloor floor = GetFloor(i);
                if (floor.GetBlockX() == BlockX && floor.GetBlockY() == BlockY)
                    return floor.GetFloorAction();
            }
            Debug.Assert(false);
            return EFloorAction.FLOORACTION_NONE;
        }

        //******************************************************************************************************************************
        // Delete methods

        private void DeleteBomber(int Index)   { Debug.Assert(!m_Prediction); m_Bombers[Index].Destroy(); }
        private void DeleteBomb(int Index)      { Debug.Assert(!m_Prediction); Debug.Assert(m_BombsInUse > 0); m_Bombs[Index].Destroy(); m_BombsInUse--; }
        private void DeleteWall(int Index)      { Debug.Assert(!m_Prediction); m_Walls[Index].Destroy(); }
        private void DeleteItem(int Index)      { Debug.Assert(!m_Prediction); m_Items[Index].Destroy(); }
        private void DeleteExplosion(int Index) { Debug.Assert(!m_Prediction); m_Explosions[Index].Destroy(); }
        private void DeleteFloor(int Index)     { Debug.Assert(!m_Prediction); m_Floors[Index].Destroy(); }

        //******************************************************************************************************************************
        // Accessors / inline equivalents

        public bool IsPrediction() { return m_Prediction; }
        public void SetPrediction(bool Active) { m_Prediction = Active; }
        public CArenaCloser GetArenaCloser() { return m_ArenaCloser; }

        public CBomber    GetBomber   (int Index) { Debug.Assert(Index >= 0 && Index < MAX_BOMBERS);    return m_Bombers[Index]; }
        public CBomb      GetBomb     (int Index) { Debug.Assert(Index >= 0 && Index < MAX_BOMBS);      return m_Bombs[Index]; }
        public CWall      GetWall     (int Index) { Debug.Assert(Index >= 0 && Index < MAX_WALLS);      return m_Walls[Index]; }
        public CItem      GetItem     (int Index) { Debug.Assert(Index >= 0 && Index < MAX_ITEMS);      return m_Items[Index]; }
        public CExplosion GetExplosion(int Index) { Debug.Assert(Index >= 0 && Index < MAX_EXPLOSIONS); return m_Explosions[Index]; }
        public CFloor     GetFloor    (int Index) { Debug.Assert(Index >= 0 && Index < MAX_FLOORS);     return m_Floors[Index]; }

        public int MaxFloors()     { return MAX_FLOORS; }
        public int MaxWalls()      { return MAX_WALLS; }
        public int MaxBombs()      { return MAX_BOMBS; }
        public int MaxItems()      { return MAX_ITEMS; }
        public int MaxExplosions() { return MAX_EXPLOSIONS; }
        public int MaxBombers()    { return MAX_BOMBERS; }
        public int MaxTeams()      { return MAX_TEAMS; }

        /// <summary>Return how many bombs are currently in use</summary>
        public int BombsInUse() { return m_BombsInUse; }

        private bool GetBlockHas(int BlockX, int BlockY, int flags)
        {
            Debug.Assert(BlockX >= 0 && BlockX < Globals.ARENA_WIDTH);
            Debug.Assert(BlockY >= 0 && BlockY < Globals.ARENA_HEIGHT);
            return (m_BlockHas[BlockX, BlockY] & flags) != 0;
        }

        private void SetBlockHas(int BlockX, int BlockY, int flags)
        {
            Debug.Assert(BlockX >= 0 && BlockX < Globals.ARENA_WIDTH);
            Debug.Assert(BlockY >= 0 && BlockY < Globals.ARENA_HEIGHT);
            m_BlockHas[BlockX, BlockY] |= flags;
        }

        //******************************************************************************************************************************

        public void SetDisplay(CDisplay? pDisplay)
        {
            m_pDisplay = pDisplay;
            for (int i = 0; i < MaxFloors();     i++) m_Floors[i].SetDisplay(pDisplay);
            for (int i = 0; i < MaxWalls();      i++) m_Walls[i].SetDisplay(pDisplay);
            for (int i = 0; i < MaxBombs();      i++) m_Bombs[i].SetDisplay(pDisplay);
            for (int i = 0; i < MaxBombers();    i++) m_Bombers[i].SetDisplay(pDisplay);
            for (int i = 0; i < MaxItems();      i++) m_Items[i].SetDisplay(pDisplay);
            for (int i = 0; i < MaxExplosions(); i++) m_Explosions[i].SetDisplay(pDisplay);
        }

        public void SetSound(CSound? pSound)
        {
            m_pSound = pSound;
            for (int i = 0; i < MaxFloors();     i++) m_Floors[i].SetSound(pSound);
            for (int i = 0; i < MaxWalls();      i++) m_Walls[i].SetSound(pSound);
            for (int i = 0; i < MaxBombs();      i++) m_Bombs[i].SetSound(pSound);
            for (int i = 0; i < MaxBombers();    i++) m_Bombers[i].SetSound(pSound);
            for (int i = 0; i < MaxItems();      i++) m_Items[i].SetSound(pSound);
            for (int i = 0; i < MaxExplosions(); i++) m_Explosions[i].SetSound(pSound);
        }

        public void SetOptions(COptions? pOptions) { m_pOptions = pOptions; m_ArenaCloser.SetOptions(pOptions); }

        //******************************************************************************************************************************
        // Block query helpers

        public bool IsWall              (int bx, int by) { return GetBlockHas(bx, by, BlockHas.WALL); }
        public bool IsSoftWall          (int bx, int by) { return GetBlockHas(bx, by, BlockHas.WALLSOFT); }
        public bool IsHardWall          (int bx, int by) { return GetBlockHas(bx, by, BlockHas.WALLHARD); }
        public bool IsFallingWall       (int bx, int by) { return GetBlockHas(bx, by, BlockHas.WALLFALLING); }
        public bool IsBurningWall       (int bx, int by) { return GetBlockHas(bx, by, BlockHas.WALLBURNING); }
        public bool IsItem              (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEM); }
        public bool IsBombItem          (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMBOMB); }
        public bool IsFlameItem         (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMFLAME); }
        public bool IsRollerItem        (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMROLLER); }
        public bool IsKickItem          (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMKICK); }
        public bool IsSkullItem         (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMSKULL); }
        public bool IsThrowItem         (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMTHROW); }
        public bool IsPunchItem         (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMPUNCH); }
        public bool IsRemoteItem        (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMREMOTE); }
        public bool IsBurningItem       (int bx, int by) { return GetBlockHas(bx, by, BlockHas.ITEMBURNING); }
        public bool IsBomb              (int bx, int by) { return GetBlockHas(bx, by, BlockHas.BOMB); }
        public bool IsFlame             (int bx, int by) { return GetBlockHas(bx, by, BlockHas.FLAME); }
        public bool IsExplosion         (int bx, int by) { return GetBlockHas(bx, by, BlockHas.EXPLOSION); }
        public bool IsBomber            (int bx, int by) { return GetBlockHas(bx, by, BlockHas.BOMBER); }
        public bool IsAliveBomber       (int bx, int by) { return GetBlockHas(bx, by, BlockHas.BOMBERALIVE); }
        public bool IsDyingBomber       (int bx, int by) { return GetBlockHas(bx, by, BlockHas.BOMBERDYING); }
        public bool IsFloor             (int bx, int by) { return GetBlockHas(bx, by, BlockHas.FLOOR); }
        public bool IsFloorWithMoveEffect(int bx, int by) { Debug.Assert(IsFloor(bx, by)); return GetBlockHas(bx, by, BlockHas.FLOORWITHMOVEEFFECT); }

        public int ToBlock   (int Position) { return Position / Globals.BLOCK_SIZE; }
        public int ToPosition(int Block)    { return Block * Globals.BLOCK_SIZE; }
    }
}
