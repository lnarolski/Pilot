# Pilot
Mobilna aplikacja do zdalnego sterowania komputerem poprzez sieć komputerową dla systemów Android i Windows (UWP). Sterowany komputer musi posiadać uruchomioną [aplikację serwera](https://github.com/lnarolski/PilotServer).

# Działanie
Zadaniem aplikacji jest kontrola działania myszki i klawiatury na komputerze z zainstalowanym systemem Windows. Aplikacja umożliwia obsługę aplikacji multimedialnych uruchomionych na komputerze (np. Spotify, VLC) oraz otwieranie zapamiętanych adresów stron WWW.

# Bugi
- Wyświetlanie klawiatury ekranowej nie działa w aplikacji UWP, ponieważ Xamarin nie udostępnia API do obsługi klawiatury ekranowej
