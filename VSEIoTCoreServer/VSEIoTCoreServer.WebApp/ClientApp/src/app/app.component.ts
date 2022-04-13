import { Location } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ConfigurationService } from './services/device-config.service';

import { ServerService } from './services/server.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  constructor(
    private location: Location,
    private readonly serverService: ServerService,
    readonly configurationService: ConfigurationService
  ) { }

  ngOnInit(): void {
    this.serverService.startLiveStatusRefresh();
  }
}
