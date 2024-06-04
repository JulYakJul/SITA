namespace SITA.MessageLogic
{
    public class ByteBuffer
    {
        public byte[] buffer;
        public int count = 0;
        public int currsor = 0;

        public ByteBuffer()
        {
            buffer = new byte[20];
        }

        public byte Get(int pos)
        {
            return buffer[(currsor + pos) % buffer.Length];
        }
        public void Add(byte elem)
        {
            buffer[(currsor + count++) % buffer.Length] = elem;
        }
        public void Skip()
        {
            ++currsor;
            --count;
        }
        public void Сopy(byte[] target)
        {
            for (int i = 0; i < count; ++i)
            {
                target[i] = Get(i);
            }
        }
    }
}
