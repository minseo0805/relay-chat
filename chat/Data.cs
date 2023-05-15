using System.Net.Sockets;
using System.Text;

namespace chat
{
    class Data
    {
        // 연결이 확인된 클라이언트를 넣어줄 클래스.
        // 클라이언트가 상대방 이름과 메시지를 보내면 해당 이름에 맞는 클라이언트 번호를 키로 딕셔너리에 접근하는 방식 
        public TcpClient tcpClient { get; set; }
        public Byte[] readBuffer { get; set; }
        public StringBuilder currentMsg { get; set; }
        // 서버는 클라이언트가 접속할 떄마다 이름과 번호 부여 
        public string clientName { get; set; } 
        public int clientNumber { get; set; }

        public Data(TcpClient tcpClient) //tcpclient data 받음 
        {
            currentMsg = new StringBuilder();
            readBuffer = new byte[1024];

         

            this.tcpClient = tcpClient;

            char[] splitDivision = new char[2];
            splitDivision[0] = '.';
            splitDivision[1] = ':';

            string[] temp = null;

            temp = tcpClient.Client.LocalEndPoint.ToString().Split(splitDivision);

            this.clientNumber = int.Parse(temp[3]);
        }
    }
}