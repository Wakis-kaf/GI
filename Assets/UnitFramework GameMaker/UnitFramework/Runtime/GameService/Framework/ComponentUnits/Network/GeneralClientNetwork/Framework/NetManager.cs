using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnitFramework.Runtime.Network;
using UnityEngine;


namespace UnitFramework.Runtime.Network
{
    public static class NetManager
    {
        #region 变量定义(private)

        // 定义套接字
        private static Socket socket;

        // 定义接收缓冲区
        private static ByteArray readBuff;

        // 定义写入队列
        private static Queue<ByteArray> writeQueue;

        // 消息列表长度
        private static int msgCount = 0;

        // 每一次Update处理的消息量
        private static readonly int MAX_MESSAGE_FIRE = 10;

        // 定义消息列表
        private static List<MsgBase> msgList = new List<MsgBase>();

        // 事件监听列表
        private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

        // 消息监听列表
        private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

        // 心跳间隔事件
        public static int pingInterval = 30;

        // 上一次发送Ping的时间
        private static float lastPingTime = 0;

        // 上一次受到Pong的时间
        private static float lastPongTime = 0;

        #region 状态判断(bool)

        private static bool isConnecting = false;

        private static bool isClosing = false;

        // 是否启用心跳机制，启用后会耗费更多的流量
        public static bool isUsePing = true;

        #endregion 状态判断(bool)

        #endregion 变量定义(private)

        #region 变量定义(public)

        // 事件委托类型
        public delegate void EventListener(string err);

        // 消息委托类型
        public delegate void MsgListener(MsgBase msgBase);

        #endregion 变量定义(public)

        #region 暴露给外部的API

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="netEvent">要监听的事件类型 </param>
        /// <param name="listener">要添加的事件委托</param>

