using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AtlasSimulator
{
    public partial class Client
    {
        private void HandleVersionAndCryptKeyResponse(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleVersionAndCryptKeyResponse");
            SendLoginRequest(_username, _password);
        }

        private void HandleLoginGrantedResponse(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleLoginGrantedResponse");
            if (_hasConnectionSlot)
            {
                Console.WriteLine($"{_username} logged in");
                ConnectionSlots.Release();
                _hasConnectionSlot = false;
            }
            LoggedIn = true;
            _pingTimer = new Timer(Ping, null, 0, 15000);
            SendCharacterSelectRequest();
        }

        private void HandlePingReply(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandlePingReply - NI");
            //throw new System.NotImplementedException();
        }

        private void HandleSetSessionId(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleSetSessionId");
            SessionId = ReadShortLowEndian(buffer, ref pos);

            if (!OverviewRequested)
            {
                SendOverviewRequest();
            }
            else if (CharacterSelected)
            {
                SendGameOpenRequest();
            }
            
            //SessionId = buffer
        }

        private void HandleSetRealm(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleSetRealm - NI");
            //throw new System.NotImplementedException();
        }

        private void HandleCharacterOverview(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleCharacterOverview");
            SendCharacterSelectRequest();
        }

        private void HandleStartArena(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling SendGameOpenRequest");
            //throw new System.NotImplementedException();
        }

        private void HandleGameOpenReply(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleGameOpenReply");
            SendWorldInitRequest();
        }

        private void HandleLOSCheck(byte[] buffer, int pos, int bodyLen)
        {
            var player = ReadUShort(buffer, ref pos);
            var target = ReadUShort(buffer, ref pos);
            var unknwn1 = ReadUShort(buffer, ref pos);
            var unknwn2 = ReadUShort(buffer, ref pos);
            SendLOSCheck(player, target);
        }

        private void HandleStatusUpdate(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleStatusUpdate - NI");
            //throw new System.NotImplementedException();
        }

        private void HandleSetPlayerPositionAndOid(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleSetPlayerPositionAndOid");
            var x_loc = ReadFloat32LowEndian(buffer, ref pos);
            var y_loc = ReadFloat32LowEndian(buffer, ref pos);
            var z_loc = ReadFloat32LowEndian(buffer, ref pos);
            var oid = ReadUShort(buffer, ref pos);
            PositionHeading = ReadUShort(buffer, ref pos); // Heading
            if (_positionTimer == null)
            {
                _positionTimer = new Timer(PositionTimerCallback, null, 1000, 250);
                //_actionTimer = new Timer(_actionCallback, null, 850, _actionDelay);
            }

            ZoneX = x_loc;
            ZoneY = y_loc;
            ZoneZ = z_loc;
            PositionSpeed = 0;
        }
        
        private void HandlePlayerPositionUpdate(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandlePlayerPositionUpdate");
            var x_loc = ReadFloat32LowEndian(buffer, ref pos);
            var y_loc = ReadFloat32LowEndian(buffer, ref pos);
            var z_loc = ReadFloat32LowEndian(buffer, ref pos);
            var speed = ReadFloat32LowEndian(buffer, ref pos);
            ReadFloat32LowEndian(buffer, ref pos); // z-speed .. don't use
            var sid = ReadUShort(buffer, ref pos);
            if(sid != SessionId || sid == 0)
                return;

            if (_positionTimer == null)
            {
                _positionTimer = new Timer(PositionTimerCallback, null, 1000, 250);
                //_actionTimer = new Timer(_actionCallback, null, 850, _actionDelay);
            }

            ZoneX = x_loc;
            ZoneY = y_loc;
            ZoneZ = z_loc;
            PositionSpeed = speed;
            ZoneId = ReadUShort(buffer, ref pos); // Zone Id
            PositionStatus = ReadUShort(buffer, ref pos); // Status
            ReadUShort(buffer, ref pos); // fly-flag, don't use
            PositionHeading = ReadUShort(buffer, ref pos); // Heading
        }

        private void HandlePlayerInitResponse(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandlePlayerInitResponse - Not Implemented");
        }

        private void HandleControlledHorse(byte[] buffer, int pos, int bodyLen)
        {
            //Console.WriteLine("Calling HandleControlledHorse");
            if (PlayerInitSent)
            {
                return;
            }
            
            SendPlayerInitRequest();
        }
    }
}