import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private connections = new Map<string, HubConnection>();
  private subjects = new Map<string, Subject<unknown>>();

  connect(hubUrl: string, hubName: string): HubConnection {
    if (this.connections.has(hubName)) return this.connections.get(hubName)!;
    const conn = new HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
    this.connections.set(hubName, conn);
    conn.start().catch(err => console.error(`SignalR [${hubName}] error:`, err));
    return conn;
  }

  on<T>(hubName: string, event: string): Subject<T> {
    const key = `${hubName}:${event}`;
    if (!this.subjects.has(key)) {
      const subject = new Subject<T>();
      this.subjects.set(key, subject as Subject<unknown>);
      const conn = this.connections.get(hubName);
      conn?.on(event, (data: T) => subject.next(data));
    }
    return this.subjects.get(key) as Subject<T>;
  }

  disconnect(hubName: string): void {
    const conn = this.connections.get(hubName);
    if (conn) {
      conn.stop();
      this.connections.delete(hubName);
    }
  }
}
