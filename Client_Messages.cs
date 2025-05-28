using System;

namespace ClientSimulator
{
    public partial class Client
    {        
        private void SendCryptKeyRequest()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendCryptKeyRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xF4);
            buffer[pos++] = 0x80 + 6;
            buffer[pos++] = 0x01; // major
            buffer[pos++] = 0x01; // minor
            buffer[pos++] = 24;   // patch
            buffer[pos++] = 61;   // rev?
            buffer[pos++] = 0x00;
            buffer[pos++] = 0x00;
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
        }

        private void SendLoginRequest(string username, string password)
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendLoginRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xA7);
            WriteUShort(buffer, ref pos, 36); // Client Type
            buffer[pos++] = 0x01; // Major
            buffer[pos++] = 0x02; // Minor
            buffer[pos++] = 0x03; // Build
            buffer[pos++] = 0;
            buffer[pos++] = 0;
            WriteLowEndianShortPascalString(buffer, ref pos, username);
            WriteLowEndianShortPascalString(buffer, ref pos, password);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
        }

        private void SendPingReply()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendPingReply)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xA3);
            WriteIntLowEndian(buffer, ref pos, 0); // Unknown
            WriteInt(buffer, ref pos, (int)DateTime.Now.Ticks);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
        }

        private void SendCharacterSelectRequest()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendCharacterSelectRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0x10);
            WriteIntLowEndian(buffer, ref pos, 0);
            buffer[pos++] = 0;
            WriteFixedWidthString(buffer, ref pos, _charName, 28);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
            CharacterSelected = true;
        }

        private void SendGameOpenRequest()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendGameOpenRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xBF);
            buffer[pos++] = 0; // Always 0? (1.127)
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
        }

        private void SendOverviewRequest()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendOverviewRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xFC);
            buffer[pos++] = 0x01;
            WriteFixedWidthString(buffer, ref pos, $"{_userName}-S", 24);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
            OverviewRequested = true;
        }

        private void SendWorldInitRequest()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendWorldInitRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xD4);
            WriteIntLowEndian(buffer, ref pos, 0);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
        }

        private void SendPlayerInitRequest()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendPlayerInitRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xE8);
            WriteIntLowEndian(buffer, ref pos, 0); // Unknown
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
            PlayerInitSent = true;
        }

        public void SendUseSkill(ushort speedData, byte spellIndex, byte spellType)
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendUseSkill)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xBB);
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
            SendTcp(buffer, pos);
        }

        public void SendUseSpell(ushort speedData, byte spellLevel, byte spellLineIndex)
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendUseSpell)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0x7D);
            WriteFloat32LowEndian(buffer, ref pos, ZoneX);
            WriteFloat32LowEndian(buffer, ref pos, ZoneY);
            WriteFloat32LowEndian(buffer, ref pos, ZoneZ);
            WriteFloat32LowEndian(buffer, ref pos, PositionSpeed);
            WriteUShort(buffer, ref pos, 0);
            WriteUShort(buffer, ref pos, 0);
            buffer[pos++] = spellLevel;
            buffer[pos++] = spellLineIndex;
            WriteUShort(buffer, ref pos, 0);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
        }

        public void SendLosCheck(ushort player, ushort targetOID)
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendLosCheck)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xD0);
            WriteUShort(buffer, ref pos, player);
            WriteUShort(buffer, ref pos, targetOID);
            WriteUShort(buffer, ref pos, 0);
            WriteUShort(buffer, ref pos, 0);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendTcp(buffer, pos);
        }

        private void SendPositionUpdate()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendPositionUpdate)} >>");
#endif

            if (!PlayerInitSent)
                return;

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xA9);
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
            SendTcp(buffer, pos);
        }

        private void SendUdpInitRequest()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendUdpInitRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0x14);
            WriteFixedWidthString(buffer, ref pos, string.Empty, 20);
            WriteUShort(buffer, ref pos, 0);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendUdp(buffer, pos);
        }

        private void SendUdpPingRequest()
        {
#if DEBUG
            Console.WriteLine($"{nameof(SendUdpPingRequest)} >>");
#endif

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(WRITTER_BUFFER_SIZE);
            int pos = WriteHeader(buffer, 0xF2);
            WriteLength(buffer, pos);
            pos = AppendChecksum(buffer, 0, pos);
            SendUdp(buffer, pos);
        }
    }
}
