# Smarthome Avalonia

## Informacje ogólne

Smarthome Avalonia to aplikacja desktopowa napisana w języku C#, której celem jest symulacja oraz wizualizacja systemu inteligentnego domu (Smart Home).
Aplikacja prezentuje stan urządzeń domowych oraz umożliwia ich logiczne zarządzanie za pomocą graficznego interfejsu użytkownika.

Projekt został wykonany z wykorzystaniem architektury MVVM oraz frameworka Avalonia UI.

Autorzy:
- Domin Jakubiec
- Przemysław Fuchs

---

## Wymagania funkcjonalne

- Wyświetlanie interfejsu użytkownika aplikacji Smart Home
- Wczytywanie danych konfiguracyjnych z pliku `status.json`
- Prezentacja aktualnego stanu urządzeń domowych
- Oddzielenie warstwy widoku od logiki aplikacji
- Możliwość dalszej rozbudowy systemu

---

## Wymagania niefunkcjonalne

- Aplikacja działa jako aplikacja desktopowa
- Projekt oparty na platformie .NET
- Wykorzystanie architektury MVVM
- Czytelny i intuicyjny interfejs użytkownika
- Możliwość uruchomienia aplikacji na różnych systemach operacyjnych
- Łatwość utrzymania i rozwijania kodu

---

## Funkcjonalności / moduły systemu

- Moduł interfejsu użytkownika (Views)
- Moduł logiki aplikacji (ViewModels)
- Moduł zasobów aplikacji (Assets)
- Moduł inicjalizacji aplikacji
- Obsługa danych konfiguracyjnych zapisanych w pliku JSON

---

## Przypadki użycia

1. Użytkownik uruchamia aplikację
2. System inicjalizuje główne komponenty
3. Aplikacja wczytuje dane z pliku `status.json`
4. System wyświetla interfejs użytkownika
5. Użytkownik przegląda aktualny stan urządzeń Smart Home

---

## Architektura systemu

Aplikacja została zaprojektowana w architekturze MVVM (Model–View–ViewModel).
Warstwa View odpowiada za prezentację interfejsu użytkownika, natomiast warstwa ViewModel zawiera logikę aplikacji i pośredniczy pomiędzy danymi a widokiem.
Dane aplikacji przechowywane są w pliku konfiguracyjnym w formacie JSON.

---

## Stos technologiczny

- Język programowania: C#
- Platforma: .NET
- Framework UI: Avalonia UI
- Architektura: MVVM
- System kontroli wersji: Git / GitHub

---

## Proces uruchomienia projektu

1. Sklonować repozytorium:
   git clone https://github.com/domJak/smarthome_avalonia.git
2. Otworzyć plik `SmartHome.sln` w Visual Studio lub innym kompatybilnym IDE
3. Upewnić się, że zainstalowane jest środowisko .NET
4. Uruchomić projekt

---

## Struktura folderów
/Assets - zasoby aplikacji
/ViewModels - logika aplikacji (MVVM)
/Views - widoki interfejsu użytkownika
App.axaml - konfiguracja aplikacji
Program.cs - punkt wejścia aplikacji
status.json - dane konfiguracyjne systemu Smart Home
SmartHome.sln - plik rozwiązania

