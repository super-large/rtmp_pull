using RtmpService.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtmpService.Pull
{
    /// <summary>
    /// 帧数据委托
    /// </summary>
    /// <param name="buf">数据</param>
    /// <param name="type">媒体类型</param>
    public delegate void FrameChangedHandle(byte[] buf, MediaType type);

    public class H264Parser
    {
        private byte[] SPS_PPS = null;

        private byte[] AVCHead = null;
        public FrameChangedHandle OnFrameHappend;

        public H264Parser(RtmpPacketPull pull)
        {
            AVCHead = pull.AVCHead;
        }

        public void GetH264Data(byte[] bytes,byte[]sps_pps)
        {
            SPS_PPS = sps_pps;
            byte[] buf = bytes;
            
            if (buf[11] == FLVPacketType.AVC_I && buf[12] == FLVPacketType.AVC_SEQUENCE_Header)
            {
                return;
            }
            
            buf = new byte[bytes.Length - 16];
            Array.Copy(bytes, 16, buf, 0, buf.Length);
            
            while (buf.Length > 4)
            {
                //var nLen = buf.ReadInt(0, Endianity.Big);
                //var nLen = (buf[0] << 24) + (buf[1] << 16) + (buf[2] << 8) + buf[3];
                uint nLen = buf.ReadUInt(0, Endianity.Big);

                var nalu = new byte[nLen];
                Array.Copy(buf, 4, nalu, 0, nLen);

                var nType = nalu[0] & 0x1f;//nalu类型

                byte[] frameBuf = null;
                if (nType == Nalu_Type.NALU_TYPE_IDR)  //i帧
                {
                    frameBuf = new byte[SPS_PPS.Length + 4 + nLen];
                    Array.Copy(SPS_PPS, 0, frameBuf, 0, SPS_PPS.Length);
                    Array.Copy(AVCHead, 0, frameBuf, SPS_PPS.Length, AVCHead.Length);
                    Array.Copy(nalu, 0, frameBuf, AVCHead.Length + SPS_PPS.Length, nalu.Length);
                }
                else
                {
                    frameBuf = new byte[4 + nLen];
                    Array.Copy(AVCHead, 0, frameBuf, 0, AVCHead.Length);
                    Array.Copy(nalu, 0, frameBuf, AVCHead.Length, nalu.Length);
                }

                OnFrameHappend?.Invoke(frameBuf,MediaType.VIDEO);
                byte[] tmpBytes = new byte[buf.Length - nLen - 4];
                Array.Copy(buf, nLen + 4, tmpBytes, 0, tmpBytes.Length);
                buf = tmpBytes;
            }
        }
    }
}
