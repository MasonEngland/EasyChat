import * as signalR from '@microsoft/signalr';
import config from "./config";

const connection = new signalR.HubConnectionBuilder().withUrl(`${config.serverUrl}/Chat`).build();

export default connection;