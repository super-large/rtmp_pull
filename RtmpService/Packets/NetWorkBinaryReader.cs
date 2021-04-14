using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtmpService.Packets
{
    public class NetWorkBinaryReader : BinaryReader
    {
        public NetWorkBinaryReader(Stream input) : base(input)
        {
        }

        public override short ReadInt16()
        {
            return BitConverter.ToInt16(ReadNetWorkBytes(sizeof(Int16)),0);
        }
        public override int ReadInt32()
        {
            return BitConverter.ToInt32(ReadNetWorkBytes(sizeof(Int32)), 0);
        }
        public override long ReadInt64()
        {
            return BitConverter.ToInt64(ReadNetWorkBytes(sizeof(Int64)), 0);
        }


        private byte[] ReadNetWorkBytes(int len)
        {
            var val = base.ReadBytes(len);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(val);
            }
            return val;
        }

    }

}
