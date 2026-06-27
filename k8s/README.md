# Kubernetes deployment (local)

This directory deploys the whole Event Ticketing platform to a **local** Kubernetes cluster
(Docker Desktop's kind-based Kubernetes, context `docker-desktop`). It is an **additive**
layer — `docker compose` still works exactly as before; this is a second way to run the same
images.

> Local only. No cloud, no AKS, no credentials, no cost.

## What gets deployed (namespace `event-ticketing`)

| Kind | Objects |
|------|---------|
| Namespace | `event-ticketing` |
| ConfigMap | `et-config` (non-secret env: ASPNETCORE_ENVIRONMENT, Redis/RabbitMQ hostnames) |
| Secret | `et-secrets` (SA password, RabbitMQ password, the 4 connection strings) |
| StatefulSet + Service | `sqlserver`, `rabbitmq` (stable identity + own PersistentVolume) |
| Deployment + PVC + Service | `redis` |
| Deployment + Service | `catalog-api`, `booking-api`, `payment-api`, `notification-api`, `gateway`, `web` |
| Ingress | `event-ticketing` (single host → gateway for APIs, web for everything else) |

```
k8s/
  00-namespace.yaml
  01-configmap.yaml
  secret.example.yaml      # placeholders; the real Secret is created separately (gitignored)
  infra/    sqlserver.yaml  redis.yaml  rabbitmq.yaml
  services/ catalog.yaml  booking.yaml  payment.yaml  notification.yaml
  gateway/  gateway.yaml  web.yaml  ingress.yaml
```

## Kubernetes concepts (quick primer)

- **Pod** — the smallest deployable unit: one (or a few) containers sharing a network identity.
- **Deployment** — declares "keep N identical pods running" for stateless apps; handles rollouts/self-healing.
- **StatefulSet** — like a Deployment but for stateful apps: stable pod name (`sqlserver-0`) and a
  dedicated PersistentVolume per pod. Used here for SQL Server and RabbitMQ.
- **PersistentVolumeClaim (PVC)** — a request for durable disk that outlives the pod, so data survives restarts.
- **Service** — a stable in-cluster DNS name + virtual IP that load-balances to the pods behind it.
  Pods talk to each other by Service name (e.g. `sqlserver`, `redis`, `catalog-api`).
- **ConfigMap / Secret** — key/value config injected as environment variables. ConfigMap = non-secret;
  Secret = sensitive (base64-stored, kept out of git here).
- **Probe** — health checks the kubelet runs: **readiness** (is it ok to send traffic?), **liveness**
  (is it stuck — restart it?), **startup** (is it still booting — don't kill it yet?). All use `/health`.
- **Ingress** — an HTTP router at the cluster edge that maps hostnames/paths to Services. Needs an
  Ingress **controller** (e.g. ingress-nginx) installed to actually serve traffic.

## localhost vs Service DNS (vs docker-compose)

In compose, services reach each other by compose service name on a shared bridge network. In Kubernetes
they reach each other by **Service name via cluster DNS** (CoreDNS) — e.g. `Server=sqlserver,1433;...`,
`redis:6379`, `RabbitMq__Host=rabbitmq`. Those names happen to be identical to the compose names, so the
connection-string *values* are the same; what differs is the resolver. Nothing uses `localhost` for
cross-service calls (a pod's `localhost` is just itself). Cross-namespace would need the FQDN
`sqlserver.event-ticketing.svc.cluster.local`.

## Prerequisites

- Docker Desktop with Kubernetes enabled (kind provisioner). `kubectl config current-context` → `docker-desktop`.
- `kubectl` on PATH.
- The app images built locally (below).

## Step 1 — build the images

```bash
# from the repo root
docker compose build
# produces: eventticketing/{catalog-api,booking-api,payment-api,notification-api,gateway,web}:latest
```

## Step 2 — make the images available to the cluster

**Docker Desktop (kind provisioner):** locally-built images are served to the cluster automatically
with `imagePullPolicy: IfNotPresent` (already set in the manifests). **No separate load step is needed.**

**Standalone `kind` cluster** (if you use one instead): load each image into the cluster:

```bash
for s in catalog-api booking-api payment-api notification-api gateway web; do
  kind load docker-image eventticketing/$s:latest --name <your-kind-cluster-name>
done
```

## Step 3 — namespace, config, and secret

```bash
kubectl apply -f k8s/00-namespace.yaml -f k8s/01-configmap.yaml

# Create the Secret (NOT committed). Either copy & edit the example:
#   cp k8s/secret.example.yaml k8s/secret.yaml   # then edit values; k8s/secret.yaml is gitignored
#   kubectl apply -f k8s/secret.yaml
# ...or create it imperatively (use your own strong values; SA password must match in all four):
kubectl create secret generic et-secrets -n event-ticketing \
  --from-literal=MSSQL_SA_PASSWORD='Str0ng_Passw0rd!' \
  --from-literal=RabbitMq__Password='Event_Passw0rd!' \
  --from-literal=ConnectionStrings__CatalogDb='Server=sqlserver,1433;Database=CatalogDb;User Id=sa;Password=Str0ng_Passw0rd!;TrustServerCertificate=True' \
  --from-literal=ConnectionStrings__BookingDb='Server=sqlserver,1433;Database=BookingDb;User Id=sa;Password=Str0ng_Passw0rd!;TrustServerCertificate=True' \
  --from-literal=ConnectionStrings__PaymentDb='Server=sqlserver,1433;Database=PaymentDb;User Id=sa;Password=Str0ng_Passw0rd!;TrustServerCertificate=True' \
  --from-literal=ConnectionStrings__NotificationDb='Server=sqlserver,1433;Database=NotificationDb;User Id=sa;Password=Str0ng_Passw0rd!;TrustServerCertificate=True'
```

## Step 4 — deploy

```bash
kubectl apply -f k8s/infra/
kubectl apply -f k8s/services/
kubectl apply -f k8s/gateway/

# watch everything come up (SQL Server takes ~30-60s)
kubectl get pods -n event-ticketing -w
```

All pods should reach `1/1 Running`:

```
NAME                                READY   STATUS
sqlserver-0                         1/1     Running
redis-...                           1/1     Running
rabbitmq-0                          1/1     Running
catalog-api-...                     1/1     Running
booking-api-...                     1/1     Running
payment-api-...                     1/1     Running
notification-api-...               1/1     Running
gateway-...                         1/1     Running
web-...                             1/1     Running
```

## Step 5 — reach the app

### Option A — Ingress (single host, no CORS) — recommended for the UI

Install the ingress-nginx controller for kind, then browse the host:

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml
kubectl wait --namespace ingress-nginx --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller --timeout=120s

# then open:
#   UI:      http://event-ticketing.localtest.me
#   API:     http://event-ticketing.localtest.me/catalog/api/events
```

`*.localtest.me` resolves to `127.0.0.1`, so no hosts-file edit is needed. The SPA is served from the
same host it calls, so there is no cross-origin request (no CORS).

### Option B — port-forward (no controller needed) — quickest for API/flow testing

```bash
kubectl port-forward -n event-ticketing svc/gateway 8088:8080
# in another terminal, hit the gateway directly:
curl http://localhost:8088/catalog/api/events
```

For the **UI** via port-forward (two origins), also forward the web and point it at the gateway:
`kubectl set env deploy/web -n event-ticketing API_BASE_URL=http://localhost:8088`,
add `http://localhost:8080` to the gateway's `Cors__AllowedOrigins`, then
`kubectl port-forward -n event-ticketing svc/web 8080:80` and open http://localhost:8080.
(The Ingress option avoids all of this.)

## Verify

```bash
kubectl get pods -n event-ticketing          # all 1/1 Running
```

End-to-end booking flow through the in-cluster gateway (port-forward `svc/gateway 8088:8080` first):

```bash
# 1) list events
curl http://localhost:8088/catalog/api/events
# 2) hold a seat
curl -X POST http://localhost:8088/booking/api/bookings/hold \
  -H "Content-Type: application/json" \
  -d '{"eventId":"11111111-1111-1111-1111-111111111111","seatId":"22222222-2222-2222-2222-222222222222","customerId":"33333333-3333-3333-3333-333333333333","amount":50}'
# 3) confirm (publishes BookingConfirmed; Payment charges -> PaymentSucceeded -> Booking marked paid)
curl -X POST http://localhost:8088/booking/api/bookings/<booking-id>/confirm
# 4) booking shows paidAtUtc; notification was sent
curl http://localhost:8088/booking/api/bookings/<booking-id>
curl http://localhost:8088/notification/api/notifications
```

## Tear down

```bash
kubectl delete namespace event-ticketing
# (the ingress controller, if installed, lives in its own namespace: kubectl delete ns ingress-nginx)
```

## Compose vs Kubernetes — what each is for

Both run the **same Docker images**; they are different orchestrators for different purposes.

| | docker-compose | Kubernetes |
|---|---|---|
| Purpose | Fast local dev / single-host run | Production-style orchestration (scheduling, self-healing, scaling) |
| Scope | One machine, one Docker engine | A cluster of nodes (here, one local node) |
| Unit | a "service" (container) | Pod (managed by Deployment/StatefulSet) |
| Networking | service names on a bridge network | Services + cluster DNS (CoreDNS) |
| Config/secrets | `.env` file | ConfigMap + Secret objects |
| Health/restart | `healthcheck` + restart policy | liveness/readiness/startup probes + controllers |
| Storage | named volumes | PersistentVolumeClaims |
| Scaling | `--scale` (basic) | `replicas`, HPA, rolling updates, self-healing |
| External access | published ports | Ingress / Service types |

**Why support both:** compose is the lowest-friction way to spin the whole system up while developing.
Kubernetes is how you'd actually run it in production — and demonstrating the same app on both shows
the images are portable and that the config is properly externalized (env/Secret-driven), which is the
whole point of the twelve-factor / container-native approach.
