import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CompanyStore } from '../../features/company/company.store';

export const companyInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(CompanyStore);
  const active = store.activeCompany();
  if (!active) return next(req);
  return next(req.clone({ setHeaders: { 'X-Company-Id': active.id } }));
};
