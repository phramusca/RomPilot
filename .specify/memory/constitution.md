<!--
Sync Impact Report:
Version change: N/A → 1.0.0 (initial creation)
Modified principles: N/A (new constitution)
Added sections: Core Principles (4 principles), Quality Standards, Development Workflow, Governance
Removed sections: N/A
Templates requiring updates:
  ✅ .specify/templates/plan-template.md - Constitution Check section updated
  ✅ .specify/templates/spec-template.md - No changes needed (already aligned)
  ✅ .specify/templates/tasks-template.md - No changes needed (already aligned)
Follow-up TODOs: None
-->

# RomPilot Constitution

## Core Principles

### I. Code Quality (NON-NEGOTIABLE)

Le code DOIT respecter les standards de qualité établis. Tous les fichiers de code source DOIVENT être :
- Lisible et maintenable : nommage explicite, fonctions courtes et focalisées, commentaires pour la logique complexe uniquement
- Cohérent : respect des conventions de style du projet (formatage, structure, patterns)
- Documenté : docstrings pour les APIs publiques, README pour les modules complexes
- Révisable : code review obligatoire avant merge, aucune exception
- Refactorisé régulièrement : dette technique identifiée et traitée dans les sprints

**Rationale** : La qualité du code est la fondation de la maintenabilité à long terme. Un code de qualité réduit les bugs, facilite l'onboarding, et accélère le développement futur.

### II. Testing Standards (NON-NEGOTIABLE)

Les tests DOIVENT être écrits selon les standards suivants :
- Test-First : Les tests sont écrits AVANT l'implémentation (TDD) pour les nouvelles fonctionnalités
- Couverture minimale : 80% de couverture de code pour le code critique, 60% pour le reste
- Types de tests requis :
  - Tests unitaires : chaque fonction/méthode publique testée isolément
  - Tests d'intégration : pour les interactions entre composants
  - Tests de contrat : pour les APIs et interfaces publiques
  - Tests end-to-end : pour les parcours utilisateur critiques
- Tests maintenables : tests clairs, indépendants, rapides, sans dépendances externes non mockées
- CI/CD : tous les tests DOIVENT passer avant merge, aucune exception

**Rationale** : Les tests garantissent la fiabilité, permettent le refactoring en toute confiance, et servent de documentation vivante du comportement attendu.

### III. User Experience Consistency

L'expérience utilisateur DOIT être cohérente à travers toute l'application :
- Design system : utilisation cohérente des composants UI, patterns d'interaction, et styles visuels
- Navigation : parcours utilisateur logiques et prévisibles, pas de surprises
- Feedback utilisateur : messages d'erreur clairs et actionnables, confirmations pour actions destructives
- Accessibilité : respect des standards WCAG 2.1 niveau AA minimum
- Responsive : adaptation cohérente sur tous les appareils et tailles d'écran
- Performance perçue : indicateurs de chargement, optimisations pour réduire la latence visible

**Rationale** : Une expérience cohérente réduit la courbe d'apprentissage, augmente la satisfaction utilisateur, et renforce la confiance dans le produit.

### IV. Performance Requirements

Les pré-requis de performance DOIVENT être respectés :
- Métriques définies : chaque fonctionnalité DOIT avoir des objectifs de performance mesurables (temps de réponse, débit, utilisation mémoire)
- Benchmarks : tests de performance automatisés dans la CI/CD pour les composants critiques
- Monitoring : instrumentation pour mesurer les performances en production
- Optimisation proactive : profilage régulier, identification et correction des goulots d'étranglement
- Scalabilité : architecture conçue pour supporter la croissance prévue (utilisateurs, données, charge)
- Limites acceptables : définition claire des seuils de performance (p95, p99) et alertes si dépassement

**Rationale** : Les performances impactent directement l'expérience utilisateur et les coûts opérationnels. Des performances dégradées peuvent compromettre l'adoption du produit.

## Quality Standards

### Code Review Process

- Tous les changements DOIVENT passer par une review par au moins un pair
- Les reviews DOIVENT vérifier la conformité avec les principes de la constitution
- Les commentaires de review DOIVENT être adressés avant merge
- Aucun merge direct sur les branches principales sans review

### Documentation Requirements

- README principal : description du projet, installation, utilisation de base
- Documentation API : générée automatiquement ou maintenue manuellement pour toutes les APIs publiques
- Guides de contribution : standards de code, processus de développement, conventions de commit
- Changelog : historique des changements majeurs, accessible et à jour

### Technical Debt Management

- Dette technique identifiée et documentée (issues, backlog)
- Priorisation : dette critique traitée dans les 2 sprints suivants
- Refactoring : intégré régulièrement dans les sprints, pas seulement en "sprint de refactoring"
- Justification : toute complexité ajoutée DOIT être justifiée dans les PRs

## Development Workflow

### Branching Strategy

- Branches de fonctionnalité : `[###-feature-name]` pour chaque nouvelle fonctionnalité
- Branches principales protégées : `main` et `develop` (si applicable)
- Merge via Pull Requests uniquement, avec review obligatoire

### Commit Standards

- Messages de commit clairs et descriptifs
- Format recommandé : `type(scope): description` (ex: `feat(auth): add OAuth2 support`)
- Commits atomiques : un commit = une modification logique

### Testing Workflow

1. Écrire les tests (TDD pour nouvelles fonctionnalités)
2. Vérifier que les tests échouent (si nouveau code)
3. Implémenter la fonctionnalité
4. Vérifier que tous les tests passent
5. Refactoriser si nécessaire
6. Soumettre pour review

### Release Process

- Versioning : Semantic Versioning (MAJOR.MINOR.PATCH)
- Changelog : mis à jour avant chaque release
- Tests : tous les tests DOIVENT passer
- Documentation : mise à jour si nécessaire
- Tagging : tags Git pour chaque release

## Governance

Cette constitution est le document de référence suprême pour toutes les décisions techniques et de développement dans le projet RomPilot. Elle prime sur toute autre pratique ou convention.

### Amendment Process

Les modifications de cette constitution DOIVENT suivre ce processus :
1. Proposition : documenter la modification proposée avec justification
2. Review : discussion et validation par l'équipe
3. Impact analysis : évaluation de l'impact sur le code existant et les templates
4. Synchronisation : mise à jour de tous les templates et documents dépendants
5. Versioning : incrément de version selon les règles de semantic versioning
6. Communication : annonce de la modification à toute l'équipe

### Versioning Policy

- **MAJOR** : Suppression ou modification rétro-incompatible de principes, changements majeurs de gouvernance
- **MINOR** : Ajout de nouveaux principes ou sections, expansion significative de guidance existante
- **PATCH** : Clarifications, corrections de typo, refinements non-sémantiques

### Compliance Review

- Toutes les Pull Requests DOIVENT vérifier la conformité avec cette constitution
- Les violations DOIVENT être documentées et justifiées dans la section "Complexity Tracking" des plans
- Les reviews de code DOIVENT inclure une vérification de conformité
- Les violations répétées DOIVENT être escaladées et traitées

### Continuous Improvement

Cette constitution est un document vivant. Elle DOIT être révisée régulièrement pour s'assurer qu'elle reste pertinente et applicable. Les retours d'expérience de l'équipe DOIVENT être intégrés dans les révisions futures.

**Version**: 1.0.0 | **Ratified**: 2025-01-27 | **Last Amended**: 2025-01-27
