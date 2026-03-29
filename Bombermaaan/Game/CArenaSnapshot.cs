/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2010 Markus Drescher
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
 *  \file CArenaSnapshot.cs
 *  \brief Arena snapshot for network exchange
 */

using System;
using System.Diagnostics;

namespace Bombermaaan
{
    /// <summary>
    /// Serialises/deserialises arena state into a fixed-size byte buffer.
    /// This is a direct port of the C++ CArenaSnapshot POD class.
    /// </summary>
    public class CArenaSnapshot
    {
        public const int ARENA_SNAPSHOT_SIZE = 32768;

        private byte[] m_Buffer = new byte[ARENA_SNAPSHOT_SIZE];
        private int m_Position;

        // ---------------------------------------------------------------

        public void Create()
        {
            // Nothing to initialise
        }

        public void Destroy()
        {
            // Nothing to clean up
        }

        /// <summary>Rewind the read/write cursor to the beginning of the buffer.</summary>
        public void Begin()
        {
            m_Position = 0;
        }

        // ---------------------------------------------------------------
        // Read helpers
        // ---------------------------------------------------------------

        public void ReadInteger(out int value)
        {
            Debug.Assert(m_Position + sizeof(int) < ARENA_SNAPSHOT_SIZE);
            value = BitConverter.ToInt32(m_Buffer, m_Position);
            m_Position += sizeof(int);
        }

        public void ReadFloat(out float value)
        {
            Debug.Assert(m_Position + sizeof(float) < ARENA_SNAPSHOT_SIZE);
            value = BitConverter.ToSingle(m_Buffer, m_Position);
            m_Position += sizeof(float);
        }

        public void ReadBoolean(out bool value)
        {
            Debug.Assert(m_Position + 1 < ARENA_SNAPSHOT_SIZE);
            value = m_Buffer[m_Position] != 0;
            m_Position += 1;
        }

        // ---------------------------------------------------------------
        // Write helpers
        // ---------------------------------------------------------------

        public void WriteInteger(int value)
        {
            Debug.Assert(m_Position + sizeof(int) < ARENA_SNAPSHOT_SIZE);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, m_Buffer, m_Position, sizeof(int));
            m_Position += sizeof(int);
        }

        public void WriteFloat(float value)
        {
            Debug.Assert(m_Position + sizeof(float) < ARENA_SNAPSHOT_SIZE);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, m_Buffer, m_Position, sizeof(float));
            m_Position += sizeof(float);
        }

        public void WriteBoolean(bool value)
        {
            Debug.Assert(m_Position + 1 < ARENA_SNAPSHOT_SIZE);
            m_Buffer[m_Position] = value ? (byte)1 : (byte)0;
            m_Position += 1;
        }

        // ---------------------------------------------------------------
        // Network serialization helpers
        // ---------------------------------------------------------------

        /// <summary>Returns a copy of the raw buffer for network transmission.</summary>
        public byte[] GetBuffer()
        {
            byte[] copy = new byte[ARENA_SNAPSHOT_SIZE];
            Array.Copy(m_Buffer, copy, ARENA_SNAPSHOT_SIZE);
            return copy;
        }

        /// <summary>Overwrites the internal buffer from received network data.</summary>
        public void SetBuffer(byte[] src)
        {
            Debug.Assert(src.Length == ARENA_SNAPSHOT_SIZE);
            Array.Copy(src, m_Buffer, ARENA_SNAPSHOT_SIZE);
        }
    }
}
