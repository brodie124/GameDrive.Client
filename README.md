# GameDrive
> **Note:** This project is not yet completed and is still in active development.

GameDrive is a self-hosted file-synchronisation tool oriented towards video game save files, and is designed to mimic the simplicity of [Steam Cloud](https://partner.steamgames.com/doc/features/cloud) (without being limited to Steam games).

## How it works
### Architecture
![](/Docs/Images/ServerClientRelationshipDiagram.png)

The application as a whole consists of two key components: [GameDrive.Client](https://github.com/brodie124/GameDrive.Client) and [GameDrive.Server](https://github.com/brodie124/GameDrive.Server).

The server is responsible for handling user authentication, as well as comparing, uploading and downloading files.
Whilst the client is responsible for identifying games/game saves, tracking these files, detecting deleted files,
and submitting this information to the server.

> File tracking is the process of creating a simple representation of an entire file.
> This allows information about a file to be sent over the network without sending the whole file, and includes data such as:
> * The path of the file
> * The hash/checksum of the file
> * When it originally created
> * When it was last modified

## Glossary

| Term           | Meaning                                                                               |
|----------------|---------------------------------------------------------------------------------------|
| Game Profile   | Collection of possible game-save directories/files, as retrieved from the PCGameWiki. |
| Storage Bucket | Collection of storage objects grouped by an ID (typically the ID of a game profile).  | 
| Storage Object | Complete representation of a specific file at a given point in time.                  |
