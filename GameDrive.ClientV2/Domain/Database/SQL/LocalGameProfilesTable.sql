CREATE TABLE "local_game_profiles" (
	"Id"	TEXT NOT NULL UNIQUE,
	"Name"	TEXT NOT NULL,
	"BaseDirectory_GdPath"	TEXT NOT NULL,
	"BaseDirectory_ResolvedPath"	TEXT NOT NULL,
	PRIMARY KEY("Id")
);