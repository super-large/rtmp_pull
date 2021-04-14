using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtmpService.Packets
{
    public class NetWorkBinaryWriter : BinaryWriter
    {

        public NetWorkBinaryWriter(Stream output):base(output)
        {
        }


        public override void Write(short value)
        {
            WriteNetWorkBytes(BitConverter.GetBytes(value));
        }

        public override void Write(int value)
        {
            WriteNetWorkBytes(BitConverter.GetBytes(value));
        }



        private void WriteNetWorkBytes(byte[] buffer)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }
    }
}