        public static void AddEventListener(NetEvent netEvent, EventListener listener)
        {
            // 添加事件
            if (eventListeners.ContainsKey(netEvent))
            {
                eventListeners[netEvent] += listener;
            }
            // 新增事件
            else
            {
                eventListeners.Add(netEvent, listener);
            }
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="netEvent">要监听的事件类型 </param>
        /// <param name="listener">要移除的事件委托</param>
        public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
        {
            if (eventListeners.ContainsKey(netEvent))
            {
                // 移除事件
                eventListeners[netEvent] -= listener;

                if (eventListeners[netEvent] == null)
                {
                    // 删除事件
                    eventListeners.Remove(netEvent);
                }
            }
        }

        /// <summary>
        /// 添加消息监听
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="listner"></param>
        public static void AddMsgListener(string msgName, MsgListener listener)
        {
            // 添加
            if (msgListeners.ContainsKey(msgName))
            {
                msgListeners[msgName] += listener;
            }
            // 新增
            else
            {
                msgListeners[msgName] = listener;
            }
        }

        /// <summary>
        /// 删除消息监听
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="listener"></param>
        public static void RemoveMsgListener(string msgName, MsgListener listener)
        {
            if (msgListeners.ContainsKey(msgName))
            {
                msgListeners[msgName] -= listener;
                // 删除
                if (msgListeners[msgName] == null)
                {
                    msgListeners.Remove(msgName);
                }
            }
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void Connect(string ip, int port)
        {
            // 状态判断
            if (socket != null && socket.Connected)
            {
                FailDebugLog("服务器连接失败，服务器已经连接!");

                return;
            }
            if (isConnecting)
            {
                FailDebugLog("服务器连接失败，服务器正在连接中!");
                return;
            }
            // 对成员进行初始化
            InitState();
            //参数设置
            /// 关闭Nagle算法,避免实时性的降低
            socket.NoDelay = true;
            // Connect
            isConnecting = true;
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="msg">msg为消息体</param>
        public static void Send(MsgBase msg)
        {
            // 进行状态判断
            if (socket == null || !socket.Connected)
            {
                return;
            }
            if (isConnecting)
            {
                return;
            }
            if (isClosing)
            {
                return;
            }
            // 数据编码
            byte[] nameBytes = MsgBase.EncodeName(msg);
            byte[] bodyBytes = MsgBase.Encode(msg);
            int len = nameBytes.Length + bodyBytes.Length;
            byte[] sendBytes = new byte[2 + len];
            // 组装长度
            sendBytes[0] = (byte)(len % 256);
            sendBytes[1] = (byte)(len / 256);
            // 组装名字
            Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
            // 组装消息体
            Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
            // 写入队列
            ByteArray ba = new ByteArray(sendBytes);
            // writeQueue的长度
            int count = 0;
            lock (writeQueue)
            {
                writeQueue.Enqueue(ba);
                count = writeQueue.Count;
            }
            if (count == 1)
            {
                socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public static void Close()
        {
            // 进行状态判断
            if (socket == null || !socket.Connected)
            {
                return;
            }

            if (isConnecting)
            {
                return;
            }

            // 如果还有数据交流
            if (writeQueue.Count > 0)
            {
                isClosing = true;
            }
            else
            {
                socket.Close();
                SuccDebugLog("已成功从服务器断开连接!");
                FireEvent(NetEvent.Close, "");
            }
        }

        /// <summary>
        /// update
        /// </summary>
        public static void Update()
        {
            MsgUpdate();
            // 启用心跳机制
            PingUpdate();
        }

        #endregion 暴露给外部的API

        #region 内部执行方法

        #region 事件相关

        /// <summary>
        /// 事件分发
        /// </summary>
        /// <param name="netEvent"></param>
        /// <param name="err"></param>
        private static void FireEvent(NetEvent netEvent, string err = "")
        {
            if (eventListeners.ContainsKey(netEvent))
            {
                eventListeners[netEvent](err);
            }
        }

        /// <summary>
        /// 消息分发
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="msgBase"></param>
        private static void FireMsg(string msgName, MsgBase msgBase)
        {
            if (msgListeners.ContainsKey(msgName))
            {
                msgListeners[msgName](msgBase);
            }
        }

        #endregion 事件相关

        #region 状态初始化

        /// <summary>
        /// 初始化状态
        /// </summary>
        private static void InitState()
        {
            // 创建socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 创建一个接收缓冲区
            readBuff = new ByteArray();
            // 写入队列
            writeQueue = new Queue<ByteArray>();
            //关闭正在连接状态
            isConnecting = false;
            isClosing = false;

            //消息列表初始化
            msgList = new List<MsgBase>();
            // 消息列表长度
            msgCount = 0;

            // 上一次发送ping的时间
            lastPingTime = Time.time;
            // 上一次受到Pong的时间
            lastPongTime = Time.time;

            // 监听Pong协议
            if (!msgListeners.ContainsKey("MsgPong"))
            {
                AddMsgListener("MsgPong", OnMsgPong);
            }
        }

        #endregion 状态初始化

        #region 回调函数

        /// <summary>
        /// 连接回调
        /// </summary>
        /// <param name="ar"></param>
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                SuccDebugLog("Socket 连接成功");
                // 分发事件
                FireEvent(NetEvent.ConnectSucc, "");
                isConnecting = false;

                // 开始接收数据
                socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
            }
            catch (SocketException ex)
            {
                FailDebugLog("Socket 连接失败" + ex.ToString());
                FireEvent(NetEvent.ConnectFail, ex.ToString());
                isConnecting = false;
            }
        }

        /// <summary>
        /// 发送回调
        /// </summary>
        /// <param name="ar"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void SendCallback(IAsyncResult ar)
        {
            // 获取 state,endState的处理
            Socket socket = (Socket)ar.AsyncState;
            // 状态判断
            if (socket == null || !socket.Connected)
            {
                return;
            }
            // EndSend
            int count = socket.EndSend(ar);
            // 获取写入队列的第一条数据
            ByteArray ba;
            lock (writeQueue)
            {
                ba = writeQueue.First();
            }
            // 完整发送
            ba.readIdx += count;
            if (ba.length == 0)
            {
                lock (writeQueue)
                {
                    writeQueue.Dequeue();
                    ba = writeQueue.First();
                }
            }
            // 继续发送
            if (ba != null)
            {
                socket.BeginSend(ba.bytes, ba.readIdx, ba
                    .length, 0, SendCallback, socket);
            }
            // 正在关闭
            else if (isClosing)
            {
                socket.Close();
            }
        }

        /// <summary>
        /// 接收回调
        /// </summary>
        private static void ReceiveCallback(IAsyncResult ar)
        {
           
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                // 获取接收数据长度
                int count = socket.EndReceive(ar);
                
                if (count == 0)
                {
                    Close();
                    return;
                }
                readBuff.writeIdx += count;
                // 处理二进制消息
                OnReceiveData();
                // 继续接收数据
                if (readBuff.remain < 8)
                {
                    readBuff.MoveBytes();
                    readBuff.ReSize(readBuff.length * 2);
                }
                socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
            }
            catch (SocketException ex)
            {
                FailDebugLog("Socket 接收失败" + ex.ToString());
            }
        }

        /// <summary>
        /// 监听Poing协议
        /// </summary>
        private static void OnMsgPong(MsgBase msgBase)
        {
            Debug.Log("lastPongTime");
            lastPongTime = Time.time;
        }

        #endregion 回调函数

        #region debug相关

        private static void SuccDebugLog(string msg)
        {
            Debug.Log("[Success 客户端] :" + msg);
        }

        private static void FailDebugLog(string msg)
        {
            Debug.LogError("[Fail 客户端] :" + msg);
        }

        #endregion debug相关

        #region 处理方法 &　逻辑执行

        /// <summary>
        /// 数据处理
        /// </summary>
        private static void OnReceiveData()
        {
          
            // 消息长度
            if (readBuff.length <= 2)
            {
                return;
            }
            // 获取消息体长度
            int readIdx = readBuff.readIdx;
            byte[] bytes = readBuff.bytes;
            Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
            if (readBuff.length < bodyLength + 2)
                return;
            readBuff.readIdx += 2;
            // 解析协议名
            int nameCount = 0;
            string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
            SuccDebugLog("接收数据成功" + protoName);
            if (protoName == "")
            {
                FailDebugLog("OnReceiveData MsgData.DecodeName 失败!");
                return;
            }
            
            readBuff.readIdx += nameCount;

            // 对提取出的协议体进行解析
            int bodyCount = bodyLength - nameCount;
            MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
            if (msgBase == null)
            {
                FailDebugLog($"OnReceiveData MsgData.DecodeName Error! Not Fount proto type {protoName}");
            }
            readBuff.readIdx += bodyCount;
            readBuff.CheckAndMoveBytes();
            // 添加到消息队列
            lock (msgList)
            {
                msgList.Add(msgBase);
            }
            msgCount++;
            // 继续读消息
            if (readBuff.length > 2)
            {
                OnReceiveData();
            }
        }

        /// <summary>
        /// 消息更新
        /// </summary>
        private static void MsgUpdate()
        {
            // 初步判断，更新效率
            if (msgCount == 0) return;
            // 重复处理消息
            for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
            {
                // 获取第一条消息
                MsgBase msgBase = null;
                lock (msgList)
                {
                    if (msgList.Count > 0)
                    {
                        msgBase = msgList[0];
                        msgList.RemoveAt(0);
                        msgCount--;
                    }
                }
                // 分发消息
                if (msgBase != null)
                {
                    FireMsg(msgBase.protoName, msgBase);
                }
                // 没有消息了就退出
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 发送ping协议
        /// </summary>
        private static void PingUpdate()
        {
            // 是否启用心跳机制
            if (!isUsePing) return;
            // 发送Ping协议
            if (Time.time - lastPingTime > pingInterval)
            {
                MsgPing msgPing = new MsgPing();
                Send(msgPing);
                lastPingTime = Time.time;
            }
            // 检测Pong时间
            if (Time.time - lastPongTime > pingInterval * 4)
            {
                Close();
            }
        }

        #endregion 处理方法 &　逻辑执行

        #endregion 内部执行方法

        #region 网络事件类型(Public)

        public enum NetEvent
        {
            ConnectSucc = 1,
            ConnectFail = 2,
            Close = 3,
        }

        #endregion 网络事件类型(Public)
    }
}