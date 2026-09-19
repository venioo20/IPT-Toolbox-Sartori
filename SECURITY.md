# Sécurité

Ne publiez jamais de mot de passe, jeton, journal de poste client ou donnée personnelle dans une issue ou une contribution.

Pour signaler une vulnérabilité, utilisez « Security > Report a vulnerability » si cette option est disponible. Sinon, ouvrez une issue demandant un canal privé, sans détails exploitables ni données sensibles.

## Protections de l'application

- Simulation par défaut ; l'exécution réelle exige une action explicite à chaque lancement.
- Aucun privilège administrateur demandé au démarrage de l'application.
- Identifiants WinGet validés et arguments séparés, sans shell.
- Source WinGet explicite, identifiant exact ; aucune désactivation du contrôle des empreintes des installateurs.
- Aucun jeton GitHub ni secret nécessaire pour utiliser l'application.
- Modules Windows incomplets désactivés dans l'interface.

La vérification du catalogue ne remplace pas la confiance dans les éditeurs. N'utilisez que des archives de provenance connue et vérifiez leur SHA-256. Une empreinte détecte une modification, mais ne remplace pas une signature numérique.

Le programme n'est pas signé. L'annulation arrête la file de travail sans garantir l'arrêt d'un installateur déjà lancé. Les journaux restent locaux et peuvent contenir des chemins propres au poste : ne les publiez pas sans les relire.
