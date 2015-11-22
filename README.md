# Point cloud video generator 

Point cloud video generator is a program wich creates point cloud of given video.<br>
UI was created on Java (SWT), core - [OpenCV] lib.

[OpenCV]:http://opencv.org/ 

## How to compile project
Steps to setup project in Intelli IDEA:
1. Download and extract OpenCV from http://opencv.org/downloads.html <br>
2. Import current project at IDE <br>
3. Go to Run/Debug Configurations and write in VM options: -Djava.library.path="C://your_path_to_opencv_dll" (e.g. -Djava.library.path="C:\opencv\build\java\x64") <br>
4. Run app <br>
5. ... <br>
6. PROFIT! <br>

## Developers
Program was developed under software engineering cource at [SSAU] in 2015-2016 by students:

* Andrey Bugakov
* Sergey Ermakov ([sergei2s31@rambler.ru])

[SSAU]:http://www.ssau.ru/english/
[sergei2s31@rambler.ru]:mailto:sergei2s31@rambler.ru?subject=Point%20cloud%20video%20generator

## FAQ
Q: I have an "Exception in thread "main" java.lang.UnsatisfiedLinkError: no opencv_java300 in java.library.path". What should I do?
A: You forget to add VM options line at Run/Debug Configurations. Go to "[How to compile project](#how-to-compile-project)" chapter to fix it.

