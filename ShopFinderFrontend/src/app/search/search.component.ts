import { Component } from '@angular/core';
import { PlaceService, Place } from '../services/place.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-search',
  standalone:true,
  imports:[FormsModule,CommonModule],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent {
  query = 'clothes';
  radius = 5000;
  places: Place[] = [];
  loading = false;
  error = '';
  lat: number | null = null;
  lng: number | null = null;

  constructor(private placeService: PlaceService) {
    this.getLocation();
  }

  getLocation() {
  if (typeof navigator === 'undefined' || !navigator.geolocation) {
    this.error = 'Geolocation not supported.';
    // default to Dublin city centre
    this.lat = 53.3498;
    this.lng = -6.2603;
    return;
  }

  navigator.geolocation.getCurrentPosition(
    pos => {
      this.lat = pos.coords.latitude;
      this.lng = pos.coords.longitude;
    },
    err => {
      console.warn(err);
      // default to Dublin city centre if denied
      this.lat = 53.3498;
      this.lng = -6.2603;
    }
  );
}


  doSearch() {
    if (!this.lat || !this.lng) {
      this.error = 'Location not available.';
      return;
    }
    this.loading = true;
    this.error = '';
    this.placeService.search(this.query, this.lat, this.lng, this.radius)
      .subscribe({
        next: places => {
          this.places = places;
          this.loading = false;
        },
        error: err => {
          console.error(err);
          this.error = 'Search failed.';
          this.loading = false;
        }
      });
  }

  getDirectionsUrl(p: Place) {
    return `https://www.google.com/maps/dir/?api=1&destination=${p.lat},${p.lng}`;
  }
}
