# The Game Loop

Here's how the game loop is currently laid out.

+------------------------+----------------------------------------------------------+
| WeaponSystems          | Spawns for attack entities as weapons are fired.         |
| WeaponEntityBuffer     | Execute attack spawning buffer                           |
| AttackSystems          | Do something with created attacks.                       |
| AttackResultSystems    | Handle result of attacks, eg spawn effects, deal damage. |
| DestroyAttacksSystem   | Flags attacks to be destroyed                            |
| PostAttackEntityBuffer | Execute buffer to destroy attacks, results from attacks. |
| MovementUpdateSystems  | Systems that update positon of entities.                 |
+------------------------+----------------------------------------------------------+
