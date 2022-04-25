import { interval, Observable, Subscription } from 'rxjs';
import { filter, take } from 'rxjs/operators';

import { Injectable } from '@angular/core';
import { IfmLoggingService } from '@ifm/sdk';

import { GlobalIoTCoreStatus, GlobalIoTCoreStatusViewModel } from '../api/models';
import { GlobalService } from '../api/services';
import { ServerInformation } from '../models/server-information';
import { ServerStoreService } from './server-store.service';
import { UtilitiesService } from './utilities.service';

@Injectable({
  providedIn: 'root',
})
export class ServerService {
  globalState$: Observable<ServerInformation | undefined> = this.serverStore.state$;
  liveStatusRefresh: Subscription | undefined;
  private readonly logger;

  constructor(private readonly apiService: GlobalService, private readonly serverStore: ServerStoreService, log: IfmLoggingService) {
    this.logger = log.withStaticScope('Server service');
  }

  startServer(): void {
    this.logger.info('Starting...');
    if (this.serverStore.state?.globalStatus === GlobalIoTCoreStatus.Stopped) {
      this.apiService.apiV1GlobalStartPost().subscribe(
        () => {
          this.logger.info('Started successfully.');
        },
        (error) => {
          this.logger.error('Start error: ', error);
        }
      );
    } else {
      this.logger.warn('Server is not stopped.');
    }
  }

  stopServer(): void {
    this.logger.info('Stopping...');
    if (this.serverStore.state?.globalStatus === GlobalIoTCoreStatus.Started || this.serverStore.state?.globalStatus === GlobalIoTCoreStatus.PartlyRunning) {
      this.apiService.apiV1GlobalStopPost().subscribe(
        () => {
          this.logger.info('Stopped successfully.');
        },
        (error) => {
          this.logger.error('Stop error: ', error);
        }
      );
    } else {
      this.logger.warn('Not running.');
    }
  }

  restartServer(): void {
    this.logger.info('Restarting...');
    if (this.serverStore.state?.globalStatus === GlobalIoTCoreStatus.Started) {
      this.apiService
        .apiV1GlobalStopPost()
        .pipe(take(1))
        .subscribe(
          () => {
            this.logger.info('Stopped successfully.');
            this.serverStore.state$
              .pipe(
                filter((server) => server?.globalStatus === GlobalIoTCoreStatus.Stopped),
                take(1)
              )
              .subscribe(() => {
                this.startServer();
              });
          },
          (error) => {
            this.logger.error('Stop error: ', error);
          }
        );
    } else {
      this.startServer();
    }
  }

  getStatus(): void {
    this.apiService
      .apiV1GlobalStatusGet$Json()
      .pipe(
        filter((serviceStatus) => UtilitiesService.isNotNullOrUndefined(serviceStatus)),
        take(1)
      )
      .subscribe(
        (globalStatus: GlobalIoTCoreStatusViewModel) => {
          if (this.serverStore.state === undefined) {
            this.serverStore.update({ globalStatus: globalStatus.status });

            this.logger.info('Status loaded: ', globalStatus.status);
          } else {
            if (globalStatus !== this.serverStore.state?.globalStatus) {
              this.serverStore.update({ globalStatus: globalStatus.status });

              this.logger.info('Status changed: ', globalStatus.status);
            }
          }
        },
        (error) => {
          this.logger.error('Error loading devices status: ', error.error);
        }
      );
  }

  startLiveStatusRefresh(): void {
    this.getStatus();
    // call api every second to get OPC UA service status
    const observable = interval(1000);
    this.liveStatusRefresh = observable.subscribe((_) => this.getStatus());
  }

  stopLiveStatusRefresh(): void {
    if (UtilitiesService.isNotNullOrUndefined(this.liveStatusRefresh)) {
      if (!this.liveStatusRefresh?.closed) {
        this.liveStatusRefresh?.unsubscribe();
      }
    }
  }
}
