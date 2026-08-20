# TeamGenocide 3.0

> Portage EXILED 9.14.2 d'un plugin de **Heisenberg3666**. Depot non affilie a
> l'auteur d'origine. Voir [NOTICE.md](NOTICE.md) pour l'attribution.

Annonce l'extinction complete d'une equipe : C.A.S.S.I.E, broadcast, hint et
effet lumineux.

**EXILED 9.14.2** — `dotnet build -c Release TeamGenocide/TeamGenocide.csproj`

## Configuration

`announcements` associe une equipe a une liste d'annonces ; une entree est tiree
au hasard a chaque declenchement. Cinq equipes sont couvertes par defaut :
Classe-D, SCP, scientifiques, Insurrection du Chaos et forces de la Fondation.

| Cle | Defaut | Role |
|---|---|---|
| `activation_delay` | `5` | Delai apres le debut du round avant activation de la detection |
| `announce_once_per_team` | `true` | Une seule annonce par equipe et par round |
| `hint_y_coordinate` | `500` | Ligne HintServiceMeow reservee a ce plugin |

Chaque annonce porte son texte C.A.S.S.I.E, ses sous-titres, son broadcast, son
hint, sa duree et son effet lumineux (couleur, zones, duree).

## Dependances

Ce plugin depend de **AugatonLib**, la bibliotheque partagee de la
collection.

| Fichier | Destination |
|---|---|
| `TeamGenocide.dll` | `Plugins/7777/` |
| `AugatonLib.dll` | `Plugins/dependencies/` |
| HintServiceMeow | `Plugins/7777/` |

`AugatonLib.dll` ne va **jamais** dans `Plugins/7777/` : EXILED
tenterait de le charger comme plugin. Il doit etre deploye avant ce plugin et
mis a jour en meme temps.

Pour compiler ce depot isolement, cloner
[AugatonLib](https://github.com/Augaton/AugatonLib) a cote,
ou passer `-p:CommonProject=chemin/vers/AugatonLib.csproj`.

## Commandes staff

| Commande | Permission | Effet |
|---|---|---|
| `teamgenocide status` | `teamgenocide.manage` | Equipes couvertes et delai d'activation |

Alias `tg`.

Toutes les commandes de la collection partagent le meme socle : verification de
permission en premiere ligne, arguments bornes en longueur, exceptions
capturees, actions a impact tracees avec l'auteur. Une commande parente sans
argument liste ses sous-commandes.

## Note de portage

La version 2.x ciblait EXILED 8.11.0. Trois defauts corriges :

- **Desabonnement par finaliseur.** Les evenements etaient souscrits dans le
  constructeur et desouscrits dans `~PlayerEvents()`. Un finaliseur s'execute
  quand le ramasse-miettes le decide, parfois jamais : les handlers survivaient
  au `reload` et se declenchaient en double. `OnDisabled` se contentait de
  mettre la reference a `null`.
- **Lumieres jamais restaurees.** `ChangeLights` coupait les lumieres puis, apres
  la duree, les recoupait au lieu de restaurer la couleur d'origine. Les
  callbacks differes n'etaient jamais annules au changement de round.
- **Faux positif au respawn.** L'annonce se declenchait meme quand le nouveau
  role appartenait a la meme equipe.

Le comptage LINQ dans le handler `ChangingRole` est remplace par une boucle
explicite, et l'etat est remis a zero a chaque transition de round.
