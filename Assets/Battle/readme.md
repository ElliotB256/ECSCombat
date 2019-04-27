# The Game Loop

Here's how the game loop is currently laid out.

| System/Group           | Description                                              |
|------------------------|----------------------------------------------------------|
| WeaponSystems          | Spawns for attack entities as weapons are fired.         |
| WeaponEntityBuffer     | Execute attack spawning buffer                           |
| AttackSystems          | Do something with created attacks.                       |
| AttackResultSystems    | Handle result of attacks, eg spawn effects, deal damage. |
| DestroyAttacksSystem   | Flags attacks to be destroyed                            |
| PostAttackEntityBuffer | Execute buffer to destroy attacks, results from attacks. |
| Equipment Systems      | Equipment changes.                                       |
| MovementUpdateSystems  | Systems that update positon of entities.                 |



## Equipment systems: 

The equipment systems are laid out as follows:

1. The EarlyEquipmentUpdateGroup updates.

| System                 | Description                                              |
|------------------------|----------------------------------------------------------|
| DisableBrokenEquipment | Adds Disabling to Equipment with health <= 0             |


1. The EquipmentUpdateGroup updates.

| System                 | Description                                              |
|------------------------|----------------------------------------------------------|
| EngineSystem           | Adds thrust from attached engines to parent thrust       | 

2. The EnableDisableSystem changes components Enabling->Enabled and Disabling->Disabled.
3. The EquipmentBufferSystem executes the results of the equipment systems and the EnableDisableSystem.

