CREATE TABLE "tracked_files" (
	"FilePath"	TEXT NOT NULL UNIQUE,
	"RelativePath"	TEXT NOT NULL UNIQUE,
	"ProfileId"	TEXT NOT NULL,
	"FileHash" TEXT NOT NULL,
	"MetadataHash" TEXT NOT NULL,
	"FirstCheckedTime"	TEXT NOT NULL,
	"LastCheckedTime"	TEXT,
	"LastSynchronisedTime"	TEXT,
	PRIMARY KEY("FilePath")
);