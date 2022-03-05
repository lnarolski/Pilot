# Pilot
[![pobierz z Google Play](https://play.google.com/intl/en_us/badges/static/images/badges/pl_badge_web_generic.png)](https://play.google.com/store/apps/details?id=pl.lnarolski.pilot&pcampaignid=pcampaignidMKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1)

Mobilna aplikacja do zdalnego sterowania komputerem poprzez sieć komputerową dla systemów Android i Windows (UWP). Sterowany komputer musi posiadać uruchomioną [aplikację serwera](https://github.com/lnarolski/PilotServer).

|Android|Windows (UWP)|
|:-------------------------:|:-------------------------:|
|<img width="900" src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot1-android.jpg">|<img width="900"  src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot1-uwp.png">|
|<img width="900" src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot2-android.png">|<img width="900"  src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot2-uwp.png">|
|<img width="900" src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot3-android.jpg">|<img width="900"  src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot3-uwp.png">|
|<img width="900" src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot4-android.png">|<img width="900"  src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot4-uwp.png">|

# Działanie
Zadaniem aplikacji jest kontrola działania myszki i klawiatury na komputerze z zainstalowanym systemem Windows. Aplikacja umożliwia obsługę aplikacji multimedialnych uruchomionych na komputerze (np. Spotify, VLC) oraz otwieranie zapamiętanych adresów stron WWW. Do kontroli można użyć także widgetu wyświetlanego na Pasku stanu lub Ekranie blokady. Dodano również sterowanie odtwarzaniem z poziomu smartwatch'a lub smartband'a (np. Mi Band lub Amazfit) podłączonego do telefonu z uruchomioną aplikacją.
W przypadku niektórych programów (np. menedżera zadań, itd.) wymagane jest uruchomienie aplikacji serwera z uprawnieniami administracyjnymi.

# Bugi
- Wyświetlanie klawiatury ekranowej nie działa w aplikacji UWP, ponieważ Xamarin nie udostępnia API do obsługi klawiatury ekranowej

# [Zrzuty ekranu](https://github.com/lnarolski/Pilot/tree/master/Screenshots)

# TODO
- Dodanie zabezpieczenia przed powielaniem pakietów z poleceniami
- Zmiana biblioteki do wykrywania gestów dotyku
- Zmiana sposobu generowania komendy wciśnięcia prawego przycisku myszy

# [Więcej informacji](https://narolski.eu/2020/09/23/pilot-opis-projektu/)
