using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading;

namespace AtlasSimulator
{
    public partial class Client : IDisposable
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _charName;

        private Timer _pingTimer;
        private Timer _positionTimer;
        private Timer _chantTimer;

        public const int ServerHeaderReadBufferSize = 3;
        public const int WriterBufferSize = 2048;
        public const int ReadBufferSize = 2048;
        private Socket _tcpSocket;

        private readonly SocketAsyncEventArgs _tcpReceiverSargs;
        private readonly byte[] TcpReadBuffer = new byte[ReadBufferSize];
        
        
        private readonly Queue<(byte[] buffer, int len)> _sendMessageQueue = new Queue<(byte[] buffer, int len)>(128);
        private readonly Semaphore _writeSemaphore = new Semaphore(1,1);
        private readonly object _sendMessageLock = new object();
        
        public Client(string username, string password, string charName)
        {
            _username = username;
            _password = password;
            _charName = charName;
            _tcpReceiverSargs = new SocketAsyncEventArgs();
            _tcpReceiverSargs.Completed += TcpReceiverSargsOnCompleted;
            _tcpReceiverSargs.SetBuffer(TcpReadBuffer, 0, ReadBufferSize);
            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private readonly byte[] _partialMessage = new byte[ReadBufferSize];
        private int _partialWritten;

        private void TcpReceiverSargsOnCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Process(socketAsyncEventArgs);
        }

