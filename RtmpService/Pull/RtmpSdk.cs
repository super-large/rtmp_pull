using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RtmpService.Pull
{
    public class RtmpSdk
    {
        private const string rtmpSocket = "\\rtmp\\RtmpSocket.dll";
        private const string rtmpLib = "\\rtmp\\librtmp.dll";

        [DllImport(rtmpSocket, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int InitSockets();
        [DllImport(rtmpSocket, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int CleanupSockets();
        [DllImport(rtmpSocket, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int ConnectStreamByLibrtmp(string uri, IntPtr r);
        [DllImport(rtmpLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr RTMP_Alloc();
        [DllImport(rtmpLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void RTMP_Init(IntPtr rtmp);
        [DllImport(rtmpLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void RTMP_Free(IntPtr rtmp);
        [DllImport(rtmpLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern int RTMP_Read(IntPtr r, byte[] buf, int size);
        [DllImport(rtmpLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RTMP_Close(IntPtr r);

        public delegate void StreamCallBack(byte[] buf, int length);
        
        public event StreamCallBack StreamCallBackEvent;
        bool _connected = false;
        const int DEFAULTBUFSIZE = 1024 * 1024;
        int _bufsize = 0;
        string _errMsg = string.Empty;
        IntPtr _rtmpPtr;
        byte[] _buf;

        public IntPtr RtmpPtr
        {
            get
            {
                return _rtmpPtr;
            }

            set
            {
                _rtmpPtr = value;
            }
        }

        public byte[] Buf
        {
            get
            {
                return _buf;
            }

            set
            {
                _buf = value;
            }
        }

        public bool Connected
        {
            get
            {
                return _connected;
            }

            set
            {
                _connected = value;
            }
        }

        public RtmpSdk()
        {
            RtmpPtr = RTMP_Alloc();
            RTMP_Init(RtmpPtr);
            _bufsize = DEFAULTBUFSIZE;
            Buf = new byte[_bufsize];
        }
        public RtmpSdk(int bufsize)
        {
            RtmpPtr = RTMP_Alloc();
            RTMP_Init(RtmpPtr);
            _bufsize = bufsize;
            Buf = new byte[_bufsize];
        }
        public string ConnectStreamByLibrtmp(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return "";
            }
            int ack = ConnectStreamByLibrtmp(uri, this.RtmpPtr);
            switch (ack)
            {
                case -1:
                    _errMsg = uri + "\tSetupURL Err\n";
                    break;
                case -2:
                    _errMsg = uri + "\tConnect Err\n";
                    break;
                case -3:
                    _errMsg = uri + "\tConnectStream Err\n";
                    break;
                case 0:
                    this.Connected = true;
                    PullStream();
                    break;
                default:
                    break;
            }
            return _errMsg;
        }


        public void PullStream()
        {
            try
            {
                if (RtmpPtr != IntPtr.Zero)
                {
                    int len = RTMP_Read(this.RtmpPtr, Buf, this._bufsize);
                    while (Connected && len > 0)
                    {

                        if (StreamCallBackEvent != null)
                        {
                            StreamCallBackEvent(Buf, len);
                        }

                        len = RTMP_Read(this.RtmpPtr, Buf, this._bufsize);
                        if (len == this._bufsize)
                        {
                            Console.WriteLine("StreamCallBackEvent 中 有效数据长度填满了 bufsize");
                            len = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("rtmp_readException:"+ex.Message+ex.StackTrace.ToString());
            }
        }

        public void ClearRtmp()
        {
            if (this.RtmpPtr != IntPtr.Zero)
            {
                Connected = false;
            }
        }

        public void Free()
        {
            if (this.RtmpPtr != IntPtr.Zero)
            {
                RTMP_Close(this.RtmpPtr);
                RTMP_Free(this.RtmpPtr);
                RtmpPtr = IntPtr.Zero;
            }
        }

    }
}
