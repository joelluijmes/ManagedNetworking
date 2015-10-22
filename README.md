# ManagedNetworking
Simple basic wrapper-library around `Socket`.

###### Todo
 - Add proper error handling
 - Add UDP support
 - Add IPv6 support


## Client

#### Basic client
Following is an example usage of a client (blocking, single-threaded)

```C#
var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222);

var client = new Client();
client.Connect(endPoint);

client.SendAll(request, request.Length);

var response = new byte[BUFSIZE];
var received = client.Receive(response, 0, response.Length);

var responseString = Encoding.UTF8.GetString(response, 0, received);
```

#### Overlapped Client
Overlapped client uses the begin/end api and provides an event based interface. When an operation is completed it fires an event. Because the method returns before the operation can be completed, return values should be ignored (they'll default to `0` or `false`.

```C#
var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222);

var client = new OverlappedClient();
client.ClientConnected += (sender, e) =>
{
    client.SendAll(request, request.Length);

    var response = new byte[BUFSIZE];
    var received = client.Receive(response, 0, response.Length);
};

client.ReceiveCompleted += (sender, e) =>
{
    var responseString = Encoding.UTF8.GetString(e.Bytes, 0, e.Count);
};

client.Connect(endPoint);
```

#### Async Client
Async clients provides an interface to use with async/await pattern (C#5). Internally it calls `socket.AsyncXXX`, which uses IOCP. AsyncClient overrides OverlappedClient but does **not** fire the events.

```C#
var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2222);

var client = new AsyncClient();
                
await client.ConnectAsync(endPoint);
await client.SendAllAsync(request, request.Length);

var response = new byte[BUFSIZE];
var received = await client.ReceiveAsync(response, 0, response.Length);

var responseString = Encoding.UTF8.GetString(response, 0, received);
```


## Server
Descriptions of the client are the same for these classes and therefore not repeated. Remember to ignore the return values of OverlappedServer.

Note that even the client matches its server, they can be mixed (i.e. `OverlappedClient client = await asyncServer.AcceptAsync<OverlappedClient>();`)

#### Basic Server
```C#
var server = new Server();
server.Listen(2222);
var client = server.AcceptClient<Client>();
```

#### Overlapped Server
```C#
var server = new OverlappedServer();
server.Listen(2222);
server.ClientConnected += (o, e) => 
    Console.WriteLine("Client connected");

server.AcceptClient<OverlappedClient>();
```

#### Async Server
```C#
var server = new AsyncServer();
server.Listen(2222);
var client = await server.AcceptClientAsync<AsyncClient>();
```
