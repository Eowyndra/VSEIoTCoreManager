import { DeviceStatus } from "../api/models";
import { IoTStatus } from "../api/models";

export interface DeviceConfigurationUI {
    name: string;
    vseType: string;
    vseIpAddress: string;
    vsePort: number;
    ioTCorePort: number;
    ioTStatus: IoTStatus;
    deviceStatus: DeviceStatus;
}
