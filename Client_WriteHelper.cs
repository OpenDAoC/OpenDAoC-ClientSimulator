using System.Text;
using System.Threading;

namespace ClientSimulator
{
    public partial class Client
    {
        private static void WriteUShort(byte[] buffer, ref int pos, int value)
        {
            buffer[pos++] = (byte) (value >> 8);
            buffer[pos++] = (byte) value;
        }

        private static int AppendChecksum(byte[] pak, int start, int lastIndexPlusOne)
        {
            byte val1 = 0x7E;
            byte val2 = 0x7E;

            while (start < lastIndexPlusOne)
            {
                val1 += pak[start++];
                val2 += val1;
            }

            ushort checkSum = (ushort) (val2 - ((val1 + val2) << 8));
            pak[lastIndexPlusOne] = (byte) (checkSum >> 8);
            pak[lastIndexPlusOne + 1] = (byte) checkSum;
            return lastIndexPlusOne + 2;
        }

        private int WriteHeader(byte[] buffer, ushort packageId)
        {
            int pos = 2;
            WriteSequence(buffer, ref pos);
            WriteSessionId(buffer, ref pos);
            WriteParameter(buffer, ref pos);
            WriteMessageType(buffer, ref pos, packageId);
            return pos;
        }

        private void WriteMessageType(byte[] buffer, ref int pos, ushort msgType)
        {
            buffer[pos++] = (byte) (msgType >> 8);
            buffer[pos++] = (byte) msgType;
        }

        private void WriteParameter(byte[] buffer, ref int pos)
        {
            WriteUShort(buffer, ref pos, 0);
        }

        private void WriteSessionId(byte[] buffer, ref int pos)
        {
            WriteUShort(buffer, ref pos, SessionId);
        }

        private static void WriteLength(byte[] buffer, int pos)
        {
            int len = pos - 10;
            buffer[0] = (byte) (len >> 8);
            buffer[1] = (byte) len;
        }

        private void WriteSequence(byte[] buffer, ref int pos)
        {
            int seq = Interlocked.Increment(ref PacketSequence);
            buffer[pos++] = (byte) (seq >> 8);
            buffer[pos++] = (byte) seq;
        }

        private static void WriteIntLowEndian(byte[] buffer, ref int pos, int i)
        {
            buffer[pos++] = (byte) i;
            buffer[pos++] = (byte) (i >> 8);
            buffer[pos++] = (byte) (i >> 16);
            buffer[pos++] = (byte) (i >> 24);
        }

        private static unsafe void WriteFloat32LowEndian(byte[] buffer, ref int pos, float f)
        {
            uint val = *(uint*) &f;
            buffer[pos++] = (byte) val;
            buffer[pos++] = (byte) (val >> 8);
            buffer[pos++] = (byte) (val >> 16);
            buffer[pos++] = (byte) (val >> 24);
        }

        private static void WriteLowEndianShortPascalString(byte[] buffer, ref int pos, string s)
        {
            int length = s.Length;
            buffer[pos++] = (byte) length;
            buffer[pos++] = (byte) (length >> 8);
            pos += Encoding.Default.GetBytes(s, 0, length, buffer, pos);
        }

        private static void WriteFixedWidthString(byte[] buffer, ref int pos, string str, int len)
        {
            int written = Encoding.Default.GetBytes(str, 0, str.Length, buffer, pos);
            int targetPos = pos + len;

            if (written > len)
            {
                pos += len;
                buffer[pos] = 0;
                return;
            }

            pos += written;

            while (pos < targetPos)
                buffer[pos++] = 0;
        }

        private void WriteInt(byte[] buffer, ref int pos, int value)
        {
            buffer[pos++] = (byte) (value >> 24);
            buffer[pos++] = (byte) (value >> 16);
            buffer[pos++] = (byte) (value >> 8);
            buffer[pos++] = (byte) value;
        }

        private ushort ReadShortLowEndian(byte[] buffer, ref int pos)
        {
            byte v1 = buffer[pos++];
            byte v2 = buffer[pos++];
            return (ushort) ((v1 & 0xff) | (v2 & 0xff) << 8);
        }

        private static unsafe float ReadFloat32LowEndian(byte[] buffer, ref int pos)
        {
            uint val = ReadUInt32LowEndian(buffer, ref pos);
            return *(float*) &val;
        }

        private static uint ReadUInt32LowEndian(byte[] buffer, ref int pos)
        {
            byte v1 = buffer[pos++];
            byte v2 = buffer[pos++];
            byte v3 = buffer[pos++];
            byte v4 = buffer[pos++];
            return (uint) (v1 | (v2 << 8) | (v3 << 16) | v4 << 24);
        }

        private ushort ReadUShort(byte[] buffer, ref int pos)
        {
            byte v1 = buffer[pos++];
            byte v2 = buffer[pos++];
            return (ushort) ((v1 << 8) | v2);
        }
    }
}
