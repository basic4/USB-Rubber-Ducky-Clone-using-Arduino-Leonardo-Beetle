# USB-Rubber-Ducky-Clone-using-Arduino-Leonardo-Beetle
This project realises a basic version of the 'H4K5' USB Rubber Ducky for under $10 by utilising a $5 CJMCU Beetle board connected to a $5 Arduino compatible microSD card reader module. Only six solder connections are needed. The microSD card carries the encoded keystrokes which are to be send to the target pc.
A simple Windows application (written in C# - source included) is used to convert a command script text file into the 'script.bin' executed on duck. The scripting language is almost 100% compatible with the 'H4K5' devices at $50.
Read the 'Instuctions' pdf document for full build and construction details.

Note: If you want to 'hide' the fact that the ducky clone is an Ardunio, its possible to make changes to some of the Arduino setup files (boards.txt and USBDescriptor.h) such that the USB VID & PID codes can be altered to make the device look like a standard keyboard only from any manufacturer you wish.)

