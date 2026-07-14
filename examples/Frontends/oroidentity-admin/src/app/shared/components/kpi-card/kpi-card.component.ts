import { Component, Input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { KpiData } from '../../../core/models';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [DecimalPipe],
  template: `
    <div class="kpi-card oro-card" [class.clickable]="!!kpi.route"
         [attr.role]="kpi.route ? 'button' : null"
         [attr.tabindex]="kpi.route ? 0 : null">
      <div class="kpi-header">
        <span class="kpi-icon">{{ kpi.icon }}</span>
        <span class="kpi-label text-muted text-sm">{{ kpi.label }}</span>
      </div>
      <div class="kpi-value">
        {{ kpi.value }}
        @if (kpi.unit) { <span class="kpi-unit text-muted text-sm">{{ kpi.unit }}</span> }
      </div>
      @if (kpi.trend !== undefined) {
        <div class="kpi-trend" [class.positive]="kpi.trend >= 0" [class.negative]="kpi.trend < 0">
          {{ kpi.trend >= 0 ? '↑' : '↓' }} {{ kpi.trend | number:'1.1-1' }}%
        </div>
      }
    </div>
  `,
  styles: [`
    .kpi-card {
      display: flex; flex-direction: column; gap: var(--oro-space-2);
      padding: var(--oro-space-5); cursor: default;
    }
    .kpi-card.clickable { cursor: pointer; transition: box-shadow var(--oro-transition); }
    .kpi-card.clickable:hover { box-shadow: var(--oro-shadow-md); }
    .kpi-header { display: flex; align-items: center; gap: var(--oro-space-2); }
    .kpi-icon { font-size: 1.4rem; }
    .kpi-value { font-size: var(--oro-text-2xl); font-weight: 700; color: var(--oro-text-primary); }
    .kpi-unit { font-size: var(--oro-text-sm); font-weight: 400; }
    .kpi-trend { font-size: var(--oro-text-sm); font-weight: 500; }
    .kpi-trend.positive { color: var(--oro-success); }
    .kpi-trend.negative { color: var(--oro-danger); }
  `]
})
export class KpiCardComponent {
  @Input({ required: true }) kpi!: KpiData;
}
