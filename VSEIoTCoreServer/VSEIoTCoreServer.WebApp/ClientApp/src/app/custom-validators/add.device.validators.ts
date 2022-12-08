import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { FormModel } from '../directives/config-page.directive';
import { AddDeviceUI } from '../models/add-device';
import { ScanFilterUI } from '../models/scan-filter';

const ipPattern = '^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[1-9])[.])((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])[.]){2}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])$';

export class AddDeviceValidators {

  static usedPortsList: number[];
  static listedDevices: AddDeviceUI[];

  static formModel: FormModel<ScanFilterUI> = {
    scanType: new FormControl('ScanType.Specific'),
    vseIpAddress: new FormControl('', [Validators.required, Validators.pattern(ipPattern)]),
    vsePort: new FormControl('', [Validators.required, Validators.min(1), Validators.max(65535)]),
    ioTCorePort: new FormControl('', [Validators.required, Validators.min(1), Validators.max(65535), AddDeviceValidators.checkPortAvailable]),
    rangeIp: new FormControl('', [Validators.required, Validators.pattern(ipPattern)]),
    rangeIp_end: new FormControl('', [Validators.required, Validators.min(0), Validators.max(255)]),
    rangePort: new FormControl('', [Validators.required, Validators.min(1), Validators.max(65535)])
  };

  static parentForm = new FormGroup(AddDeviceValidators.formModel);

  static setParentForm(form: FormGroup): void {
    AddDeviceValidators.parentForm = form;
  }

  static setUsedPortsList(usedPortsList: Array<number>): void {
    AddDeviceValidators.usedPortsList = usedPortsList;
  }

  static setListedDevices(listedDevices: Array<AddDeviceUI>): void {
    AddDeviceValidators.listedDevices = listedDevices;
  }

  static checkPortAvailable({ value }: { value: number }): ValidationErrors | null {
    // Make sure the value is a number, not a string
    value = +value;

    if (!AddDeviceValidators.usedPortsList || AddDeviceValidators.usedPortsList === null || AddDeviceValidators.usedPortsList === undefined) {
      return null;
    }

    if (value !== null && value !== undefined && AddDeviceValidators.usedPortsList.indexOf(value) > -1) { 
      return { alreadyUsed: true };
    }
    
    return null;
  }

  static checkAlreadyInList({ value }: { value: Partial<AddDeviceUI> }): ValidationErrors | null {
    if (!AddDeviceValidators.listedDevices || AddDeviceValidators.listedDevices === null || AddDeviceValidators.listedDevices === undefined) {
      return null;
    }

    if (AddDeviceValidators.parentForm.get('vseIpAddress')?.value !== null && AddDeviceValidators.parentForm.get('vsePort')?.value !== null) {
      var ip = AddDeviceValidators.parentForm.get('vseIpAddress')?.value;
      var port : number = +AddDeviceValidators.parentForm.get('vsePort')?.value;

      // Check if a device with the same vseIpAddress and vsePort is already in the list of devices
      var alreadyInList : boolean = AddDeviceValidators.listedDevices.find((s) => s.vseIpAddress === ip && s.vsePort === port) !== undefined;

      if (alreadyInList)
      {
        return { alreadyInList: true };
      }

      return null;
    }
    return null;
  }

  static updateIpValidity({ value }: { value: Partial<AddDeviceUI> }): ValidationErrors | null {
    if (AddDeviceValidators.parentForm.get('vseIpAddress')?.value !== null) {
      
      // After the vsePort has been changed re-trigger a validity check on the vseIpAddress
      AddDeviceValidators.parentForm.get('vseIpAddress')?.updateValueAndValidity();
      return null;
    }
    return null;
  }

}
