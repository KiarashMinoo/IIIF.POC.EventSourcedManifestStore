# IIIF.POC.EventSourcedManifestStore

A proof-of-concept ASP.NET Core Razor Pages application that stores IIIF Manifest state as an **append-only KurrentDB event stream** and reconstructs the Manifest aggregate on every read.

There is deliberately **no current-state relational/document database** in this POC.

Each Manifest has one deterministic KurrentDB stream. The first event contains the validated canonical Presentation API 3 Manifest. Later events represent replayable domain changes such as label changes, rights changes, Canvas additions/removals, and nested Canvas dimension updates.

The POC also stores the SDK's granular `GetChangeSet()` result as audit information beside each replayable domain event.

Core SDK:

https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net

## Recommended repository name

```text
IIIF.POC.EventSourcedManifestStore
```

## Recommended GitHub About description

**Razor Pages POC that reconstructs IIIF Manifests from KurrentDB event streams, with replayable domain events, SDK ChangeSet audit metadata, historical revisions, optimistic concurrency, and version-aware export.**

## Suggested topics

```text
iiif
dotnet
csharp
aspnet-core
razor-pages
kurrentdb
event-sourcing
event-store
cqrs
ddd
change-tracking
event-streams
optimistic-concurrency
digital-libraries
proof-of-concept
nuget
```

## Stack

- .NET 10
- ASP.NET Core Razor Pages
- KurrentDB Client 1.4.0
- KurrentDB
- IIIF Manifest Serializer for .NET 3.0.17
- Newtonsoft.Json 13.0.4 for SDK-aware audit value serialization

## Architecture

```text
IIIF Manifest id
      │
      ▼
deterministic stream name
      │
      ▼
iiif-manifest-{sha256(manifestId)}

revision 0
  iiif.manifest.imported.v1
      │
revision 1
  iiif.manifest.label-changed.v1
      │
revision 2
  iiif.canvas.height-changed.v1
      │
revision 3
  iiif.canvas.added.v1
      │
revision 4
  iiif.canvas.removed.v1
      │
      ▼
replay all events
      │
      ▼
ManifestAggregate
      │
      ▼
IIIF Manifest object
      │
      ├── serialize Presentation 3.0
      ├── serialize Presentation 2.1
      └── serialize Presentation 2.0
```

KurrentDB is the source of truth.

The application does **not** persist a separate "current Manifest" table.

## Why aggregate on retrieval?

The expected workload is a Manifest that changes relatively infrequently.

That makes replay inexpensive:

```text
1 import event
+ a handful of lifetime changes
= a small stream
```

For this workload, reconstructing the aggregate from the stream is simple and makes historical revisions a natural capability.

If streams later become large, snapshots can be introduced without changing the domain event model.

## Event schemas

The POC uses stable event names rather than CLR type names:

```text
iiif.manifest.imported.v1
iiif.manifest.label-changed.v1
iiif.manifest.rights-changed.v1
iiif.canvas.height-changed.v1
iiif.canvas.added.v1
iiif.canvas.removed.v1
iiif.manifest.deleted.v1
```

The explicit `v1` suffix leaves room for event evolution.

## Initial import

The first event is:

```text
iiif.manifest.imported.v1
```

It contains:

```text
manifestId
canonicalPresentation3Json
sourceVersion
occurredAtUtc
```

The input can be supported Presentation 2.x or 3.0 JSON.

Before writing the event, the POC:

1. validates the input;
2. detects the source Presentation version;
3. deserializes it with IIIF Manifest Serializer for .NET;
4. serializes the canonical object model as Presentation 3.0;
5. appends the import event with `StreamState.NoStream`.

This guarantees that one Manifest stream cannot be accidentally created twice.

## Replayable domain events vs SDK ChangeSet

The POC deliberately keeps two concepts separate.

### Replayable domain event

Example:

```json
{
  "canvasId": "https://example.org/canvas/9",
  "previousHeight": 1200,
  "height": 1300,
  "occurredAtUtc": "..."
}
```

This is stable and designed to rebuild the aggregate.

### SDK ChangeSet audit

The same stored event can also contain audit information produced by:

```csharp
manifest.GetChangeSet()
```

including:

```text
path
kind
property name
original value
current value
detected timestamp
```

For example:

```text
Items[0].Height
Modified
1200
1300
```

The domain event uses a stable Canvas URI for replay.

The ChangeSet preserves the exact SDK object-graph observation.

This avoids making positional paths such as `Items[0]` the permanent replay contract.

## Aggregate application

`ManifestAggregate` applies events in stream order.

Examples:

```csharp
case ManifestLabelChangedV1 e:
    Manifest!.SetLabel([new Label(e.Label)]);
    Manifest.AcceptChanges();
    break;
```

and:

```csharp
case CanvasHeightChangedV1 e:
    var canvas = Manifest!.Items
        .OfType<Canvas>()
        .Single(x => x.Id == e.CanvasId);

    canvas.SetHeight(e.Height);
    Manifest.AcceptChanges();
    break;
```

During replay, `AcceptChanges()` keeps the reconstructed SDK graph clean after each event.

That means future commands can use SDK change tracking normally.

## Command flow

A command follows this sequence:

```text
read stream
    ↓
replay aggregate
    ↓
verify expected stream revision
    ↓
mutate the actual SDK Manifest instance
    ↓
GetChangeSet()
    ↓
translate command into a replayable domain event
    ↓
append using expected stream revision
```

