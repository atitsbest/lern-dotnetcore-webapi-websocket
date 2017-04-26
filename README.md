# Zweck
Gestartet hat es mir der dotnet CLI. 
Geendet hat es mit einem kleinen ASP.NET Core WebAPI Projekt, bearbeitet in VSCode garniert mit WebSockets.

# Gelernt
 
## Neue dotnet Core Projekte erstellen mit der dotnet CLI

`dotnet new <template> -o <verzeichnis>`

Welche Templates gibt es?
`dotnet new -all`

In diesem Fall also:
`dotnet new webapi -o lernen_webapi`

# Projekte 'builden' und starten

`dotnet build`

Davor müssen aber alle Nuget-Packages erstmal installiert werden. 
VSCode fragt hier automatisch nach; per CLI geht das so: `dotnet restore` (dazu muss man sich im Verzeichnis des Projekts befinden).

Um den ASP.NET Core WebAPI Server zu starten: `dotnet run`


# WebSockets

kommt noch...