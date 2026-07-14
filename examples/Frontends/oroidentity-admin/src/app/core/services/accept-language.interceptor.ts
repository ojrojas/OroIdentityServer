import { HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export const acceptLanguageInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const translate = inject(TranslateService);
  const lang = translate.getCurrentLang() ?? 'es';
  const modified = req.clone({ setHeaders: { 'Accept-Language': lang } });
  return next(modified);
};
