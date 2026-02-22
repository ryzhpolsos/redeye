# RedEye

RedEye - полностью настраиваемая и расширяемая оболочка для Windows. Никаких больше кривых наклеек поверх Explorer-а, элементов меню "Пуск" на React Native и приколоченной гвоздями панели задач!

**RedEye позволяет вам вернуть контроль над внешним видом вашей системы.**

## Реализованный функционал
1. Расширяемое ядро со встроенной системой плагинов
2. Собственная XML-разметка для вёрстки UI-элементов оболочки с поддержкой скриптов на C# и JavaScript
3. Стандартные виджеты: панель задач, меню "Пуск", фон рабочего стола, часы, индикатор заряда батареи
4. Гибкая система привязок клавиш

## В разработке
1. Менеджер WiFi-сетей
2. Индикатор/регулятор громкости
3. Поддержка иконок трея
4. Собственный менеджер окон

## Скриншоты
<details>
  <summary>Развернуть</summary>


  ![Скриншот 1](https://raw.githubusercontent.com/ryzhpolsos/redeye/refs/heads/main/screenshots/1.png)

  ![Скриншот 2](https://raw.githubusercontent.com/ryzhpolsos/redeye/refs/heads/main/screenshots/2.png)

</details>

## Сборка
Для сборки RedEye необходимы [.NET SDK](https://dotnet.microsoft.com/en-us/download) и MinGW-w64 ([w64devkit](https://github.com/skeeto/w64devkit))
```
git clone https://github.com/ryzhpolsos/redeye
cd redeye
dotnet build -c Release -p Wmx=1
```

## Установка
В корневой папке проекта выполните указанную ниже команду, а затем перезайдите в систему.
```
reg add "HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /d "%CD%\bin\Release\netframework4.8\redeye.exe" /t REG_SZ /f
```

## Лицензия
Проект разрабатывается и распространяется на условиях лицензии [MIT](https://github.com/ryzhpolsos/redeye/blob/main/LICENSE).
