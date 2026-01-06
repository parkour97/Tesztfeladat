A megfelelő működés előfeltétele a postgresql telepítése, ami a következő linken keresztül elérhető: https://www.postgresql.org/download/

A telepítés után meg kell adni egy admin jelszót, ez szükséges lesz az sql parancsok futtatásához.

Az adatbázis táblákat, triggereket és kezdeti adatokat létrehozó SQL kód az SQL mappában található "Tables_and_data.sql" néven.

A futtatáshoz Windows alatt a psql CLI-t kell megnyitni, majd alapértelmezetten a localhost -> postgres -> 5432 -> postgres -> a konfigurációkor megadott jelszó adatokat kell kitölteni, és lefuttatni a következő kódot: \i git_repo_elérési_útja/SQL/Tables_and_data.sql

A visual studio-ban egyszerre kell induljon a 3 projekt, ezt a solution properties menüjében a Multiple startup project bepipálásával, majd mindhárom projekt melletti legördűlő menüből a Start kiválasztásával érhetjük el.
Ha ez kész, buildelhető és indítható a projekt.
