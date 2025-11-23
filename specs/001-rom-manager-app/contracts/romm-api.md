# Romm API Contract

**Date**: 2025-01-27  
**Feature**: [spec.md](../spec.md)

## API Reference

- **Documentation** : https://docs.romm.app/latest/API-and-Development/API-Reference/
- **OpenAPI Spec (Live)** : http://rpi5.local/openapi.json
- **Base URL** : Configurable par utilisateur (ex: `http://rpi5.local`)

## Authentication

Romm API utilise l'authentification par clé API.

**Header requis** :
```
Authorization: Bearer {api_key}
```

**Stockage** : Clé API stockée dans UserPreferences (chiffrée si possible)

## Endpoints Utilisés

### Platforms

**GET /api/v1/platforms**
- Liste toutes les plateformes supportées
- Utilisé pour : Vérifier correspondance consoles, obtenir IDs plateformes

**GET /api/v1/platforms/{id}**
- Détails d'une plateforme spécifique
- Utilisé pour : Obtenir conventions nommage, formats supportés

### Games

**GET /api/v1/games**
- Liste jeux avec filtres
- Paramètres : `platform_id`, `search`, etc.
- Utilisé pour : Recherche jeux, synchronisation métadonnées

**GET /api/v1/games/{id}**
- Détails d'un jeu spécifique
- Utilisé pour : Récupération métadonnées complètes

**POST /api/v1/games**
- Création nouveau jeu
- Utilisé pour : Export jeux vers Romm

**PUT /api/v1/games/{id}**
- Mise à jour jeu
- Utilisé pour : Synchronisation métadonnées utilisateur

### Files

**POST /api/v1/files**
- Upload fichier ROM
- Utilisé pour : Export fichiers vers Romm

**GET /api/v1/files/{id}**
- Détails fichier
- Utilisé pour : Vérification statut upload

## Data Models

### Platform

```json
{
  "id": "string",
  "name": "string",
  "slug": "string",
  "igdb_id": "integer",
  "logo": "string",
  "extensions": ["string"]
}
```

### Game

```json
{
  "id": "string",
  "name": "string",
  "platform_id": "string",
  "igdb_id": "integer",
  "summary": "string",
  "rating": "float",
  "release_date": "string",
  "developer": "string",
  "publisher": "string",
  "genre": "string",
  "cover": "string",
  "screenshots": ["string"],
  "videos": ["string"]
}
```

### User Data (favorites, ratings, etc.)

```json
{
  "favorite": "boolean",
  "rating": "float",
  "play_count": "integer",
  "last_played": "datetime",
  "time_played": "integer"
}
```

## Error Handling

### HTTP Status Codes

- `200 OK` : Succès
- `201 Created` : Ressource créée
- `400 Bad Request` : Requête invalide
- `401 Unauthorized` : Authentification requise
- `404 Not Found` : Ressource non trouvée
- `500 Internal Server Error` : Erreur serveur

### Error Response Format

```json
{
  "error": "string",
  "message": "string",
  "details": {}
}
```

## Implementation Notes

### HttpClient Configuration

```csharp
var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl),
    DefaultRequestHeaders =
    {
        Authorization = new AuthenticationHeaderValue("Bearer", apiKey)
    }
};
```

### Retry Strategy

- Retry automatique sur erreurs réseau temporaires
- Exponential backoff pour rate limiting
- Timeout configurable (défaut 30s)

### Rate Limiting

- Respecter limites API Romm
- Throttling si nécessaire
- Cache réponses pour réduire appels

## Testing

### Mock Server

- Utiliser mock server pour tests
- Réponses prévisibles pour scénarios tests
- Validation requêtes envoyées

### Integration Tests

- Tests avec instance Romm locale si disponible
- Tests avec données de test isolées
- Nettoyage après tests

