using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RtmpService.Packets;


namespace RtmpService.Pull
{
    public class RtmpPacketPull
    {
        private RtmpSdk _rtmpClass;
        ADTSContext _adts;
        private H264Parser _h26Parser;
        private AACParser _aacParser;

        public byte[] SPS = null;
        public byte[] PPS = null;
        public byte[] SPS_PPS = null;

        public byte[] AVCHead = new byte[] { 0x00, 0x00, 0x00, 0x01 };
        private string _url;
        private Thread _streamThread;
        private bool _Connecting;
        private bool _boxRealStream = false;

        private bool _rtmpReqFailed = false;
        private bool _isStop = false;
        private ushort Width = 0;
        private ushort Height = 0;

        public RtmpPacketPull(string url)
        {
            _h26Parser = new H264Parser(this);
            _aacParser = new AACParser();
            _adts = new ADTSContext();
            _url = url;
            _rtmpClass = new RtmpSdk();
        }

        public void Start()
        {
            _isStop = true;
            _boxRealStream = true;
            _streamThread = new Thread(new ThreadStart(StreamThread));
            _streamThread.IsBackground = true;
            _streamThread.Start();
            _h26Parser.OnFrameHappend = FramePacket;
        }

        public void Stop()
        {
            _isStop = false;
            _h26Parser.OnFrameHappend = null;
           
            DisconnectRtmp();
        }

        private void StreamThread()
        {
            while (_isStop)
            {
                if (_boxRealStream)
                {
                    ConnectRtmp();//连接
                    _boxRealStream = false;
                }
                Thread.Sleep(2000);
            }
        }

