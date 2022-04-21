import { SelectionModel } from '@angular/cdk/collections';
import { AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { IfmDialogService } from '@ifm/components';
import { IfmLoggingService, UseIfmLogging } from '@ifm/sdk';
import { DeviceConfigurationUI } from 'src/app/models/device-configuration';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ConfigPageDirective, FormModel } from 'src/app/directives/config-page.directive';
import { ConfigurationService } from 'src/app/services/device-config.service';
import { MatDialog } from '@angular/material/dialog';
import { interval } from 'rxjs';
import { filter, take, takeUntil } from 'rxjs/operators';
import { DeviceConfigurationViewModel, DeviceStatus, IoTStatus, StatusViewModel } from 'src/app/api/models';
import { AddDeviceComponent } from './add-device/add-device.component';
import { mapper } from 'src/app/services/mapper';

@Component({
  selector: 'app-data-sources',
  templateUrl: './data-sources.component.html',
  styleUrls: ['./data-sources.component.scss'],
  providers: [IfmDialogService, UseIfmLogging('Data sources component')]
})
export class DataSourcesComponent extends ConfigPageDirective implements OnInit, AfterViewInit, OnDestroy {
  public IoTStatus = IoTStatus;
  public DeviceStatus = DeviceStatus;
  formModel: FormModel<DeviceConfigurationUI> = {
    name: new FormControl(''),
    vseType: new FormControl(''),
    vseIpAddress: new FormControl(''),
    vsePort: new FormControl(''),
    ioTCorePort: new FormControl(''),
    ioTStatus: new FormControl(''),
    deviceStatus: new FormControl('')
  };
  form = new FormGroup(this.formModel);
  columns: string[] = ['checkbox', 'name', 'vseType', 'vseIpAddress', 'vsePort', 'ioTCorePort', 'ioTStatus', 'deviceStatus'];
  dataSource: MatTableDataSource<DeviceConfigurationUI> = new MatTableDataSource<DeviceConfigurationUI>();
  selection = new SelectionModel<DeviceConfigurationUI>(true, []);
  @ViewChild(MatSort) sort = new MatSort();
  @ViewChild(MatPaginator) paginator: MatPaginator | undefined;
  deviceList: DeviceConfigurationViewModel[] | undefined;

  constructor(
    readonly configurationService: ConfigurationService,
    private readonly logger: IfmLoggingService,
    public dialog: MatDialog,
    private cd: ChangeDetectorRef,
    private readonly ifmDialog: IfmDialogService
  ) {
    super();
  }

  ngOnInit(): void {
    this.initializeSubscriptions();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    // tslint:disable-next-line:no-shadowed-variable
    this.dataSource.filterPredicate = (device, filter: string): boolean => device.name.toLowerCase().includes(filter);
    this.cd.detectChanges();
  }

  public loadDeviceList(): void {
    this.configurationService.getDevices()
      .pipe(
        filter(devices => devices !== undefined),
        takeUntil(this.destroyed$)
      )
      .subscribe(devices => {
        this.deviceList = devices;
        var devicesUI = new Array<DeviceConfigurationUI>();
        devices.forEach(device => {
          var deviceUI = mapper.map<DeviceConfigurationViewModel, DeviceConfigurationUI>(device, 'DeviceConfigurationUI', 'DeviceConfigurationViewModel');
          devicesUI.push(deviceUI);
        });
        this.dataSource.data = devicesUI;
      }
      );
  }

  addDevice(): void {
    const dialogRef = this.dialog.open(AddDeviceComponent, {
      autoFocus: false,
      disableClose: true
    });

    dialogRef.componentInstance.deviceList = this.deviceList;

    dialogRef.afterClosed()
      .subscribe(newDevices => {
        if (newDevices._selected?.length > 0) {
          this.configurationService.addDevices(newDevices._selected).subscribe(
            () => {
              this.logger.debug('Devices added successfully.');
              this.loadDeviceList();
            },
            (error) => {
              console.log("error adding device: " + error);
              this.logger.error('Error adding devices: ', error);
            });
        }
      });
  }

  private initializeSubscriptions(): void {
    this.loadDeviceList();
    //call api every second to get devices status
    interval(1000)
      .pipe(takeUntil(this.destroyed$))
      .subscribe(_ => {
        this.refreshStatus();
      }
      );
  }

  private refreshStatus(): void {
    this.deviceList?.forEach(device => {
      this.configurationService.getDeviceStatus(device.id)
        .pipe(
          take(1),
          takeUntil(this.destroyed$)
        )
        .subscribe((status: StatusViewModel) => {
          const dev = this.dataSource.data.find(s => s.vseIpAddress === device.vseIpAddress && s.ioTCorePort === device.ioTCorePort);
          if (dev !== undefined) {
            dev.deviceStatus = status.deviceStatus ?? DeviceStatus.Disconnected;
            dev.ioTStatus = status.ioTStatus ?? IoTStatus.Stopped;
          }
        },
          (error) => {
            this.logger.error('Error getting device status: ', error.error);
          });
    });
  }

  private getPageData(): DeviceConfigurationUI[] {
    return this.dataSource._pageData(this.dataSource._orderData(this.dataSource.filteredData));
  }

  isAllSelected(): boolean {
    return this.getPageData().every((row) => this.selection.isSelected(row));
  }

  masterToggle(): void {
    this.isAllSelected() ? this.selection.clear() : this.selection.select(...this.getPageData());
  }

  handlePageEvent(): void {
    this.selection.clear();
  }

  handleSortEvent(): void {
    this.selection.clear();
  }
}
