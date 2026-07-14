import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { UserPreferences } from '../../core/models';
import { ThemeService } from '../../core/services/theme.service';

interface SettingsState {
  prefs: UserPreferences;
  loading: boolean;
  saving: boolean;
}

const DEFAULT_PREFS: UserPreferences = {
  theme: 'system', language: 'es', currency: 'COP',
  dateFormat: 'dd/MM/yyyy', fiscalYearStart: 1,
  notificationsEnabled: true, emailNotifications: true,
  pushNotifications: false, dailySummaryEmail: false,
  aiConfidenceThreshold: 85, autoApproveHighConfidence: false,
  sidebarCollapsed: false,
};

export const SettingsStore = signalStore(
  { providedIn: 'root' },
  withState<SettingsState>({ prefs: DEFAULT_PREFS, loading: false, saving: false }),
  withMethods(store => {
    const http = inject(HttpClient);
    const theme = inject(ThemeService);
    return {
      loadPreferences(userId:string , tenantId:string): void {
        patchState(store, { loading: true });
        // http.get<UserPreferences>(`/api/users/preferences?userId=${userId}&tenantId=${tenantId}`).subscribe({
        //   next: prefs => patchState(store, { prefs }),
        //   complete: () => patchState(store, { loading: false }),
        // });
      },
      updatePreference<K extends keyof UserPreferences>(key: K, value: UserPreferences[K]): void {
        const updated = { ...store.prefs(), [key]: value };
        patchState(store, { prefs: updated, saving: true });
        if (key === 'theme') theme.apply(value as 'light' | 'dark' | 'system');
        // http.put('/api/users/preferences', updated).subscribe({
        //   complete: () => patchState(store, { saving: false }),
        // });
      },
      reset(): void {
        patchState(store, { prefs: DEFAULT_PREFS, saving: true });
        // http.put('/api/users/preferences', DEFAULT_PREFS).subscribe({
        //   complete: () => patchState(store, { saving: false }),
        // });
      },
    };
  })
);
