import { Component, inject } from '@angular/core';
import { ToastService } from '../../toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  template: `
    <div class="toast-container">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast toast-{{ toast.type }}" role="alert">
          <span class="toast-icon">{{ iconMap[toast.type] }}</span>
          <span class="toast-message">{{ toast.message }}</span>
          <button class="toast-close" (click)="toastService.dismiss(toast.id)">✕</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed; top: calc(var(--oro-header-height) + 0.5rem); right: 1rem;
      z-index: 10000; display: flex; flex-direction: column; gap: 0.5rem;
      max-width: 400px;
    }
    .toast {
      display: flex; align-items: center; gap: 0.5rem;
      padding: 0.75rem 1rem; border-radius: var(--oro-radius);
      background: var(--oro-surface); border: 1px solid var(--oro-border);
      box-shadow: var(--oro-shadow-lg); animation: slideIn 0.2s ease-out;
      font-size: var(--oro-text-sm);
    }
    .toast-success { border-left: 4px solid var(--oro-success); }
    .toast-error { border-left: 4px solid var(--oro-danger); }
    .toast-warning { border-left: 4px solid var(--oro-warning); }
    .toast-info { border-left: 4px solid var(--oro-info); }
    .toast-icon { font-size: 1.1rem; flex-shrink: 0; }
    .toast-message { flex: 1; color: var(--oro-text-primary); }
    .toast-close {
      background: none; border: none; cursor: pointer; font-size: 0.875rem;
      color: var(--oro-text-secondary); padding: 0; line-height: 1;
    }
    .toast-close:hover { color: var(--oro-text-primary); }
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class ToastComponent {
  readonly toastService = inject(ToastService);
  readonly iconMap: Record<string, string> = {
    success: '✓', error: '✕', warning: '⚠', info: 'ℹ',
  };
}
