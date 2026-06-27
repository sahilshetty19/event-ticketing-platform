// Development environment (used by `ng serve`). Talks to the gateway on its default dev port.
declare global {
  interface Window {
    __APP_CONFIG__?: { apiBaseUrl?: string };
  }
}

export const environment = {
  production: false,
  apiBaseUrl: globalThis.window?.__APP_CONFIG__?.apiBaseUrl ?? 'http://localhost:8088'
};