        private void ConnectRtmp()
        {
            if (_Connecting)
            {
                return;
            }

            _Connecting = true;
            try
            {
                RtmpSdk.InitSockets();
                _rtmpClass.StreamCallBackEvent += PacketCallback;
               Console.WriteLine("Rtmp pull:" + _url);

                string ack = _rtmpClass.ConnectStreamByLibrtmp(_url);
                _rtmpClass.Free();
                if (ack != "")
                {
                    Console.WriteLine("ConnectRtmpPull over:" + _url);
                    _rtmpReqFailed = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RtmpPull: failed ! Exception:" + ex.Message + ex.StackTrace);
            }
            _Connecting = false;
            _boxRealStream = true;
        }

        internal void DisconnectRtmp()
        {
            _rtmpClass.StreamCallBackEvent -= PacketCallback;
            if (!_rtmpReqFailed)
            {
                _rtmpClass.ClearRtmp();
            }
            RtmpSdk.CleanupSockets();
            _rtmpReqFailed = false;
            _Connecting = false;
            Console.WriteLine("DisConnectRtmpPull:" + _url);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="mediaType">媒体类型(0视频/1音频)</param>
        private void FramePacket(byte[] buf, MediaType mediaType)
        {
            if (mediaType ==  MediaType.VIDEO)
            {
                using (Stream stream = new FileStream("E:\\video.es", FileMode.Append, FileAccess.Write))
                {
                    stream.Write(buf, 0, buf.Length);
                }
            }
            else
            {
                using (Stream stream = new FileStream("E:\\audio.aac", FileMode.Append, FileAccess.Write))
                {
                    stream.Write(buf, 0, buf.Length);
                }
            }
        }

        public void PacketCallback(byte[] buf, int len)
        {
            byte[] data = new byte[len];
            Array.Copy(buf, data, len);

            //FLV
            if (data[0] == FLVPacketType.F && data[1] == FLVPacketType.L && data[2] == FLVPacketType.V)
            {
                //0 = AVC sequence header
                int vIndex = ByteIndexOf(data, new byte[] { FLVPacketType.AVC_I, FLVPacketType.AVC_SEQUENCE_Header });
                if (vIndex == -1)
                {
                    return;
                }


                byte[] sps_ppsBuf = new byte[data.Length - vIndex];
                Array.Copy(data, vIndex, sps_ppsBuf, 0, sps_ppsBuf.Length);

                //0x67 sps
                ushort spsLen = sps_ppsBuf.ReadUShort(11, Endianity.Big);
                SPS = new byte[spsLen];
                Array.Copy(sps_ppsBuf, 13, SPS, 0, spsLen);
                //0x68 pps
                int ppsLen = sps_ppsBuf.ReadUShort(14 + spsLen, Endianity.Big);
                PPS = new byte[ppsLen];
                Array.Copy(sps_ppsBuf, 16 + spsLen, PPS, 0, ppsLen);
                SPS_PPS = GetSPSPPS(SPS, PPS);
            }

            byte packetType = data[0];

            if (packetType == RtmpPacketType.RTMP_PACKET_TYPE_VIDEO)
            {
                //视频
                _h26Parser.GetH264Data(data, SPS_PPS);
            }
            else if (packetType == RtmpPacketType.RTMP_PACKET_TYPE_AUDIO)
            {
                //音频
                len = 0;
                buf = new byte[data.Length - 11];
                Array.Copy(data, 11, buf, 0, buf.Length);

                //AF表示的含义：
                //1）第一个字节af，a就是10代表的意思是AAC，
                //Format of SoundData.The following values are defined:
                //                0 = Linear PCM, platform endian
                //1 = ADPCM
                //2 = MP3
                //3 = Linear PCM, little endian
                //4 = Nellymoser 16 kHz mono
                //5 = Nellymoser 8 kHz mono
                //6 = Nellymoser
                //7 = G.711 A - law logarithmic PCM
                //8 = G.711 mu - law logarithmic PCM
                //9 = reserved
                //10 = AAC
                //11 = Speex
                //14 = MP3 8 kHz
                //15 = Device - specific sound
                //  Formats 7, 8, 14, and 15 are reserved.
                //AAC is supported in Flash Player 9,0,115,0 and higher.
                //Speex is supported in Flash Player 10 and higher.
                //2）第一个字节中的后四位f代表如下
                //前2个bit的含义采样频率，这里是二进制11，代表44kHZ
                //Sampling rate.The following values are defined:
                //                0 = 5.5 kHz
                //1 = 11 kHz
                //2 = 22 kHz
                //3 = 44 kHz
                //第3个bit，代表 音频用16位的
                //Size of each audio sample.This parameter only pertains to
                //uncompressed formats.Compressed formats always decode
                //to 16 bits internally.
                //0 = 8 - bit samples
                //1 = 16 - bit samples
                //  第4个bit代表声道
                //Mono or stereo sound
                //0 = Mono sound
                //1 = Stereo sound
                if (buf[0] != 0xaf)  //AAC格式音频
                {
                    return;
                }

                if (buf[1] == 0x00)  //AAC sequence header
                {
                    _aacParser.aac_decode_extradata(buf, buf.Length - 2);
                }
                else if (buf[1] == 0x01)  //AAC raw
                {
                    byte[] adts = new byte[7];
                    _aacParser.aac_set_adts_head(adts, buf.Length - 2);

                    byte[] aac = new byte[buf.Length + 5];
                    Array.Copy(adts, 0, aac, 0, 7);
                    Array.Copy(buf, 2, aac, 7, aac.Length - 7);

                    FramePacket(aac, MediaType.AUDIO);
                }
            }
        }


        private byte[] GetSPSPPS(byte[] sps, byte[] pps)
        {
            int avcHeadLen = AVCHead.Length;
            var spspps = new byte[sps.Length + pps.Length + avcHeadLen * 2];
            Array.Copy(AVCHead, 0, spspps, 0, avcHeadLen);
            Array.Copy(sps, 0, spspps, avcHeadLen, sps.Length);
            Array.Copy(AVCHead, 0, spspps, sps.Length + avcHeadLen, avcHeadLen);
            Array.Copy(pps, 0, spspps, sps.Length + avcHeadLen * 2, pps.Length);
            return spspps;
        }

        /// <summary>
        /// 查找BYTE数组中子数组的索引
        /// </summary>
        /// <param name="srcBytes">源数组</param>
        /// <param name="searchBytes">目标数组</param>
        /// <returns>索引值</returns>
        public int ByteIndexOf(byte[] srcBytes, byte[] searchBytes)
        {
            if (srcBytes == null) { return -1; }
            if (searchBytes == null) { return -1; }
            if (srcBytes.Length == 0) { return -1; }
            if (searchBytes.Length == 0) { return -1; }
            if (srcBytes.Length < searchBytes.Length) { return -1; }
            for (int i = 0; i < srcBytes.Length - searchBytes.Length; i++)
            {
                if (srcBytes[i] == searchBytes[0])
                {
                    if (searchBytes.Length == 1) { return i; }
                    bool flag = true;
                    for (int j = 1; j < searchBytes.Length; j++)
                    {
                        if ((srcBytes[i + j] & 0x0f) != searchBytes[j])//此处根据使用稍作更改，实际需要搜索的数据为00 00 01 X5
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) { return i; }
                }
            }
            return -1;
        }
    }
}
