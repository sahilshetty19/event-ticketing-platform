import { Routes } from '@angular/router';
import { EventsListComponent } from './pages/events-list/events-list';
import { EventDetailComponent } from './pages/event-detail/event-detail';
import { BookingStatusComponent } from './pages/booking-status/booking-status';

export const routes: Routes = [
  { path: '', component: EventsListComponent, title: 'Events' },
  { path: 'events/:id', component: EventDetailComponent, title: 'Event detail' },
  { path: 'bookings/:id', component: BookingStatusComponent, title: 'Your booking' },
  { path: '**', redirectTo: '' }
];
