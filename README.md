# GameDrive
> **Note:** This project is not yet completed and is still in active development.

GameDrive is a self-hosted file-synchronisation tool oriented towards video game save files, and is designed to mimic the simplicity of [Steam Cloud](https://partner.steamgames.com/doc/features/cloud) (without being limited to Steam games).

## How it works

The application as a whole consists of two components: [GameDrive.Client](https://github.com/brodie124/GameDrive.Client) and [GameDrive.Server](https://github.com/brodie124/GameDrive.Server).


## Glossary

| Term           | Meaning                                                                               |
|----------------|---------------------------------------------------------------------------------------|
| Game Profile   | Collection of possible game-save directories/files, as retrieved from the PCGameWiki. |
| Storage Bucket | Collection of storage objects grouped by an ID (typically the ID of a game profile).  | 
| Storage Object | Complete representation of a specific file at a given point in time.                  |
