import { Injectable } from '@angular/core';
import { IfmBaseStore } from '@ifm/sdk';

import { ServerInformation } from '../models/server-information';

@Injectable({
  providedIn: 'root',
})
export class ServerStoreService extends IfmBaseStore<ServerInformation | undefined> {
  constructor() {
    super(undefined);
  }

  update(settings: ServerInformation): void {
    if (this.state !== undefined) {
      this.patchState(settings);
    } else {
      this.setState(settings);
    }
  }
}
