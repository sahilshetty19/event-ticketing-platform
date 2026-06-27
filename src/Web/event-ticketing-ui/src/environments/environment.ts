// Production environment. `apiBaseUrl` prefers the runtime value injected by env.js
// (so the same built image works in any environment), falling back to the compiled default.
declare global {
  interface Window {
    __APP_CONFIG__?: { apiBaseUrl?: string };
  }
}

export const environment = {
  production: true,
  apiBaseUrl: globalThis.window?.__APP_CONFIG__?.apiBaseUrl ?? 'http://localhost:8088'
};
