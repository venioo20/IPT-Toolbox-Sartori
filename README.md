# IPT Toolbox Sartori — Informatique Pour Tous

Version 0.4.0. Projet Windows Forms C# .NET 8, modifié à partir de votre v0.1.

**Site officiel :** https://venioo20.github.io/IPT-Toolbox-Sartori/

## Licences professionnelles (première étape)

Le bouton **LICENCES PRO** ouvre un inventaire local permettant d'ajouter un produit, son édition, son type de licence, le nombre de places, les échéances et, lorsque le contrat l'autorise, ses clés. Les licences peuvent être attribuées manuellement à un client et un poste, puis libérées. Des états signalent un stock faible, épuisé ou une expiration proche.

Le coffre complet est chiffré par Windows pour l'utilisateur courant et stocké dans `%LOCALAPPDATA%/IPT Toolbox Sartori/license-vault.dat`. Il ne doit pas être copié dans le dépôt GitHub. Les clés ne sont jamais affichées dans les tableaux ou écrites dans les journaux. Cette première étape ne réalise aucune activation automatique auprès d'un éditeur et ne synchronise pas plusieurs techniciens.

## IPT Toolbox Pro

L'accès Pro est protégé par une licence signée. La clé privée de signature reste hors du dépôt ; seule la clé publique de vérification est incluse dans l'application.

- abonnement mensuel : 9,90 € via PayPal ;
- licence définitive : 79,90 € via PayPal ;
- accès au coffre Licences Pro ;
- ajout local de logiciels personnalisés avec un identifiant WinGet exact.

Le paiement PayPal n'active pas automatiquement le logiciel. Le propriétaire vérifie le paiement, puis crée une clé avec `tools/Create-IPTProLicense.ps1` et sa clé privée conservée séparément. Les clés commerciales des logiciels tiers restent à la charge du professionnel et ne sont jamais fournies par IPT Toolbox.

## Utilisation

Extraire entièrement l'archive, puis ouvrir `publish/win-x64/IPT.Toolbox.exe` (ou `IPT.Toolbox.exe` dans l'archive application seule). Choisir un profil ou des logiciels, puis cliquer sur PRÉPARER CE PC.

Au démarrage : aucun logiciel coché, mode simulation actif. La simulation ne lance aucune commande WinGet. Pour installer, cocher explicitement MODE EXÉCUTION puis cliquer sur PRÉPARER CE PC. Cette autorisation n'est pas mémorisée après fermeture.

La version autonome inclut .NET 8. Windows x64 est requis. Les installations réelles nécessitent WinGet (Installateur d'application Microsoft), Internet et parfois une élévation Windows. Les licences et comptes propres aux logiciels restent applicables. Les journaux sont enregistrés dans logs : extraire dans un dossier accessible en écriture.

## Changements

- Nom et titre : IPT Toolbox Sartori — Informatique Pour Tous.
- 50 logiciels, affichés par catégorie : Utilitaires, Internet, Bureautique, Multimédia, Création, Technicien, Développement, Sécurité et sauvegarde.
- 8 profils : Personnalisé, Essentiel, Bureautique, Multimédia, Création, Technicien, Développement, Sécurité et sauvegarde.
- Aucun logiciel présélectionné ; simulation par défaut sans commande WinGet.
- Installation par identifiant exact et source winget.
- Pas d'outil de contournement, nettoyage de registre, activation non officielle ou désactivation antivirus.
- Options restauration, confidentialité et Windows Update désactivées et marquées « bientôt » : ces modules ne sont pas implémentés.
- Contexte nullable corrigé dans MainForm.Designer.cs avec #nullable enable.
- Initialisation corrigée pour appliquer le premier profil après chargement de la liste.

## Catalogue

config/packages.json contient les 50 noms, catégories, identifiants et notes. config/profiles.json définit les profils. config/winget-verification.json conserve les liens des 50 dossiers confirmés dans le dépôt Microsoft WinGet le 19 septembre 2026.
Source : https://github.com/microsoft/winget-pkgs

La présence du manifeste valide l'identifiant, pas la réussite future de toutes les installations. Les versions disponibles et les licences peuvent évoluer. Aucun des logiciels du catalogue n'a été installé pour cette livraison.

## Compilation

Depuis PowerShell : ./build.ps1 puis ./publish.ps1. Un SDK compatible .NET 8 est nécessaire. Cette livraison a été construite avec le SDK 10.0.401 en conservant net8.0-windows.

Les tests dans tests/IPT.Toolbox.Checks.csproj contrôlent le catalogue, les profils, la simulation complète sans WinGet et son annulation. Avec .NET 8 installé : dotnet run --project tests/IPT.Toolbox.Checks.csproj -- .

## Limites

En mode réel, la détection des logiciels déjà présents dépend de WinGet. La simulation décrit seulement les actions prévues. L'annulation ne garantit pas l'arrêt d'un installateur Windows déjà lancé. L'application n'est pas signée numériquement. Aucun test d'installation réelle ni contrôle visuel interactif n'a été effectué pour cette livraison.

## Publication et sécurité

Les commandes WinGet utilisent des arguments séparés et des identifiants validés. Le dépôt contient une politique SECURITY.md, des exclusions de fichiers sensibles et une compilation automatique avec permissions en lecture seule. Les actions sont figées sur leurs empreintes de commit.

Aucune licence de réutilisation générale n'est accordée pour le moment. La visibilité publique ne vaut pas autorisation de redistribution du code ; les conditions GitHub restent applicables.

