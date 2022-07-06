# Super simple barebones TCP server&client

Tcp server and client with .net framework 4.7.1.

CTCPConnection will not detect it has lost connection to the server until an attempt to send a packet is made, due to that the server will not remove this connection from the connections list until some data is sent.

Its the most barebones implementation of a tcp server and client, therefore it does not have any kind of protection vs attacks such as syn flood, nor any features like keep alive packets.

The solution has an example project inside with both server and client implementations.

