import * as signalr from "@microsoft/signalr";

//const conn = new signalr.HubConnectionBuilder().withUrl("https://localhost:7176/scheduleHub").configureLogging(signalr.LogLevel.Debug).build();
const conn = new signalr.HubConnectionBuilder()
  .withUrl(process.env.VUE_APP_HTTPS + "/scheduleHub")
  .configureLogging(signalr.LogLevel.Debug)
  .build();

export default conn;
