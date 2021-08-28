# AtlasClientSimulator

A C# based headless DAOC client intended to provide ease of debugging and stress testing the AtlasCore GameServer.

This is currently based on client version 1124.

## Getting started

This project references AtlasCore/DOLDatabase ("..\AtlasCore\Net5\DOLDatabase\DOLDatabase.csproj") and must be cloned in parallel with AtlasCore. It might be possible to remove this reference at some point.

The simulator has pretty limited capabilties at the moment and is mostly hardcoded to handle creation/login of level 50 paladins.

One usage is provided that will create the required accounts and chars in the database if needed. You will need to update PlayerCreator.cs with your specific mySQL database details (username/password)

In program.cs - spamAlbDragon() you can change some settings related to the spawn location and number of chars created. 

## TODO

Create CLI that provides access to functionality such as Account Creation, Char Creation, Login/Logout, and Spellcasting.

Move "spamAlbDragon()" functionality into a menu option of the CLI and create a pattern for the team to create mocked/preset scenarios to be used, this will ensure that when the team is running benchmarks there is a consistent laod being applied.