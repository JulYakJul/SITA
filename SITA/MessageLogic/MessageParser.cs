using SITA.MessageLogic.Models;
using System.Net.Sockets;

namespace SITA.MessageLogic
{
    public static class MessageParser
    {
        private const int HEDER_SIZE = 20;
        private const int DATA_LENGTH_POS = 14;

        public static List<SITAMessage> Parse(NetworkStream stream, ByteBuffer buffer, int count)
        {
            List<SITAMessage> result = new();
            while (count >= HEDER_SIZE - buffer.count)
            {
                if (!SearchAppId(stream, buffer, SITAMessage.AppId, ref count))
                {
                    break;
                }
                ushort dataLength = GetUnsignrdShortField(buffer, DATA_LENGTH_POS);
                if (dataLength > count)
                {
                    break;
                }
                count -= dataLength;
                byte[] bytes = new byte[dataLength + HEDER_SIZE];
                buffer.Сopy(bytes);
                if(dataLength > 0)
                    stream.Read(bytes, HEDER_SIZE, dataLength);
                result.Add(SITAMessage.Create(bytes));
                buffer.count = 0;
            }
            return result;
        }
        private static bool SearchAppId(NetworkStream stream, ByteBuffer buffer, byte[] appId, ref int count)
        {
            while (count > 0)
            {
                if (buffer.count < HEDER_SIZE)
                {
                    buffer.Add((byte)stream.ReadByte());
                    --count;
                    continue;
                }

                if (CheckSubsection(buffer, appId, 0))
                    return true;

                buffer.Skip();
            }

            return buffer.count == HEDER_SIZE && CheckSubsection(buffer, appId, 0);
        }
        private static bool CheckSubsection(ByteBuffer heder, byte[] subsection, int start)
        {
            for (int i = 0; i < subsection.Length; ++i)
            {
                if (heder.Get(i + start) != subsection[i])
                {
                    return false;
                }
            }
            return true;
        }
        private static ushort GetUnsignrdShortField(ByteBuffer heder, int fieldPos)
        {
            return (ushort)(heder.Get(fieldPos + 1) << 8 | heder.Get(fieldPos));
        }
    }
}
