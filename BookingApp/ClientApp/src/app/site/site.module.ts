import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { RegisterComponent } from './auth/register/register.component';
import { LoginComponent } from './auth/login/login.component';
import { ForgetComponent } from './auth/forget/forget.component';
import { ResetComponent } from './auth/reset/reset.component';
import { ErrorComponent } from './error/error.component';
import { ResourceComponent } from './resource/resource.component';
import { TreeComponent } from './tree/tree.component';
import { SiteRuleComponent } from './rule/rule.component';
import { AppHeaderComponent } from './header/header.component';
import { BreadcrumbsComponent } from './breadcrumbs/breadcrumbs.component';
import { AppFooterComponent } from './footer/footer.component';
import { BookingsModule } from './bookings/bookings.module';
import { TosComponent } from './tos/tos.component';
import { FlatComponent } from './flat/flat.component';
import { MaterialModule } from '../material/material.module';
import {CarouselModule} from '../../../node_modules/primeng/carousel';
import { NgxSpinnerModule } from 'ngx-spinner';
import { AgmCoreModule } from '@agm/core'

@NgModule({
  declarations: [
    TreeComponent,
    TosComponent,
    ResourceComponent,
    RegisterComponent,
    LoginComponent,
    ForgetComponent,
    ResetComponent,
    ErrorComponent,
    AppHeaderComponent,
    AppFooterComponent,
    BreadcrumbsComponent,
    SiteRuleComponent,
    FlatComponent,
  ],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    BookingsModule,
    MaterialModule,
    CarouselModule,
    NgxSpinnerModule,
    AgmCoreModule.forRoot({
      // please get your own API key here:
      // https://developers.google.com/maps/documentation/javascript/get-api-key?hl=en
      apiKey: 'AIzaSyC8oANL0zwXFHFjbw7jnzRGrEclTAByyLw'
    })
  ],
  exports: [
    AppHeaderComponent,
    BreadcrumbsComponent,
    AppFooterComponent
  ],
  providers: [],
  bootstrap: []
})

export class SiteModule {
}
