import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { startWith } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { EventDto } from '../../models/api-models';

@Component({
  selector: 'app-events-list',
  imports: [DatePipe, RouterLink, ReactiveFormsModule],
  templateUrl: './events-list.html',
  styleUrl: './events-list.css'
})
export class EventsListComponent implements OnInit {
  private readonly api = inject(ApiService);

  // Reactive form control drives a live, client-side filter.
  readonly search = new FormControl('', { nonNullable: true });
  private readonly term = toSignal(this.search.valueChanges.pipe(startWith('')), { initialValue: '' });

  readonly events = signal<EventDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly filtered = computed(() => {
    const q = this.term().toLowerCase().trim();
    const all = this.events();
    if (!q) return all;
    return all.filter(e =>
      e.name.toLowerCase().includes(q) ||
      e.venueName.toLowerCase().includes(q) ||
      e.city.toLowerCase().includes(q));
  });

  ngOnInit(): void {
    this.api.getEvents().subscribe({
      next: events => { this.events.set(events); this.loading.set(false); },
      error: () => { this.error.set('Could not load events. Is the gateway running?'); this.loading.set(false); }
    });
  }
}
