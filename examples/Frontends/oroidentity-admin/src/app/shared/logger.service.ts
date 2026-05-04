import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

export type LoggerLevel = 'Debug' | 'Information' | 'Warning' | 'Error' | 'Fatal'

@Injectable({
  providedIn: 'root'
})
export class LoggerSeqService {

  constructor(private http: HttpClient){}

  logger(type: LoggerLevel, message: string, application: string = 'activities-stack-web') {
    var request = `{ "@t": "${new Date().toISOString()}", "@l": "${type}", "@mt": "application: ${application}, message: ${message}" }`;

     console.log("message", message)
    // this.http.post("/seq/ingest/clef", JSON.stringify(request), {
    //   observe: 'response',
    //   headers:{ 'content-type': 'application/vnd.serilog.clef'}
    // }).subscribe({
    //   next: response => {
    //     if (isDevMode())
    //       console.debug(response)
    //   },
    //   error: error => {
    //     if (isDevMode())
    //       console.error(error)
    //   },
    //   complete: () => {
    //     if (isDevMode())
    //       console.debug("complete registry logger")
    //   }
    // });
  }
}
