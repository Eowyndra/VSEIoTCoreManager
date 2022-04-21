import { Injectable } from '@angular/core';
import { IfmLoggingService } from '@ifm/sdk';
import { Observable, of } from 'rxjs';
import { filter, map, take } from 'rxjs/operators';
import { AddDeviceViewModel, DeviceConfigurationViewModel, StatusViewModel } from '../api/models';
import { DeviceService } from '../api/services';
import { AddDeviceUI } from '../models/add-device';
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

  addDevices(addDevices: AddDeviceUI[]): Observable<void> {
    var addDevicesModel = new Array<AddDeviceViewModel>();
    addDevices.forEach(device => {
      var addDeviceModel = mapper.map<AddDeviceUI, AddDeviceViewModel>(device, 'AddDeviceViewModel', 'AddDeviceUI');
      addDevicesModel.push(addDeviceModel);
    });
    return this.deviceApi.apiV1DevicePost({ body: addDevicesModel });
  }
}
