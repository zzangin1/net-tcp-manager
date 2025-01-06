# NetTcpManager
- 서버, 클라이언트 TCP 통신 라이브러리

# 스택
- .NET 8.0
- Socket 클래스

# Server
- 서버 Open
- 서버 Close
- 다중 클러이언트 연결
- 데이터 수신 쓰레드
- 송수신 데이터 처리 후 MessageQueue에 데이터 전달
  
# Client
- 서버 Connect
- 서버 Disconnect
- 데이터 수신 쓰레드
- 송수신 데이터 처리 후 MessageQueue에 데이터 전달

# MessageQueue
- 서버, 클라이언트 간 직접 통신 아닌 MessageQueue를 통해 데이터 송수신
- SendQueue에 담긴 데이터 서버 혹은 클라이언트에 송신
- RecvQueue에 담긴 데이터 처리 로직
