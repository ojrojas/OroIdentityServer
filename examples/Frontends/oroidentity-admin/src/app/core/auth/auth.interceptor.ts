import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  console.log("Interceptor ejecutado para : ", req.url);
  if (req.url.includes('openid-configuration')) return next(req);
  const token = localStorage.getItem('access_token');
  if (!token) {
    console.log("Request sin token");
    return next(req);
  }

  console.log("Request con token");
  console.log("token: ", token);
  const cloned = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  return next(cloned);
};
