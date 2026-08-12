# IIIF.POC.EventSourcedManifestStore

A proof-of-concept .NET 10 Razor Pages application that stores IIIF Manifest state as append-only KurrentDB event streams and reconstructs the Manifest on every read, instead of keeping a current-state relational or document database.

Each Manifest has one deterministic KurrentDB stream, derived from its IIIF URI as `iiif-manifest-{sha256(manifestId)}`. The first event on that stream is the validated, canonical Presentation 3.0 Manifest; later events are replayable domain changes — label and rights changes, Canvas additions and removals, Canvas height changes, and a tombstone delete. Every mutation also captures the IIIF Manifest Serializer for .NET SDK's `GetChangeSet()` result as audit information stored alongside its domain event.

## Repository contents

- `IIIF.POC.EventSourcedManifestStore/` — the application described above.
- `Publication/` — a Medium article, a LinkedIn post, repository metadata, and publishing notes written about this proof of concept.
- `docs/` — the generated documentation described below.

## Documentation

This repository publishes generated documentation under [`docs/README.md`](docs/README.md). The catalog below links to each area and its direct children.

- Domain `Types:1` `Files:1` `Diagrams:✓`
  - Events `Types:12` `Files:12` `Diagrams:✓`
- Infrastructure `Types:5` `Files:5` `Diagrams:✓`
- Models `Types:6` `Files:6` `Diagrams:✓`
- Pages `Types:2` `Files:6` `Diagrams:✓`
  - Manifests `Types:4` `Files:8` `Diagrams:✓`
  - Shared `Types:0` `Files:1` `Diagrams:✗`
- Services `Types:3` `Files:3` `Diagrams:✓`

**Last generated:** 12 August 2026

## Package dependencies

| Package | Version | Description | Links |
|---|---|---|---|
| IIIF.Manifest.Serializer.Net | 3.0.17 | Version-aware IIIF Presentation API manifest serializer using Newtonsoft.Json. | [NuGet](https://www.nuget.org/packages/IIIF.Manifest.Serializer.Net/3.0.17) · [Repository](https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net) · MIT — used in [Domain](docs/Domain/README.md) and [Services](docs/Services/README.md#package-dependencies) |
| KurrentDB.Client | 1.4.0 | The base gRPC client library for the Kurrent platform. | [NuGet](https://www.nuget.org/packages/KurrentDB.Client/1.4.0) · [Repository](https://github.com/kurrent-io/KurrentDB-Client-Dotnet) · [kurrent.io](https://kurrent.io/) — used in [Infrastructure](docs/Infrastructure/README.md#package-dependencies) |
| Newtonsoft.Json | 13.0.4 | Json.NET, a high-performance JSON framework for .NET. | [NuGet](https://www.nuget.org/packages/Newtonsoft.Json/13.0.4) · [newtonsoft.com/json](https://www.newtonsoft.com/json) · MIT — used in [Services](docs/Services/README.md#package-dependencies) |

No custom NuGet feed is configured; all three packages resolve from `https://api.nuget.org/v3/index.json`.

## Build

```bash
dotnet restore
dotnet build -c Release
```

## Run locally

Start KurrentDB with the compose file in the project folder:

```bash
cd IIIF.POC.EventSourcedManifestStore
docker compose up -d
```

This starts a single-node, insecure KurrentDB instance (service `kurrentdb`, container `iiif-kurrentdb`) with its Admin UI at `http://localhost:2113`, matching the default `KurrentDB:ConnectionString` in `appsettings.json` (`kurrentdb://localhost:2113?tls=false`).

Then restore and run the application:

```bash
dotnet restore
dotnet run
```

## Core SDK

This proof of concept is built on IIIF Manifest Serializer for .NET:

https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net

## License

This repository is free to use.
