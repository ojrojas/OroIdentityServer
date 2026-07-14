import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly http = inject(HttpClient);
  private readonly translate = inject(TranslateService);

  init(): void {
    this.translate.setDefaultLang('es');
    const saved = localStorage.getItem('language') ?? 'es';
    this.translate.use(saved);
  }

  setLanguage(lang: string): void {
    this.translate.use(lang);
    localStorage.setItem('language', lang);
  }

  get currentLang(): string {
    return this.translate.currentLang ?? 'es';
  }
}
