using System;

namespace WebFormServer.Engine.Models
{
    public class TransmittedData
    {
        private byte[] data;
        private int capacity;
        private int offset;
        private int multiplier;

        public byte[] Data
        {
            get 
            {
                if((capacity - offset) < 512)
                {
                    capacity = capacity * 2;
                    byte[] temp = new byte[capacity];
                    Array.Copy(data, temp, Offset);
                    data = temp;
                }

                return data; 
            }
        }

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public int Freespace
        {
            get { return capacity - offset; }
        }

        public TransmittedData() : this(1024)
        {
        }

        public TransmittedData(int length)
        {
            this.data = new byte[length];
            this.capacity = length;
            this.offset = 0;
            multiplier = 2;
        }

    }
}
