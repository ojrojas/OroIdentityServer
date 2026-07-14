import { Directive, inject, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { LayoutService } from '../../core/services/layout.service';

type Breakpoint = 'desktop' | 'tablet' | 'mobile';

@Directive({ selector: '[appShowOn]', standalone: true })
export class ShowOnDirective {
  private readonly layout = inject(LayoutService);
  private readonly tplRef = inject(TemplateRef<unknown>);
  private readonly vcRef = inject(ViewContainerRef);

  @Input() set appShowOn(bp: Breakpoint) {
    const show = bp === 'desktop' ? this.layout.isDesktop()
               : bp === 'tablet'  ? this.layout.isTablet()
               : this.layout.isMobile();
    this.vcRef.clear();
    if (show) this.vcRef.createEmbeddedView(this.tplRef);
  }
}
