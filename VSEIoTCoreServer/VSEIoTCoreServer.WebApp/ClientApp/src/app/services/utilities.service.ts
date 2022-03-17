import { Injectable } from '@angular/core';

@Injectable()
export class UtilitiesService {  
  static baseUrl(): any {
    let base = '';

    if (window.location.origin) {
      base = window.location.origin;
    } else {
      base = window.location.protocol + '//' + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
    }

    return base.replace(/\/$/, '');
  }

  static isNotNullOrUndefined(input: any) {
    return input !== null && input !== undefined;
  }
}
