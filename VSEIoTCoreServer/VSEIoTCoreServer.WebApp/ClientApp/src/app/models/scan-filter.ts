import { ScanType } from "../enums/scan-type";

export interface ScanFilterUI {
  scanType: ScanType;
  vseIpAddress: string;
  vsePort: number;
  ioTCorePort: number;
  rangeIp: string;
  rangeIp_end: number;
  rangePort: number;
}
