import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Place {
  placeId: string;
  name: string;
  address: string;
  lat: number;
  lng: number;
  photoReference?: string;
  photoUrl?: string;
  openingHoursSummary?: string;
  isOpenNow?: boolean;
  phoneNumber?: string;
  types?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class PlaceService {
  private apiBase = 'http://localhost:5108/api/places'; // update if backend runs on different port

  constructor(private http: HttpClient) {}

  search(query: string, lat: number, lng: number, radius = 5000): Observable<Place[]> {
    const params = new HttpParams()
      .set('query', query)
      .set('lat', lat.toString())
      .set('lng', lng.toString())
      .set('radius', radius.toString());

    return this.http.get<Place[]>(`${this.apiBase}/search`, { params });
  }

  getPhotoUrl(photoReference: string, maxwidth = 400) {
    return `${this.apiBase}/photo?photoreference=${encodeURIComponent(photoReference)}&maxwidth=${maxwidth}`;
  }
}
