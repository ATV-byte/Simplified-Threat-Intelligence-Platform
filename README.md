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

# Authorization and Authentication

# 1️⃣ With Microsoft Entra ID

## How it works

- API does **not** handle username/password.
- Authentication is done on the Microsoft login page.
- Entra ID issues a JWT (RS256 signed) and publishes the public key in a JWKS.
- Your API validates the JWT using Microsoft’s JWKS endpoint:  
  `https://login.microsoftonline.com/{tenant}/discovery/v2.0/keys`
- Authorization is based on `roles`, `scp`, or custom claims from the token.

## Flow

1. The client obtains a token from Entra (Authorization Code Flow or Client Credentials).
2. The API receives a request with `Authorization: Bearer <token>`.
3. The API validates the signature, expiration, and issuer.
4. If valid → processing continues, otherwise returns **401 Unauthorized**.


## Folder structure for Microsoft Entra ID

```plaintext
src/
  Simplified-Threat-Intelligence-Platform/
    Auth/
        EntraJwtExtensions .cs // add services, policy, scopes - basically config file
    appsettings.json
```

## Entra flow
[![](https://mermaid.ink/img/pako:eNplUmFr2zAQ_StCn1pwYsdOHFdsgS4pIysjhZYNRiCo8jkRsyVPkrOmIf99J3lZWeMv0unevffuzkcqdAmUUQu_OlACFpJvDW_WiuDHO6dV1zyD6eOWGyeFbLlyZF5LwIPb8-1qqRxgrdPm-hJ-p5zhHv1VCqOtrs5Py8Ul-PZh6aFPOwPcedraP61Vj-z1BrNZIGDkYfX4RGKNZndpvE-HSez0T1Afnk08Qz_KbdyhhY8ilG2EgRJPyWsbEFZoTPJWsjh2coCXeFhCxbva9XJBZYByvS4jaZKQI-FCgLWbIEVOF97QLyOf79CYZ2x4_ZsbiA0ITAfdW7SrjXzlTmrFyCfAvPmPtWdEordWv_FaltwBsXKruOsMkL3kZLVczOMv3-8fA_OVtLYDE-H6Sul3GhF4aSNiRXv9rqXg0heSmJQSR7EHc3jTfdf06p5caUPGSUZkRaSyXVXhyvxUwxSRnUZ0a2RJmTMdRLQB03Af0qNnXVO3gwbWlOH1PGS6Vicsw8X_0Lo5VxrdbXeUVbgmjLrWd_333_wHAVWCmetOOcqKwEDZkb5QNkrz4ThN88kkzW8m4yyfRvRAWTYd3oySdJRkSZEXo2R6iuhr0EyG00meFnmaZ-N8UhSj7PQHXDEChQ?type=png)](https://mermaid.live/edit#pako:eNplUmFr2zAQ_StCn1pwYsdOHFdsgS4pIysjhZYNRiCo8jkRsyVPkrOmIf99J3lZWeMv0unevffuzkcqdAmUUQu_OlACFpJvDW_WiuDHO6dV1zyD6eOWGyeFbLlyZF5LwIPb8-1qqRxgrdPm-hJ-p5zhHv1VCqOtrs5Py8Ul-PZh6aFPOwPcedraP61Vj-z1BrNZIGDkYfX4RGKNZndpvE-HSez0T1Afnk08Qz_KbdyhhY8ilG2EgRJPyWsbEFZoTPJWsjh2coCXeFhCxbva9XJBZYByvS4jaZKQI-FCgLWbIEVOF97QLyOf79CYZ2x4_ZsbiA0ITAfdW7SrjXzlTmrFyCfAvPmPtWdEordWv_FaltwBsXKruOsMkL3kZLVczOMv3-8fA_OVtLYDE-H6Sul3GhF4aSNiRXv9rqXg0heSmJQSR7EHc3jTfdf06p5caUPGSUZkRaSyXVXhyvxUwxSRnUZ0a2RJmTMdRLQB03Af0qNnXVO3gwbWlOH1PGS6Vicsw8X_0Lo5VxrdbXeUVbgmjLrWd_333_wHAVWCmetOOcqKwEDZkb5QNkrz4ThN88kkzW8m4yyfRvRAWTYd3oySdJRkSZEXo2R6iuhr0EyG00meFnmaZ-N8UhSj7PQHXDEChQ)

# 2️⃣ Without Microsoft Entra (Self-Hosted JWT)

## How it works

- API has an endpoint `/auth/login` that receives `username + password`.
- The API checks the user in your database (passwords stored with hash+salt).
- If valid, the API issues a JWT (HS256 or RS256).
- The API validates the JWT on each request.

## Flow

1. The client sends `POST /auth/login` with credentials.
2. The API validates them and generates a token using:
   - Your secret key (**HS256**), or
   - Your private key (**RS256**).
3. The API validates the token locally (**HS256**) or from its own JWKS (**RS256**).
4. Authorization is based on `roles` from the payload.

## Additional file/folder structure

```plaintext
src/
  Simplified-Threat-Intelligence-Platform/
    Auth/
      LocalJwtAuthenticationMiddleware.cs
      LocalTokenService.cs
      PasswordHasher.cs
    Controllers/
      AuthController.cs
    appsettings.json
```

## Self-Hosted flow

[![](https://mermaid.ink/img/pako:eNptU21v2jAQ_isnf5iClJIQINBo6xSg21hVFY1qkyYk5CYHsUrszC9dKeK_z06Aja75Yl_yvN3Z2ZFM5EgSovCXQZ7hhNG1pOWCg32o0YKb8gFlU1dUapaxinIN4w1Du1B13HlTrtFytZCt_-Gp0YUDp7MpeIEVLgItHpG_AZ2MHPBW8LUAL6vF1VuKVsni7guJVDvvTSMuUQkjM7SUhtTku7i6chkSmN3N7-GfBO8fZHC1g8ZoyXL_uFWYSdQ-qExU-BH2jZoTsVqTUQKfGM8PYHgHTyjZagsFVQV4o7HcVjpI5Vrw6BB-Mro4hTiwdo248oGpNNPsCc9sLLwJn0AUhhZNswyVWta5faiXpd5W-GGEVKL0AZ8rZgewZPyQ2-m9HsNsmsDnazeEigUl3fy23EBiZj_Xw3DeQrIXqpngCTTaZ96HjLPpqaHvdMNyqhEUW3OqjUTwvtymYxASvs3T4Ho8mafw9cfNvFV7eEzZrqnJ68wubNU677yO6TlG8IhbBWxlj3tblqgly1p_E7wa0t0NeNazF3Ydg3FlVit7ZeoDdQOxTOKTtWQ5SbQ06JMSZUldSXZOdUF0gSUuSGK3Oa6o2egFWfC9pdmL91OI8siUwqwLkqzoRtnKVG4Ahx_o9FYiz1GOheGaJJ2o1iDJjjy7Km73oiju96P4st_rxgOfbEnSHbQvO2HUCbvhMB52wsHeJy-1a9ge9ONoGEdxtxf3h8NOd_8HH7w2ZA?type=png)](https://mermaid.live/edit#pako:eNptU21v2jAQ_isnf5iClJIQINBo6xSg21hVFY1qkyYk5CYHsUrszC9dKeK_z06Aja75Yl_yvN3Z2ZFM5EgSovCXQZ7hhNG1pOWCg32o0YKb8gFlU1dUapaxinIN4w1Du1B13HlTrtFytZCt_-Gp0YUDp7MpeIEVLgItHpG_AZ2MHPBW8LUAL6vF1VuKVsni7guJVDvvTSMuUQkjM7SUhtTku7i6chkSmN3N7-GfBO8fZHC1g8ZoyXL_uFWYSdQ-qExU-BH2jZoTsVqTUQKfGM8PYHgHTyjZagsFVQV4o7HcVjpI5Vrw6BB-Mro4hTiwdo248oGpNNPsCc9sLLwJn0AUhhZNswyVWta5faiXpd5W-GGEVKL0AZ8rZgewZPyQ2-m9HsNsmsDnazeEigUl3fy23EBiZj_Xw3DeQrIXqpngCTTaZ96HjLPpqaHvdMNyqhEUW3OqjUTwvtymYxASvs3T4Ho8mafw9cfNvFV7eEzZrqnJ68wubNU677yO6TlG8IhbBWxlj3tblqgly1p_E7wa0t0NeNazF3Ydg3FlVit7ZeoDdQOxTOKTtWQ5SbQ06JMSZUldSXZOdUF0gSUuSGK3Oa6o2egFWfC9pdmL91OI8siUwqwLkqzoRtnKVG4Ahx_o9FYiz1GOheGaJJ2o1iDJjjy7Km73oiju96P4st_rxgOfbEnSHbQvO2HUCbvhMB52wsHeJy-1a9ge9ONoGEdxtxf3h8NOd_8HH7w2ZA)
