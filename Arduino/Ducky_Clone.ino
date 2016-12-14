#include <Keyboard.h>
#include <SD.h>
#include <Wire.h>
#include <SPI.h>
//  *********************************************
//  **  BASIC RUBBER DUCKY CLONE   v1.0        **
//  *********************************************
//  **    basic4@privatoria.net   Sep. 2015    **
//  **                                         **
//  **    Requires the encodering App to work  **
//  **    correctly.                           **
//  *********************************************
String filename = "script.bin";
int multi = 0;
int kill = 0;
int status = 0;
String result = "READY";
void sendKeyByte(int inpx)
{
       switch(inpx)
           {
              case 251:
                  //Start of a multi-key command (eg. CTRL + ALT + DEL)
                  multi = 1;
                  break;
              case 252:
                   //The KEY command on (windows 'ALT' keycodes)
                   multi = 0;
                   Keyboard.press(130);
                   delay(50);
                   break;
              case 253:
                   //Switches KEY command off
                   multi = 0;
                   Keyboard.release(130);
                    delay(10);
                    break;
              case 254:
                   //End of a multi-key command
                   multi = 0;
                   Keyboard.releaseAll();
                   delay(5);
                   break;
               default:
                    if(multi == 0)
                    { 
                       //Normal Single character code
                       Keyboard.write(inpx);
                       delay(20);
                    }
                    else
                    {
                       //Part of a multi-key command 
                       Keyboard.press(inpx);
                       delay(30);
                    }
                    break;
           }
  

}

String readFile(String filename)
{
  File thisOne;
  Serial.println("Opening Password File: " + filename);
  if (!SD.begin(10))
  {
    return ("SD_ERROR");
  }
  char thisFilex[filename.length() + 1];
  filename.toCharArray(thisFilex, filename.length() + 1);

  if (SD.exists(thisFilex))
  {
    //Load the file
    thisOne = SD.open(thisFilex, FILE_READ);
    int macChange = 3;
    if (thisOne)
    {
      while (thisOne.available())
      {
        //Read bytes
        byte x = thisOne.read();
        if (x > -1)
        {
          if (x == 0xFF)
          {
            //delay command - read next 2 bytes
            byte a = thisOne.read();
            byte b = thisOne.read();
            int deltime = (int)a * (int) b;
            delay (deltime);
          }
          else
          {
            sendKeyByte((int)x);
          }
        }
      }
      thisOne.close();
      return ("DONE");
    }
  }
  return ("NO_FILE");
}


void setup() {
  // put your setup code here, to run once:
  pinMode(13, OUTPUT);  
  digitalWrite(13,LOW);
  Serial.begin(9600);
  Keyboard.begin();
  delay(300);
  result = readFile(filename);

}

void loop() {
  //Set the Outcome via LED
  //SOLID = Completed
  //SLOW FLASH = NO CARD
  //QUICK FLASH  = 'script.bin' not found
  if (result=="DONE")
  {
        digitalWrite(13,HIGH);
  }
  if(result=="SD_ERROR")
  {
        while(1)
        {
            digitalWrite(13,HIGH);
            delay(1000);
            digitalWrite(13,LOW);
            delay(1000);
        }
  }
  if(result=="NO_FILE")
  {
        while(1)
        {
            digitalWrite(13,HIGH);
            delay(125);
            digitalWrite(13,LOW);
            delay(125);
        }
  }
}
