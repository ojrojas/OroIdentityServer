import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _theme = signal<'light' | 'dark' | 'system'>('system');
  readonly theme = this._theme.asReadonly();

  constructor() {
    const saved = localStorage.getItem('oro-theme') as 'light' | 'dark' | 'system' | null;
    if (saved) this.apply(saved);
    else this.apply('system');
  }

  apply(theme: 'light' | 'dark' | 'system'): void {
    this._theme.set(theme);
    localStorage.setItem('oro-theme', theme);
    const isDark = theme === 'dark' || (theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);
    document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
  }

  toggle(): void {
    this.apply(this._theme() === 'dark' ? 'light' : 'dark');
  }
}
