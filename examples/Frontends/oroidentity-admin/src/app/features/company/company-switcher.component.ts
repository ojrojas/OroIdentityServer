import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { CompanyStore } from './company.store';
import { Company } from '../../core/models';

@Component({
  selector: 'app-company-switcher',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  template: `
    <div class="company-switcher">
      <button class="switcher-trigger oro-btn oro-btn-ghost" (click)="toggleDropdown()">
        <span class="company-icon">🏢</span>
        <span class="company-name truncate">{{ store.activeCompanyName() || ('COMMON.SEARCH' | translate) }}</span>
        <span>▾</span>
      </button>
      @if (open()) {
        <div class="switcher-dropdown oro-card">
          @if (store.companies().length > 10) {
            <input class="oro-input search-input" placeholder="Buscar empresa..." [(ngModel)]="searchTerm"
                   (input)="onSearch($event)" />
          }
          <ul class="company-list">
            @for (company of filtered(); track company.id) {
              <li>
                <button class="company-item" [class.active]="company.id === store.activeCompany()?.id"
                        (click)="select(company)">
                  <span class="company-icon">🏢</span>
                  <div class="company-info">
                    <span class="company-item-name">{{ company.name }}</span>
                    <span class="company-item-tax text-muted text-sm">{{ company.taxId }}</span>
                  </div>
                  @if (company.id === store.activeCompany()?.id) { <span>✓</span> }
                </button>
              </li>
            }
          </ul>
        </div>
      }
    </div>
  `,
  styles: [`
    .company-switcher { position: relative; }
    .switcher-trigger { gap: var(--oro-space-2); max-width: 220px; }
    .company-name { max-width: 140px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .switcher-dropdown {
      position: absolute; top: calc(100% + 4px); left: 0;
      width: 280px; padding: var(--oro-space-2) 0; z-index: 200;
      box-shadow: var(--oro-shadow-lg);
    }
    .search-input { margin: var(--oro-space-2) var(--oro-space-3); width: calc(100% - 1.5rem); }
    .company-list { list-style: none; max-height: 300px; overflow-y: auto; }
    .company-item {
      display: flex; align-items: center; gap: var(--oro-space-3);
      width: 100%; padding: var(--oro-space-2) var(--oro-space-4);
      background: transparent; border: none; cursor: pointer; text-align: left;
      transition: background var(--oro-transition);
    }
    .company-item:hover { background: var(--oro-bg); }
    .company-item.active { color: var(--oro-primary); font-weight: 500; }
    .company-info { display: flex; flex-direction: column; flex: 1; min-width: 0; }
    .company-item-name { font-size: var(--oro-text-sm); font-weight: 500; }
    .company-icon { font-size: 1rem; }
  `]
})
export class CompanySwitcherComponent {
  readonly store = inject(CompanyStore);
  readonly open = signal(false);
  searchTerm = '';

  filtered(): Company[] {
    const term = this.searchTerm.toLowerCase();
    return this.store.companies().filter(c =>
      !term || c.name.toLowerCase().includes(term) || c.taxId.includes(term)
    );
  }

  toggleDropdown(): void { this.open.update(v => !v); }

  select(company: Company): void {
    this.store.setActiveCompany(company);
    this.open.set(false);
  }

  onSearch(e: Event): void {
    this.searchTerm = (e.target as HTMLInputElement).value;
  }
}
