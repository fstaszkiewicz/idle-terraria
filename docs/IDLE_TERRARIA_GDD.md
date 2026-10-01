# IDLE TERRARIA

## Dokument Projektowy Gry i Architektury Systemu

**Typ projektu:** edukacyjna, przeglądarkowa gra multiplayer Idle RPG  
**Charakter projektu:** projekt non-profit, fanmade, realizowany w ramach pracy licencjackiej  
**Repozytorium:** `fstaszkiewicz/idle-terraria`  
**Główna technologia backendu:** .NET 10 / C#  
**Baza danych:** PostgreSQL  
**ORM:** Entity Framework Core  
**Stan dokumentu:** wersja projektowa zgodna z aktualnym stanem backendu na gałęzi `main`  
**Data opracowania:** 25 września 2026 r.

---

# Spis treści

1. [Cel i status dokumentu](#1-cel-i-status-dokumentu)
2. [Tabela postępu projektu](#2-tabela-postępu-projektu)
3. [CZĘŚĆ I: Opisowe założenia projektowe i mechaniki gry](#część-i-opisowe-założenia-projektowe-i-mechaniki-gry)
   1. [Wizja projektu](#31-wizja-projektu)
   2. [Grupa docelowa i model biznesowy](#32-grupa-docelowa-i-model-biznesowy)
   3. [Główna pętla rozgrywki](#33-główna-pętla-rozgrywki)
   4. [Progresja horyzontalna i współzależność aktywności](#34-progresja-horyzontalna-i-współzależność-aktywności)
   5. [Świat i progresja biomów](#35-świat-i-progresja-biomów)
   6. [Bohater i statystyki](#36-bohater-i-statystyki)
   7. [Ekwipunek, prefiksy i ulepszanie](#37-ekwipunek-prefiksy-i-ulepszanie)
   8. [System aktywności](#38-system-aktywności)
   9. [Siedziba gracza i NPC](#39-siedziba-gracza-i-npc)
   10. [Sklep i gospodarka](#310-sklep-i-gospodarka)
   11. [Walka PvE i Idle](#311-walka-pve-i-idle)
   12. [Arena PvP](#312-arena-pvp)
   13. [Gildie, osady i wojny](#313-gildie-osady-i-wojny)
   14. [Inwazje grupowe](#314-inwazje-grupowe)
   15. [Interfejs użytkownika](#315-interfejs-użytkownika)
   16. [Balans i ochrona przed dominacją jednej strategii](#316-balans-i-ochrona-przed-dominacją-jednej-strategii)
4. [CZĘŚĆ II: Architektura techniczna i implementacja](#część-ii-architektura-techniczna-i-implementacja)
   1. [Rzeczywisty zakres implementacji](#41-rzeczywisty-zakres-implementacji)
   2. [Podział aplikacji na warstwy](#42-podział-aplikacji-na-warstwy)
   3. [Przepływ żądania HTTP](#43-przepływ-żądania-http)
   4. [Konfiguracja aplikacji i uruchamianie](#44-konfiguracja-aplikacji-i-uruchamianie)
   5. [Warstwa API](#45-warstwa-api)
   6. [Warstwa usług aplikacyjnych](#46-warstwa-usług-aplikacyjnych)
   7. [Autoryzacja i bezpieczeństwo](#47-autoryzacja-i-bezpieczeństwo)
   8. [Model domenowy i encje](#48-model-domenowy-i-encje)
   9. [Baza danych i Entity Framework Core](#49-baza-danych-i-entity-framework-core)
   10. [Relacje i integralność danych](#410-relacje-i-integralność-danych)
   11. [Strategia usuwania i Multiple Cascade Paths](#411-strategia-usuwania-i-multiple-cascade-paths)
   12. [Migracje](#412-migracje)
   13. [Mechanizmy jeszcze niezaimplementowane](#413-mechanizmy-jeszcze-niezaimplementowane)
   14. [Docelowy przepływ obliczeń Idle](#414-docelowy-przepływ-obliczeń-idle)
   15. [Testowanie i jakość](#415-testowanie-i-jakość)
5. [Roadmapa implementacyjna](#5-roadmapa-implementacyjna)
6. [Otwarte decyzje projektowe](#6-otwarte-decyzje-projektowe)
7. [Podsumowanie](#7-podsumowanie)

---

# 1. Cel i status dokumentu

Niniejszy dokument pełni jednocześnie trzy funkcje:

1. opisuje projekt gry z perspektywy projektowania rozgrywki,
2. definiuje docelową architekturę systemu,
3. wskazuje różnicę pomiędzy planowanymi mechanikami a funkcjonalnościami obecnie znajdującymi się w kodzie.

Dokument jest źródłem założeń projektowych dla dalszego rozwoju gry. Każdą nową mechanikę należy przed implementacją opisać w odpowiedniej części dokumentu, a po implementacji uaktualnić tabelę postępu.

## 1.1. Zasada rozdzielenia projektu od implementacji

Założenia opisane w części pierwszej są docelową wizją gry. Nie wszystkie elementy są jeszcze obecne w kodzie.

Część druga opisuje stan faktyczny backendu:

- istnieje projekt ASP.NET Core Web API,
- istnieje konfiguracja Entity Framework Core z PostgreSQL,
- istnieje model encji obejmujący większość planowanych systemów,
- działa rejestracja i logowanie użytkownika,
- działa generowanie tokenów JWT,
- działa odczyt profilu gracza,
- istnieją migracje bazy danych,
- nie istnieje jeszcze implementacja większości właściwych mechanik gry,
- serwisy `CombatMathService`, `IdleBatchingService` i `WanderingShopService` są obecnie pustymi szkieletami,
- huby SignalR istnieją jako pliki, ale nie są jeszcze podłączone do potoku aplikacji.

---

# 2. Tabela postępu projektu

Statusy używane w dokumencie:

- **Zaimplementowano** – funkcjonalność posiada działający kod lub kompletny model techniczny.
- **W trakcie** – istnieją encje, migracje, interfejsy lub częściowa logika, ale funkcja nie tworzy jeszcze kompletnego przepływu użytkownika.
- **Do zrobienia** – funkcja jest opisana projektowo, ale nie posiada istotnej implementacji.

## 2.1. Postęp funkcjonalny

| Mechanika / system | Status | Uzasadnienie |
|---|---|---|
| Projekt backendu ASP.NET Core Web API | Zaimplementowano | Istnieje projekt `IdleTerraria.Api`, konfiguracja hosta i kontrolery. |
| Połączenie z PostgreSQL | Zaimplementowano | `Program.cs` rejestruje `ApplicationDbContext` przez `UseNpgsql`. |
| Konto użytkownika | Zaimplementowano | Istnieje encja `Account`, tabela `accounts`, email, hash hasła i data utworzenia. |
| Relacja Konto–Gracz 1:1 | Zaimplementowano | `Player.AccountId` posiada unikalny indeks i klucz obcy do `accounts`. |
| Rejestracja | Zaimplementowano | `AuthController` i `AuthService` tworzą konto, gracza i statystyki w transakcji. |
| Logowanie | Zaimplementowano | Logowanie wykorzystuje email, hashowanie haseł i JWT. |
| JWT i autoryzacja | Zaimplementowano | Skonfigurowano walidację podpisu, wystawcy, odbiorcy i czasu życia tokenu. |
| Profil gracza | Zaimplementowano | `GET /api/Player/me` zwraca dane profilu oraz podstawowe statystyki. |
| Podstawowe statystyki gracza | Zaimplementowano | Istnieją encje `Player` i `PlayerStats`. |
| Poziom, EXP, złoto, pył i energia | Zaimplementowano jako model danych | Pola znajdują się w tabeli `players`, ale brak logiki ich przyznawania i zużywania. |
| Arena ELO | W trakcie | Pole `ArenaElo` istnieje w `Player`, ale brak silnika walk i rankingów. |
| Ekwipunek | W trakcie | Istnieją `Inventory`, `ItemTemplate`, `ItemCategory` i migracje, ale brak operacji zdobywania, zakładania i sprzedaży. |
| Prefiksy przedmiotów | W trakcie | Pole `Inventory.Prefix` istnieje, lecz nie ma generatora RNG ani systemu przekuwania. |
| Ulepszanie przedmiotów | W trakcie | `Inventory.UpgradeLevel` istnieje, lecz nie ma logiki kosztów, szans i zużywania materiałów. |
| Loadouty | W trakcie | Istnieje encja `Loadout` wraz z referencjami do broni, pancerza i chowańca. Brak obsługi API i kalkulatora statystyk. |
| Drzewko umiejętności | W trakcie | Istnieją `SkillTree` i `PlayerUnlockedNode`, ale brak walidacji, kosztów i przyznawania punktów. |
| Profesje poboczne | W trakcie | Istnieje `PlayerProfession`, ale brak progresji Mining, Fishing, Lumbering i Bug Catching. |
| Stan aktywności Idle | W trakcie | Istnieje `PlayerActivityState` z timestampami potrzebnymi do batchingu. Brak kalkulatora nagród. |
| Batching Idle | Do zrobienia | `IdleBatchingService` jest pustym szkieletem. |
| Kalkulator walki PvE | Do zrobienia | `CombatMathService` jest pustym szkieletem. |
| Siedziba gracza | W trakcie | Istnieje `HeadquarterNpc`, morale i odblokowanie NPC, ale brak logiki siedziby. |
| Wędrowny handlarz | W trakcie | Istnieje `WanderingShopStock` z ceną, modyfikatorem i datą wygaśnięcia. Brak rotacji i zakupów. |
| Rzemiosło Goblina | Do zrobienia | Brak endpointów i logiki Reforge/Upgrade. |
| PvP logi JSONB | W trakcie | Istnieje `PvpLog` z `JsonDocument` mapowanym do `jsonb`, ale brak generowania logów. |
| Arena PvP | Do zrobienia | Brak kontrolera, serwisu walk oraz animowalnego przebiegu starcia. |
| SignalR – czat | Do zrobienia | `ChatHub.cs` istnieje, lecz jest pusty i nie jest mapowany w `Program.cs`. |
| SignalR – wydarzenia gry | Do zrobienia | `GameEventHub.cs` istnieje, lecz nie ma implementacji ani rejestracji. |
| Gildie / osady | W trakcie | Istnieją `Settlement`, `SettlementMember` i `SettlementUpgrade`. Brak operacji zarządzania. |
| Wojny gildii | Do zrobienia | Brak drabinki, rejestracji uczestników, kalkulatora oraz nagród. |
| Inwazje grupowe PvE | Do zrobienia | Brak encji wydarzeń, harmonogramu, kalkulacji DPS i nagród. |
| Sklepy i waluty | W trakcie | Model przechowuje złoto i pył oraz stock sklepu, ale nie istnieje gospodarka gry. |
| Frontend React | Do zrobienia w tym repozytorium | W repozytorium znajduje się backend C#; nie ma katalogu klienta React/Vite. |
| Mobilny interfejs Swipe UI | Do zrobienia | Brak kodu frontendu. |
| Animacje walki | Do zrobienia | Brak logów walk i klienta odtwarzającego przebieg. |
| Sezony / serwery świata | Do zrobienia | Brak modelu świata, serwera gry i systemu rankingów per świat. |

## 2.2. Postęp techniczny

| Obszar techniczny | Status | Uzasadnienie |
|---|---|---|
| ASP.NET Core 10 | Zaimplementowano | Projekt używa `net10.0`. |
| Dependency Injection | Zaimplementowano | Serwisy i kontekst są rejestrowane w `Program.cs`. |
| EF Core | Zaimplementowano | Istnieje kontekst, encje i migracje. |
| PostgreSQL / Npgsql | Zaimplementowano | Skonfigurowano provider Npgsql. |
| Fluent API dla relacji krytycznych | Zaimplementowano | Relacje Account–Player, PvpLog–Player i Loadout–Inventory mają jawne konfiguracje. |
| Ochrona przed Multiple Cascade Paths | Zaimplementowano częściowo | Relacje PvP i Loadout–Inventory używają `DeleteBehavior.Restrict`. |
| DTO dla API | Zaimplementowano | Istnieją osobne requesty i response’y dla autoryzacji oraz profilu. |
| Warstwa usług | W trakcie | Istnieją interfejsy i działające usługi autoryzacji/profilu; logika gry nie jest gotowa. |
| Migracje wersjonujące schemat | Zaimplementowano | Istnieje kilka migracji oraz `ApplicationDbContextModelSnapshot`. |
| Walidacja wejścia | Zaimplementowano częściowo | Requesty używają Data Annotations, ale brakuje kompleksowej walidacji domenowej. |
| Testy automatyczne | Do zrobienia | W aktualnym drzewie repozytorium nie ma projektu testowego. |
| Obsługa wyjątków i jednolity format błędów | Do zrobienia | Kontrolery zwracają lokalne obiekty błędów, brak globalnego middleware. |
| Logowanie i monitoring | W trakcie | Podstawowy logging ASP.NET Core istnieje, brak monitoringu zdarzeń gry. |

---

# CZĘŚĆ I: OPISOWE ZAŁOŻENIA PROJEKTOWE I MECHANIKI GRY

# 3.1. Wizja projektu

`Idle Terraria` jest przeglądarkową grą wieloosobową typu Idle RPG, inspirowaną uniwersum, biomami i przedmiotami znanymi z gry Terraria.

Gra łączy:

- asynchroniczną progresję,
- rozwój bohatera,
- zbieranie i ulepszanie ekwipunku,
- aktywności poboczne,
- ekonomię opartą na wielu zasobach,
- rywalizację PvP,
- współpracę gildyjną,
- grupowe wydarzenia PvE.

Najważniejszym założeniem projektowym jest umożliwienie graczowi rozwijania postaci bez konieczności ciągłego przebywania w grze. Jednocześnie system nie powinien sprowadzać się do prostego oczekiwania na nagrody. Gracz powinien regularnie podejmować decyzje dotyczące:

- wyboru aktywności,
- zarządzania energią,
- wyboru biomu,
- konfiguracji loadoutu,
- inwestowania zasobów,
- rozwijania profesji,
- uczestniczenia w wydarzeniach społecznościowych.

## 3.1.1. Główne filary projektu

1. **Asynchroniczna progresja**  
   Postać zdobywa zasoby podczas nieobecności gracza.

2. **Progresja pozioma**  
   Rozwój nie może opierać się wyłącznie na poziomie doświadczenia. Każda aktywność powinna wpływać na inne systemy.

3. **Znaczenie decyzji gracza**  
   Wybór aktywności powinien mieć konsekwencje ekonomiczne i progresyjne.

4. **Brak Pay-to-Win**  
   Wszystkie istotne przewagi są zdobywane poprzez rozgrywkę.

5. **Brak kary za utratę postępu**  
   Porażka nie usuwa przedmiotów ani doświadczenia.

6. **Czytelna architektura systemu**  
   Mechaniki powinny być implementowane w sposób umożliwiający testowanie i dalszy rozwój.

---

# 3.2. Grupa docelowa i model biznesowy

## 3.2.1. Grupa docelowa

Gra jest kierowana przede wszystkim do graczy w wieku 16–40 lat, w szczególności:

- osób pracujących lub uczących się,
- graczy preferujących krótkie sesje,
- użytkowników urządzeń mobilnych,
- fanów gier RPG, Terraria, Shakes & Fidget i Hero Zero,
- graczy zainteresowanych optymalizacją postaci.

## 3.2.2. Model biznesowy

Projekt ma charakter edukacyjny i non-profit.

Założenia:

- brak mikropłatności,
- brak płatnej energii,
- brak płatnych skrzynek,
- brak płatnych przewag statystycznych,
- rzadkie waluty są zdobywane wyłącznie poprzez grę,
- wszystkie konta posiadają dostęp do tych samych mechanik.

---

# 3.3. Główna pętla rozgrywki

Podstawowy cykl gry przedstawia się następująco:

1. Gracz wybiera aktywność.
2. Serwer zapisuje stan aktywności.
3. Aktywność trwa w czasie rzeczywistym lub asynchronicznie.
4. Po powrocie gracza serwer oblicza nagrody.
5. Gracz otrzymuje:
   - doświadczenie,
   - złoto,
   - przedmioty,
   - surowce,
   - doświadczenie profesji,
   - waluty specjalne.
6. Zasoby są wykorzystywane do:
   - poprawy statystyk,
   - ulepszania ekwipunku,
   - rozwijania profesji,
   - rozbudowy siedziby,
   - utrzymania NPC,
   - udziału w PvP i wydarzeniach gildyjnych.
7. Gracz wybiera kolejną aktywność lub zmienia konfigurację bohatera.

## 3.3.1. Aktywne i pasywne elementy gry

### Aktywne

- przyjmowanie zadań,
- konfiguracja loadoutu,
- wydawanie walut,
- wybór biomu,
- rejestracja do wojny lub inwazji,
- zakup i ulepszanie przedmiotów,
- zarządzanie osadą.

### Pasywne

- walka Idle,
- ekspedycje NPC,
- generowanie zasobów,
- regeneracja energii,
- rotacja sklepu,
- rozwój profesji podczas aktywności.

---

# 3.4. Progresja horyzontalna i współzależność aktywności

## 3.4.1. Definicja progresji horyzontalnej

Progresja horyzontalna oznacza, że rozwój gracza nie wynika wyłącznie z podnoszenia poziomu postaci.

Gracz rozwija się równolegle w kilku osiach:

- poziom bojowy,
- ekwipunek,
- profesje,
- statystyki,
- drzewka umiejętności,
- siedziba,
- chowańce i wierzchowce,
- reputacja lub ELO,
- rozwój gildii.

Żadna z tych osi nie powinna całkowicie zastępować pozostałych.

## 3.4.2. Współzależność aktywności

Każda aktywność dostarcza określonych zasobów, które są potrzebne w innych systemach.

| Aktywność | Główne nagrody | Systemy korzystające z nagród |
|---|---|---|
| Polowanie | EXP, złoto, ekwipunek | Poziom, statystyki, loadouty, reforge |
| Górnictwo | Rudy, metale, klejnoty | Ulepszanie ekwipunku, siedziba, Goblin |
| Wędkarstwo | Ryby, przynęty, skrzynki | Mikstury, utrzymanie NPC, pył |
| Wyrąb | Drewno, zioła | Siedziba, NPC, mikstury |
| Zbieractwo | Owady, materiały biologiczne | Wędkarstwo, Zoolog, morale NPC |
| PvP | ELO, nagrody rankingowe | Rankingi, status gracza, rozwój osady |
| Wojny gildii | Emblematy chwały, złoto | Ulepszenia gildii i osady |
| Inwazje | Skrzynie eventowe, materiały | Endgame, osada, przedmioty specjalne |

## 3.4.3. Mechanizm wąskiego gardła

System wąskiego gardła ma zapobiegać strategii polegającej na rozwijaniu wyłącznie jednej aktywności.

Przykład:

1. Gracz skupia się wyłącznie na walce.
2. Zdobywa wysoki poziom, złoto i doświadczenie.
3. Odblokowuje dostęp do przedmiotów wyższego tieru.
4. Próbuje ulepszyć wyposażenie.
5. Okazuje się, że:
   - nie posiada wystarczającego poziomu Górnictwa,
   - jego kilof ma zbyt niski tier,
   - nie posiada odpowiednich rud,
   - nie ma klejnotów wymaganych do zwiększenia szansy ulepszenia.
6. Postęp bojowy zostaje czasowo ograniczony.
7. Gracz musi rozwinąć Górnictwo, aby kontynuować rozwój ekwipunku.

Analogiczne ograniczenia występują w innych kierunkach:

- brak Wyrębu ogranicza rozwój Siedziby,
- brak Zbieractwa ogranicza utrzymanie Wędkarza i Zoologa,
- brak Wędkarstwa ogranicza produkcję mikstur,
- brak walki ogranicza dostęp do złota potrzebnego do rozwoju profesji,
- brak aktywności gildyjnej ogranicza rozwój budynków osady.

## 3.4.4. Zasady projektowania wąskich gardeł

Wąskie gardło powinno:

- być przewidywalne,
- wynikać z decyzji gracza,
- nie blokować całkowicie rozgrywki,
- umożliwiać alternatywne ścieżki rozwiązania,
- nie wymagać jednej konkretnej aktywności przez długi czas,
- nagradzać wcześniejsze przygotowanie.

Wąskie gardło nie może działać jako kara. Jego celem jest skierowanie gracza do zaniedbanego systemu i pokazanie pełnej wartości progresji horyzontalnej.

---

# 3.5. Świat i progresja biomów

## 3.5.1. Założenia świata

Akcja gry rozgrywa się w świecie inspirowanym Terrarią. Lokacje są przedstawione humorystycznie i satyrycznie.

Narracja może wykorzystywać:

- absurdalne zadania,
- ironiczne opisy przedmiotów,
- prześmiewcze dialogi NPC,
- kontrast pomiędzy epickimi nazwami a codziennymi problemami bohatera.

## 3.5.2. Przedziały progresji

| Poziomy | Obszar | Główne wydarzenie |
|---|---|---|
| 1–50 | Las i Podziemia | Początek rozgrywki |
| 51–100 | Zepsucie i Dżungla | Rozwój profesji i trudniejsze prefiksy |
| 101–150 | Loch i Piekło | Wall of Flesh |
| 151–200 | Karmazyn i Uświęcenie | Odblokowanie wyższych tierów |
| 201–250 | Inwazje i Mechaniczni Bossowie | Rozwój grupowy |
| 251–300 | Głębia Dżungli i Wieże Celestjalne | Moon Lord |
| 300+ | Nieskończona Otchłań | Skalowanie endgame |

## 3.5.3. Odblokowanie zawartości

Odblokowanie powinno zależeć przede wszystkim od:

- wykonania zadań,
- pokonania bossów,
- postępu biomu,
- poziomu profesji,
- rozwoju siedziby,
- aktywności gildyjnej.

Sam poziom gracza nie powinien być jedynym warunkiem dostępu.

## 3.5.4. Hardmode

Pokonanie Wall of Flesh jest globalnym kamieniem milowym postaci lub świata.

Odblokowuje:

- nowe tiery przedmiotów,
- nowe materiały,
- nowe profesje i receptury,
- trudniejsze warianty biomów,
- rozszerzoną pulę przeciwników.

Pokonanie Moon Lorda kończy główną kampanię i odblokowuje Nieskończoną Otchłań.

---

# 3.6. Bohater i statystyki

## 3.6.1. Brak sztywnej klasy

Gracz nie wybiera klasy przy tworzeniu konta. Styl gry wynika z:

- wyposażonej broni,
- pancerza,
- chowańca,
- wierzchowca,
- loadoutu,
- statystyk,
- punktów drzewa umiejętności,
- prefiksów przedmiotów.

Dzięki temu gracz może zmienić rolę bez tworzenia nowej postaci.

## 3.6.2. Podstawowe statystyki

Aktualny model danych obejmuje:

- Siłę,
- Zręczność,
- Szczęście,
- Witalność.

Statystyki są przechowywane w tabeli `player_stats`.

### Siła

Wpływa przede wszystkim na obrażenia fizyczne i skuteczność broni.

### Zręczność

Wpływa na:

- uniki,
- szybkość,
- celność,
- wybrane aktywności zręcznościowe.

### Szczęście

Wpływa na:

- ciosy krytyczne,
- szanse na rzadkie potwory,
- jakość prefiksów,
- wydarzenia losowe.

### Witalność

Wpływa na:

- maksymalne zdrowie,
- przeżywalność,
- skuteczność loadoutu Boss,
- odporność na długie starcia.

## 3.6.3. Punkty z poziomów i zakup statystyk

Gracz otrzymuje punkty za awans, ale może również kupować punkty za złoto.

Koszt kolejnego punktu:

\[
Koszt_{Złoto} = Koszt_{Baza} \times Mnożnik^N
\]

gdzie:

- `Koszt_Baza` – cena pierwszego punktu,
- `Mnożnik` – współczynnik wzrostu ceny,
- `N` – liczba punktów zakupionych wcześniej za złoto.

Licznik `N` jest reprezentowany przez pole `StatsBoughtN` w encji `Player`.

Punkty zdobyte za poziom nie powinny zwiększać `StatsBoughtN`. Zapobiega to sytuacji, w której darmowe punkty wpływają na ceny płatnych punktów.

## 3.6.4. Miękkie skalowanie bossów

Bossowie nie powinni wymagać wyłącznie określonego poziomu.

Dostęp zależy od:

- postępu biomu,
- wykonanych zadań,
- poprzednich zwycięstw,
- spełnienia warunków fabularnych.

Odpowiednio przygotowany gracz może pokonać bossa wcześniej, ale wymaga to dobrej konfiguracji i wykorzystania progresji horyzontalnej.

---

# 3.7. Ekwipunek, prefiksy i ulepszanie

## 3.7.1. Szablony przedmiotów i instancje

System wykorzystuje dwa poziomy danych:

1. **Szablon przedmiotu**  
   Określa wspólne dane, takie jak nazwa, tier, wartość bazowa i szybkość ataku.

2. **Instancja przedmiotu w ekwipunku**  
   Określa właściciela, prefiks, poziom ulepszenia i ilość.

W kodzie odpowiadają temu:

- `ItemTemplate`,
- `Inventory`,
- `ItemCategory`.

## 3.7.2. Tiery

Planowane jest 12 tierów przedmiotów.

Tier określa:

- podstawową siłę przedmiotu,
- wymagania dotyczące materiałów,
- biom, w którym przedmiot może się pojawić,
- maksymalny poziom ulepszenia,
- koszt przekuwania.

## 3.7.3. Prefiksy

Przedmiot otrzymuje prefiks podczas zdobycia.

Przykładowe prefiksy:

- `Broken` – ujemny modyfikator,
- `Weak` – lekko ujemny modyfikator,
- `Normal` – brak modyfikatora,
- `Strong` – dodatni modyfikator,
- `Godly` – bardzo silny modyfikator.

Prefiks jest przechowywany jako tekst w `Inventory.Prefix`.

Docelowo system powinien posiadać:

- tabelę lub konfigurację prefiksów,
- modyfikatory statystyk,
- wagi losowania,
- reguły dostępności prefiksów według tieru,
- obsługę przekuwania.

## 3.7.4. Ulepszanie

Przedmiot może zostać ulepszony od `+1` do `+10`.

Ulepszanie:

- zwiększa bazowe statystyki,
- wymaga złota,
- wymaga odpowiednich metali,
- ma malejącą szansę powodzenia,
- nie niszczy przedmiotu przy porażce,
- zużywa materiały niezależnie od wyniku.

Poziom ulepszenia jest obecnie reprezentowany przez `Inventory.UpgradeLevel`.

## 3.7.5. Loadouty

Gracz może posiadać oddzielne konfiguracje:

- `PVE`,
- `PVP`,
- `BOSS`.

Loadout zapamiętuje:

- broń,
- pancerz,
- chowańca,
- przypisane węzły drzewa,
- docelowo również mikstury i wierzchowca.

Aktualna encja `Loadout` posiada referencje do:

- `Player`,
- `Inventory` jako Weapon,
- `Inventory` jako Armor,
- `Inventory` jako Pet.

Podczas Idle powinien być używany loadout PVE. Podczas walki PvP powinien być używany loadout PVP.

---

# 3.8. System aktywności

Gra posiada pięć głównych aktywności.

## 3.8.1. Polowanie

Cel:

- zdobywanie EXP,
- zdobywanie złota,
- zdobywanie ekwipunku,
- rozwój poziomu bohatera.

Wizualizacja:

- postać walcząca z przeciwnikami,
- floating combat text,
- efekty krytycznych trafień,
- animowane nagrody.

## 3.8.2. Górnictwo

Cel:

- zdobywanie rud,
- zdobywanie metali,
- zdobywanie klejnotów,
- rozwój profesji Mining.

Tier kilofa wpływa na:

- dostęp do twardszych złóż,
- efektywność wydobycia,
- prawdopodobieństwo rzadkich materiałów.

Górnictwo jest głównym wąskim gardłem systemu ulepszania ekwipunku.

## 3.8.3. Wędkarstwo

Cel:

- zdobywanie ryb,
- zdobywanie skrzynek,
- pozyskiwanie składników do mikstur,
- zdobywanie Gwiezdnego Pyłu.

Wędkarstwo zależy od:

- mocy wędkowania,
- rodzaju przynęty,
- biomu,
- poziomu profesji.

## 3.8.4. Wyrąb

Cel:

- zdobywanie drewna,
- zdobywanie ziół,
- rozwój Siedziby,
- utrzymanie niektórych NPC.

Drewno jest jednym z podstawowych zasobów infrastrukturalnych.

## 3.8.5. Zbieractwo

Cel:

- zdobywanie owadów,
- zdobywanie materiałów biologicznych,
- rozwój Zoologa,
- dostarczanie przynęty Wędkarzowi,
- utrzymanie morale NPC.

---

# 3.9. Siedziba gracza i NPC

Siedziba jest lokalnym centrum zarządzania bohaterem.

Rozbudowa siedziby:

- odblokowuje NPC,
- zwiększa liczbę dostępnych funkcji,
- tworzy nowe koszty utrzymania,
- zwiększa liczbę możliwych aktywności.

## 3.9.1. NPC

### Przewodnik

- zleca zadania,
- odblokowuje kolejne elementy świata,
- zużywa energię gracza.

### Pielęgniarka

- zwiększa regenerację energii,
- może wymagać ziół i materiałów medycznych.

### Goblin Majsterkowicz

- umożliwia Reforge,
- umożliwia Upgrade,
- wymaga metali, złota i klejnotów.

### Wędkarz

- wykonuje ekspedycje wędkarskie,
- zdobywa ryby i skrzynki,
- wymaga przynęty.

### Zoolog

- zarządza chowańcami i wierzchowcami,
- rozwija ich statystyki,
- wymaga owadów.

### Mechanik i Cyborg

- modyfikują parametry trybu Idle,
- wydłużają czas przechowywania łupów,
- umożliwiają automatyczną sprzedaż.

### Wiedźmiarz

- tworzy mikstury i flaszki,
- wymaga ziół,
- pozwala przypisywać mikstury do loadoutów.

### Demolicjonista

- oferuje natychmiastowe eksplozje w kopalni,
- gwarantuje większy jednorazowy zrzut rud,
- pełni funkcję kosztownego skrótu.

## 3.9.2. Morale NPC

Każdy NPC posiada parametr morale.

Morale:

- spada przy korzystaniu z usług,
- spada podczas ekspedycji,
- odnawia się po dostarczeniu materiałów,
- może ograniczać skuteczność usług.

W aktualnym modelu reprezentuje je:

- `HeadquarterNpc.MoralePercentage`,
- `HeadquarterNpc.IsUnlocked`.

---

# 3.10. Sklep i gospodarka

## 3.10.1. Wędrowny Handlarz

Asortyment jest rotacyjny.

Każdy wpis sklepu posiada:

- przedmiot,
- cenę w złocie,
- modyfikator ceny,
- czas wygaśnięcia.

W kodzie odpowiada temu encja `WanderingShopStock`.

Cena końcowa:

\[
Cena = Cena_{Bazowa} \times (1 + Modyfikator_{Ceny})
\]

Modyfikator powinien mieścić się w przedziale od `-15%` do `+15%`.

## 3.10.2. Główne waluty

### Złoto

Wykorzystywane do:

- zakupu statystyk,
- ulepszania,
- przekuwania,
- zakupów u Handlarza,
- opłat gildyjnych.

### Gwiezdny Pył

Wykorzystywany do:

- specjalnych resetów sklepu,
- nagród za osiągnięcia,
- wybranych usług endgame.

### Emblematy Chwały

Waluta grupowa używana do:

- rozwoju budynków osady,
- ulepszania sieci Pylonów,
- ulepszania globalnych buffów.

## 3.10.3. Zasady ekonomii

Każdy system generujący zasoby powinien posiadać odpowiadający mu odpływ waluty.

Przykłady:

- złoto – statystyki, Reforge, sklep,
- rudy – Upgrade,
- drewno – Siedziba,
- owady – Wędkarz i Zoolog,
- ryby – NPC i mikstury,
- pył – reset sklepu i specjalne funkcje,
- emblematy – rozwój osady.

---

# 3.11. Walka PvE i Idle

## 3.11.1. Walka asynchroniczna

Serwer nie symuluje każdego uderzenia w czasie rzeczywistym.

Podczas odbioru nagród:

1. pobierany jest czas od ostatniego obliczenia,
2. odczytywany jest aktywny stan aktywności,
3. wyliczany jest DPS,
4. określana jest liczba pokonanych przeciwników,
5. losowane są nagrody,
6. aktualizowany jest timestamp.

Stan aktywności jest reprezentowany przez `PlayerActivityState`, który przechowuje:

- typ aktywności,
- biom,
- `StartedAt`,
- `LastBatchCalculatedAt`.

## 3.11.2. Pula przeciwników

Przykładowy rozkład:

| Typ przeciwnika | Szansa | Charakterystyka |
|---|---:|---|
| Zwykły potwór | 85% | Stabilny EXP i podstawowy loot |
| Rzadki potwór | 14% | Więcej złota i lepsza jakość łupu |
| Miniboss | 1% | Duży zastrzyk EXP i rzadki przedmiot |

## 3.11.3. Krzywa doświadczenia

\[
EXP_{req} = 100 \times Level^{2.15}
\]

Zysk z potwora:

\[
EXP_{mob} = Base_{mob} \times Mnożnik_{biomu} \times \left(1 + \frac{Level_{mob}}{10}\right)
\]

## 3.11.4. Statystyka całkowita

\[
Stat_{total} =
(Stat_{lvl} + Stat_{gold} + Stat_{gear} + Mount_{bonus})
\times (1 + Tree_{\%})
\]

## 3.11.5. Obrażenia

\[
Dmg_{hit} =
Weapon_{base}
\times \left(1 + \frac{Stat_{primary}}{100}\right)
\times (1 + Pet_{bonus})
\]

## 3.11.6. DPS

\[
DPS =
\frac{Dmg_{hit}}{Weapon_{speed}}
\times (1 + Tree_{dmg})
\]

## 3.11.7. Redukcja obrażeń

\[
DR_{\%} =
\frac{Armor_{total}}
{Armor_{total} + (50 \times Level_{enemy})}
\]

## 3.11.8. Unik

\[
Dodge_{\%} =
\frac{Stat_{dexterity}}
{Stat_{dexterity} + (20 \times Level_{enemy})}
+ Tree_{dodge}
\]

## 3.11.9. Cios krytyczny

\[
Crit_{\%} =
Base_{crit}
+
\frac{Stat_{luck}}
{Stat_{luck} + (10 \times Level_{player})}
+
Tree_{crit}
\]

Powyższe wzory są założeniami projektowymi. Przed implementacją powinny zostać zamknięte w osobnym module kalkulatora i pokryte testami jednostkowymi.

---

# 3.12. Arena PvP

Arena PvP opiera się na walkach asynchronicznych.

## 3.12.1. Przebieg walki

1. Atakujący wybiera przeciwnika.
2. Serwer pobiera zapisany loadout PVP obu graczy.
3. Serwer oblicza wynik walki.
4. Serwer generuje sekwencję zdarzeń.
5. Sekwencja jest zapisywana jako JSON.
6. Klient odtwarza walkę jako animację.

## 3.12.2. Redukcja obrażeń PvP

\[
DR_{\% PvP} =
\frac{Armor_{Def}}
{Armor_{Def} + (50 \times Level_{Atk})}
+
Tree_{PvPDef}
-
Tree_{ArmorPen}
\]

## 3.12.3. Unik PvP

\[
Dodge_{\% PvP} =
\frac{Stat_{Dex(Def)}}
{Stat_{Dex(Def)} + Stat_{Dex(Atk)}}
+
Tree_{Dodge(Def)}
-
Tree_{Accuracy(Atk)}
\]

## 3.12.4. Krytyczne trafienie PvP

\[
Crit_{\% PvP} =
Base_{crit}
+
\frac{Stat_{Luck(Atk)}}
{Stat_{Luck(Atk)} + Stat_{Luck(Def)}}
+
Tree_{Crit(Atk)}
-
Tree_{CritResist(Def)}
\]

## 3.12.5. Finalne obrażenia PvP

\[
Dmg_{PvP} =
Dmg_{hit}
\times
(1 + Tree_{PvPDamage(Atk)} - Tree_{PvPResist(Def)})
\]

---

# 3.13. Gildie, osady i wojny

## 3.13.1. Osada

Osada jest wspólną przestrzenią dla grupy graczy.

Model docelowo obejmuje:

- lidera,
- członków,
- skarbiec złota,
- Emblematy Chwały,
- ranking ELO,
- budynki i ulepszenia.

W kodzie odpowiadają temu:

- `Settlement`,
- `SettlementMember`,
- `SettlementUpgrade`.

## 3.13.2. Członkostwo

Każdy członek posiada:

- identyfikator gracza,
- identyfikator osady,
- flagę rejestracji do wojny,
- wkład złota.

Wymóg ręcznej rejestracji zapobiega automatycznemu uwzględnianiu nieaktywnych kont.

## 3.13.3. Wojny gildii

Wojna działa jako eliminacyjna drabinka 1v1.

Zasady:

- uczestnicy są wybierani spośród zgłoszonych członków,
- zawodnicy mogą być sortowani według poziomu,
- zwycięzca przechodzi do kolejnej walki,
- zachowuje uszczuplone zdrowie,
- wojna trwa do eliminacji jednej drużyny.

Nagrody:

- ELO,
- złoto,
- EXP,
- Emblematy Chwały,
- rozwój budynków osady.

---

# 3.14. Inwazje grupowe

Inwazja jest wydarzeniem PvE przeznaczonym dla osady.

## 3.14.1. Fazy wydarzenia

### Faza zagrożenia

System tworzy wydarzenie, np.:

- Inwazja Goblinów,
- Krwawy Księżyc,
- Inwazja Marsjańska,
- Piracki Najazd.

### Faza mobilizacji

Członkowie mają określony czas na:

- zgłoszenie uczestnictwa,
- wskazanie loadoutu PVE,
- przygotowanie postaci.

### Rozstrzygnięcie

Serwer sumuje DPS wszystkich zgłoszonych graczy.

Jeśli wspólny DPS pokona pulę HP wydarzenia, osada wygrywa.

### Nagrody

- skrzynie eventowe,
- rzadkie przedmioty,
- Emblematy Chwały,
- materiały do rozbudowy osady.

---

# 3.15. Interfejs użytkownika

## 3.15.1. Mobile-first

Interfejs powinien być projektowany przede wszystkim dla urządzeń mobilnych.

Główne elementy:

- Sticky Header,
- pasek złota, pyłu i energii,
- hamburger menu,
- Bottom Navigation,
- obsługa gestów Swipe,
- duże obszary klikalne,
- czytelne komunikaty o nagrodach.

## 3.15.2. Desktop

W układzie desktopowym należy wykorzystać:

- centralną scenę,
- panel statystyk,
- panel ekwipunku,
- panel aktywności,
- boczne informacje o stanie postaci.

## 3.15.3. Styl wizualny

Podstawą są:

- głębokie szarości, np. `#1e1e24`,
- pikselowa stylistyka,
- kolory biomów,
- czerwone obrażenia,
- złote ciosy krytyczne,
- animowane paski postępu,
- nieblokujące Sticky Toasty.

## 3.15.4. Zasada nieinwazyjnych nagród

Zdobycie rzadkiego przedmiotu nie powinno przerywać aktywności Idle.

Zamiast pełnego okna modalnego należy wykorzystać:

- krótki hitlag,
- delikatne drżenie ekranu,
- Sticky Toast,
- animację przedmiotu przesuwającego się do paska zasobów.

---

# 3.16. Balans i ochrona przed dominacją jednej strategii

## 3.16.1. Cele balansu

System powinien:

- nagradzać aktywność,
- szanować czas gracza,
- nie wymagać ciągłego klikania,
- ograniczać skuteczność jednostronnej specjalizacji,
- pozwalać na comeback,
- unikać nieodwracalnych błędów.

## 3.16.2. Mechanizmy równoważące

1. Wąskie gardła między aktywnościami.
2. Wykładniczy koszt statystyk.
3. Ograniczona pojemność przechowywania łupów.
4. Koszty utrzymania NPC.
5. Rotujący sklep.
6. Malejąca szansa ulepszeń.
7. Wymóg rejestracji do wydarzeń grupowych.
8. Oddzielne loadouty dla PvE i PvP.
9. Brak utraty przedmiotów po porażce.
10. Rozdzielenie poziomu postaci od poziomu profesji.

---

# CZĘŚĆ II: ARCHITEKTURA TECHNICZNA I IMPLEMENTACJA

# 4.1. Rzeczywisty zakres implementacji

Aktualne repozytorium zawiera przede wszystkim backend.

Główny projekt:

```text
IdleTerraria.Api/
```

Najważniejsze katalogi:

```text
IdleTerraria.Api/
├── Controllers/
├── DTOs/
├── Data/
├── Entities/
├── Hubs/
├── Migrations/
├── Security/
├── Services/
├── Program.cs
├── IdleTerraria.Api.csproj
└── appsettings.json
```

W aktualnym repozytorium nie ma jeszcze:

- aplikacji React,
- konfiguracji Vite,
- klienta SignalR,
- komponentów interfejsu,
- testów automatycznych,
- implementacji pełnej pętli gry.

Dokument projektowy opisuje docelowy system, natomiast kod backendu znajduje się na wcześniejszym etapie rozwoju.

---

# 4.2. Podział aplikacji na warstwy

Aktualny backend można podzielić logicznie na następujące warstwy.

## 4.2.1. Warstwa prezentacji

Odpowiada za:

- przyjmowanie żądań HTTP,
- walidację requestów,
- autoryzację endpointów,
- zwracanie kodów HTTP,
- mapowanie wyników usług na odpowiedzi API.

Pliki:

```text
IdleTerraria.Api/Controllers/
├── AuthController.cs
└── PlayerController.cs
```

## 4.2.2. Warstwa DTO

DTO oddzielają kontrakt API od encji bazy danych.

Struktura:

```text
IdleTerraria.Api/DTOs/
├── Requests/
│   ├── LoginRequest.cs
│   └── RegisterRequest.cs
└── Responses/
    ├── AuthResponse.cs
    └── PlayerProfileResponse.cs
```

DTO zapobiegają bezpośredniemu ujawnianiu:

- hashy haseł,
- wewnętrznych relacji EF Core,
- danych technicznych encji,
- niepotrzebnych pól bazodanowych.

## 4.2.3. Warstwa usług aplikacyjnych

Warstwa usług zawiera przypadki użycia aplikacji.

Obecne usługi:

- `AuthService`,
- `JwtTokenService`,
- `PlayerService`.

Docelowe usługi:

- `IdleBatchingService`,
- `CombatMathService`,
- `WanderingShopService`,
- `InventoryService`,
- `LoadoutService`,
- `SkillTreeService`,
- `ProfessionService`,
- `SettlementService`,
- `PvpService`.

## 4.2.4. Warstwa domenowa

Obejmuje:

- reguły walki,
- reguły dropu,
- wzory statystyk,
- reguły progresji,
- reguły profesji,
- reguły gospodarki.

Obecnie część tej warstwy istnieje jedynie jako plan lub puste klasy usług.

## 4.2.5. Warstwa dostępu do danych

Za dostęp do bazy odpowiada:

```text
IdleTerraria.Api/Data/ApplicationDbContext.cs
```

Kontekst zawiera `DbSet` dla encji domenowych i konfiguruje relacje przy pomocy Fluent API.

## 4.2.6. Warstwa infrastruktury

Obejmuje:

- PostgreSQL,
- Npgsql,
- EF Core,
- migracje,
- konfigurację JWT,
- konfigurację środowiska.

---

# 4.3. Przepływ żądania HTTP

Przykładowy przepływ pobrania profilu:

1. Klient wysyła `GET /api/Player/me`.
2. Middleware JWT waliduje token.
3. `PlayerController` odczytuje claim `player_id`.
4. Kontroler wywołuje `IPlayerService`.
5. `PlayerService` wykonuje zapytanie przez `ApplicationDbContext`.
6. Dane `Player` i `PlayerStats` są mapowane na `PlayerProfileResponse`.
7. API zwraca odpowiedź JSON.

Przepływ rejestracji:

1. Klient wysyła `POST /api/Auth/register`.
2. ASP.NET Core waliduje `RegisterRequest`.
3. `AuthService` normalizuje email.
4. Sprawdzane są zajętość emaila i username.
5. Rozpoczynana jest transakcja.
6. Tworzone są:
   - `Account`,
   - `Player`,
   - `PlayerStats`.
7. Hasło jest zapisywane jako hash.
8. Transakcja zostaje zatwierdzona.
9. `JwtTokenService` generuje token.
10. API zwraca `AuthResponse`.

---

# 4.4. Konfiguracja aplikacji i uruchamianie

Projekt używa:

```xml
<TargetFramework>net10.0</TargetFramework>
```

Najważniejsze pakiety:

- `Microsoft.AspNetCore.Authentication.JwtBearer`,
- `Microsoft.AspNetCore.OpenApi`,
- `Microsoft.EntityFrameworkCore.Design`,
- `Microsoft.EntityFrameworkCore.Tools`,
- `Npgsql.EntityFrameworkCore.PostgreSQL`.

## 4.4.1. Rejestracja zależności

W `Program.cs` rejestrowane są:

- `ApplicationDbContext`,
- `IPlayerService`,
- `IAuthService`,
- `IJwtTokenService`,
- `IPasswordHasher<Account>`.

## 4.4.2. Konfiguracja PostgreSQL

Konfiguracja korzysta z klucza:

```text
ConnectionStrings:DefaultConnection
```

Przykładowa konfiguracja:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=IdleTerrariaDB;Username=postgres;"
  }
}
```

Hasło do bazy powinno być przechowywane poza repozytorium, np. w User Secrets lub zmiennych środowiskowych.

## 4.4.3. Konfiguracja JWT

Wymagane są:

- `Jwt:Key`,
- `Jwt:Issuer`,
- `Jwt:Audience`,
- `Jwt:ExpirationMinutes`.

Klucz JWT:

- musi istnieć,
- musi mieć co najmniej 32 znaki,
- nie powinien być zapisany w publicznym pliku konfiguracyjnym.

---

# 4.5. Warstwa API

## 4.5.1. AuthController

Dostępne endpointy:

```text
POST /api/Auth/register
POST /api/Auth/login
```

### Rejestracja

Request zawiera:

- email,
- username,
- password,
- confirm password.

Walidacja:

- email musi mieć poprawny format,
- username ma od 3 do 20 znaków,
- username może zawierać litery, cyfry i `_`,
- hasło ma od 8 do 100 znaków,
- `ConfirmPassword` musi odpowiadać hasłu.

### Logowanie

Request zawiera:

- email,
- password.

Błędne dane skutkują odpowiedzią `401 Unauthorized`.

## 4.5.2. PlayerController

Dostępny endpoint:

```text
GET /api/Player/me
```

Endpoint wymaga autoryzacji JWT.

Zwracane dane obejmują:

- identyfikator gracza,
- username,
- poziom,
- doświadczenie,
- złoto,
- pył,
- energię,
- ELO,
- Siłę,
- Zręczność,
- Witalność,
- Szczęście.

---

# 4.6. Warstwa usług aplikacyjnych

## 4.6.1. AuthService

`AuthService` odpowiada za:

- rejestrację,
- normalizację emaila,
- sprawdzenie zajętości emaila,
- sprawdzenie zajętości username,
- utworzenie konta,
- utworzenie gracza,
- utworzenie statystyk,
- transakcyjny zapis danych,
- weryfikację hasła podczas logowania.

Rejestracja konta, gracza i statystyk odbywa się w jednej transakcji bazodanowej.

## 4.6.2. PlayerService

`PlayerService` pobiera profil gracza wraz ze statystykami.

Zapytanie wykorzystuje:

```csharp
.Include(p => p.Stats)
```

Następnie encja jest mapowana na DTO. W przypadku braku rekordu statystyk wartości są zastępowane zerami.

## 4.6.3. JwtTokenService

Serwis generuje token z claimami:

- `sub`,
- `account_id`,
- `player_id`,
- email,
- nazwa gracza.

Walidacja tokenu w `Program.cs` obejmuje:

- podpis,
- wystawcę,
- odbiorcę,
- czas życia,
- brak dodatkowego `ClockSkew`.

---

# 4.7. Autoryzacja i bezpieczeństwo

## 4.7.1. Oddzielenie konta od postaci

Model rozdziela:

- dane logowania konta,
- dane rozwoju postaci.

`Account` przechowuje:

- email,
- hash hasła,
- datę utworzenia.

`Player` przechowuje:

- username,
- poziom,
- zasoby,
- statystyki,
- ELO.

Dzięki temu dane uwierzytelniające nie są mieszane z danymi rozgrywki.

## 4.7.2. Hashowanie haseł

Do hashowania wykorzystywany jest:

```csharp
IPasswordHasher<Account>
```

Implementacja korzysta z `PasswordHasher<Account>`.

Hasła nie są zapisywane w postaci jawnej.

## 4.7.3. Integralność danych

Integralność zapewniają:

- unikalny indeks emaila,
- unikalny indeks username,
- unikalny indeks `Player.AccountId`,
- klucze obce,
- transakcja rejestracji,
- ograniczenia długości pól.

## 4.7.4. Dalsze zalecenia

Do wdrożenia pozostają:

- rate limiting logowania,
- blokada wielokrotnych nieudanych prób,
- odświeżane tokeny,
- unieważnianie sesji,
- globalny model błędów,
- audyt zmian ekonomicznych,
- zabezpieczenie przed replay attack dla operacji nagród.

---

# 4.8. Model domenowy i encje

## 4.8.1. Account

Tabela:

```text
accounts
```

Pola:

- `id`,
- `email`,
- `password_hash`,
- `created_at`.

Relacja:

- jeden `Account` posiada jednego `Player`.

## 4.8.2. Player

Tabela:

```text
players
```

Pola:

- `id`,
- `account_id`,
- `username`,
- `level`,
- `experience`,
- `gold`,
- `stardust`,
- `energy`,
- `arena_elo`,
- `current_biome_id`,
- `stats_bought_n`.

Relacje:

- 1:1 z `Account`,
- 1:1 z `PlayerStats`,
- 1:N z `Inventory`,
- 1:N z `Loadout`,
- 1:N z `PlayerProfession`,
- 1:N z `PvpLog`,
- 1:N z `HeadquarterNpc`,
- 1:1 z `PlayerActivityState`.

## 4.8.3. PlayerStats

Tabela:

```text
player_stats
```

Klucz główny:

```text
player_id
```

Pola:

- `stat_strength`,
- `stat_dexterity`,
- `stat_luck`,
- `stat_vitality`.

Jest to relacja 1:1 z `Player`.

## 4.8.4. PlayerActivityState

Tabela:

```text
player_activity_states
```

Przechowuje stan aktualnie wykonywanej aktywności:

- typ aktywności,
- biom,
- czas rozpoczęcia,
- czas ostatniego batchowania.

Jest podstawą przyszłego mechanizmu Idle.

## 4.8.5. PlayerProfession

Tabela:

```text
player_professions
```

Klucz złożony:

```text
(player_id, profession_type)
```

Pola:

- typ profesji,
- doświadczenie,
- poziom profesji.

## 4.8.6. ItemCategory

Tabela:

```text
item_categories
```

Pola:

- nazwa,
- informacja, czy przedmiot można wyposażyć.

## 4.8.7. ItemTemplate

Tabela:

```text
item_templates
```

Pola:

- kategoria,
- nazwa,
- tier,
- wartość bazowa,
- szybkość ataku.

## 4.8.8. Inventory

Tabela:

```text
inventory
```

Pola:

- właściciel,
- szablon,
- prefiks,
- poziom ulepszenia,
- ilość.

Relacje do ekwipunku są wykorzystywane przez `Loadout`.

## 4.8.9. Loadout

Tabela:

```text
loadouts
```

Pola:

- gracz,
- typ roli,
- broń,
- pancerz,
- chowaniec.

Typ roli przechowywany jest jako tekst, np.:

```text
PVE
PVP
BOSS
```

## 4.8.10. SkillTree

Tabela:

```text
skill_trees
```

Pola:

- typ drzewa,
- nazwa węzła,
- typ modyfikatora,
- wartość modyfikatora.

## 4.8.11. PlayerUnlockedNode

Tabela:

```text
player_unlocked_nodes
```

Klucz złożony:

```text
(player_id, node_id, loadout_id)
```

Model pozwala przypisać odblokowany węzeł do konkretnego loadoutu.

## 4.8.12. Settlement

Tabela:

```text
settlements
```

Pola:

- nazwa,
- lider,
- Emblematy Chwały,
- złoto w skarbcu,
- ELO osady.

## 4.8.13. SettlementMember

Tabela:

```text
settlement_members
```

Pola:

- gracz,
- osada,
- rejestracja do wojny,
- wkład złota.

## 4.8.14. SettlementUpgrade

Tabela:

```text
settlement_upgrades
```

Klucz złożony:

```text
(settlement_id, upgrade_type)
```

Pola:

- typ ulepszenia,
- poziom.

## 4.8.15. HeadquarterNpc

Tabela:

```text
headquarter_npcs
```

Pola:

- gracz,
- nazwa NPC,
- morale,
- odblokowanie.

## 4.8.16. WanderingShopStock

Tabela:

```text
wandering_shop_stock
```

Pola:

- szablon przedmiotu,
- cena,
- modyfikator ceny,
- czas wygaśnięcia.

## 4.8.17. PvpLog

Tabela:

```text
pvp_logs
```

Pola:

- atakujący,
- obrońca,
- `battle_data` typu JSONB.

`battle_data` ma przechowywać przebieg walki w formacie możliwym do odtworzenia przez klienta.

---

# 4.9. Baza danych i Entity Framework Core

## 4.9.1. ApplicationDbContext

`ApplicationDbContext` zawiera następujące zbiory:

```csharp
DbSet<Account> Accounts
DbSet<Player> Players
DbSet<PlayerStats> PlayerStats
DbSet<PlayerActivityState> PlayerActivityStates
DbSet<PlayerProfession> PlayerProfessions
DbSet<ItemCategory> ItemCategories
DbSet<ItemTemplate> ItemTemplates
DbSet<Inventory> Inventories
DbSet<Loadout> Loadouts
DbSet<SkillTree> SkillTrees
DbSet<PlayerUnlockedNode> PlayerUnlockedNodes
DbSet<Settlement> Settlements
DbSet<SettlementMember> SettlementMembers
DbSet<SettlementUpgrade> SettlementUpgrades
DbSet<HeadquarterNpc> HeadquarterNpcs
DbSet<WanderingShopStock> WanderingShopStocks
DbSet<PvpLog> PvpLogs
```

## 4.9.2. Relacja Account–Player

Konfiguracja:

```csharp
modelBuilder.Entity<Account>()
    .HasOne(a => a.Player)
    .WithOne(p => p.Account)
    .HasForeignKey<Player>(p => p.AccountId)
    .OnDelete(DeleteBehavior.Cascade);
```

Oznacza to:

- `Account` posiada dokładnie jednego gracza,
- `Player.AccountId` jest kluczem obcym,
- `Player.AccountId` posiada unikalny indeks,
- usunięcie konta usuwa powiązaną postać.

W modelu bazy relację 1:1 wzmacniają:

- klucz obcy,
- unikalny indeks `IX_players_account_id`,
- wymagane pole `AccountId`.

## 4.9.3. Unikalność emaila

Konfiguracja:

```csharp
modelBuilder.Entity<Account>()
    .HasIndex(a => a.Email)
    .IsUnique();
```

Dzięki temu baza nie pozwala utworzyć dwóch kont z tym samym adresem email.

Email jest dodatkowo normalizowany w `AuthService`:

```csharp
request.Email.Trim().ToLowerInvariant()
```

## 4.9.4. Unikalność username

Konfiguracja:

```csharp
modelBuilder.Entity<Player>()
    .HasIndex(p => p.Username)
    .IsUnique();
```

Dzięki temu username jest unikalny na poziomie bazy danych, a nie tylko w logice aplikacji.

Przed utworzeniem gracza `AuthService` wykonuje również sprawdzenie logiczne z porównaniem niewrażliwym na wielkość liter.

Docelowo należy rozważyć dodatkową normalizowaną kolumnę `NormalizedUsername`, ponieważ zachowanie unikalności w PostgreSQL może zależeć od sposobu porównywania tekstu.

---

# 4.10. Relacje i integralność danych

Najważniejsze relacje:

| Relacja | Typ | Zasada |
|---|---|---|
| Account–Player | 1:1 | Każde konto posiada jedną postać |
| Player–PlayerStats | 1:1 | Każda postać posiada statystyki |
| Player–Inventory | 1:N | Gracz posiada wiele przedmiotów |
| Player–Loadout | 1:N | Gracz może posiadać wiele konfiguracji |
| Loadout–Inventory | N:1 | Loadout wskazuje broń, pancerz i chowańca |
| Player–Profession | 1:N | Gracz ma osobny poziom każdej profesji |
| Player–ActivityState | 1:1 | Jedna aktywność bazowa na gracza |
| Player–SettlementMember | 1:0..1 | Gracz może należeć do jednej osady |
| Settlement–SettlementUpgrade | 1:N | Osada posiada wiele typów ulepszeń |
| Player–PvpLog | 1:N | Gracz może występować w wielu logach |
| ItemCategory–ItemTemplate | 1:N | Kategoria posiada wiele szablonów |

---

# 4.11. Strategia usuwania i Multiple Cascade Paths

## 4.11.1. Problem

Entity Framework Core i PostgreSQL mogą zgłosić błąd `Multiple Cascade Paths`, gdy usunięcie jednej encji prowadziłoby do wielu równoległych ścieżek kaskadowego usuwania tych samych danych.

Najbardziej podatne są:

- logi PvP, które wskazują dwóch graczy,
- loadouty wskazujące wiele przedmiotów,
- przedmioty należące do gracza i jednocześnie referencjonowane przez loadout.

## 4.11.2. Relacje PvpLog–Player

`PvpLog` posiada dwa klucze obce do `Player`:

- `AttackerId`,
- `DefenderId`.

Konfiguracja:

```csharp
modelBuilder.Entity<PvpLog>()
    .HasOne(p => p.Attacker)
    .WithMany()
    .HasForeignKey(p => p.AttackerId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<PvpLog>()
    .HasOne(p => p.Defender)
    .WithMany()
    .HasForeignKey(p => p.DefenderId)
    .OnDelete(DeleteBehavior.Restrict);
```

Zastosowanie `Restrict` oznacza, że usunięcie gracza nie usuwa automatycznie jego logów PvP.

Jest to celowe, ponieważ:

- logi mogą mieć wartość historyczną,
- log może dotyczyć dwóch graczy,
- kaskada mogłaby prowadzić do wielu ścieżek usuwania,
- usuwanie konta nie powinno przypadkowo usuwać wspólnych danych historycznych.

Docelowo należy wybrać jedną z dwóch strategii:

1. zachować logi i ustawiać identyfikatory uczestników na `NULL`,
2. wykonywać kontrolowane usuwanie logów w osobnej transakcji.

## 4.11.3. Relacje Loadout–Inventory

Loadout wskazuje:

- broń,
- pancerz,
- chowańca.

Wszystkie trzy relacje mają:

```csharp
.OnDelete(DeleteBehavior.Restrict)
```

Zapobiega to usunięciu przedmiotu, który nadal jest używany w aktywnym loadoucie.

Przed usunięciem przedmiotu aplikacja powinna:

1. sprawdzić, czy przedmiot jest używany w loadoucie,
2. zablokować usunięcie lub wymusić zmianę loadoutu,
3. dopiero potem usunąć przedmiot.

## 4.11.4. Account–Player

Relacja Account–Player jest inna niż powyższe:

```csharp
.OnDelete(DeleteBehavior.Cascade)
```

Jest to bezpieczna kaskada, ponieważ:

- konto jest właścicielem postaci,
- postać nie ma sensu bez konta,
- relacja jest jednoznaczna,
- nie prowadzi do wielokrotnych ścieżek do logów PvP.

---

# 4.12. Migracje

W repozytorium istnieją migracje:

```text
20260920132400_InitialCreate
20260921072658_AddCompleteSchema
20260921074151_CompleteSchema
20260923143440_AddAccounts
```

Najważniejsza migracja funkcjonalna dotycząca uwierzytelniania:

```text
20260923143440_AddAccounts
```

Dodaje:

- tabelę `accounts`,
- kolumnę `players.account_id`,
- unikalny indeks na `players.account_id`,
- unikalny indeks na `accounts.email`,
- klucz obcy `players.account_id -> accounts.id`,
- kaskadowe usuwanie konta i postaci.

`ApplicationDbContextModelSnapshot.cs` opisuje bieżący model EF Core i powinien być traktowany jako techniczne odwzorowanie aktualnej konfiguracji kontekstu.

Po każdej zmianie encji należy:

1. zmodyfikować model,
2. wygenerować migrację,
3. przejrzeć wygenerowany SQL,
4. zweryfikować zachowanie kluczy obcych,
5. zastosować migrację na środowisku testowym,
6. dopiero później wdrożyć ją produkcyjnie.

---

# 4.13. Mechanizmy jeszcze niezaimplementowane

## 4.13.1. IdleBatchingService

Plik istnieje, ale nie zawiera implementacji.

Docelowo powinien:

- pobierać `PlayerActivityState`,
- obliczać czas nieobecności,
- ograniczać go maksymalnym czasem magazynowania,
- korzystać z loadoutu PVE,
- wyliczać DPS,
- losować przeciwników,
- przyznawać nagrody,
- zapisywać timestamp,
- działać transakcyjnie.

## 4.13.2. CombatMathService

Plik istnieje, ale nie zawiera implementacji.

Docelowo powinien być czystym serwisem obliczeniowym, bez bezpośredniego dostępu do bazy.

Powinien otrzymywać:

- statystyki gracza,
- dane ekwipunku,
- bonusy loadoutu,
- dane przeciwnika,
- parametry biomu.

Powinien zwracać:

- obrażenia,
- DPS,
- redukcję obrażeń,
- szansę uniku,
- szansę krytyka,
- wynik pojedynku,
- log zdarzeń.

## 4.13.3. WanderingShopService

Plik istnieje, ale nie zawiera implementacji.

Docelowo powinien:

- generować ofertę,
- dobierać przedmioty według tieru,
- obliczać modyfikator ceny,
- ustawiać `ExpiresAt`,
- realizować reset oferty,
- zabezpieczać zakup przed podwójnym wydaniem waluty.

## 4.13.4. SignalR

Pliki:

```text
Hubs/ChatHub.cs
Hubs/GameEventHub.cs
```

obecnie nie implementują funkcji.

Dodatkowo `Program.cs` nie zawiera:

```csharp
builder.Services.AddSignalR();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<GameEventHub>("/hubs/events");
```

Podłączenie SignalR należy wykonać dopiero po zaprojektowaniu:

- autoryzacji połączeń,
- nazw grup,
- limitów wiadomości,
- obsługi rozłączeń,
- filtrowania czatu,
- publikacji wydarzeń gry.

---

# 4.14. Docelowy przepływ obliczeń Idle

## 4.14.1. Rozpoczęcie aktywności

1. Klient wysyła żądanie rozpoczęcia aktywności.
2. Serwer sprawdza, czy gracz może ją rozpocząć.
3. Serwer zapisuje:
   - typ aktywności,
   - biom,
   - czas rozpoczęcia,
   - czas ostatniego batchowania.
4. Serwer blokuje konkurencyjną aktywność, jeśli system przewiduje tylko jeden slot.

## 4.14.2. Odbiór nagród

1. Klient wysyła żądanie odbioru nagród.
2. Serwer rozpoczyna transakcję.
3. Odczytuje gracza, loadout i stan aktywności.
4. Oblicza czas trwania.
5. Nakłada limit magazynowania nagród.
6. Oblicza parametry postaci.
7. Oblicza liczbę cykli walki lub wydobycia.
8. Losuje wydarzenia i nagrody.
9. Aktualizuje zasoby.
10. Zapisuje nowe timestampy.
11. Zatwierdza transakcję.
12. Zwraca wynik wraz z listą nagród.

## 4.14.3. Ochrona przed podwójną wypłatą

Operacja odbioru nagród musi być odporna na:

- wielokrotne kliknięcie,
- równoległe żądania,
- odświeżenie strony,
- utratę połączenia po stronie klienta.

Wymagane mechanizmy:

- transakcja,
- blokada rekordu lub kontrola wersji,
- atomowa aktualizacja `LastBatchCalculatedAt`,
- idempotencja operacji odbioru.

---

# 4.15. Testowanie i jakość

## 4.15.1. Testy jednostkowe

Należy utworzyć osobny projekt testowy dla:

- wzorów obrażeń,
- wzorów EXP,
- kosztów statystyk,
- szans prefiksów,
- szans ulepszeń,
- generowania cen sklepu,
- kalkulatora PvP.

## 4.15.2. Testy integracyjne

Należy przetestować:

- rejestrację,
- logowanie,
- unikalność emaila,
- unikalność username,
- kaskadowe usunięcie Account–Player,
- restrykcje PvpLog,
- restrykcje Loadout–Inventory,
- migracje PostgreSQL,
- transakcję nagród Idle.

## 4.15.3. Testy kontraktowe API

Należy ustalić stabilny kontrakt dla:

- `AuthResponse`,
- `PlayerProfileResponse`,
- wyników aktywności,
- logów PvP,
- błędów walidacyjnych.

## 4.15.4. Testy obciążeniowe

Szczególnie ważne są:

- równoległe odbieranie nagród Idle,
- masowe kalkulacje po długiej nieobecności,
- generowanie logów PvP,
- operacje sklepu,
- wydarzenia gildyjne.

---

# 5. Roadmapa implementacyjna

## Etap 1 – Fundament techniczny

Status: **częściowo zrealizowany**

- [x] Projekt ASP.NET Core Web API.
- [x] Konfiguracja .NET 10.
- [x] Połączenie z PostgreSQL.
- [x] Entity Framework Core.
- [x] Migracje.
- [x] Konta i gracze.
- [x] JWT.
- [x] Endpoint profilu.
- [ ] Globalny middleware błędów.
- [ ] Testy automatyczne.
- [ ] Dokumentacja OpenAPI dla wszystkich endpointów.

## Etap 2 – Podstawowa pętla gry

Status: **do zrealizowania**

- [x] Rozpoczynanie aktywności.
- [x] Zatrzymywanie aktywności.
- [x] Batching Idle.
- [ ] Przyznawanie złota i EXP.
- [x] Krzywa poziomów.
- [x] Limit przechowywania nagród.
- [x] Ochrona przed podwójną wypłatą.
- [ ] Pierwszy biom.
- [ ] Podstawowa pula przeciwników.

## Etap 3 – Ekwipunek i loadouty

Status: **model częściowo gotowy**

- [x] Encje przedmiotów.
- [x] Encja ekwipunku.
- [x] Encja loadoutu.
- [x] Relacje Loadout–Inventory.
- [ ] API ekwipunku.
- [ ] Zakładanie przedmiotów.
- [ ] Walidacja właściciela przedmiotu.
- [ ] Prefiksy.
- [ ] Reforge.
- [ ] Upgrade.
- [ ] Automatyczny wybór loadoutu Idle.

## Etap 4 – Profesje i progresja horyzontalna

Status: **model częściowo gotowy**

- [x] Encja `PlayerProfession`.
- [x] Encja `PlayerActivityState`.
- [ ] Mining.
- [ ] Fishing.
- [ ] Lumbering.
- [ ] Bug Catching.
- [ ] Poziomy profesji.
- [ ] Wąskie gardła.
- [ ] Wymagania tierów narzędzi.
- [ ] Materiały profesji.

## Etap 5 – Siedziba i NPC

Status: **model częściowo gotowy**

- [x] Encja `HeadquarterNpc`.
- [x] Morale NPC.
- [x] Flaga odblokowania.
- [ ] Rozbudowa siedziby.
- [ ] Odblokowywanie NPC.
- [ ] Zużywanie morale.
- [ ] Przedmioty konsumpcyjne.
- [ ] Zadania Przewodnika.
- [ ] Usługi Goblina.
- [ ] Ekspedycje Wędkarza.

## Etap 6 – Sklep i gospodarka

Status: **model częściowo gotowy**

- [x] Encja `WanderingShopStock`.
- [x] Cena.
- [x] Modyfikator ceny.
- [x] Data wygaśnięcia.
- [ ] Generowanie oferty.
- [ ] Rotacja 12/24 godziny.
- [ ] Zakup przedmiotu.
- [ ] Reset oferty.
- [ ] Ekonomiczne odpływy walut.
- [ ] Audyt transakcji.

## Etap 7 – PvP

Status: **do zrealizowania**

- [x] Pole ELO gracza.
- [x] Encja `PvpLog`.
- [x] JSONB dla danych walki.
- [ ] Kalkulator PvP.
- [ ] Wybór przeciwnika.
- [ ] Generowanie logu walki.
- [ ] Endpoint historii walk.
- [ ] Ranking.
- [ ] Bilety PvP.
- [ ] Ograniczenie matchmakingu.

## Etap 8 – Osady i wojny gildii

Status: **model częściowo gotowy**

- [x] Encja osady.
- [x] Członkostwo.
- [x] Ulepszenia.
- [x] Rejestracja do wojny.
- [ ] Tworzenie osady.
- [ ] Zaproszenia.
- [ ] Zarządzanie członkami.
- [ ] Skarbiec.
- [ ] Wojny.
- [ ] Drabinka eliminacyjna.
- [ ] Nagrody grupowe.

## Etap 9 – SignalR i frontend

Status: **do zrealizowania**

- [ ] Klient React.
- [ ] Logowanie po stronie klienta.
- [ ] Zustand lub inny store.
- [ ] Mobile-first UI.
- [ ] Arena walki.
- [ ] Sticky Toasty.
- [ ] SignalR ChatHub.
- [ ] SignalR GameEventHub.
- [ ] Animacja logów PvP.
- [ ] Obsługa połączeń utraconych.

---

# 6. Otwarte decyzje projektowe

## 6.1. Światy i serwery

Należy ustalić, czy:

- `World` będzie osobną encją,
- ranking będzie per świat,
- postać należy do jednego świata,
- konto może posiadać wiele postaci na różnych światach,
- sezon tworzy nowy świat czy tylko nową tabelę rankingową.

Aktualny model `Account`–`Player` zakłada jedną postać na konto i nie zawiera jeszcze pojęcia świata.

## 6.2. Właścicielstwo NPC

Obecne `HeadquarterNpc.PlayerId` jest nullable i pozwala na wiele rekordów NPC przypisanych do gracza.

Należy dodać ograniczenie unikalności dla pary:

```text
(player_id, npc_name)
```

Zapobiegnie to utworzeniu dwóch takich samych NPC dla jednej siedziby.

## 6.3. Unikalność username bez rozróżniania wielkości liter

Obecna logika aplikacji sprawdza username przez:

```csharp
p.Username.ToLower() == username.ToLower()
```

Docelowo należy rozważyć:

- `NormalizedUsername`,
- indeks na `NormalizedUsername`,
- rozszerzenie PostgreSQL `citext`,
- indeks funkcyjny `lower(username)`.

## 6.4. Logi PvP po usunięciu gracza

Należy zdecydować, czy:

- zachowywać logi historyczne z wartościami `NULL`,
- anonimizować dane gracza,
- usuwać logi ręcznie,
- całkowicie blokować usunięcie gracza, jeśli istnieją logi.

Obecna konfiguracja `Restrict` zabezpiecza bazę, ale nie rozwiązuje procesu usunięcia na poziomie domeny.

## 6.5. Model aktywności

Należy ustalić, czy gracz:

- może wykonywać tylko jedną aktywność jednocześnie,
- może równolegle prowadzić walkę i profesję,
- ma osobne sloty dla aktywności,
- może delegować profesje NPC.

Obecny `PlayerActivityState` sugeruje jeden główny stan aktywności na gracza.

## 6.6. Źródło definicji przedmiotów

Należy ustalić, czy dane przedmiotów będą:

- zapisane wyłącznie w bazie,
- seedowane przez EF Core,
- przechowywane w plikach JSON,
- zarządzane przez panel administracyjny.

## 6.7. Walidacja loadoutów

Przed użyciem loadoutu serwer powinien sprawdzić:

- czy przedmiot należy do tego samego gracza,
- czy kategoria przedmiotu jest poprawna,
- czy broń może być użyta jako broń,
- czy pancerz może być użyty jako pancerz,
- czy chowaniec jest poprawnym typem,
- czy przedmiot nie został usunięty lub zablokowany.

---

# 7. Podsumowanie

`Idle Terraria` posiada obecnie solidny fundament backendowy:

- ASP.NET Core Web API,
- PostgreSQL,
- Entity Framework Core,
- model kont i graczy,
- autoryzację JWT,
- podstawowe DTO,
- migracje,
- model ekwipunku,
- model loadoutów,
- model profesji,
- model osad,
- model sklepu,
- model logów PvP.

Najważniejsze elementy, które należy teraz implementować, to:

1. właściwa pętla Idle,
2. kalkulator walki,
3. system nagród,
4. ekwipunek i loadouty,
5. profesje oraz wąskie gardła,
6. siedziba i NPC,
7. PvP,
8. gospodarka,
9. SignalR,
10. frontend.

Najważniejszą zasadą projektową jest zachowanie równowagi pomiędzy progresją pionową i poziomą. Sam poziom postaci nie powinien wystarczać do osiągania maksymalnej skuteczności. Gracz powinien rozwijać aktywności komplementarne, ponieważ każda z nich dostarcza zasobów niezbędnych do dalszego rozwoju pozostałych systemów.

W warstwie technicznej należy zachować rozdzielenie:

- kontrolerów,
- DTO,
- usług aplikacyjnych,
- reguł domenowych,
- dostępu do danych,
- infrastruktury.

Szczególną uwagę należy poświęcić integralności ekonomii gry, idempotencji operacji Idle oraz kontrolowaniu relacji usuwania w Entity Framework Core. Aktualna konfiguracja `Restrict` dla logów PvP i referencji loadoutów jest niezbędna, aby uniknąć błędów `Multiple Cascade Paths` oraz przypadkowego usuwania danych historycznych lub wyposażenia używanego przez postać.