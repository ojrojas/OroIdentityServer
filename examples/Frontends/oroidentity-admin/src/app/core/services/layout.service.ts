import { Injectable, inject, signal, computed } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LayoutService {
  private readonly bp = inject(BreakpointObserver);

  private readonly isDesktop$ = this.bp.observe([Breakpoints.Web]).pipe(map(r => r.matches));
  private readonly isTablet$ = this.bp.observe([Breakpoints.Tablet]).pipe(map(r => r.matches));

  readonly isDesktop = toSignal(this.isDesktop$, { initialValue: true });
  readonly isTablet = toSignal(this.isTablet$, { initialValue: false });
  readonly isMobile = computed(() => !this.isDesktop() && !this.isTablet());

  readonly sidebarCollapsed = signal(false);

  toggleSidebar(): void {
    this.sidebarCollapsed.update(v => !v);
  }

  collapseSidebarOnSmall(): void {
    if (!this.isDesktop()) this.sidebarCollapsed.set(true);
  }
}
