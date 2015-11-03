Overall explanations:

SharedCardGame.exe is a demo software demonstrating the strength of the .Net duplex WCF service based on a net.tcp binding.

I developped it as a personnal exercice to practice the WCF .Net programming technic with C#. 
  
SharedCardGame.exe actually is a Windows (Winform) .Net application acting as a WCF client consumming a WCF duplex Service 
I have developped and hosted on IIS 7 on a dedicated server located at the IP adress 94.23.220.199.

Note: The .Net Framework 4.5 or higher is required on the Windows computer running this application. If it is not already 
installed on your Windows PC (it should already be if you run Windows 8), you can download it at 
https://www.microsoft.com/en-us/download/details.aspx?id=30653

This client/Server dispatched application constitutes a collaborative environment by emulating a set of playing cards shared 
on a virtual playing area. Many players located on different locations may run the client application to play together with 
a set of shared playing cards.

All the players running the client SharedCardGame application see the same shared playing area and can manipulate the playing 
cards like a real set of playing cards if they were sitting together around a real game table. All the client applications of
the players communicate with the WCF duplex service hosted on the server at 94.23.220.199. The WCF service handle the shared 
state of the shared environment and report the changes of this environment state to the connected clients. So the connected
clients will always see the exact identical state of the playing environment. (Except for the visible face of their private 
cards.) 

The players can move the cards by left-clicking and dragging them to their new location. (Except the cards located in the 
private area of another player).

They also can flip them by right clicking them.

They can gather them to form a stack of cards by dragging a card (or a stack of cards) on another card (or stack of cards). 
The dragged card (or stack) will be included and placed on the top of the target card or stack of cards to constitute a new 
unic stack of cards. (The order and facing up/down of the card(s) inserted on the new stack will not be modified).

They also can gather a set of cards and/or stacks of cards located in a rectangular region of the playing area by left-clicking 
on the background of the playing area and then draging the mouse to delimit a gathering rectangular region. When the left 
button of the mouse is released, all the cards and/or stacks of cards entirely contained inside the gathering region will be 
stacked together (all face down) and shuffled up.

To extract the first card of a stack of cards, the players can right-click on any stack of cards to make appear the first card 
as a separate standalone card.

All the moves/flip/gatherings are reported by the Web service to each client/player.

Moerover, each player has his own private area that he (and only he) can place where he wants around the playing area.
The cards placed entirely in this private area are seen "upside down" from the owner only of this private area and can be dragged
by himself only.

The private area is where each player will place his private cards.

Usage:

- Be sure the hosted Web Service (net.tcp://94.23.220.199/ScgBroadcastorService/Service.svc) is running on the server (it normally
  should) and that you have the latest SharedCardGame.exe client program.
  (You can check the following URL with your browser: http://94.23.220.199/ScgBroadcastorService/Service.svc
   it should display a ScgBroadcastorService Service description page with links to the wsdl document describing 
   the exposed Web Service.)
- Be sure outgoing TCP requests are not blocked by your external/corporate firewall. (Since the duplex WCF service hosted on the
  server uses net.tcp binding, it requires that the client can send outgoing TCP requests. This should be OK on most private ISP 
  but may not be possible from the LAN of enterprises whose corporate external firewall implements a restrictive wide policy. 
- Run the SharedCardGame.exe client app. 
- Type in your personnal player name/pseudo and click the "Register player" button.
  If the table is not full (10 players max) the current state of the playing area will appear, and your private area will appear
  in the upper left corner.
  Note: if the application hangs after you clicked the "Register Player" buttons and finally raises an unhandled exception after 
  a while, saying: "Could not connect to net.tcp://94.23.220.199/ScgBroadcastorService/Service.svc. The connection attempt lasted
  for a time span of [about 20 seconds]. TCP error code 10060: A connection attempt failed because the connected party did not properly
  respond after a period of time, or established connection failed because connected host has failed to respond 94.23.220.199:808."
  This probably means an external firewall of your LAN is blocking outgoing TCP requests.
- Once registered, you can hide the registering pane by moving up the splitter separating the game area pane from the registering 
  pane.
- Move your private playing area to the wished location around the game playing area. (Do not let it stay in the upper left corner
  since other new players may appear here at any time !)
- Other players can join the game at any moment during the game. (But awill be limited to 10 players max).
- Move and flip the cards according to the rules of the game you want to play... 
  It's only a virtual shared card playing area ! No rules of any game are implemented in this app. 
  It only emulates a playing card deck shared on the net to allow remote players to play together the game they want to play !

You can launch two clients (with different player names !) and observe how reactive the WCF service is. The state of the environment
and the card moves are really reported by the server in real time with a very short response time.


Base technical tutorial document on duplex WPF:
http://www.codeproject.com/Articles/596287/Broadcasting-Events-with-a-Duplex-WCF-Service

In case of problem in enabling net.tcp on the iis server:
http://stackoverflow.com/questions/3188618/enabling-net-tcp

