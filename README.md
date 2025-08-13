# Simplified Threat Intelligence Platform

A minimal API built with ASP.NET Core and MongoDB to experiment with storing and searching malware indicators.

## Features
- Create and update malware entries with multiple indicators.
- Search for malware records by indicator values.
- Automatically creates MongoDB indexes for efficient queries.
- Swagger UI exposed in development for interactive exploration.

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/) 7.0 or later
- [MongoDB](https://www.mongodb.com/) instance

### Configuration
Set your MongoDB connection string and database name in `appsettings.json`:

```json
"Mongo": {
  "ConnectionString": "mongodb://localhost:27017",
  "Database": "stip"
}
```

### Run the API
```bash
dotnet restore
dotnet run
```
The API listens on "https://localhost:7008;http://localhost:5062" with Swagger UI available at `/swagger`.

## Project Structure
| Folder | Description |
| --- | --- |
| `Controllers/` | HTTP API endpoints |
| `Services/` | Domain logic for malware and indicators |
| `Repositories/` | MongoDB data access layer |
| `Dtos/` | Request and response models |
| `Data/` | Index initialization |

## Development Notes
- Dependency injection wires controllers, services and repositories.
- MongoDB indexes are ensured at startup via `IndexInitializer`.
- Rules for Duplicate Detection

Rule 1 — Identical Name ⇒ Duplicate
If there is already a malware record in the database with the same name → it is considered a duplicate.

Return HTTP 409 Conflict.

Check using FindByNameAsync(name) (a unique index on name is recommended).

Rule 2 — Same Campaign Even if Names Differ
A record is considered a duplicate if all the following are true:

Time window — the absolute difference between the createdDate values of the two documents is ≤ 120 seconds.

Labels subset/superset — the set of labels in one document is a subset of the other’s labels.

Indicator values subset/superset — the set of indicatorValues in one document is a subset of the other’s indicator values.

| Method & Path                  | Description                                             | Request body                                                                                                                                                                                                                                 | Success response                                                                                            | Error responses               |
| ------------------------------ | ------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- | ----------------------------- |
| **POST** `/api/malware`        | Create a new malware entry.                             | **MalwareCreateDto**:<br>• `name` *(string)*<br>• `labels` *(list\<string>)*<br>• `createdDate`, `updatedDate` *(epoch)*<br>• `indicators` *(list of objects with `type`, `value`, `createdDate`, `updatedDate`, optional `expirationDate`)* | **201 Created**<br>`{ "id": "<id>" }`                                                                       | **409 Conflict** on duplicate |
| **GET** `/api/malware/recent`  | Return malware created or updated in the last 30 days.  | —                                                                                                                                                                                                                                            | **200 OK** — list of `MalwareViewDto` (`id`, `name`, `labels`, `updatedDate`)<br>**204 No Content** if none | —                             |
| **POST** `/api/malware/search` | Search malware whose indicators match any given values. | **SearchRequestDto**:<br>• `values` *(list\<string>)*                                                                                                                                                                                        | **200 OK** — list of `MalwareViewDto`<br>**204 No Content** if none                                         | —                             |
| **PUT** `/api/malware/{id}`    | Update an existing malware.                             | **MalwareUpdateDto** (same fields as `MalwareCreateDto`)                                                                                                                                                                                     | **200 OK** — no body                                                                                        | **409 Conflict** on duplicate |

