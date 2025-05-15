using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClientSimulator
{
    public partial class Client : IDisposable
    {
        public const int WRITTER_BUFFER_SIZE = 1024 * 64;
        public const int READ_BUFFER_SIZE = 1024 * 64;
        private static SemaphoreSlim _connectionSlots = new(100, 100);

        private string _userName;
        private string _password;
        private string _charName;
        private Timer _pingTimer;
        private Timer _udpPingTimer;
        private Timer _positionTimer;
        private Socket _tcpSocket;
        private static Socket _udpSocket;
        private bool _hasConnectionSlot;
        private bool _disposed;

        private byte[] _tcpReadBuffer = new byte[READ_BUFFER_SIZE];
        private SocketAsyncEventArgs _tcpReceiverSocketArgs;
        private Queue<(byte[] buffer, int len)> _tcpSendQueue = new(128);
        private SemaphoreSlim _tcpSendSemaphore = new(1, 1);
        private object _tcpSendLock = new();
        private byte[] _partialTcpMessage = new byte[1024 * 64];
        private int _partialTcpWritten;

        private static byte[] _udpReadBuffer = new byte[READ_BUFFER_SIZE];
        private static SocketAsyncEventArgs _udpReceiverSocketArgs;
        private static Queue<(byte[] buffer, int len)> _udpSendQueue = new(128);
        private static SemaphoreSlim _udpSendSemaphore = new(1, 1);
        private static object _udpSendLock = new();
        private static byte[] _partialUdpMessage = new byte[1024 * 64];
        private static int _partialUdpWritten;

        public Timer ActionTimer { private get; set; }

        static Client()
        {
            _udpReceiverSocketArgs = new SocketAsyncEventArgs();
            _udpReceiverSocketArgs.Completed += UdpReceiverSocketArgsOnCompletion;
            _udpReceiverSocketArgs.SetBuffer(_udpReadBuffer, 0, READ_BUFFER_SIZE);
            _udpReceiverSocketArgs.RemoteEndPoint = Program.SERVER_UDP_ENDPOINT;
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _udpSocket.Bind(new IPEndPoint(IPAddress.Parse(Program.LOCAL_IP), 0));
        }

        public Client(string username, string password, string charName, GameLocation initialGLocation)
        {
            _userName = username;
            _password = password;
            _charName = charName;

            _tcpReceiverSocketArgs = new SocketAsyncEventArgs();
            _tcpReceiverSocketArgs.Completed += TcpReceiverSocketArgsOnCompletion;
            _tcpReceiverSocketArgs.SetBuffer(_tcpReadBuffer, 0, READ_BUFFER_SIZE);
            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ZoneX = initialGLocation.x;
            ZoneY = initialGLocation.y;
            ZoneZ = initialGLocation.z;
        }

        private void TcpReceiverSocketArgsOnCompletion(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            ProcessIn(_tcpReceiverSocketArgs, _tcpSocket, _tcpReadBuffer, _partialTcpMessage, ref _partialTcpWritten, false);
        }

        private static void UdpReceiverSocketArgsOnCompletion(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            // We're using a single listen UDP socket and there's no way to tell clients apart, so we just discard it.
        }

        private void ProcessIn(SocketAsyncEventArgs args, Socket socket, byte[] readBuffer, byte[] partialMessage, ref int partialWritten, bool isUdp)
        {
            int headerSize = isUdp ? 5 : 3;

            try
            {
                while (true)
                {
                    int read = args.BytesTransferred;

                    if (read == 0)
                    {
                        Console.WriteLine("Read 0");
                        Dispose();
                        return;
                    }

                    int position = 0;
                    int length;
                    byte msgType;

                    if (partialWritten > 0)
                    {
                        byte v1, v2;

                        if (partialWritten == 1)
                        {
                            v1 = partialMessage[0];
                            v2 = readBuffer[0];
                            length = (v1 << 8) | v2;

                            int remaining = length + 2;
                            Buffer.BlockCopy(readBuffer, 0, partialMessage, 1, remaining);
                            position = remaining;
                        }
                        else if (partialWritten == 2)
                        {
                            v1 = partialMessage[0];
                            v2 = partialMessage[1];

                            length = (v1 << 8) | v2;

                            int remaining = length + 1;
                            Buffer.BlockCopy(readBuffer, 0, partialMessage, 2, remaining);
                            position = remaining;
                        }
                        else
                        {
                            v1 = partialMessage[0];
                            v2 = partialMessage[1];

                            length = (v1 << 8) | v2;

                            int remaining = length - (partialWritten - headerSize);
                            Buffer.BlockCopy(readBuffer, 0, partialMessage, partialWritten, remaining);
                            position = remaining;
                        }

                        msgType = partialMessage[2];
                        partialWritten = 0;
                        ProcessReceivedPacket(partialMessage, headerSize, length, msgType);
                    }

                    while (position < read)
                    {
                        if (read - position < headerSize)
                        {
                            Buffer.BlockCopy(readBuffer, position, partialMessage, 0, read - position);
                            partialWritten = read - position;
                            break;
                        }

                        byte v1 = readBuffer[position++];
                        byte v2 = readBuffer[position++];
                        length = (v1 << 8) | v2;

                        if (isUdp)
                            position += 2;

                        msgType = readBuffer[position++];

                        if (read - position < length)
                        {
                            try
                            {
                                Buffer.BlockCopy(readBuffer, position - headerSize, partialMessage, 0, read - position + headerSize);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine($"Buffer.BlockCopy failed: {readBuffer} | {position - headerSize} | {partialMessage} | {read - position + headerSize}");
                                Dispose();
                                return;
                            }

                            partialWritten = read - position + headerSize;
                            break;
                        }

                        ProcessReceivedPacket(readBuffer, position, length, msgType);
                        position += length;
                    }

                    bool asyncReceived;

                    try
                    {
                        asyncReceived = socket.ReceiveAsync(args);
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine($"{nameof(ProcessIn)} failed: SocketException");
                        Dispose();
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        Console.WriteLine($"{nameof(ProcessIn)} failed: NullReferenceException");
                        Dispose();
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{nameof(ProcessIn)} failed:\n{e}");
                        Dispose();
                        return;
                    }

                    if (asyncReceived)
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(ProcessIn)} failed:\n{e}");
                Dispose();
            }
        }

        public void Login()
        {
            Console.WriteLine($"{_userName} logging in");

            try
            {
                _connectionSlots.Wait();
                _hasConnectionSlot = true;
                _tcpSocket.BeginConnect(Program.REMOTE_IP, Program.REMOTE_TCP_PORT, OnConnected, null);
            }
            catch (SocketException)
            {
                Console.WriteLine($"{nameof(Login)} failed: SocketException");
                Dispose();
                return;
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"{nameof(Login)} failed: ObjectDisposedException");
                Dispose();
                return;
            }

            // Not working, let's just use the TCP sockets for now.
            // BeginListenUdp();
        }

        private void BeginListenUdp()
        {
            try
            {
                if (!_udpSocket.ReceiveFromAsync(_udpReceiverSocketArgs))
                    UdpReceiverSocketArgsOnCompletion(null, null);
            }
            catch (SocketException)
            {
                Console.WriteLine($"{nameof(BeginListenUdp)} failed: SocketException");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"{nameof(BeginListenUdp)} failed: ObjectDisposedException");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(BeginListenUdp)} failed: {e}");
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
                Console.WriteLine($"{nameof(OnConnected)} failed: SocketException");
                Dispose();
                return;
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"{nameof(OnConnected)} failed: ObjectDisposedException");
                Dispose();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(OnConnected)} failed:\n{e}");
                Dispose();
                return;
            }

            Connected = true;
            SendCryptKeyRequest();
            bool asyncReceived;

            try
            {
                asyncReceived = _tcpSocket.ReceiveAsync(_tcpReceiverSocketArgs);
            }
            catch (SocketException)
            {
                Console.WriteLine($"{nameof(OnConnected)} failed: SocketException");
                Dispose();
                return;
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"{nameof(OnConnected)} failed: ObjectDisposedException");
                Dispose();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(OnConnected)} failed:\n{e}");
                Dispose();
                return;
            }
            
            if (!asyncReceived)
                TcpReceiverSocketArgsOnCompletion(null, null);
        }

        private void ProcessReceivedPacket(byte[] buffer, int position, int length, byte code)
        {
            switch (code)
            {
                case 0x22:
                    HandleVersionAndCryptKeyResponse(buffer, position, length);
                    break;
                case 0x2A:
                    HandleLoginGrantedResponse(buffer, position, length);
                    break;
                case 0x29:
                    HandlePingReply(buffer, position, length);
                    break;
                case 0x28:
                    HandleSetSessionId(buffer, position, length);
                    break;
                case 0xFE:
                    HandleSetRealm(buffer, position, length);
                    break;
                case 0xFD:
                    HandleCharacterOverview(buffer, position, length);
                    break;
                case 0xB1:
                    HandleStartArena(buffer, position, length);
                    break;
                case 0x2D:
                    HandleGameOpenReply(buffer, position, length);
                    break;
                case 0xAD:
                    HandleStatusUpdate(buffer, position, length);
                    break;
                case 0x20:
                    HandleSetPlayerPositionAndOid(buffer, position, length);
                    break;
                case 0xA9:
                    HandlePlayerPositionUpdate(buffer, position, length);
                    break;
                case 0x2B:
                    HandlePlayerInitResponse(buffer, position, length);
                    break;
                case 0x4E:
                    HandleControlledHorse(buffer, position, length);
                    break;
                case 0xD0:
                    HandleLosCheck(buffer, position, length);
                    break;
                case 0x2F:
                    HandleUDPInitReply(buffer, position, length);
                    break;
            }
        }

        public void SendTcp(byte[] buffer, int length)
        {
            try
            {
                if (!_tcpSendSemaphore.Wait(0))
                {
                    lock (_tcpSendLock)
                    {
                        _tcpSendQueue.Enqueue((buffer, length));

                        if (!_tcpSendSemaphore.Wait(0))
                            return;

                        (buffer, length) = _tcpSendQueue.Dequeue();
                    }
                }

                _tcpSocket.BeginSend(buffer, 0, length, SocketFlags.None, OnTcpSent, buffer);
            }
            catch (SocketException)
            {
                Console.WriteLine($"{nameof(SendTcp)} failed: SocketException");
                Dispose();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine($"{nameof(SendTcp)} failed: NullReferenceException");
                Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(SendTcp)} failed:\n{e}");
                Dispose();
            }
        }

        public void SendUdp(byte[] buffer, int length)
        {
            // Not working, let's just use the TCP sockets for now.
            return;

            try
            {
                if (!_udpSendSemaphore.Wait(0))
                {
                    lock (_udpSendLock)
                    {
                        _udpSendQueue.Enqueue((buffer, length));

                        if (!_udpSendSemaphore.Wait(0))
                            return;

                        (buffer, length) = _udpSendQueue.Dequeue();
                    }
                }

                _udpSocket.BeginSendTo(buffer, 0, length, SocketFlags.None, Program.SERVER_UDP_ENDPOINT, OnUdpSent, buffer);
            }
            catch (SocketException)
            {
                Console.WriteLine($"{nameof(SendUdp)} failed: SocketException");
                Dispose();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine($"{nameof(SendUdp)} failed: NullReferenceException");
                Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(SendUdp)} failed:\n{e}");
                Dispose();
            }
        }

        private void OnTcpSent(IAsyncResult result)
        {
            SocketError socketError;

            try
            {
                _tcpSocket.EndSend(result, out socketError);
            }
            catch (SocketException)
            {
                Console.WriteLine($"{nameof(OnTcpSent)} failed: SocketException");
                Dispose();
                return;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine($"{nameof(OnTcpSent)} failed: NullReferenceException");
                Dispose();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(OnTcpSent)} failed:\n{e}");
                Dispose();
                return;
            }

            byte[] buffer = (byte[]) result.AsyncState;
            ArrayPool<byte>.Shared.Return(buffer, true);

            if (socketError != SocketError.Success)
            {
                Console.WriteLine($"{nameof(OnTcpSent)} failed: {socketError}");
                Dispose();
                return;
            }

            try
            {
                lock (_tcpSendLock)
                {
                    if (_tcpSendQueue.Count == 0)
                    {
                        _tcpSendSemaphore.Release();
                        return;
                    }

                    int length;
                    (buffer, length) = _tcpSendQueue.Dequeue();
                    _tcpSocket.BeginSend(buffer, 0, length, SocketFlags.None, OnTcpSent, buffer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(OnTcpSent)} failed:\n{e}");
                Dispose();
                return;
            }
        }

        private void OnUdpSent(IAsyncResult result)
        {
            SocketError socketError;

            try
            {
                _udpSocket.EndSend(result, out socketError);
            }
            catch (SocketException)
            {
                Console.WriteLine($"{nameof(OnUdpSent)} failed: SocketException");
                Dispose();
                return;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine($"{nameof(OnUdpSent)} failed: NullReferenceException");
                Dispose();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(OnUdpSent)} failed:\n{e}");
                Dispose();
                return;
            }

            byte[] buffer = (byte[]) result.AsyncState;
            ArrayPool<byte>.Shared.Return(buffer, true);

            if (socketError != SocketError.Success)
            {
                Console.WriteLine($"{nameof(OnUdpSent)} failed: {socketError}");
                Dispose();
                return;
            }

            try
            {
                lock (_udpSendLock)
                {
                    if (_udpSendQueue.Count == 0)
                    {
                        _udpSendSemaphore.Release();
                        return;
                    }

                    int length;
                    (buffer, length) = _udpSendQueue.Dequeue();
                    _udpSocket.BeginSend(buffer, 0, length, SocketFlags.None, OnUdpSent, buffer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(OnUdpSent)} failed:\n{e}");
                Dispose();
                return;
            }
        }

        private void Ping(object state)
        {
            if (Connected && LoggedIn)
                SendPingReply();
            else
                _pingTimer.Dispose();
        }

        private void UdpPing(object state)
        {
            if (Connected && LoggedIn)
                SendUdpInitRequest(); // We ping with init, server replies and we sent ping back.
            else
                _udpPingTimer.Dispose();
        }

        private void PositionUpdateTimerCallback(object state)
        {
            if (Connected && LoggedIn)
                SendPositionUpdate();
            else
                _positionTimer.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_hasConnectionSlot)
            {
                _connectionSlots.Release();
                _hasConnectionSlot = false;
            }
            
            if (!_disposed)
            {
                _disposed = true;
                _pingTimer?.Dispose();
                _udpPingTimer?.Dispose();
                _positionTimer?.Dispose();
                _tcpSocket?.Dispose();
                _udpSocket?.Dispose();
                ActionTimer?.Dispose();
                _tcpReceiverSocketArgs?.Dispose();
                _tcpSendSemaphore?.Dispose();
                _udpSendSemaphore?.Dispose();
                Console.WriteLine($"Disposed {_charName}");
            }
        }
    }

    public class GameLocation(int x, int y, int z, int zone)
    {
        public int x = x, y = y, z = z;
        public int zone = zone;
    };
}
