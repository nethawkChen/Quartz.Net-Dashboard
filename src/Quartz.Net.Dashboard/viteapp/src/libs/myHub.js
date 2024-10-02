import * as signalr from "@microsoft/signalr";

//const conn = new signalr.HubConnectionBuilder().withUrl("https://localhost:7176/scheduleHub").configureLogging(signalr.LogLevel.Debug).build();
const conn = new signalr.HubConnectionBuilder()
  .withUrl(import.meta.env.VITE_HTTPS + "/scheduleHub")
  .configureLogging(signalr.LogLevel.Debug)
  .build();

export default conn;
