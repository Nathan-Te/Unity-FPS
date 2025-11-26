Ce fichier contient ce qui est actuellement implémenté dans le projet et à quel état si besoins de précisions.
Certaines fonctionnalités peuvent être tag comme n'étant pas complément mises en place via le tag "A-COMPLETER" (le système existe mais je n'ai pas encore crée les animations / VFX / SFX à inclure dans le système voire pas testé ce système)

=> Contrôleur & Mouvements (HeavyFPSController)
Moteur Physique : Déplacement basé sur Rigidbody (Interpolate). Gestion de l'inertie. Possibilité de s'accroupir

=> Body Awareness
Head Bob : Mouvement de tête procédural lié à la vitesse.
Heavy Sway : Retard de l'arme, inclinaison (Tilt) et respiration dynamique liée à la fatigue.
Landing Impact : Choc visuel et baisse de l'arme lors d'une chute lourde.
Stamina System : Jauge d'endurance limitant le sprint et influençant le tremblement de l'arme.
Vaulting : Enjambement procédural des obstacles (hauteur variable) via Raycasts. -> A-COMPLETER Animation de vault
Free Aim : La visée est découplée de la caméra dans une "deadzone" centrale.

=> Interactions Physiques (Interaction System)
Grab & Drag (PhysicsGrabber)
Système type Amnesia. Maintenir Clic Gauche pour attraper, bouger la souris pour manipuler.
Lancer : Clic Droit pour jeter l'objet avec impulsion.
Gestion des masses : Les objets lourds ralentissent le joueur.

=> Mécanismes Physiques
Tiroirs (PhysicsDrawer) : S'ouvrent en tirant la souris vers soi (ConfigurableJoint). Son progressif. A-COMPLETER SFX
Valves & Roues (PhysicsValve) : Se tournent en faisant des cercles à la souris. Événements déclenchés après X tours. A-COMPLETER SFX
Leviers (PhysicsLever) : Activation par basculement physique. A-COMPLETER SFX + Mettre en commun avec le Levier activable pour gérer les 2 fonctionnements ?
Leviers (Activable) : Activation en appuyant sur la touche E A-COMPLETER SFX + Mettre en commun avec le Levier physique pour gérer les 2 fonctionnements ?
Plaques de Pression (PressurePlate) : Activation par le poids du joueur ou d'objets (SpringJoint).
Interactions Classiques (PlayerInteractor) : Raycast simple (Touche E) pour les boutons, interrupteurs et ramassage d'objets.

=> Inventaire & Items (Inventory System)
Système Tetris : Grille spatiale (X, Y). Les objets ont une largeur/hauteur.
Fonctionnalités UI :
    Drag & Drop avec rotation (Touche R).
    Inspection 3D des objets (InspectionManager). A-COMPLETER à revoir la caméra d'inspection depuis le passage à HDRP
    Drop d'objets (réapparition physique dans le monde avec conservation des données). 
    Stacking : Les objets (ex: munitions) se cumulent dans une même case.

=> Combat & Armes (Combat System)
Architecture Héritée : Classe mère WeaponBase gérant les munitions, le rechargement et l'audio.
Hitscan Weapon (Pistolet/Fusil) : Tir immédiat (Raycast), application de force physique à l'impact, Decals. A-COMPLETER Animations / VFX / SFX / Decals
Projectile Weapon (Grenade/RPG) :
    Tir physique avec gravité.
    Système de visée convergent (le projectile part de la caméra vers le point visé par le crosshair décalé).
    Explosifs : Mode "Impact" (Roquette) ou "Timer" (Grenade). Dégâts de zone et force d'explosion. A-COMPLETER Animations / VFX / SFX / Decals
Wall Bounce (WeaponCollision) : L'arme se relève et recule physiquement si le joueur est trop près d'un mur. Le tir est bloqué.
Recul Procédural :
    Arme : Kickback et rotation.
    Caméra : La vue monte à chaque tir.
    UI : Le réticule dynamique s'écarte.

=> Gestion des armes via l'inventaire
Weapon Manager : Lier l'inventaire aux mains du joueur.

=> Environnement & Destruction
Portes Dynamiques : Ouverture physique, verrouillage, coup d'épaule (Bash) en sprintant. A-COMPLETER SFX
Portes Automatiques : Portes coulissantes (Sliding) avec détection de présence. A-COMPLETER SFX

=> Destruction Utilitaires :
Cadenas : Peuvent être brisés par un tir pour déverrouiller une porte. A-COMPLETER MODEL + MODEL CASSÉ
Planches : Peuvent être détruites (tir) ou arrachées à la main (PhysicsGrabber vs Joint BreakForce). A-COMPLETER MODEL + MODEL CASSÉ
Digicode : Interaction physique touche par touche. A-COMPLETER MODEL + SFX
Ascenseur : Plateforme physique mobile. A-COMPLETER MODEL + SFX

=> Phase "Game Feel" & Audio
Surface Manager : Système central pour gérer les matériaux.
Sons de pas (Footsteps) selon la texture (Bois, Métal, Béton). A-COMPLETER SFX
Impacts de balles (Sons + Particules spécifiques par matière). A-COMPLETER DECAL + SFX