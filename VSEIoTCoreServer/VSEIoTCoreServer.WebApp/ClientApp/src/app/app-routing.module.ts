import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ServerStatusComponent } from './components/server-status/server-status.component';
import { DataSourcesComponent } from './components/data-sources/data-sources.component';

const routes: Routes = [
  { path: '', redirectTo: '/server-status', pathMatch: 'full' },
  { path: 'server-status', component: ServerStatusComponent },
  { path: 'data-sources', component: DataSourcesComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
  providers: [],
})
export class AppRoutingModule {}
