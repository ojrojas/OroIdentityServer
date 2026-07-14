import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CopilotChatPanelComponent } from '../../../features/copilot/copilot-chat-panel.component';
import { SettingsStore } from '../../../features/settings/settings.store';
import { AuthService } from '../../auth/auth.service';
import { LayoutService } from '../../services/layout.service';
import { ToastComponent } from '../../../shared/components/toast/toast.component';
import { HeaderComponent } from '../header/header.component';
import { LoadingBarComponent } from '../loading-bar/loading-bar.component';
import { SidebarComponent } from '../sidebar/sidebar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent, LoadingBarComponent, CopilotChatPanelComponent, ToastComponent],
  template: `
    <app-loading-bar />
    <app-sidebar />
    <app-header />
    <main class="shell-content" [class.sidebar-collapsed]="layout.sidebarCollapsed()">
      <router-outlet />
    </main>
    <app-copilot-chat-panel />
    <app-toast />
  `,
  styles: [`
    :host { display: block; height: 100vh; }
    .shell-content {
      margin-left: var(--oro-sidebar-width);
      margin-top: var(--oro-header-height);
      min-height: calc(100vh - var(--oro-header-height));
      padding: var(--oro-space-6);
      background: var(--oro-bg);
      transition: margin-left var(--oro-transition-slow);
    }
    .shell-content.sidebar-collapsed {
      margin-left: var(--oro-sidebar-collapsed);
    }
    @media (max-width: 1023px) {
      .shell-content { margin-left: 0; }
    }
  `]
})
export class ShellComponent implements OnInit {
  readonly layout = inject(LayoutService);
  private readonly settingsStore = inject(SettingsStore);
  private readonly profile = inject(AuthService).userProfile();

  ngOnInit(): void {
    this.settingsStore.loadPreferences(this.profile?.sub as string, this.profile?.tenantId as string);
    const collapsed = this.settingsStore.prefs().sidebarCollapsed as boolean | undefined;
    if (collapsed !== undefined) {
      this.layout.sidebarCollapsed.set(collapsed);
    }
  }
}