        private void Process(SocketAsyncEventArgs sargs)
        {
            try
            {
                while (true)
                {
                    var read = sargs.BytesTransferred;

                    if (read == 0)
                    {
                        Console.WriteLine("Read 0");
                        Dispose();
                        return;
                    }
                    
                    var pos = 0;

                    int len;
                    byte msgType;
                    if (_partialWritten > 0)
                    {
                        byte v1, v2;
                        if (_partialWritten == 1)
                        {
                            v1 = _partialMessage[0];
                            v2 = TcpReadBuffer[0];
                            len = (v1 << 8) | v2;

                            var remaining = len + 2;
                            Buffer.BlockCopy(TcpReadBuffer, 0, _partialMessage, 1, remaining);
                            pos = remaining;
                        }
                        else if (_partialWritten == 2)
                        {
                            v1 = _partialMessage[0];
                            v2 = _partialMessage[1];

                            len = (v1 << 8) | v2;

                            var remaining = len + 1;
                            Buffer.BlockCopy(TcpReadBuffer, 0, _partialMessage, 2, remaining);
                            pos = remaining;
                        }
                        else
                        {
                            v1 = _partialMessage[0];
                            v2 = _partialMessage[1];

                            len = (v1 << 8) | v2;

                            var remaining = len - (_partialWritten - 3);
                            Buffer.BlockCopy(TcpReadBuffer, 0, _partialMessage, _partialWritten - 1, remaining);
                            pos = remaining;
                        }

                        msgType = _partialMessage[2];
                        _partialWritten = 0;
                        ProcessReceivedPackage(_partialMessage, 3, len, msgType);

                    }

                    while (pos < read)
                    {
                        if (read - pos < 3)
                        {
                            Buffer.BlockCopy(TcpReadBuffer, pos, _partialMessage, 0, read - pos);
                            _partialWritten = read - pos;
                            break;
                        }

                        var v1 = TcpReadBuffer[pos++];
                        var v2 = TcpReadBuffer[pos++];

                        len = (v1 << 8) | v2;
                        msgType = TcpReadBuffer[pos++];

                        if (read - pos < len)
                        {
                            Buffer.BlockCopy(TcpReadBuffer, pos - 3, _partialMessage, 0, read - pos + 3);
                            _partialWritten = read - pos + 3;
                            break;
                        }

                        ProcessReceivedPackage(TcpReadBuffer, pos, len, msgType);
                        pos += len;
                    }

                    bool asyncReceived;

                    try
                    {
                        asyncReceived = _tcpSocket.ReceiveAsync(_tcpReceiverSargs);
                    }
                    catch (SocketException)
                    {
                        Dispose();
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        Dispose();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unexpected Exception (Process): {0}", ex);
                        Dispose();
                        return;
                    }
                    
                    if (asyncReceived)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Exception (Process2): {0}", e);
                Dispose();
            }
        }

        public static System.Threading.Semaphore ConnectionSlots = new Semaphore(1, 1);

        private bool _hasConnectionSlot;
        public void Login()
        {
            try
            {
                Console.WriteLine($"{_username} logging in");
                _hasConnectionSlot = ConnectionSlots.WaitOne();
                _tcpSocket.BeginConnect(Program.Host, Program.Port, OnConnected, null);
            }
            catch (SocketException)
            {
                Console.WriteLine("Login Failed");
                Dispose();
                return;
            }
        }

        private void OnConnected(IAsyncResult ar)
        {
            try
            {
                _tcpSocket.EndConnect(ar);
            }
            catch (SocketException)
            { 
                
                Console.WriteLine("Login Failed");
                Dispose();
                return;
            }
            
            Connected = true;

            SendCryptKeyRequest();

            bool asyncReceived;

            try
            {
                asyncReceived = _tcpSocket.ReceiveAsync(_tcpReceiverSargs);
            }
            catch (SocketException)
            {
                Dispose();
                return;
            }
            catch (NullReferenceException)
            {
                Dispose();
                return;
            }
            catch (Exception ex)
            {
                Dispose();
                Console.WriteLine("Unexpected Exception (OnConnected): {0}", ex);
                return;
            }
            
            if (!asyncReceived)
            {
                Process(_tcpReceiverSargs);
            }
        }

        private void ProcessReceivedPackage(byte[] buffer, int pos, int bodyLen, byte msgType)
        {
            switch (msgType)
            {
                case 0x22:
                    HandleVersionAndCryptKeyResponse(buffer, pos, bodyLen);
                    break;
                case 0x2A:
                    HandleLoginGrantedResponse(buffer, pos, bodyLen);
                    break;
                case 0x29:
                    HandlePingReply(buffer, pos, bodyLen);
                    break;
                case 0x28:
                    HandleSetSessionId(buffer, pos, bodyLen);
                    break;
                case 0xFE:
                    HandleSetRealm(buffer, pos, bodyLen);
                    break;
                case 0xFD:
                    HandleCharacterOverview(buffer, pos, bodyLen);
                    break;
                case 0xB1:
                    HandleStartArena(buffer, pos, bodyLen);
                    break;
                case 0x2D:
                    HandleGameOpenReply(buffer, pos, bodyLen);
                    break;
                case 0xAD:
                    HandleStatusUpdate(buffer, pos, bodyLen);
                    break;
                case 0x20:
                    HandleSetPlayerPositionAndOid(buffer, pos, bodyLen);
                    break;
                case 0xA9:
                    HandlePlayerPositionUpdate(buffer, pos, bodyLen);
                    break;
                case 0x2B:
                    HandlePlayerInitResponse(buffer, pos, bodyLen);
                    break;
                case 0x4E:
                    HandleControlledHorse(buffer, pos, bodyLen);
                    break;
            }
        }

        public void Send(byte[] buffer, int len)
        {
            try
            {
                if (!_writeSemaphore.WaitOne(0))
                {
                    lock (_sendMessageLock)
                    {
                        _sendMessageQueue.Enqueue((buffer, len));

                        if (!_writeSemaphore.WaitOne(0))
                        {
                            return;
                        }

                        (buffer, len) = _sendMessageQueue.Dequeue();
                    }
                }
                _tcpSocket.BeginSend(buffer, 0, len, SocketFlags.None, OnSent, buffer);
            }
            catch (SocketException)
            {
                Dispose();
            }
            catch (NullReferenceException)
            {
                Dispose();
            }
            catch (Exception ex)
            {
                Dispose();
                Console.WriteLine("Unexpected Exception (Send): {0}", ex);
            }
        }

        private void OnSent(IAsyncResult ar)
        {
            SocketError err;
            try
            {
                _tcpSocket.EndSend(ar, out err);
            }
            catch (SocketException)
            {
                Dispose();
                return;
            }
            catch (NullReferenceException)
            {
                Dispose();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception (OnSent): {0}", ex);
                Dispose();
                return;
            }

            var buffer = (byte[]) ar.AsyncState;
            ArrayPool<byte>.Shared.Return(buffer, true);
            if (err != SocketError.Success)
            {
                var foo = err;
                Dispose();
                return;
            }

            try
            {
                lock (_sendMessageLock)
                {
                    if (_sendMessageQueue.Count == 0)
                    {
                        _writeSemaphore.Release();
                        return;
                    }

                    int len;
                    (buffer, len) = _sendMessageQueue.Dequeue();
                    _tcpSocket.BeginSend(buffer, 0, len, SocketFlags.None, OnSent, buffer);
                }
            }
            catch (Exception ex)
            {
                Dispose();
                Console.WriteLine("Unexpected Exception (OnSent2): {0}", ex);
                return;
            }
        }

        private void Ping(object state)
        {
            if (Connected && LoggedIn)
            {
                SendPingReply();
            }
            else
            {
                _pingTimer.Dispose();
            }
        }
        
        private void PositionTimerCallback(object state)
        {
            if (Connected && LoggedIn)
            {
                SendPositionUpdate();
            }
            else
            {
                _positionTimer.Dispose();
            }
        }

        private bool _disposed;
        
        public void Dispose()
        {
            Console.WriteLine("Disposing");
            if (_hasConnectionSlot)
            {
                ConnectionSlots.Release();
                _hasConnectionSlot = false;
            }
            
            if (!_disposed)
            {
                
                
                _disposed = true;
                
                _pingTimer?.Dispose();
                _positionTimer?.Dispose();
                _chantTimer?.Dispose();
                _tcpSocket?.Dispose();
                _tcpReceiverSargs?.Dispose();
                _writeSemaphore?.Dispose();

                _pingTimer = null;
                _positionTimer = null;
                _chantTimer = null;
                _tcpSocket = null;
                
                Console.WriteLine("Disposed {0}", _charName);
            }
        }
    }
}