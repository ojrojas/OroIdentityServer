import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { ScopeDto } from '../../core/models';

interface ScopesState {
  scopes: ScopeDto[];
  loading: boolean;
}

export const ScopesStore = signalStore(
  { providedIn: 'root' },
  withState<ScopesState>({ scopes: [], loading: false }),
  withMethods((store, http = inject(HttpClient)) => ({
    load(): void {
      patchState(store, { loading: true });
      http.get<ScopeDto[]>('/api/scopes').subscribe({
        next: scopes => patchState(store, { scopes: scopes ?? [] }),
        complete: () => patchState(store, { loading: false }),
      });
    },
    create(cmd: { name: string; resources: string[] }): void {
      http.post('/api/scopes', cmd).subscribe(() => this.load());
    },
    update(name: string, cmd: { name: string; resources: string[] }): void {
      http.put(`/api/scopes/${name}`, cmd).subscribe(() => this.load());
    },
    delete(name: string): void {
      http.delete(`/api/scopes/${name}`).subscribe(() => this.load());
    },
  }))
);
