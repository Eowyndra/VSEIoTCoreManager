/* tslint:disable */
/* eslint-disable */
import { DeviceStatus } from './device-status';
import { IoTStatus } from './io-t-status';
export interface DeviceConfigurationViewModel {
  deviceStatus?: DeviceStatus;
  id: number;
  ioTCorePort: number;
  ioTStatus?: IoTStatus;
  name?: null | string;
  vseIpAddress: string;
  vsePort: number;
  vseType?: null | string;
}
