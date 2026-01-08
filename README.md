# Tesztfeladat

A feladat egy három fő részből álló rendszer:
  Egy kezelőfelület ami egy a Blazor használatával készült webalkalmazás
  Egy szerver, ami .Net WEB Api elérésen keresztül biztosít kapcsolatot az adatbázissal (egy PostgreSQL adatbázis, aminek a kezelését EntityFramework végzi)
  És egy erőforrás monitor, ami egy .Net working service, kétirányú TCP kapcsolattal a szerverrel
  
## Installation

PostgreSQL telepítése alapértelmezett beállításokkal, tetszőleges jelszóval ( https://www.postgresql.org/download/ )

Psql CLI megnyitása (autentikáció az alapértelmezett adatokkal: localhost -> postgres -> 5432 -> postgres -> jelszó)

A következő lefuttatása az imént nyitott CLI-ben: \i git_repo_elérési_útja/SQL/Tables_and_data.sql

VisualStudio telepítése ( https://visualstudio.microsoft.com/ ) ASP.NET and web development, és ha nincs benne a csomagban a .NET 8.0 Runtime és WebAssembly hozzáadásával az installerben

A VisualStudio megnyitásakor Open a projekt or solution, és a git_repo_elérési_útja/Device mappában található Device.slnx fájl megnyitása

A Solution Explorerben jobb kattintás a Solution-re, majd Properties -> Configure Startup Projects -> Multiple startup projects -> Mindjárom projektnél az action-t Start-ra kell állítani -> Ok

A Start vagy a Start without debugging gombokkal indítható a rendszer

## Usage

A kezelőfelület és a swagger automatikusan nyílik egy-egy böngésző ablakban

A kezelőfelületen regisztrációt követően be lehet jelentkezni

Bejelentkezés után a NavBar-on látható menük között lehet barangolni

A kezdőlap (Home) mutatja az aktuális eszközöket, azok utólsó mért értékeit, és hogy elérhetőek-e

A History oldalon láthatjuk az összes mérést, vagy szűrhetünk eszköz vagy idősáv alapján

A Device Parameters oldalon pedig láthatjuk milyen változókkal dolgozik egy-egy eszköz, illetve módosítani tudjuk azokat

Van egy Logout gomb weboldal jobb felső sarkában, amivel kijelentkezhetünk
