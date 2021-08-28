using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace AtlasSimulator
{
    public partial class Client
    {        
        private void SendCryptKeyRequest()
        {
            //Console.WriteLine("Calling SendCryptKeyRequest");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xF4);
            
            buffer[pos++] = 0x80 + 6;
            buffer[pos++] = 0x01; // major 
            buffer[pos++] = 0x01; // minor
            buffer[pos++] = 24;   // patch
            buffer[pos++] = 61;   // rev?
            buffer[pos++] = 0x00;
            buffer[pos++] = 0x00;
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
        }

        private void SendLoginRequest(string username, string password)
        {
            //Console.WriteLine("Calling SendLoginRequest");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xA7);
            
            WriteUShort(buffer, ref pos, 36); // Client Type
            Console.WriteLine($"Current pos = {pos}");
            buffer[pos++] = 0x01; // Major
            buffer[pos++] = 0x02; // Minor
            buffer[pos++] = 0x03; // Build
            buffer[pos++] = 0;
            buffer[pos++] = 0;
            WriteLowEndianShortPascalString(buffer, ref pos, username);
            WriteLowEndianShortPascalString(buffer, ref pos, password);
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
        }

        private void SendPingReply()
        {
            //Console.WriteLine("Calling SendPingReply");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xA3);
            
            WriteIntLowEndian(buffer, ref pos, 0); // Unknown
            
            WriteInt(buffer, ref pos, (int)DateTime.Now.Ticks);
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
        }

        private void SendCharacterSelectRequest()
        {
            //Console.WriteLine("Calling SendCharacterSelectRequest");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0x10);
            
            WriteIntLowEndian(buffer, ref pos, 0); // Unknown
            buffer[pos++] = 0; // Unknown
            WriteFixedWidthString(buffer, ref pos, _charName, 28);
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
            
            CharacterSelected = true;
        }

        private void SendGameOpenRequest()
        {
            //Console.WriteLine("Calling SendGameOpenRequest");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xBF);

            buffer[pos++] = 0; // Use UDP = false
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
        }

        private void SendOverviewRequest()
        {
            //Console.WriteLine("Calling SendOverviewRequest");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xFC);

            buffer[pos++] = 0x01;
            WriteFixedWidthString(buffer, ref pos, $"{_username}-S", 24);
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
            OverviewRequested = true;
        }

        private void SendWorldInitRequest()
        {
            //Console.WriteLine("Calling SendWorldInitRequest");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xD4);

            WriteIntLowEndian(buffer, ref pos, 0); // Unknown
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
        }

        private void SendPlayerInitRequest()
        {
            //Console.WriteLine("Calling SendPlayerInitRequest");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xE8);

            WriteIntLowEndian(buffer, ref pos, 0); // Unknown
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
            PlayerInitSent = true;
        }

        public void SendUseSkill(ushort speedData, byte spellIndex, byte spellType)
        {
            //Console.WriteLine("Calling SendUseSkill: " + spellType.ToString());
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xBB);
            WriteFloat32LowEndian(buffer, ref pos, ZoneX);
            WriteFloat32LowEndian(buffer, ref pos, ZoneY);
            WriteFloat32LowEndian(buffer, ref pos, ZoneZ);
            WriteFloat32LowEndian(buffer, ref pos, PositionSpeed);
            WriteUShort(buffer, ref pos, 0);
            WriteUShort(buffer, ref pos, 0);
            buffer[pos++] = spellIndex;
            buffer[pos++] = spellType;
            WriteUShort(buffer, ref pos, 0);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
        }
        
        private void SendPositionUpdate()
        {
            //Console.WriteLine("Calling SendPositionUpdate");
            var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WriterBufferSize);
            var pos = WriteHeader(buffer, 0xA9);
            WriteFloat32LowEndian(buffer, ref pos, ZoneX);
            WriteFloat32LowEndian(buffer, ref pos, ZoneY);
            WriteFloat32LowEndian(buffer, ref pos, ZoneZ);
            WriteFloat32LowEndian(buffer, ref pos, PositionSpeed);
            WriteFloat32LowEndian(buffer, ref pos, 0); // zero z-speed
            WriteUShort(buffer, ref pos, SessionId);
            WriteUShort(buffer, ref pos, ZoneId);
            WriteUShort(buffer, ref pos, PositionStatus); // Status
            WriteUShort(buffer, ref pos, 0); // fly flag
            WriteUShort(buffer, ref pos, PositionHeading); // Heading
            buffer[pos++] = 0x80; // flags
            buffer[pos++] = 0; // unknown
            buffer[pos++] = 0; // unknown
            buffer[pos++] = 100; // health %
            buffer[pos++] = 100; // mana %
            buffer[pos++] = 100; // endu %
            
            
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            
            Send(buffer, pos);
        }
    }
}