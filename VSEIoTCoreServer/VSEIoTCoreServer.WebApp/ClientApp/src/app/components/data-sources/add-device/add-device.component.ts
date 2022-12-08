import { AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { SelectionModel } from '@angular/cdk/collections';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { IfmDialogService } from '@ifm/components';
import { UseIfmLogging } from '@ifm/sdk';
import { AddDeviceUI } from 'src/app/models/add-device';
import { FormModel } from 'src/app/directives/config-page.directive';
import { ScanFilterUI } from 'src/app/models/scan-filter';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { ScanType } from 'src/app/enums/scan-type';
import { DeviceConfigurationViewModel } from 'src/app/api/models';
import { AddDeviceValidators } from 'src/app/custom-validators/add.device.validators';

const ipPattern = '^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[1-9])[.])((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])[.]){2}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])$';

@Component({
  selector: 'app-add-device',
  templateUrl: './add-device.component.html',
  styleUrls: ['./add-device.component.scss'],
  providers: [IfmDialogService, UseIfmLogging('Add Device component')]
})
export class AddDeviceComponent implements OnInit, AfterViewInit, OnDestroy {
  public ScanType = ScanType;
  defaultScanType = ScanType.Specific;
  columns: string[] = ['checkbox', 'vseIpAddress', 'vsePort', 'ioTCorePort', 'onboardStatus'];
  dataSource: MatTableDataSource<AddDeviceUI> = new MatTableDataSource<AddDeviceUI>();
  selection = new SelectionModel<AddDeviceUI>(true, []);
  @ViewChild(MatSort) sort = new MatSort();
  public isLoading = false;
  public searched = false; // will become true after first device scan/add to list
  public formModel: FormModel<ScanFilterUI> = {
    scanType: new FormControl('ScanType.Specific'),
    vseIpAddress: new FormControl('', [Validators.required, Validators.pattern(ipPattern), AddDeviceValidators.checkAlreadyInList]),
    vsePort: new FormControl('', [Validators.required, Validators.min(1), Validators.max(65535), AddDeviceValidators.updateIpValidity]),
    ioTCorePort: new FormControl('', [Validators.required, Validators.min(1), Validators.max(65535), AddDeviceValidators.checkPortAvailable]),
    rangeIp: new FormControl('', [Validators.required, Validators.pattern(ipPattern)]),
    rangeIp_end: new FormControl('', [Validators.required, Validators.min(0), Validators.max(255)]),
    rangePort: new FormControl('', [Validators.required, Validators.min(1), Validators.max(65535)])
  };
  @Output() public form: FormGroup;
  protected readonly destroyed$ = new Subject();
  deviceList: DeviceConfigurationViewModel[] | undefined;
  usedPortsList: number[] = new Array<number>();
  // Issue#59 will make this port changeable --> ToDo: get the current global IoTCore Port from the configuration
  globalIoTCorePort: number = 8090;

  constructor(
    private cd: ChangeDetectorRef
  ) { 
    this.form = new FormGroup(this.formModel);
  }

  get scanType(): AbstractControl { return this.form.get('scanType') as AbstractControl; }

  get vseIpAddress(): AbstractControl { return this.form.get('vseIpAddress') as AbstractControl; }

  get vsePort(): AbstractControl { return this.form.get('vsePort') as AbstractControl; }

  get ioTCorePort(): AbstractControl { return this.form.get('ioTCorePort') as AbstractControl; }

  get rangeIp(): AbstractControl { return this.form.get('rangeIp') as AbstractControl; }

  get rangeIp_end(): AbstractControl { return this.form.get('rangeIp_end') as AbstractControl; }

  get rangePort(): AbstractControl { return this.form.get('rangePort') as AbstractControl; }

  ngOnInit(): void {
    AddDeviceValidators.setParentForm(this.form);
  }

  ngAfterViewInit(): void {
    this.updateUsedPortsList();
    this.updateAlreadyListed();
    this.ioTCorePort.setValue(this.findFreePort());
    this.dataSource.sort = this.sort;
    this.cd.detectChanges();
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  scanNetwork(): void {
    //not yet implemented
  }

  addDevice(scanFilter: ScanFilterUI): AddDeviceUI {
    const vseIpAddress = scanFilter.vseIpAddress;
    const vsePort: number = +scanFilter.vsePort;

    // Check if a device with the same vseIpAddress and vsePort is already configured
    var alreadyExists = this.deviceList?.find((s) => s.vseIpAddress === vseIpAddress && s.vsePort === vsePort);

    // If the device is already on-boarded, display the IoTCore Port it already has
    const ioTCorePort: number = alreadyExists !== undefined ? +alreadyExists.ioTCorePort : +scanFilter.ioTCorePort;
    const onboardStatus: boolean = alreadyExists !== undefined;

    const device: AddDeviceUI = {
      name: '',  //name field not defined in UI, name will be automatically assigned in the back-end
      vseIpAddress,
      vsePort,
      ioTCorePort,
      onboardStatus
    };

    return device;
  }

  specific(): void {
    this.selection.clear();
    this.isLoading = true;
    this.searched = true;
    const device = this.addDevice(this.form.getRawValue());
    var devices = this.dataSource.data;
    devices.push(device);
    this.dataSource.data = devices;
    this.isLoading = false;
    this.updateUsedPortsList();
    this.updateAlreadyListed();
    this.ioTCorePort.setValue(this.findFreePort());
  }

  range(): void {
    //not yet implemented
  }

  isAllSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.filter((s) => s.onboardStatus === false).length;
    return numSelected === numRows;
  }

  masterToggle(): void {
    this.isAllSelected() ? this.selection.clear() : this.dataSource._orderData(this.dataSource.data).forEach(row => {
      if (row.onboardStatus === false) {
        this.selection.select(row);
      }
    });
  }

  selectableRows(): number {
    const notOnboarded = this.dataSource.data.filter((s) => s.onboardStatus === false).length;
    return notOnboarded;
  }

  typeChanged(): void {
    this.dataSource.data = [];
    this.searched = false;
  }

  tableHidden(): boolean {
    return true;
  }

  updateUsedPortsList(): void {
    // Add the IoTCore Port of each device from the data base
    this.deviceList?.forEach(device => {
      if (!this.usedPortsList?.includes(device.ioTCorePort)) {
        this.usedPortsList?.push(device.ioTCorePort);
      }
    });

    // Add the IoTCore Port of each device currently in the add-device dialog list
    this.dataSource.data.forEach(device => {
      if (!this.usedPortsList?.includes(device.ioTCorePort)) {
        this.usedPortsList?.push(device.ioTCorePort);
      }
    });

    // Add the global IoTCore Port
    this.usedPortsList?.push(this.globalIoTCorePort);

    // Update the validator with the current list of already used ports
    AddDeviceValidators.setUsedPortsList(this.usedPortsList);
  }

  updateAlreadyListed(): void {
    // Update the validator with the current list of devices in the add-device dialog
    AddDeviceValidators.setListedDevices(this.dataSource.data);

    // Retrigger validity check
    this.formModel.vseIpAddress.updateValueAndValidity();
  }

  findFreePort(): number {
    // The range of ports starts at the global IoTCore Port +1
    var freePort:number = this.globalIoTCorePort+1;

    while(true) {
      if(this.isPortFree(freePort)) {
        break;
      }
      freePort++;
      continue;
    }

    return freePort;
  }

  isPortFree(port: number): boolean {
    var portTaken = this.usedPortsList?.includes(port) as boolean;
    return portTaken ? false : true; 
  }
}