The POC includes commands for:

- changing the Manifest label;
- toggling rights;
- increasing the first Canvas height;
- adding a Canvas;
- removing the last Canvas;
- deleting the Manifest through a tombstone event.

## Optimistic concurrency

KurrentDB stream revisions provide the concurrency boundary.

The page posts the revision it was rendered from:

```text
expectedRevision = 7
```

The command appends with that expected revision.

If another request has already appended revision 8, the append fails rather than silently overwriting history.

No separate database row-version column is required.

## Historical reconstruction

Every event on the Timeline page has a **View state** action.

For example:

```text
revision 0 → imported state
revision 1 → after label change
revision 2 → after Canvas height change
revision 3 → after Canvas addition
```

To reconstruct revision 2, the event store replays only:

```text
0
1
2
```

and stops before later events.

The same historical aggregate can then be serialized as Presentation 3.0, 2.1, or 2.0.

## Deletion

Delete does not erase the stream.

It appends:

```text
iiif.manifest.deleted.v1
```

The aggregate becomes tombstoned for current reads and rejects new mutations.

Historical revisions before the tombstone remain available.

This is useful for auditability and makes deletion semantics explicit.

A production implementation would need a separate policy for legal/physical erasure requirements.

## Stream naming

The stream name is derived from the Manifest URI:

```text
iiif-manifest-{sha256(manifestId)}
```

This avoids putting arbitrary URI characters directly into stream names while remaining deterministic.

The Manifest URI remains inside the import event and reconstructed aggregate.

## Razor Pages

The POC includes:

```text
Pages/
  Index
  Manifests/
    Create
    Details
    Timeline
    Delete
```

### Index

Enter a IIIF Manifest id.

The app calculates its stream name and replays the stream.

### Create

Paste Presentation 2.x or 3.0 JSON and create revision 0.

### Details

Shows:

- current or historical revision;
- number of events replayed;
- Canvas count;
- source version;
- deterministic stream name;
- reconstructed Presentation 3 JSON;
- mutation commands;
- export to 3.0, 2.1, or 2.0.

### Timeline

Shows every immutable event plus its SDK ChangeSet audit.

### Delete

Appends a tombstone event.

## Run locally

Start KurrentDB:

```bash
docker compose up -d
```

The local Admin UI is available at:

```text
http://localhost:2113
```

Restore and run:

```bash
dotnet restore
dotnet run
```

The development connection string is:

```text
kurrentdb://localhost:2113?tls=false
```

## Important POC boundaries

### Initial event size

The import event stores the entire canonical Manifest JSON.

KurrentDB events have a maximum event size, so exceptionally large Manifests should use another bootstrap strategy, such as:

- a URI/object-storage reference;
- several initialization events;
- an application snapshot store;
- a compact binary representation.

For ordinary POC-sized Manifests, the full import event keeps the example understandable.

### Listing all Manifests

The POC intentionally does not build a global Manifest list projection.

It opens a Manifest by its IIIF id.

A production system should create a projection/read model for:

- browse/search;
- recently updated Manifests;
- labels;
- collection membership;
- deleted state;
- counts.

That read model is derived data. The event stream remains the source of truth.

### Snapshotting

The POC replays from revision 0 every time.

That is intentional because the expected streams are short.

If a Manifest accumulates hundreds or thousands of events, introduce snapshots such as:

```text
snapshot at revision 500
+ replay 501..current
```

Snapshotting is a performance optimization, not a replacement for the event stream.

### Event evolution

Production event sourcing needs explicit policies for:

- event versioning;
- upcasting;
- schema compatibility;
- old event readers;
- changed SDK behavior over time.

Never rewrite historical events just because the current code model changed.

## Why not store the ChangeSet itself as the replay contract?

The SDK ChangeSet is excellent audit information.

It is intentionally not the only domain event model here.

A path like:

```text
Items[2].Label
```

describes where the SDK observed a change at that moment.

A replayable event should preferably say:

```text
CanvasLabelChanged
canvasId = https://example.org/canvas/9
```

Stable resource identity is safer than a positional index for long-lived event history.

## Why KurrentDB fits the POC

The model maps naturally:

```text
one Manifest
    → one stream

one successful command
    → one new immutable event

stream revision
    → aggregate version / concurrency token

stream replay
    → current or historical Manifest state
```

The POC therefore uses the database for what it is designed around instead of treating sparse Manifest changes as conventional metric samples.

## Production follow-ups

Before production use, add:

- authentication and authorization;
- secure KurrentDB TLS configuration;
- command actor/correlation/causation metadata;
- a global projection/read model;
- integration tests against a real KurrentDB container;
- event upcasters;
- snapshot support if needed;
- retry/error handling policies;
- observability;
- event payload size limits;
- resource-level business rules;
- physical erasure policy where required;
- backup and restore procedures.

## Official references

KurrentDB .NET client:

https://docs.kurrent.io/clients/dotnet/

Reading streams:

https://docs.kurrent.io/clients/dotnet/v1.4/reading-events

Appending and optimistic concurrency:

https://docs.kurrent.io/clients/dotnet/v1.4/appending-events

KurrentDB installation:

https://docs.kurrent.io/server/v26.0/quick-start/installation

## License

Add the license appropriate for the repository before publishing.
