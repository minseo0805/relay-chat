using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace chat
{
    class Manger
    {
        //ConcurrentDictionary<TKey,TValue>에서는 추가하거나 제거하려고 시도하기 전에 키의 존재 여부를 먼저 확인하는 코드가 필요하지 않게 하는 편리한 몇 가지 메서드를 제공
        // key는 클라이언트의 IP의 네번째 옥텟, Value는 연결된 클라이언트 객체


        public static ConcurrentDictionary<int, Data> clientDic = new ConcurrentDictionary<int, Data>();
        public event Action<string, string> messageParsingAction = null;
        public event Action<string, int> EventHandler = null;

        //새로 접속하는 클라이언트에 데이터 추가 
        public void AddClient(TcpClient newClient)
        {
            Data currentClient = new Data(newClient);

            try
            {//BeginRead  비동기처리 
                //AsyncCallback의 매개변수에 콜백메서드를 등록, 클라이언트가 메시지를 보내서 서버가 해당 메시지를 읽게 됐을 때 콜백메서드가 실행
                currentClient.tcpClient.GetStream().BeginRead(currentClient.readBuffer, 0, currentClient.readBuffer.Length, new AsyncCallback(DataReceived), currentClient);
                clientDic.TryAdd(currentClient.clientNumber, currentClient);
            }

            catch (Exception e)
            {

            }
        }
        //현재 접속중인 클라이언트의 접속 기록 데이터 받음 
       
        private void DataReceived(IAsyncResult ar)
        {
            // 콜백메서드(피호출자가 호출자의 해당 메서드를 실행시킴)
            //즉 데이터를 읽었을때 실행

            Data client = ar.AsyncState as Data;

            // 콜백으로 받아온 Data를 ClientData로 형변환

            try
            {
                
             
                int byteLength = client.tcpClient.GetStream().EndRead(ar);
                string strData = Encoding.Default.GetString(client.readBuffer, 0, byteLength);
                client.tcpClient.GetStream().BeginRead(client.readBuffer, 0, client.readBuffer.Length, new AsyncCallback(DataReceived), client);

                if (string.IsNullOrEmpty(client.clientName))
                {
                    if (EventHandler != null)
                    {
                        if (CheckID(strData)) //id확인 후 접속 기록 받음 
                        {
                            string userName = strData.Substring(3);
                            client.clientName = userName;
                            string accessLog = string.Format("[{0}] {1} 클라이언트 서버접속완료!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), client.clientName);
                            EventHandler.Invoke(accessLog, Switch.ADD_ACCESS_LOG);
                            return;
                        }
                    }

                }


                if (messageParsingAction != null)
                {
                    messageParsingAction.BeginInvoke(client.clientName, strData, null, null);
                }

            }
            catch (Exception e)
            {

            }
        }

        // 클라이언트는 최초 접속시 "이름" 을 보내도록 구현
        // '%^&' 기호가 왔다면 서버는 해당클라이언트에게 이름을 부여
        private bool CheckID(string ID)
        {
            if (ID.Contains("%^&"))
                return true;

            return false;
        }
    }
}