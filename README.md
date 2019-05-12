# Flow-shop GA

Projekt z przedmiotu Optymalizacja kombinatoryczna (2019 r.)

Autor:
Mateusz Bąk\
Politechnika Poznańska, Wydział Informatyki

## Uwagi

Przy pierwszym uruchomieniu programu w katalogu z plikiem .exe (np. `Flow-shop\bin\Release`) generują się pliki konfiguracyjne oraz katalog "instances".

Do katalogu `instances` trafiają wszystkie wygenerowane przez program instancje oraz jest to domyślny katalog wyszukiwania instancji do rozwiązania.\
Pliki `*_in.txt` zawierają dane instancji zgodnie z formatem opisanym w pliku Problemy2018.pdf.\
Pliki `*_out.txt` zawierają dane uzyskanych rozwiązań zgodnie z formatem opisanym w pliku Problemy2018.pdf.\
Pliki `*.in` zawierają zserializowane dane instancji i są wykorzystywane po wydaniu polecenia rozwiązania instancji.

Generator instancji tworzy zarówno plik `<numer_instancji>_in.txt`, jak i `<numer_instancjii>.in`.
Aby uruchomić rozwiązywanie instancji opisanej w pliku `*_in.txt` w przypadku, gdy brakuje pliku `*.in`, program umożliwia zaimportowanie instancji z pliku tekstowego i tworzy plik `*.in`.

Plik `InstanceGeneratorProperties.xml` umożliwia regulację parametrów generatora instancji.\
Plik `SolverProperties.xml` umożliwia regulację parametrów metaheurystyki.\
Plik `Schedule.xml` był używany podczas testowania do tworzenia harmonogramów rozwiązywania instancji.
