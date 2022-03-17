/* tslint:disable */
/* eslint-disable */
import { DeviceStatus } from './device-status';
import { IoTStatus } from './io-t-status';
export interface DeviceConfigurationViewModel {
  deviceStatus?: DeviceStatus;
  id: number;
  ioTCorePort: number;
  ioTStatus?: IoTStatus;
  vseIpAddress: string;
  vsePort: number;
  vseType: string;
}
