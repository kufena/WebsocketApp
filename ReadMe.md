## A simple websocket server and client.

This started out as an implementation of the code from this video

  https://www.youtube.com/watch?v=gDdPGI1c6ro

which is really simple stuff, but seems to work ok.

I'm not quite sure how correct it is - it is the first time I've investigate web sockets.
However, I have noted that there are two packages.  Firstly, the one we're using here

    System.Net.WebSockets

and also

    Microsoft.AspNetCore.WebSockets

which seems to provide middleware for ASP.NET.

I guess the next step is to persue the middleware package, and try to attempt message
sending between clients, as in a chat app scenario.