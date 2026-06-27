// TypeScript interfaces mirroring the backend DTOs (serialized as camelCase JSON).

export interface EventDto {
  id: string;
  name: string;
  description: string;
  startsAtUtc: string;
  venueName: string;
  city: string;
}

export interface EventDetailDto extends EventDto {
  seatCount: number;
}

export type SeatStatus = 'Available' | 'Held' | 'Booked';

export interface SeatDto {
  id: string;
  section: string;
  row: string;
  number: number;
  status: SeatStatus;
}

export interface HoldSeatRequest {
  eventId: string;
  seatId: string;
  customerId: string;
  amount: number;
}

export type BookingStatus = 'Held' | 'Confirmed' | 'Cancelled' | 'Expired';

export interface BookingResponse {
  id: string;
  eventId: string;
  seatId: string;
  customerId: string;
  amount: number;
  status: BookingStatus;
  heldAtUtc: string;
  expiresAtUtc: string;
  confirmedAtUtc: string | null;
  paidAtUtc: string | null;
}
