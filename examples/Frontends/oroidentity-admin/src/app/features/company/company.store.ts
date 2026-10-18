import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { signalStore, withState, withMethods, withComputed } from '@ngrx/signals';
import { computed } from '@angular/core';
import { Company } from '../../core/models';

interface CompanyState {
  companies: Company[];
  activeCompany: Company | null;
  loading: boolean;
}

export const CompanyStore = signalStore(
  { providedIn: 'root' },
  withState<CompanyState>({ companies: [], activeCompany: null, loading: false }),
  withComputed(store => ({
    hasCompanies: computed(() => store.companies().length > 0),
    activeCompanyName: computed(() => store.activeCompany()?.name ?? ''),
  })),
  withMethods(store => {
    const http = inject(HttpClient);
    return {
      loadCompanies(): void {
        (store as any)._loading?.set(true);
        // No /api/companies on the identity server yet; a 404 must leave the switcher empty
        // rather than bubble an unhandled observable error.
        http.get<Company[]>('/api/companies').subscribe({
          next: companies => {
            (store as any)._companies.set(companies ?? []);
            if (companies?.length && !store.activeCompany()) {
              (store as any)._activeCompany.set(companies[0]);
            }
          },
          error: () => (store as any)._companies.set([]),
          complete: () => (store as any)._loading?.set(false),
        });
      },
      setActiveCompany(company: Company): void {
        (store as any)._activeCompany.set(company);
      },
    };
  })
);
