import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  BookingResponse,
  EventDetailDto,
  EventDto,
  HoldSeatRequest,
  SeatDto
} from '../models/api-models';

/**
 * Single typed gateway client. All calls go through the YARP gateway base URL,
 * which is resolved from Angular environment config (overridable at runtime).
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  // --- Catalog ---
  getEvents(): Observable<EventDto[]> {
    return this.http.get<EventDto[]>(`${this.baseUrl}/catalog/api/events`);
  }

  getEvent(id: string): Observable<EventDetailDto> {
    return this.http.get<EventDetailDto>(`${this.baseUrl}/catalog/api/events/${id}`);
  }

  getSeats(eventId: string): Observable<SeatDto[]> {
    return this.http.get<SeatDto[]>(`${this.baseUrl}/catalog/api/events/${eventId}/seats`);
  }

  // --- Booking ---
  holdSeat(request: HoldSeatRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.baseUrl}/booking/api/bookings/hold`, request);
  }

  confirmBooking(bookingId: string): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.baseUrl}/booking/api/bookings/${bookingId}/confirm`, {});
  }

  getBooking(bookingId: string): Observable<BookingResponse> {
    return this.http.get<BookingResponse>(`${this.baseUrl}/booking/api/bookings/${bookingId}`);
  }
}
