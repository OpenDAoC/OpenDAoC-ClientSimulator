using System;
using System.Threading;

namespace ClientSimulator
{
    public partial class Client
    {
        private void HandleVersionAndCryptKeyResponse(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleVersionAndCryptKeyResponse)}");
#endif

            SendLoginRequest(_userName, _password);
        }

        private void HandleLoginGrantedResponse(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleLoginGrantedResponse)}");
#endif

            if (LoggedIn)
                return;

            if (_hasConnectionSlot)
            {
                Console.WriteLine($"{_userName} logged in");
                _connectionSlots.Release();
                _hasConnectionSlot = false;
            }

            LoggedIn = true;
            _pingTimer = new Timer(Ping, null, 0, 15000);
            _udpPingTimer = new Timer(UdpPing, null, 5000, 60000);
            SendCharacterSelectRequest();
        }

        private void HandlePingReply(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandlePingReply)}");
#endif
        }

        private void HandleSetSessionId(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleSetSessionId)}");
#endif

            SessionId = ReadShortLowEndian(buffer, ref pos);

            if (!OverviewRequested)
                SendOverviewRequest();
            else if (CharacterSelected)
                SendGameOpenRequest();
        }

        private void HandleSetRealm(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleSetRealm)}");
#endif
        }

        private void HandleCharacterOverview(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleCharacterOverview)}");
#endif

            SendCharacterSelectRequest();
        }

        private void HandleStartArena(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleStartArena)}");
#endif
        }

        private void HandleGameOpenReply(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleGameOpenReply)}");
#endif

            SendWorldInitRequest();
        }

        private void HandleLosCheck(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleLosCheck)}");
#endif

            ushort player = ReadUShort(buffer, ref pos);
            ushort target = ReadUShort(buffer, ref pos);
            _ = ReadUShort(buffer, ref pos);
            _ = ReadUShort(buffer, ref pos);
            SendLosCheck(player, target);
        }

        private void HandleStatusUpdate(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleStatusUpdate)}");
#endif
        }

        private void HandleSetPlayerPositionAndOid(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleSetPlayerPositionAndOid)}");
#endif
            float x_loc = ReadFloat32LowEndian(buffer, ref pos);
            float y_loc = ReadFloat32LowEndian(buffer, ref pos);
            float z_loc = ReadFloat32LowEndian(buffer, ref pos);
            ushort oid = ReadUShort(buffer, ref pos);
            PositionHeading = ReadUShort(buffer, ref pos); // Heading
            _positionTimer ??= new Timer(PositionUpdateTimerCallback, null, 1000, 250);
            ZoneX = x_loc;
            ZoneY = y_loc;
            ZoneZ = z_loc;
            PositionSpeed = 0;
        }
        
        private void HandlePlayerPositionUpdate(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandlePlayerPositionUpdate)}");
#endif
        }

        private void HandlePlayerInitResponse(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandlePlayerInitResponse)}");
#endif
        }

        private void HandleControlledHorse(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleControlledHorse)}");
#endif

            if (PlayerInitSent)
                return;

            SendPlayerInitRequest();
        }

        private void HandleUDPInitReply(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleUDPInitReply)}");
#endif

            SendUdpPingRequest();
        }

        private void HandleRegionChanged(byte[] buffer, int pos, int bodyLen)
        {
#if DEBUG
            Console.WriteLine($"<< {nameof(HandleRegionChanged)}");
#endif

            PlayerInitSent = false;
            SendWorldInitRequest();
        }
    }
}
