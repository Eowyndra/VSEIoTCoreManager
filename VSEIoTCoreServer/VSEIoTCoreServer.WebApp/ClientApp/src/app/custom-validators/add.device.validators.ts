import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { FormModel } from '../directives/config-page.directive';
import { ScanFilterUI } from '../models/scan-filter';

const ipPattern = '^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[1-9])[.])((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])[.]){2}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])$';

export class AddDeviceValidators {

  static usedPortsList: number[];

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

  static checkPortAvailable({ value }: { value: number }): ValidationErrors | null {
    value = +value;
    // console.log("Value%o", value)
    if (!AddDeviceValidators.usedPortsList || AddDeviceValidators.usedPortsList === null || AddDeviceValidators.usedPortsList === undefined) {
      return null;
    }
    if (value !== null && value !== undefined) {
      // global IoT Core port is not available
      // todo get global IoT Core port from settings when it is implemented
      if (value === 8090) {
        return { alreadyUsed: true };
      }
      
      if (AddDeviceValidators.usedPortsList.indexOf(value) > -1) {
          return { alreadyUsed: true };
      }

      return null;
    }
    return null;
  }

}
