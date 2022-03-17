import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { environment } from '../environments/environment';

//components
import { AppComponent } from './app.component';
import { HeaderComponent } from './components/header/header.component';
import { HelpCentreComponent } from './components/header/help-centre/help-centre.component';
import { ServerStatusComponent } from './components/server-status/server-status.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { AppRoutingModule } from './app-routing.module';

//Services
import { UtilitiesService } from './services/utilities.service';

//Material
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatDialogModule } from '@angular/material/dialog';

//ifm
import {
  IfmActionBarModule,
  IfmButtonModule,
  IfmCardModule,
  IfmIconModule,
  IfmLinkModule,
  IfmMenuModule,
  IfmScrollPanelModule,
  IfmTemplateModule
} from '@ifm/components';
import { IfmLoggingService, IfmLogLevel, IfmStateModule, IfmStateService, IfmTransModule, SetIfmLogLevel } from '@ifm/sdk';
import { IfmInfoDialogModule, IfmMasterDetailModule } from 'ifm-ui-components';

//Third Party
import { getBrowserCultureLang } from '@ngneat/transloco';
import { first } from 'rxjs/operators';


if (environment.production) {
  SetIfmLogLevel('VSE IoT Core Server Config', IfmLogLevel.Info);
} else {
  SetIfmLogLevel('VSE IoT Core  Server Config', IfmLogLevel.Debug);
}

const availableLanguages = ['en-GB'];



@NgModule({
  declarations: [
    AppComponent,
    //Header
    HeaderComponent,
    HelpCentreComponent,

    //Server Status Page
    ServerStatusComponent,

    //Sidebar
    SidebarComponent,
  ],
  imports: [
    //Routing & Standard Modules
    AppRoutingModule,
    BrowserAnimationsModule,
    BrowserModule,
    HttpClientModule,
    FormsModule,

    //ifm UI Modules
    IfmActionBarModule,
    IfmButtonModule,
    IfmCardModule,
    IfmIconModule,
    IfmInfoDialogModule,
    IfmLinkModule,
    IfmMenuModule,
    IfmTemplateModule,
    IfmScrollPanelModule,
    IfmMasterDetailModule,

    //Material
    MatToolbarModule,
    MatDialogModule,

    //Libs
    IfmStateModule.forRoot(),
    IfmTransModule.forRoot(
      {
        prodMode: environment.production,
        availableLangs: availableLanguages,
        defaultLang: 'en-GB',
        fallbackLang: 'en-GB',
        missingHandler: {
          useFallbackTranslation: true,
          logMissingKey: false,
        },
      },
      {
        host: UtilitiesService.baseUrl(),
      }
    ),
    IfmTransModule.forChild('vseiot')
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
  private readonly logger;

  constructor(private readonly ifmState: IfmStateService, log: IfmLoggingService) {
    this.logger = log.withStaticScope('App module');
    this.setBrowserLanguage();
  }

  public setBrowserLanguage(): void {
    const browserLang: string = getBrowserCultureLang();
    this.logger.debug(`Browser language: ${browserLang}`);
    if (availableLanguages.find((s) => s === browserLang) && browserLang !== 'en-CA') {
      this.ifmState.setLanguage(browserLang);
    } else {
      this.logger.info(`Language ${browserLang} does not exist in our translations. Setting language to en-GB.`);
      this.ifmState.setLanguage('en-GB');
    }

    this.ifmState
      .selectLanguage$()
      .pipe(first())
      .subscribe((lang) => {
        document.getElementsByTagName('html').item(0)?.setAttribute('lang', lang.toLowerCase());
      });
  }
}
