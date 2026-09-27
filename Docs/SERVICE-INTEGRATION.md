# Production services boundary

The first playable is offline. No analytics, ad, IAP, crash-reporting or backend
SDK is installed or connected. Existing provider-named analytics classes are
offline placeholders, not evidence of a working Firebase/GameAnalytics account.

`ConsentAnalyticsService` defaults to no consent, drops pre-consent events,
forwards only while opted in, and stops on revocation. It is a reusable boundary,
not a complete consent UI or a substitute for provider-side deletion. Route any
future analytics adapter through it at the composition root; do not expose the
underlying provider to gameplay. Initialize a real SDK only after the relevant
consent decision; a wrapper cannot prevent an SDK's own automatic collection.

## Minimal event contract

| Event | Allowed game fields | Trigger |
| --- | --- | --- |
| session_start | app_version | Opted-in session starts |
| tutorial_complete | tutorial_version | First loop completed |
| merge | item_id, resulting_tier | Successful merge only |
| order_complete | order_id | Verified one-time reward |
| purchase | product_id, transaction_status | Verified provider callback |

Avoid player names, email, chat, raw device identifiers, or save payloads in events.
The table is a contract for future adapters; the consent wrapper does not validate
event schemas or certify payloads as free of personal data.

## Required external setup before integration can be complete

- Provider project/account and environment-specific application identifiers.
- Privacy/consent text and data retention/deletion decisions for the actual game.
- Test ad units or store sandbox products and verified reward/receipt callbacks.
- Backend authentication, authoritative economy checks and a versioned cloud-save
  conflict policy. Do not silently choose the latest timestamp and overwrite saves.
- Opt-out, deletion, offline, retry, duplicate reward and tampered receipt tests.

Credentials belong in the provider dashboard/CI secret store. None are generated,
read from unrelated apps, or substituted with fictitious production values.
