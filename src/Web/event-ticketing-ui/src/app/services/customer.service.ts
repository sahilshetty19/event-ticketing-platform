import { Injectable } from '@angular/core';

/**
 * There is no auth in this demo, so we mint a stable per-browser customer id and
 * keep it in localStorage. Every booking is attributed to this id.
 */
@Injectable({ providedIn: 'root' })
export class CustomerService {
  private static readonly Key = 'event-ticketing.customerId';

  getCustomerId(): string {
    let id = localStorage.getItem(CustomerService.Key);
    if (!id) {
      id = crypto.randomUUID();
      localStorage.setItem(CustomerService.Key, id);
    }
    return id;
  }
}
