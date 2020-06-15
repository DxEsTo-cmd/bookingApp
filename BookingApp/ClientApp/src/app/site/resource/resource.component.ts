import { Component, OnInit, Input, NgModule } from '@angular/core';

import { ResourceService } from '../../services/resource.service';
import { Resource } from '../../models/resource';
import { ActivatedRoute, Router } from '@angular/router';
import { Logger } from '../../services/logger.service';
import { AuthService } from '../../services/auth.service';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import { BookingsComponent } from '../bookings/bookings/bookings.component';
import { API_URL, URL_WITH_HOST } from '../../globals';
import { MouseEvent } from '@agm/core';
import { UserInfoService } from '../../services/user-info.service';

@NgModule({
  declarations: [
    BookingsComponent
  ]
})
@Component({
  selector: 'app-resource',
  templateUrl: './resource.component.html',
  styleUrls: ['./resource.component.css']
})
export class ResourceComponent implements OnInit {

  resource: Resource;
  id: number;
  selectedBookings: BookingsComponent;
  loading: boolean;
  url: string = URL_WITH_HOST;
  latitude = 48.90842;
  longitude = 24.67371;
  markers: marker[] = []
  userId: string;

  constructor(
    private resourceService: ResourceService,
    private actRoute: ActivatedRoute,
    private authService: AuthService,
    private router: Router,
    private userInfo: UserInfoService
  ) {
  }

  authChangedSubscription: any;

  ngOnInit() {
    this.userId = this.userInfo.userId;
    this.loading = true;
    this.actRoute.params.subscribe(params => {this.id = +params['id'];});
    this.resetData();
    console.log(URL_WITH_HOST);
    this.authChangedSubscription = this.authService.AuthChanged.subscribe(() => this.resetData());
  }

  ngOnDestroy() {
    this.authChangedSubscription.unsubscribe();
  };

  mapClicked($event: MouseEvent) {
    this.markers.push({
      lat: $event.coords.lat,
      lng: $event.coords.lng,
      draggable: true
    });
  }

  resetData() {
    this.resourceService.getResource(this.id).subscribe((response: Resource) => {
      this.resource = response;
      this.resource.coordinates.forEach(element => {
        element.draggable = false;
        this.markers.push(element);
      });
    }, error => { this.router.navigate(['/error']); });
  }

  goToChat() {
    //window.open('http://google.com','Window','width=600,height=300');
    window.open('http://localhost:4200/?userId='+ this.userId +'&resourceId=' + this.resource.id,'MyWindow','width=600,height=700');

    //window.open('http://www.google.com/{chat.resourceId}','MyWindow','width=600,height=300')
  }
}

interface marker {
	lat: number;
	lng: number;
	label?: string;
	draggable: boolean;
}
