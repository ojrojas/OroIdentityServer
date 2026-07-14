import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-loading-bar',
  standalone: true,
  template: `
    @if (visible()) {
      <div class="loading-bar" role="progressbar" aria-label="Loading...">
        <div class="loading-bar-fill"></div>
      </div>
    }
  `,
  styles: [`
    .loading-bar {
      position: fixed; top: 0; left: 0; right: 0; z-index: 9999;
      height: 3px; background: transparent;
    }
    .loading-bar-fill {
      height: 100%; background: var(--oro-primary);
      animation: loading-progress 1.5s ease-in-out infinite;
    }
    @keyframes loading-progress {
      0% { width: 0%; margin-left: 0; }
      50% { width: 70%; margin-left: 15%; }
      100% { width: 0%; margin-left: 100%; }
    }
  `]
})
export class LoadingBarComponent {
  readonly visible = signal(false);
  show(): void { this.visible.set(true); }
  hide(): void { this.visible.set(false); }
}
