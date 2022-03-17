import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';

import { Component, OnDestroy } from '@angular/core';

import { GlobalIoTCoreStatus } from '../../api/models';
import { ServerService } from '../../services/server.service';
import { UtilitiesService } from '../../services/utilities.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent implements OnDestroy {
  public GlobalStatusEnum = GlobalIoTCoreStatus;
  public globalStatus: GlobalIoTCoreStatus | undefined;
  private readonly destroyed$ = new Subject();

  constructor(private readonly serverService: ServerService) {
    this.liveServerStatus();
  }

  public ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  private liveServerStatus(): void {
    // react to VSE IoT Core service status changes
    this.serverService.globalState$
      .pipe(
        filter((settings) => UtilitiesService.isNotNullOrUndefined(settings)),
        takeUntil(this.destroyed$)
      )
      .subscribe((settings) => {
        this.globalStatus = settings?.globalStatus;
      });
  }
}
