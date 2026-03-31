import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:3000/Chat").build();

export default connection;