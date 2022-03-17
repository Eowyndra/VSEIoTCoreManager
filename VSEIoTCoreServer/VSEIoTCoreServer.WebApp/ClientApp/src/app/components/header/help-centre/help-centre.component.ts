import { Subject } from 'rxjs';
import { filter, map, takeUntil } from 'rxjs/operators';

import { Component, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { UseIfmLogging } from '@ifm/sdk';

@Component({
  selector: 'app-help-centre',
  templateUrl: './help-centre.component.html',
  styleUrls: ['./help-centre.component.scss'],
  providers: UseIfmLogging('License Info Component'),
})
export class HelpCentreComponent implements OnDestroy {
  public year: number = new Date().getFullYear();
  destroyed$ = new Subject();

  public appName = "MyApp";  //needs to be gathered from Constant or backend

  public version = "1.0.0"; //needs to be gathered from Constant or backend

  constructor(public dialog: MatDialog) {}

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  displayLicenses(): void {
    //Implement Licenses Dialog
  }
}
