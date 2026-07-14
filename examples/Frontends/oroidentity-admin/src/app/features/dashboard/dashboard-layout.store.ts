import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { Subject } from 'rxjs';

export interface WidgetPosition {
  id: string;
  order: number;
}

interface DashboardLayoutState {
  widgets: WidgetPosition[];
  loading: boolean;
  saving: boolean;
}

const save$ = new Subject<WidgetPosition[]>();

export const DashboardLayoutStore = signalStore(
  { providedIn: 'root' },
  withState<DashboardLayoutState>({ widgets: [], loading: false, saving: false }),
  withMethods(store => {
    const http = inject(HttpClient);

    // save$.pipe(debounceTime(1000)).subscribe(widgets => {
    //   const layout = JSON.stringify(widgets);
    //   http.put('/api/users/preferences/dashboard-layout', { layout }).subscribe({
    //     complete: () => patchState(store, { saving: false }),
    //   });
    // });

    return {
      loadLayout(userId: string, tenantId: string): void {
        patchState(store, { loading: true });
        // http.get<{ dashboardLayout?: string }>(`/api/users/preferences?userId=${userId}&tenantId=${tenantId}`)
        //   .subscribe({
        //     next: prefs => {
        //       const widgets: WidgetPosition[] = prefs.dashboardLayout
        //         ? JSON.parse(prefs.dashboardLayout)
        //         : [];
        //       patchState(store, { widgets });
        //     },
        //     complete: () => patchState(store, { loading: false }),
        //   });
      },
      saveLayout(widgets: WidgetPosition[]): void {
        patchState(store, { widgets, saving: true });
        save$.next(widgets);
      },
      moveWidget(dragIndex: number, dropIndex: number): void {
        const current = [...store.widgets()];
        const [moved] = current.splice(dragIndex, 1);
        current.splice(dropIndex, 0, moved);
        const reordered = current.map((w, i) => ({ ...w, order: i }));
        patchState(store, { widgets: reordered, saving: true });
        save$.next(reordered);
      },
    };
  })
);
