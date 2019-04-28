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

### The EarlyEquipmentUpdateGroup updates.

| System                 | Description                                              |
|------------------------|----------------------------------------------------------|
| DisableBrokenEquipment | Adds Disabling to enabled Equipment with health <= 0     |
| EquipSystem            | Adds equipment to parent's EquipmentList, adds Equipped  |

### EarlyEquipmentBufferSystem

Executes changes from the EarlyEquipmentUpdateGroup.

### The EquipmentUpdateGroup updates.

| System                 | Description                                              |
|------------------------|----------------------------------------------------------|
| EngineSystem           | Adds/removes thrust from attached engines to parent      | 

### EnableDisableSystem

Changes components Enabling->Enabled and Disabling->Disabled.
Deletes the components Disabling and Enabling.

### EquipmentBufferSystem

Executes the results of the equipment systems and the EnableDisableSystem.

