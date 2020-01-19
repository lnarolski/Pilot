# Pilot
Mobilna aplikacja do zdalnego sterowania komputerem poprzez sieć komputerową dla systemów Android i Windows (UWP). Sterowany komputer musi posiadać uruchomioną [aplikację serwera](https://github.com/lnarolski/PilotServer).

| | |
|:-------------------------:|:-------------------------:|
|<img width="900" src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot1-android.png">|<img width="900"  src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot1-uwp.png">|
|<img width="900" src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot2-android.png">|<img width="900"  src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot2-uwp.png">|
|<img width="900" src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot3-android.png">|<img width="900"  src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot3-uwp.png">|
|<img width="900" src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot4-android.png">|<img width="900"  src="https://raw.githubusercontent.com/lnarolski/Pilot/master/Screenshots/screenshot4-uwp.png">|

# Działanie
Zadaniem aplikacji jest kontrola działania myszki i klawiatury na komputerze z zainstalowanym systemem Windows. Aplikacja umożliwia obsługę aplikacji multimedialnych uruchomionych na komputerze (np. Spotify, VLC) oraz otwieranie zapamiętanych adresów stron WWW.

# Bugi
- Wyświetlanie klawiatury ekranowej nie działa w aplikacji UWP, ponieważ Xamarin nie udostępnia API do obsługi klawiatury ekranowej

# [Zrzuty ekranu](https://github.com/lnarolski/Pilot/tree/master/Screenshots)

# TODO
- Dodanie obsługi połączenia przez protokół UDP
- Dodanie zabezpieczenia przed powielaniem pakietów z poleceniami
- Dodanie włączania WiFi w telefonie podczas uruchamiania aplikacji
- Dodanie możliwości zmiany języka na angielski
