# Unt -> User Network Transport

UDP Network with support for reliable sending

I've only been learning C# for 1 year and this is my first project that I like to develop.

# Documentation

Directory namespace Unt

  Methods NetServer!
  
    Start("port") Starts the server to receive and send data.
    Stop() Stops processing of all data.
    Send("array byte", "length byte array", "sending type, (false = Unreliable)/(true = Reliable)", "where to send") Send to, packet.
    SendAll("array byte", "length byte array", "sending type, (false = Unreliable)/(true = Reliable)", "skip client") Send to all, packet.   
    AddAction(() => "action"()) Add, an action to a queue, to be executed in another thread.    
    Tick() Perform all actions in the queue.
    
  Properties NetServer!
  
    OnClientConnected(EndPoint) When a new client connects.
    OnClientDisconnected(EndPoint) When a client disconnects.
    OnHandler(byte[], int, bool, EndPoint) Received packet.
    TimeOutClient Client idle time ms. after which the client will be disconnected.
    IsRuning If data is being processed
    
  Methods NetClient!
  
    Connect("ipAddress", "port") Connect to the server and start to receive and send data.
    Disconnect("(false = Unreliable)/(true = Reliable)") Disconnect from the serverю.    
    Stop() Stops processing of all data.    
    Send("array byte", "length byte array", "sending type, (false = Unreliable)/(true = Reliable)") Send to, packet.    
    AddAction(() => "action"()) Add, an action to a queue, to be executed in another thread.    
    Tick() Perform all actions in the queue.
    
  Properties NetClient!
  
    OnConnected() When a new client connects.   
    OnDisconnected() When a client disconnects.   
    OnHandler(byte[], int, bool, EndPoint) Received packet.    
    TimeOutServer Server idle time ms. after which the client will be disconnected.   
    IsRuning If data is being processed    
    Status Client status   
    Ping The time of the road here and there
    
# Unity Game Test
![GameTest](https://user-images.githubusercontent.com/114677727/215162075-1d8b9259-4907-419d-abea-c73d0b32175e.png)
