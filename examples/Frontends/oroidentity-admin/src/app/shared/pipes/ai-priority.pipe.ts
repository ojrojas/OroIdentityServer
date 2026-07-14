import { Pipe, PipeTransform } from '@angular/core';

type Priority = 'Critical' | 'High' | 'Medium' | 'Low';

@Pipe({ name: 'aiPriority', standalone: true })
export class AiPriorityPipe implements PipeTransform {
  private readonly map: Record<Priority, { label: string; icon: string; css: string }> = {
    Critical: { label: 'Crítica', icon: '🔴', css: 'priority-critical' },
    High:     { label: 'Alta',    icon: '🟠', css: 'priority-high' },
    Medium:   { label: 'Media',   icon: '🟡', css: 'priority-medium' },
    Low:      { label: 'Baja',    icon: '🟢', css: 'priority-low' },
  };

  transform(value: Priority, mode: 'label' | 'icon' | 'css' = 'label'): string {
    return this.map[value]?.[mode] ?? value;
  }
}
