import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { CustomerService } from '../../services/customer.service';
import { EventDetailDto, SeatDto } from '../../models/api-models';

@Component({
  selector: 'app-event-detail',
  imports: [DatePipe, RouterLink],
  templateUrl: './event-detail.html',
  styleUrl: './event-detail.css'
})
export class EventDetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly customer = inject(CustomerService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private eventId = '';
  readonly event = signal<EventDetailDto | null>(null);
  readonly seats = signal<SeatDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly holding = signal(false);

  // Group seats into sections -> rows -> seats for rendering the map.
  readonly sections = computed(() => {
    const bySection = new Map<string, Map<string, SeatDto[]>>();
    for (const seat of this.seats()) {
      const rows = bySection.get(seat.section) ?? new Map<string, SeatDto[]>();
      const row = rows.get(seat.row) ?? [];
      row.push(seat);
      rows.set(seat.row, row);
      bySection.set(seat.section, rows);
    }
    return [...bySection.entries()]
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([section, rowsMap]) => ({
        section,
        rows: [...rowsMap.entries()]
          .sort((a, b) => a[0].localeCompare(b[0]))
          .map(([row, seats]) => ({ row, seats: seats.sort((x, y) => x.number - y.number) }))
      }));
  });

  ngOnInit(): void {
    this.eventId = this.route.snapshot.paramMap.get('id') ?? '';

    this.api.getEvent(this.eventId).subscribe({
      next: e => this.event.set(e),
      error: () => this.error.set('Could not load the event.')
    });

    this.api.getSeats(this.eventId).subscribe({
      next: s => { this.seats.set(s); this.loading.set(false); },
      error: () => { this.error.set('Could not load seats.'); this.loading.set(false); }
    });
  }

  selectSeat(seat: SeatDto): void {
    if (seat.status !== 'Available' || this.holding()) return;

    this.holding.set(true);
    this.api.holdSeat({
      eventId: this.eventId,
      seatId: seat.id,
      customerId: this.customer.getCustomerId(),
      amount: 50
    }).subscribe({
      next: booking => this.router.navigate(['/bookings', booking.id]),
      error: () => {
        this.error.set('Could not hold that seat — it may already be taken. Try another.');
        this.holding.set(false);
      }
    });
  }
}
