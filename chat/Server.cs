using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace chat
{
    public class Server
    {
        //서버 시작 

        Manger _clientManager = null;
        ConcurrentBag<string> AccessLog = null;
        //ConcurrentBag은 평소 자주 사용하는 List을 병렬처리하여 추가하거나 삭제하는 경우에 데이터가 안전하게 보관되도록 만든 클래스

        public Server()
        {
            // 생성자에서는 클라이언트매니저의 객체를 생성,
            // 접근로그를 담을 컬렉션 생성

            _clientManager = new Manger();
            AccessLog = new ConcurrentBag<string>();
            _clientManager.EventHandler += ClientEvent; //이벤트 핸들러 +=연산자를 사용해서 이벤트 추가 
            Task serverStart = Task.Run(() => //비동기 메서드를 호출할 때는 Task.Run() 메서드를 사용
            {
                ServerRun();
            });


        }


        //클라이언트가 메시지 보내는 과정 
        //MessageParsing()
        //list 생성, string 배열 문자열로 반환 
        private void MessageParsing(string sender, string message)
        {
            List<string> msgList = new List<string>();

            string[] msgArray = message.Split('>');
            foreach (var item in msgArray)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                msgList.Add(item);
            }
            SendMsgToClient(msgList, sender);

        }

        // SendMsgToClient
        //Format() 메소드를 이용하여 문자열을 출력
        //메시지 구문 분석, 메시지 주고 받음 

        private void SendMsgToClient(List<string> msgList, string sender)
        {
          
            string parsedMessage = "";
            string receiver = "";  
           
            int senderNumber = -1;
            int receiverNumber = -1;

            foreach (var item in msgList)
            {  // "<"기호는 상대방과 내용을 구별하기위해 사용
                string[] splitedMsg = item.Split('<');

                receiver = splitedMsg[0];
                parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]); 

                senderNumber = GetClinetNumber(sender);
                receiverNumber = GetClinetNumber(receiver);

                if (senderNumber == -1 || receiverNumber == -1)
                {
                    return;
                }
                //Contains 문자열 비교 
                if (parsedMessage.Contains("<GiveMeUserList>"))
                {
                    string userListStringData = "관리자<";
                    foreach (var el in Manger.clientDic)
                    {
                        userListStringData += string.Format("${0}", el.Value.clientName);
                    }
                    //'>' 기호는 버퍼에 쌓인 메시지를 구별하기 위해 사용
                    // Socket은 byte[] 형식으로 데이터를 주고받으므로 userListByteData 

                    userListStringData += ">";
                    byte[] userListByteData = new byte[userListStringData.Length];
                    userListByteData = Encoding.Default.GetBytes(userListStringData);
                    Manger.clientDic[receiverNumber].tcpClient.GetStream().Write(userListByteData, 0, userListByteData.Length);
                    return;
                }



            }
        }

        // 클라이언트의 이름을 통해 ClientNumber를 얻는 메서드
        // ClientDictionary는 ClientNumber를 키로 사용하고있어서 클라이언트의 번호를 통해 클라이언트 객체를 반환
        private int GetClinetNumber(string targetClientName)
        {
            foreach (var item in Manger.clientDic)
            {
                if (item.Value.clientName == targetClientName)
                {
                    return item.Value.clientNumber;
                }
            }
            return -1;
        }

        // 접근로그 저장하는 메서드
        // Switch 클래스로 정적변수에 번호를 지정
        private void ClientEvent(string message, int key)
        {
            switch (key)
            {
                case Switch.ADD_ACCESS_LOG:
                    {
                        AccessLog.Add(message);
                        break;
                    }
              
            }
        }

        // 서버를 돌리는 과정
        private void ServerRun()
        {
            // TcpListener 생성자에 붙는 매개변수, 첫번째는 IP를 두번째는 port 번호
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 9999));
            listener.Start();
            // 클라이언트의 접속을 확인하면  해당클라이언트의 메시지를 읽도록 대기시키고 while문을 통해 다시 클라이언트 접속 대기 

            while (true)
            {
                Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();

                acceptTask.Wait();

                // 클라이언트 객체를 만들어 9999에 연결한 client를 받음 
                TcpClient newClient = acceptTask.Result;

                _clientManager.AddClient(newClient);
            }
        }

        // 서버가 돌아가는 콘솔 로직
        public void ConSoleVIew()
        {
            while (true)
            {
                Console.WriteLine("<<<<RELAY CHAT>>>>");
                Console.WriteLine("*******서버*******");
                Console.WriteLine("1.접속중인 인원");
                Console.WriteLine("2.접속 기록");
                Console.WriteLine("******************");


                string key = Console.ReadLine();
                int order = 0;


                if (int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        case Switch.SHOW_CURRENT_CLIENT:   //현재 접속 인원 확인 
                            {
                                ShowCurrentClient();
                                break;
                            }
                        case Switch.SHOW_ACCESS_LOG:      //접속 기록 확인 
                            {
                                ShowAccessLog();
                                break;
                            }
                  
                        default:             //그외에 잘못 입력 
                            {
                                Console.WriteLine("잘못 입력했어요");
                                Console.ReadKey();
                                break;
                            }
                    }
                }

                else
                {
                    Console.WriteLine("잘못 입력했어요");
                    Console.ReadKey();
                }
                Console.Clear();   //콘솔 초기화 
             
            }
        }


        //접속기록확인
        private void ShowAccessLog()
        {
            if (AccessLog.Count == 0)
            {
                Console.WriteLine("접속 기록이 없어요");
                Console.ReadKey();
                return;
            }

            foreach (var item in AccessLog)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        //접속인원확인
        private void ShowCurrentClient()
        {
            if (Manger.clientDic.Count == 0)
            {
                Console.WriteLine("접속중인 인원이 없어요");
                Console.ReadKey();
                return;
            }

            foreach (var item in Manger.clientDic)
            {
                Console.WriteLine(item.Value.clientName);
            }
            Console.ReadKey();
        }
    }
}