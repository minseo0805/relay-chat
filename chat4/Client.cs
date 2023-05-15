using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace chat4
{
    class Client
    {
        TcpClient client = null;
        ConcurrentBag<string> sendMessageListToView = null;
        ConcurrentBag<string> receiveMessageListToView = null;
        private string name = null;

        // 서버 구동
        public void Run()
        {
            sendMessageListToView = new ConcurrentBag<string>();
            receiveMessageListToView = new ConcurrentBag<string>();

            while (true)
            {
                Console.WriteLine("<<<<<<<RELAY CHAT>>>>>>>");
                Console.WriteLine("*******클라이언트******");
                Console.WriteLine("1.서버연결");
                Console.WriteLine("2.메시지 전송");
                Console.WriteLine("3.메시지 확인");
                Console.WriteLine("***********************");

                string key = Console.ReadLine();
                int order = 0;


                if (int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        case Switch.CONNECT:
                            {
                                if (client != null) //동일한 클라이언트 연결시 
                                {
                                    Console.WriteLine("이미 연결됐어요.");
                                    Console.ReadKey();
                                }
                                else
                                {
                                    Connect();
                                }

                                break;
                            }
                        case Switch.SEND_MESSAGE:
                            {
                                if (client == null) //클라이언트와 연결 안하고 메시지를 보낼시 
                                {
                                    Console.WriteLine("먼저 서버랑 연결해주세요");
                                    Console.ReadKey();
                                }
                                else
                                {
                                    SendMessage();
                                }
                                break;
                            }
                        case Switch.SEND_MSG_VIEW: //보낸 메시지 보여줌 
                            {
                                SendMessageView();
                                break;
                            }
                        default:               //그외에 잘못 입력 
                            {
                                Console.WriteLine("잘못 입력했어요");
                                Console.ReadKey();
                                break;
                            }
                    }
                }


                Console.Clear(); //콘솔 초기화 

            }
        }

        // 사용자로부터 받은 메시지를 확인하는 기능입니다.
        private void ReceiveMessageVIew()
        {
            if (receiveMessageListToView.Count == 0)
            {
                Console.WriteLine("받은 메시지가 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in receiveMessageListToView)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        // 사용자에게 보낸 메시지를 확인하는 기능
        private void SendMessageView()
        {
            if (sendMessageListToView.Count == 0)
            {
                Console.WriteLine("메시지 없음");
                Console.ReadKey();
                return;
            }
            foreach (var item in sendMessageListToView)
            {
                Console.WriteLine(item);

            }
            Console.ReadKey();
        }




        // 사용자가 메시지를 보내는 기능
        private void SendMessage()
        {
            string getUserList = string.Format("{0}<GiveMeUserList>", name);
            byte[] getUserByte = Encoding.Default.GetBytes(getUserList);
            client.GetStream().Write(getUserByte, 0, getUserByte.Length);

            Console.WriteLine("수신자를 입력하세요");
            string receiver = Console.ReadLine();

            Console.WriteLine("보낼 메시지를 입력하세요");
            string message = Console.ReadLine();

            if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message))
            {
                Console.WriteLine("수신자와 보낼 메시지를 확인하세요");
                Console.ReadKey();
                return;
            }
            //Format() 메소드를 이용하여 문자열을 출력
            string parsedMessage = string.Format("{0}<{1}>", receiver, message);

            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedMessage);

            client.GetStream().Write(byteData, 0, byteData.Length);
            sendMessageListToView.Add(string.Format("[{0}] 수신자 : {1}, 메시지 : {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), receiver, message));
            Console.WriteLine("전송성공!");
            Console.ReadKey();
        }

        // 서버에 접속하는 메서드
        private void Connect()
        {
            Console.WriteLine("이름을 입력하세요");

            name = Console.ReadLine();

            string parsedName = "%^&" + name;
            if (parsedName == "%^&")
            {
                Console.WriteLine("제대로된 이름을 입력하세요");
                Console.ReadKey();
                return;
            }

            client = new TcpClient();

            // 하나의 PC에서 사용하므로 루프백IP를 사용

            client.Connect("127.0.0.3", 9999);
            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedName);
            client.GetStream().Write(byteData, 0, byteData.Length);
            ;


            Console.WriteLine("서버연결 성공 이제 메시지를 보낼 수 있어요!");
            Console.ReadKey();
        }
    }
}


