# WizLib
WiZ Color &amp; Tunable White Light Bulb .NET API Library

This library is in active development, and it's brand new so things will undoubtedly change while I consider next steps.

The library, itself, is pretty well documented, so far (the Bulb class is, anyway.)

Internal WiZ API workings gleaned from Python and Java source (https://github.com/sbidy/pywizlight) and also (https://github.com/SRGDamia1/openhab2-addons).

There is nothing happening with the UWP project, right now, it's just an empty shell.  In the future I will come back around to it or just remove it, entirely.

The **WiZ** class library is implmented in .NET Standard and requires only the NewtonSoft.Json Nuget package.

The example program **WizBulb** is built for Windows Desktop in .NET 5.0, and requires the DataTools library. 

When you clone the project, be sure to run __git submodule init__ and __git submodule update__ from the root project directory.

Quick notes: When you select a bulb in the application, you can change the name by clicking into the text on the right-hand panel.  If you select all the bulbs in the room you can change the name of the room, and the name of the house.  

You can save these settings to a JSON file, and they will automatically load the next time the application is run.



