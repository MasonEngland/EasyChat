## write down how you plan to design features here to avoid making design decisions during programming


## User Flow

Current draft of the user flow is that a user connects to the base url which leads them to a visually appealing page where they can create a chat room, It will also contain information on how to join a chat room. Upon creating a chat room users with make a nickname for that chat room that others will know them by. The nickname should be remembered for a short term amount of time. A message should display when a user joins or leaves a chat room. 


## Features

* AI chatbot user can call using a chat command possibly starting with a / or something
* Video streaming from one computer to another
* NO AUTHENTICATION REQUIRED
* anyone with the link can join the chat room
* keep alive option to keep a room alive when people leave
* backround worker to delete unused chat rooms
* socket server chat (obviously)
* Image uploads
* file uploads
* possibly link fallowing


## design for the chat message feature

This uses signalR to handle socket connection and chat rooms. Each user will call the "JoinRoom" method via the socket connection to be given a connection Id and then added to a specific chat group (like a chat room). This method takes a room Id and connects a user to the chat room that corrisponds to that id. Now any message sent to that group will be recieved by that user. After the chat room is joined messages can be sent via "SendMessage" and recieved via "ReceiveMessage" and all clients should have a "CatchError" for in case the socket server needs to communicate an error to the client. 

to display files the method name is "SendFile" and "RecieveFile"


add a CatchError signalr method for error handling and add a "RecieveFile" method to get the id and name of a file that can then be used to download files. 
Recieve file takes a user, file name, and file id.


## github actions tests for setup

# test 1: failed

# test 2: failed

# test 3: success