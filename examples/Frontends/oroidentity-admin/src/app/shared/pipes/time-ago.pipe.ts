import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'timeAgo', standalone: true, pure: false })
export class TimeAgoPipe implements PipeTransform {
  transform(value: string | Date): string {
    const date = value instanceof Date ? value : new Date(value);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    if (diffSec < 60) return 'hace un momento';
    const diffMin = Math.floor(diffSec / 60);
    if (diffMin < 60) return `hace ${diffMin} min`;
    const diffHr = Math.floor(diffMin / 60);
    if (diffHr < 24) return `hace ${diffHr} h`;
    const diffDay = Math.floor(diffHr / 24);
    if (diffDay < 7) return `hace ${diffDay} días`;
    const diffWk = Math.floor(diffDay / 7);
    if (diffWk < 5) return `hace ${diffWk} sem`;
    const diffMo = Math.floor(diffDay / 30);
    if (diffMo < 12) return `hace ${diffMo} mes${diffMo > 1 ? 'es' : ''}`;
    return `hace ${Math.floor(diffMo / 12)} año${Math.floor(diffMo / 12) > 1 ? 's' : ''}`;
  }
}
