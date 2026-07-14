import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { RoleDto, CreateRoleCommand, ApiEnvelope } from '../../core/models';

interface RolesState {
  roles: RoleDto[];
  loading: boolean;
}

export const RolesStore = signalStore(
  { providedIn: 'root' },
  withState<RolesState>({ roles: [], loading: false }),
  withMethods((store, http = inject(HttpClient)) => ({
    load(): void {
      patchState(store, { loading: true });
      http.get<ApiEnvelope<RoleDto[]>>('/api/roles').subscribe({
        next: res => patchState(store, { roles: res.data ?? [] }),
        complete: () => patchState(store, { loading: false }),
      });
    },
    create(cmd: CreateRoleCommand): void {
      http.post('/api/roles', cmd).subscribe(() => this.load());
    },
    update(id: string, cmd: CreateRoleCommand): void {
      http.put(`/api/roles/${id}`, { ...cmd, id }).subscribe(() => this.load());
    },
    delete(id: string): void {
      http.delete(`/api/roles/${id}`).subscribe(() => this.load());
    },
  }))
);
