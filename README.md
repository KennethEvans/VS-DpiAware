# DPI Info

DPI Info is a C# Windows application that displays the DPI awareness of running processes as well as other information that may be useful. There has been an ongoing problem with running applications on high-resolution monitors in Windows. One often gets a user interface that is too small to read comfortably or one that is pixelated. The options that Microsoft has provided to deal with these issues have improved with new major updates to Windows, but it may be helpful to have this information to decide what to do.

DPI Info lists information about each process that has a Main Window. This information includes the Main Window title, the window handle, the process id (PID), whether it is a 32-bit or 64-bit process, whether it is a topmost window, the window position and size, its DPI, its Process DPI Awareness, its DPI Awareness Context, and its DPI Awareness. It also displays information about the Monitors attached to the system and whether HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\SideBySide\PreferExternalManifest is set in the registry.

See http://kenevans.net/opensource/DpiInfo/Help/Overview.html

**Installation**

If you are installing from a download, just unzip the files into a directory somewhere convenient. Then run it from there. If you are installing from a build, copy these files and directories from the bin/Release directory to a convenient directory.

* DpiInfo.exe
* Help

To uninstall, just delete these files.


**More Information**

More information and FAQ are at https://kennethevans.github.io as well as more projects from the same author.

Licensed under the MIT license. (See: https://en.wikipedia.org/wiki/MIT_License)