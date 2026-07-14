import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { PermissionDto, CreatePermissionCommand, UpdatePermissionCommand, ApiEnvelope } from '../../core/models';

interface PermissionsState {
  permissions: PermissionDto[];
  loading: boolean;
}

export const PermissionsStore = signalStore(
  { providedIn: 'root' },
  withState<PermissionsState>({ permissions: [], loading: false }),
  withMethods((store, http = inject(HttpClient)) => ({
    load(): void {
      patchState(store, { loading: true });
      http.get<ApiEnvelope<PermissionDto[]>>('/api/permissions').subscribe({
        next: res => patchState(store, { permissions: res.data ?? [] }),
        complete: () => patchState(store, { loading: false }),
      });
    },
    create(cmd: CreatePermissionCommand): void {
      http.post('/api/permissions', cmd).subscribe(() => this.load());
    },
    update(id: string, cmd: Omit<UpdatePermissionCommand, 'permissionId'>): void {
      http.put(`/api/permissions/${id}`, { ...cmd, permissionId: id }).subscribe(() => this.load());
    },
    delete(id: string): void {
      http.delete(`/api/permissions/${id}`).subscribe(() => this.load());
    },
  }))
);
