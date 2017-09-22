\ ===[ Code Addendum 01 ]============================================
\             gforth: SDL/OpenGL Graphics Part VII
\ ===================================================================
\    File Name: sdl-tut07.fs
\       Author: Timothy Trussell
\         Date: 03/27/2010
\  Description: SDL_ttf Fonts Demonstration
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-20
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================

page .( ---[ Loading gforth: SDL/OpenGL sdl-tut07.fs ]---) cr

[IFDEF] ---marker---
  ---marker---
[ENDIF]

marker ---marker---

decimal

\ ---[ Library Modules ]---------------------------------------------
\ Needs the v1.0c SDL Library module

require sdllib.fs
require ttflib.fs

\ ---[ Variables ]---------------------------------------------------

0 value %quit                                             \ exit flag

\ ---[ Define an SDL Event ]-----------------------------------------
\ We will use this for polling the SDL Event subsystem

create event sdl-event% %allot drop

\ Define White

create TextColor[] 255 C, 255 C, 255 C, 0 C,

\ ---[ String Definitions ]------------------------------------------

\ ---[ Font Names ]--------------------------------------------------
\ These are passed to the ttf-openfont function, and therefore need
\ the trailing "0" byte to allow the string to be converted to a
\ zero-delimited C type string.

\ If you are going to use different fonts, change these names to
\ match the fonts you have.

0 [if]
: TTF-N  s" ttf-fonts/Times_New_Roman.ttf0" ;      \ Linux font names
: TTF-B  s" ttf-fonts/Times_New_Roman_Bold.ttf0" ;
: TTF-BI s" ttf-fonts/Times_New_Roman_Bold_Italic.ttf0" ;
: TTF-I  s" ttf-fonts/Times_New_Roman_Italic.ttf0" ;
[else]
: TTF-N  s" ttf-fonts/times.ttf0" ;              \ Windows font names
: TTF-B  s" ttf-fonts/timesbd.ttf0" ;
: TTF-BI s" ttf-fonts/timesbi.ttf0" ;
: TTF-I  s" ttf-fonts/timesi.ttf0" ;
[then]

\ ---[ Alphanumeric ]------------------------------------------------

: AlphaNs s" ABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890" ;

\ ---[ Wait-For-Exit ]-----------------------------------------------
\ Loop until the 'X' block is clicked, or Alt-F4 is pressed

: Wait-For-Exit ( -- )
  begin
    %quit 0=
  while
    event sdl-pollevent if
      event sdl-event-type C@ SDL_QUIT = if
        1 to %quit                                 \ exit on SDL_QUIT
      then
    then
  repeat
;

: Click-To-Exit-Message ( -- )
  \ Display this at the bottom of the screen surface
  s" Click the 'X' box to exit0" TTF-N 20 window-screenh 1- over -
  TextColor[] screen-surface FWriteCenter
;

: ttfdemo ( -- )
  1024x768x16 s" SDL TrueType Font Demo0" InitGraph
  ttf-init s" Unable to initialize sdl-ttf system" Error-End
  0 to %quit
  \ Display the font path data as the text messages
  TTF-N   TTF-N  28 0 0      TextColor[] screen-surface FWriteAt
  TTF-B   TTF-B  28 0 28     TextColor[] screen-surface FWriteAt
  TTF-BI  TTF-BI 28 0 28 2 * TextColor[] screen-surface FWriteAt
  TTF-I   TTF-I  28 0 28 3 * TextColor[] screen-surface FWriteAt
  AlphaNs TTF-N  28 0 28 6 * TextColor[] screen-surface FWriteAt
  AlphaNs TTF-B  28 0 28 7 * TextColor[] screen-surface FWriteAt
  AlphaNs TTF-BI 28 0 28 8 * TextColor[] screen-surface FWriteAt
  AlphaNs TTF-I  28 0 28 9 * TextColor[] screen-surface FWriteAt
  Click-To-Exit-Message             \ display the how to exit message
  screen-surface sdl-flip drop                    \ display the image
  Wait-For-Exit                            \ close the window to exit
  ttf-quit                               \ quit the sdl-ttf subsystem
  CloseGraph
;

cr .( SDL-Tut07: ttfdemo ) cr

