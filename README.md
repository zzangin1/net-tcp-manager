# NetTcpManager
- 서버, 클라이언트 TCP 통신 라이브러리

# 스택
- .NET 8.0
- Socket 클래스

# 기능
## Server
- 서버 Open
- 서버 Close
- 클라이언트로부터 데이터 수신
- 클라이언트로부터 데이터 수신 시 클라이언트에 응답 데이터 송신
  
## Client
- 서버 Connect
- 서버 Disconnect
- 서버에 데이터 송신

## MessageQueue
- 서버, 클라이언트 간 직접 통신 아닌 MessageQueue를 통해 데이터 송수신
- SendQueue : 송신 데이터를 큐에 넣고 큐에서 송신 데이터 처리
- RecvQueue : 수신 데이터를 큐에 넣고 큐에서 수신 데이터 처리
