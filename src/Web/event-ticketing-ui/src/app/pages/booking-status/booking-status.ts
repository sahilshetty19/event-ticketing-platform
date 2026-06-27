import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription, interval, switchMap, takeWhile } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { BookingResponse } from '../../models/api-models';

@Component({
  selector: 'app-booking-status',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './booking-status.html',
  styleUrl: './booking-status.css'
})
export class BookingStatusComponent implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  private bookingId = '';
  private readonly subs = new Subscription();

  readonly booking = signal<BookingResponse | null>(null);
  readonly error = signal<string | null>(null);
  readonly confirming = signal(false);
  readonly secondsLeft = signal(0);

  readonly isPaid = computed(() => !!this.booking()?.paidAtUtc);
  readonly isConfirmed = computed(() => this.booking()?.status === 'Confirmed' || this.isPaid());

  ngOnInit(): void {
    this.bookingId = this.route.snapshot.paramMap.get('id') ?? '';
    this.load();
    // Tick the hold countdown once a second.
    this.subs.add(interval(1000).subscribe(() => this.tick()));
  }

  private load(): void {
    this.api.getBooking(this.bookingId).subscribe({
      next: b => { this.booking.set(b); this.tick(); },
      error: () => this.error.set('Could not load this booking.')
    });
  }

  private tick(): void {
    const b = this.booking();
    if (b?.status === 'Held') {
      const ms = this.asUtc(b.expiresAtUtc).getTime() - Date.now();
      this.secondsLeft.set(Math.max(0, Math.floor(ms / 1000)));
    }
  }

  // Server timestamps are UTC (fields are named *Utc). If the string lacks a timezone
  // designator, treat it as UTC so the browser doesn't misread it as local time.
  private asUtc(value: string): Date {
    return /[zZ]|[+-]\d{2}:\d{2}$/.test(value) ? new Date(value) : new Date(value + 'Z');
  }

  confirm(): void {
    if (this.confirming()) return;
    this.confirming.set(true);
    this.api.confirmBooking(this.bookingId).subscribe({
      next: b => { this.booking.set(b); this.pollUntilPaid(); },
      error: () => { this.error.set('Could not confirm — the hold may have expired.'); this.confirming.set(false); }
    });
  }

  // Payment happens asynchronously (Booking -> Payment -> Booking marks paid),
  // so poll the booking every 2s until paidAtUtc appears, then stop.
  private pollUntilPaid(): void {
    this.subs.add(
      interval(2000).pipe(
        switchMap(() => this.api.getBooking(this.bookingId)),
        takeWhile(b => !b.paidAtUtc, true)
      ).subscribe({
        next: b => this.booking.set(b),
        error: () => {}
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }
}
