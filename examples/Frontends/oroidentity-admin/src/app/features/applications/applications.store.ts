import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { ApplicationDto, PagedResult } from '../../core/models';

interface ApplicationsState {
  applications: ApplicationDto[];
  loading: boolean;
}

export const ApplicationsStore = signalStore(
  { providedIn: 'root' },
  withState<ApplicationsState>({ applications: [], loading: false }),
  withMethods((store, http = inject(HttpClient)) => ({
    load(): void {
      patchState(store, { loading: true });
      // PagedResult<ApplicationDto>: the endpoint returns { items, totalCount, ... }, not a bare array.
      http.get<PagedResult<ApplicationDto>>('/api/applications').subscribe({
        next: page => patchState(store, { applications: page?.items ?? [] }),
        complete: () => patchState(store, { loading: false }),
      });
    },
    create(cmd: ApplicationDto): void {
      http.post('/api/applications', cmd).subscribe(() => this.load());
    },
    update(clientId: string, cmd: ApplicationDto): void {
      http.put(`/api/applications/${clientId}`, { ...cmd, clientId }).subscribe(() => this.load());
    },
    delete(clientId: string): void {
      http.delete(`/api/applications/${clientId}`).subscribe(() => this.load());
    },
  }))
);
