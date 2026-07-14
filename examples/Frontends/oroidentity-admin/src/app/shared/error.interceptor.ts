import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, isDevMode } from '@angular/core';
import { catchError, tap, throwError } from 'rxjs';
import { LoggerSeqService } from './logger.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const logger = inject(LoggerSeqService);
  return next(req).pipe(tap({
    next: (event) => {
      if (isDevMode())
        console.log("Information", "Http request completed: " + { "content": req.body, "eventType": event.type });
    },
    error: (error) => {
      logger.logger("Error", error);
      (catchError(handleErrorResponse));
    }
  }));
}

function handleErrorResponse(error: HttpErrorResponse): ReturnType<typeof throwError> {
  const errorResponse = `{Error: {
    code: ${error.status},
    type: ${error.type},
    message: ${error.message},
    thing: ${error.headers.keys() + error.statusText}
  }}`;

  return throwError(() => errorResponse);
}
