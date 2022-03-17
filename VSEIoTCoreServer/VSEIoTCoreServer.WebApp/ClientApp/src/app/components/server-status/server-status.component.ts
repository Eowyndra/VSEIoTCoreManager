import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';

import { Component, OnDestroy } from '@angular/core';

import { GlobalIoTCoreStatus } from '../../api/models';
import { ServerService } from '../../services/server.service';

@Component({
  selector: 'app-server-status',
  templateUrl: './server-status.component.html',
  styleUrls: ['./server-status.component.scss'],
})
export class ServerStatusComponent implements OnDestroy {
  public GlobalStatusEnum = GlobalIoTCoreStatus;
  public globalStatus: GlobalIoTCoreStatus | undefined;
  private readonly destroyed$ = new Subject();

  constructor(private readonly serverService: ServerService) {
    this.initializeSubscriptions();
  }

  public ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  public startServer(): void {
    this.serverService.startServer();
  }

  public stopServer(): void {
    this.serverService.stopServer();
  }

  private initializeSubscriptions(): void {
    this.serverService.globalState$
      .pipe(
        filter((settings) => settings !== undefined),
        takeUntil(this.destroyed$)
      )
      .subscribe((settings) => {
        this.globalStatus = settings?.globalStatus;
      });
  }
}
