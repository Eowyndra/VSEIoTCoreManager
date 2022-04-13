import { Injectable } from '@angular/core';
import { IfmLoggingService } from '@ifm/sdk';
import { Observable } from 'rxjs';
import { filter, map, take } from 'rxjs/operators';
import { DeviceConfigurationViewModel, StatusViewModel } from '../api/models';
import { DeviceService } from '../api/services';
import { DeviceConfigurationUI } from '../models/device-configuration';
import { mapper } from './mapper';

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService {
  public configurationChanged = false;
  public modelInvalid = false;
  private readonly logger;

  constructor(
    private readonly deviceApi: DeviceService,
    log: IfmLoggingService
  ) {
    this.logger = log.withStaticScope('Configuration service');
  }

  getDevices(): Observable<DeviceConfigurationViewModel[]> {
    return this.deviceApi.apiV1DeviceGet$Json()
               .pipe(
                 filter((result) => result !== undefined),
                 take(1),
                 map((result) => {
                   return result;
                 })
               );
  }

  getDevicesUI(): Observable<DeviceConfigurationUI[]> {
    return this.deviceApi.apiV1DeviceGet$Json()
               .pipe(
                 filter((result) => result !== undefined),
                 take(1),
                 map((result) => {
                   return result.map((s) =>
                     mapper.map<DeviceConfigurationViewModel, DeviceConfigurationUI>(s, 'DeviceConfigurationUI', 'DeviceConfigurationViewModel')
                   );
                 })
               );
  }

  getDeviceStatus(id: number): Observable<StatusViewModel> {
    return this.deviceApi.apiV1DeviceIdStatusGet$Json({ id })
    .pipe(
      filter((result) => result !== undefined),
      take(1),
      map((result) => {
        return result;
      })
    );
  }
}
