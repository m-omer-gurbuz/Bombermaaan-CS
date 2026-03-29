/************************************************************************************

    Copyright (C) 2016 Billy Araujo
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
 *  \file CNetwork.cs
 *  \brief Network communication (C# port of CNetwork.cpp/h)
 *
 *  SDL_net replaced with System.Net.Sockets (TcpListener / TcpClient).
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum ENetworkMode
    {
        NETWORKMODE_LOCAL,
        NETWORKMODE_SERVER,
        NETWORKMODE_CLIENT
    }

    public enum ESocketType
    {
        SOCKET_SERVER,
        SOCKET_CLIENT
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Manages the network communication</summary>
    public class CNetwork
    {
        private ENetworkMode m_NetworkMode;

        // Server side: listener + accepted client socket
        private TcpListener?      m_Listener;
        private TcpClient?        m_ServerClient;   // accepted connection (server role)

        // Client side: outbound connection
        private TcpClient?        m_Client;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CNetwork()
        {
            m_NetworkMode = ENetworkMode.NETWORKMODE_LOCAL;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public ENetworkMode NetworkMode() => m_NetworkMode;

        public void SetNetworkMode(ENetworkMode networkMode)
        {
            m_NetworkMode = networkMode;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool Connect(string ipAddressString, int port)
        {
            if (m_NetworkMode == ENetworkMode.NETWORKMODE_SERVER)
            {
                try
                {
                    m_Listener = new TcpListener(IPAddress.Any, port);
                    m_Listener.Start();

                    // Wait for the client (blocking, mirrors the C++ while loop)
                    while (true)
                    {
                        if (m_Listener.Pending())
                        {
                            m_ServerClient = m_Listener.AcceptTcpClient();
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    CLog.GetLog().Write($"listen/accept failed: {ex.Message}\n");
                    return false;
                }
            }
            else if (m_NetworkMode == ENetworkMode.NETWORKMODE_CLIENT)
            {
                try
                {
                    m_Client = new TcpClient(ipAddressString, port);
                }
                catch (Exception ex)
                {
                    CLog.GetLog().Write($"connection failed: {ex.Message}\n");
                    return false;
                }
            }

            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool Disconnect()
        {
            if (m_NetworkMode != ENetworkMode.NETWORKMODE_LOCAL)
            {
                m_Client?.Close();
                m_ServerClient?.Close();
                m_Listener?.Stop();
            }

            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Send raw bytes over the specified socket.</summary>
        public bool Send(ESocketType socketType, byte[] buf, int len)
        {
            try
            {
                NetworkStream stream = GetStream(socketType)!;
                stream.Write(buf, 0, len);
                return true;
            }
            catch (Exception ex)
            {
                CLog.GetLog().Write($"send error: {ex.Message}\n");
                return false;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Receive exactly <paramref name="len"/> bytes (blocking).</summary>
        public int Receive(ESocketType socketType, byte[] buf, int len)
        {
            try
            {
                NetworkStream stream = GetStream(socketType)!;
                return stream.Read(buf, 0, len);
            }
            catch (Exception ex)
            {
                CLog.GetLog().Write($"receive error: {ex.Message}\n");
                return -1;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Non-blocking receive: returns 0 immediately if no data is available.</summary>
        public int ReceiveNonBlocking(ESocketType socketType, byte[] buf, int len)
        {
            try
            {
                NetworkStream stream = GetStream(socketType)!;
                if (!stream.DataAvailable)
                    return 0;
                return stream.Read(buf, 0, len);
            }
            catch (Exception ex)
            {
                CLog.GetLog().Write($"receive error: {ex.Message}\n");
                return -1;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool SendCommandChunk(CCommandChunk commandChunk)
        {
            byte[] data = SerializeCommandChunk(commandChunk);

            // Send checksum
            ulong checksum = CheckSum(data);
            byte[] checksumBytes = BitConverter.GetBytes((uint)checksum);
            Send(ESocketType.SOCKET_SERVER, checksumBytes, 4);

            // Send command chunk
            Send(ESocketType.SOCKET_SERVER, data, data.Length);

            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool ReceiveCommandChunk(CCommandChunk commandChunk)
        {
            // Receive checksum
            byte[] checksumBytes = new byte[4];
            int received = 0;
            int remaining = 4;
            do
            {
                int r = Receive(ESocketType.SOCKET_CLIENT, checksumBytes, remaining);
                if (r <= 0) break;
                received  += r;
                remaining -= r;
            } while (remaining > 0);

            uint expectedChecksum = BitConverter.ToUInt32(checksumBytes, 0);

            // Receive command chunk bytes
            byte[] data = new byte[GetCommandChunkSize()];
            received  = 0;
            remaining = data.Length;
            do
            {
                int r = Receive(ESocketType.SOCKET_CLIENT, data, remaining);
                if (r <= 0)
                {
                    CLog.GetLog().Write("receive error: connection closed\n");
                    return false;
                }
                received  += r;
                remaining -= r;
            } while (remaining > 0);

            if (received == data.Length && CheckSum(data) == expectedChecksum)
            {
                DeserializeCommandChunk(commandChunk, data);
                return true;
            }

            return false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool SendSnapshot(CArenaSnapshot snapshot)
        {
            byte[] data = SerializeSnapshot(snapshot);

            // Send checksum
            ulong checksum = CheckSum(data);
            byte[] checksumBytes = BitConverter.GetBytes((uint)checksum);
            Send(ESocketType.SOCKET_CLIENT, checksumBytes, 4);

            // Send snapshot to the client
            return Send(ESocketType.SOCKET_CLIENT, data, data.Length);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool ReceiveSnapshot(CArenaSnapshot snapshot)
        {
            // Receive checksum
            byte[] checksumBytes = new byte[4];
            int remaining = 4;
            int received  = 0;
            do
            {
                int r = Receive(ESocketType.SOCKET_SERVER, checksumBytes, remaining);
                if (r <= 0) break;
                received  += r;
                remaining -= r;
            } while (remaining > 0);

            uint expectedChecksum = BitConverter.ToUInt32(checksumBytes, 0);

            // Receive snapshot bytes
            byte[] data = SerializeSnapshot(snapshot); // use to get size
            int size = data.Length;
            data      = new byte[size];
            received  = 0;
            remaining = size;
            do
            {
                int r = Receive(ESocketType.SOCKET_SERVER, data, remaining);
                if (r <= 0)
                {
                    CLog.GetLog().Write("receive error: connection closed\n");
                    return false;
                }
                received  += r;
                remaining -= r;
            } while (remaining > 0);

            if (received == size && CheckSum(data) == expectedChecksum)
            {
                DeserializeSnapshot(snapshot, data);
                return true;
            }

            return false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>DJB2 hash over raw bytes.</summary>
        public ulong CheckSum(byte[] buf)
        {
            ulong hash = 5381;
            foreach (byte c in buf)
            {
                if (c == 0) break;
                hash = ((hash << 5) + hash) + c; // hash * 33 + c
            }
            return hash;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private NetworkStream? GetStream(ESocketType socketType)
        {
            if (socketType == ESocketType.SOCKET_SERVER)
                return m_Client?.GetStream();
            else
                return m_ServerClient?.GetStream();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Serialization helpers for CCommandChunk
        // Format: NumberOfSteps (int32) + Steps[] (EBomberMove int32, EBomberAction int32, float32) * MAX_STEPS

        private static int GetCommandChunkSize()
        {
            return 4 + CCommandChunk.MAX_STEPS_IN_COMMAND_CHUNK * (4 + 4 + 4);
        }

        private static byte[] SerializeCommandChunk(CCommandChunk chunk)
        {
            byte[] buf = new byte[GetCommandChunkSize()];
            int offset = 0;

            int n = chunk.GetNumberOfSteps();
            Buffer.BlockCopy(BitConverter.GetBytes(n), 0, buf, offset, 4); offset += 4;

            for (int i = 0; i < CCommandChunk.MAX_STEPS_IN_COMMAND_CHUNK; i++)
            {
                int   move   = i < n ? (int)chunk.GetStepMove(i)   : 0;
                int   action = i < n ? (int)chunk.GetStepAction(i) : 0;
                float dur    = i < n ? chunk.GetStepDuration(i)    : 0f;

                Buffer.BlockCopy(BitConverter.GetBytes(move),   0, buf, offset, 4); offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(action), 0, buf, offset, 4); offset += 4;
                Buffer.BlockCopy(BitConverter.GetBytes(dur),    0, buf, offset, 4); offset += 4;
            }

            return buf;
        }

        private static void DeserializeCommandChunk(CCommandChunk chunk, byte[] buf)
        {
            int offset = 0;

            int n = BitConverter.ToInt32(buf, offset); offset += 4;
            chunk.Reset();

            for (int i = 0; i < n && i < CCommandChunk.MAX_STEPS_IN_COMMAND_CHUNK; i++)
            {
                var move   = (EBomberMove)  BitConverter.ToInt32(buf, offset); offset += 4;
                var action = (EBomberAction)BitConverter.ToInt32(buf, offset); offset += 4;
                float dur  = BitConverter.ToSingle(buf, offset);               offset += 4;
                chunk.Store(move, action, dur);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Serialization helpers for CArenaSnapshot: raw buffer copy (mirrors C++ memcpy approach)
        private static byte[] SerializeSnapshot(CArenaSnapshot snapshot)
        {
            return snapshot.GetBuffer();
        }

        private static void DeserializeSnapshot(CArenaSnapshot target, byte[] buf)
        {
            target.SetBuffer(buf);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
