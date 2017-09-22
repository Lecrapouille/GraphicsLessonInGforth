\ ===[ Code Addendum 01 ]============================================
\                gforth: SDL/OpenGL Graphics Part XII
\ ===================================================================
\    File Name: sdl-raycaster3.fs
\       Author: Timothy Trussell
\         Date: 06/13/2010
\  Description: A Simple Raycasting Engine
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-22
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\             Original C source by Jonas Wustrack 2006\ ===================================================================
\ Walls are "paper thin" in this version.
\ Added textures from <wolftextures.png> image file
\ -- This file has 8 textures, each in a 64x64 format, for an image
\    size of 512x64.
\ ===================================================================

page
.( ---[ Loading SDL Simple Raycaster Engine Demo ]---) cr

[IFDEF] ---marker---
  ---marker---
[ENDIF]

marker ---marker---

decimal

\ ---[ Library Modules ]---------------------------------------------

\ Load the SDL Library interface
require sdllib.fs
require sdlkeysym.fs
require sdl-basicfontlib.fs

\ Load the fast rendering functions
require sdl-raycast3-1.00.fs

\ ---[ Generate-Textures ]-------------------------------------------
\ Set to 1 to generate the wall texture
\ Set to 0 to load the <wolftextures.png> image file.

0 value Generate-Textures

\ ---[ Structures ]--------------------------------------------------

\ ---[ Define an SDL Event ]-----------------------------------------
\ We will use this for polling the SDL Event subsystem

create event sdl-event% %allot drop
create offset% sdl-rect% %allot drop

struct
  sfloat% field .x
  sfloat% field .y
  sfloat% field .pos-inc
  sfloat% field .slide-inc
  sfloat% field .dir
  sfloat% field .dir-inc
end-struct person%

struct
  sfloat% field .ax
  sfloat% field .ay
  sfloat% field .bx
  sfloat% field .by
  sfloat% field .coef-x
  sfloat% field .coef-y
  sfloat% field .aconst
  int%    field .wtype
end-struct wall%

create person person% %allot person% nip 0 fill

\ ---[ Define Global Variables/Pointers/Flags ]----------------------

0 value %quit
0 value texture-surface                             \ texture pattern
0 value floor-surface                                 \ floor pattern
1 value ShowKeys                   \ 1=show key help on screen; 0=off
1 value ShowFPS                         \ 1=show fps on screen; 0=off
0 value fmaxy                          \ font-cursor-max-y short form

0 value frames                                  \ for the FPS display
0 value ticks
0 value my-fps

variable mousex                           \ for the mouse motion code
variable mousey
0 value temp-surface                 \ pointer to surface to store to
0 value temp-index                   \ y coordinate of pixels to plot

0e       fconstant init-x
1.5e     fconstant init-y
15.7e    fconstant init-dir                           \ originally 0e
0.03e    fconstant dir-inc
0.05e    fconstant pos-inc
PI 3e F/ fconstant Vision-A

\ If using the <wolftextures.png> file, do not compile this part

Generate-Textures [if]

\ ---[ Build-Texture ]-----------------------------------------------
\ This uses a 20x80x32 SDL surface to store the texture data.
\ The original 8-bit values are modified to add the g/b colors,
\ giving a 24-bit value (which is what SDL uses).
\ The 8-bit values in the table that follows represent the RED data.
\ The values are being stored backwards (from right to left).

: Make-Texture ( [20 #s] n -- )
  to temp-index
  20 0 do
    temp-surface 19 i - temp-index 3 pick 16 LSHIFT $1414 OR PutPixel
    drop                     \ lose the 8-bit value we just processed
  loop
;

: Build-Texture ( *dst -- )
  to temp-surface
\  00  01  02  03  04  05  06  07  08  09
\  10  11  12  13  14  15  16  17  18  19 index Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $64 $64 
  $64 $64 $64 $64 $64 $64 $3C $1E $3C $64 00 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $1E $3C $3C 01 Make-Texture
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 02 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $1E 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 03 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $3C $1E 
  $3C $64 $64 $64 $64 $64 $64 $64 $64 $64 04 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 05 Make-Texture
  $80 $80 $80 $6E $6E $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 06 Make-Texture
  $80 $64 $80 $80 $6E $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 07 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 08 Make-Texture
  $80 $80 $80 $80 $80 $80 $6E $64 $3C $1E 
  $3C $64 $6E $80 $80 $80 $80 $80 $80 $80 09 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $3C $1E 
  $3C $64 $64 $64 $64 $64 $64 $64 $64 $64 10 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $1E 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 11 Make-Texture
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 12 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $1E $3C $3C 13 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $64 $64 
  $64 $64 $64 $64 $64 $64 $3C $1E $3C $64 14 Make-Texture
  $80 $80 $80 $80 $80 $80 $6E $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 15 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 16 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 17 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 18 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 19 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $64 $64 
  $64 $64 $64 $64 $64 $64 $3C $1E $3C $64 20 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $1E $3C $3C 21 Make-Texture
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 22 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $1E 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 23 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $3C $1E 
  $3C $64 $64 $64 $64 $64 $64 $64 $64 $64 24 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 25 Make-Texture
  $80 $80 $80 $6E $6E $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 26 Make-Texture
  $80 $64 $80 $80 $6E $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 27 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 28 Make-Texture
  $80 $80 $80 $80 $80 $80 $6E $64 $3C $1E 
  $3C $64 $6E $80 $80 $80 $80 $80 $80 $80 29 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $3C $1E 
  $3C $64 $64 $64 $64 $64 $64 $64 $64 $64 30 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $1E 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 31 Make-Texture
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 32 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $1E $3C $3C 33 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $64 $64 
  $64 $64 $64 $64 $64 $64 $3C $1E $3C $64 34 Make-Texture
  $80 $80 $80 $80 $80 $80 $6E $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 35 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 36 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 37 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 38 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 39 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $64 $64 
  $64 $64 $64 $64 $64 $64 $3C $1E $3C $64 40 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $1E $3C $3C 41 Make-Texture
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 42 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $1E 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 43 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $3C $1E 
  $3C $64 $64 $64 $64 $64 $64 $64 $64 $64 44 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 45 Make-Texture
  $80 $80 $80 $6E $6E $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 46 Make-Texture
  $80 $64 $80 $80 $6E $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 47 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 48 Make-Texture
  $80 $80 $80 $80 $80 $80 $6E $64 $3C $1E 
  $3C $64 $6E $80 $80 $80 $80 $80 $80 $80 49 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $3C $1E 
  $3C $64 $64 $64 $64 $64 $64 $64 $64 $64 50 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $1E 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 51 Make-Texture
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 52 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $1E $3C $3C 53 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $64 $64 
  $64 $64 $64 $64 $64 $64 $3C $1E $3C $64 54 Make-Texture
  $80 $80 $80 $80 $80 $80 $6E $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 55 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 56 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 57 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 58 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 59 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $64 $64 
  $64 $64 $64 $64 $64 $64 $3C $1E $3C $64 60 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $1E $3C $3C 61 Make-Texture
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 62 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $1E 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 63 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $3C $1E 
  $3C $64 $64 $64 $64 $64 $64 $64 $64 $64 64 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 65 Make-Texture
  $80 $80 $80 $6E $6E $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 66 Make-Texture
  $80 $64 $80 $80 $6E $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 67 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $64 $3C $1E 
  $3C $64 $80 $80 $80 $80 $80 $80 $80 $80 68 Make-Texture
  $80 $80 $80 $80 $80 $80 $6E $64 $3C $1E 
  $3C $64 $6E $80 $80 $80 $80 $80 $80 $80 69 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $3C $1E 
  $3C $64 $64 $64 $64 $64 $64 $64 $64 $64 70 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $1E 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 71 Make-Texture
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 
  $1E $1E $1E $1E $1E $1E $1E $1E $1E $1E 72 Make-Texture
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $1E $3C $3C 73 Make-Texture
  $64 $64 $64 $64 $64 $64 $64 $64 $64 $64 
  $64 $64 $64 $64 $64 $64 $3C $1E $3C $64 74 Make-Texture
  $80 $80 $80 $80 $80 $80 $6E $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 75 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 76 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 77 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 78 Make-Texture
  $80 $80 $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $3C $1E $3C $64 79 Make-Texture
;

[else]
  : Build-Texture ;      \ need the stub since it is referenced later
[then]

\ ---[ Build-Floor ]-------------------------------------------------
\ Grayscale image for the ceiling/floor textures
\ Since this is simply an SDL surface, we can put data into it via
\ the PutPixel function.
\
\ Should also be able to use images for these...

: Make-Floor ( [20 #s] ndx -- )
  to temp-index
  20 0 do                               \ the floor is a 20x20 matrix
    temp-surface 19 i - temp-index 3 pick PutPixel
    drop                     \ lose the 8-bit value we just processed
  loop
;

: Build-Floor ( *dst -- )
  to temp-surface
\  00  01  02  03  04  05  06  07  08  09
\  10  11  12  13  14  15  16  17  18  19 index Make-Floor
  $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 
  $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 0 Make-Floor
  $00 $3C $3C $3C $3C $3C $3C $3C $3C $3C 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $00 1 Make-Floor
  $00 $3C $64 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 2 Make-Floor
  $00 $3C $64 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 3 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 4 Make-Floor
  $00 $3C $80 $64 $64 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 5 Make-Floor
  $00 $3C $80 $80 $6E $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 6 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 7 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $5A $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 8 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $64 $64 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 9 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 10 Make-Floor
  $00 $3C $3C $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 11 Make-Floor
  $00 $3C $64 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 12 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 13 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 14 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $80 $80 $3C $00 15 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $80 $64 $3C $3C $00 16 Make-Floor
  $00 $3C $80 $80 $80 $80 $80 $80 $80 $80 
  $80 $80 $80 $80 $80 $64 $64 $64 $3C $00 17 Make-Floor
  $00 $3C $3C $3C $3C $3C $3C $3C $80 $64 
  $3C $3C $3C $3C $3C $3C $3C $3C $3C $00 18 Make-Floor
  $00 $00 $00 $00 $00 $00 $00 $00 $00 $00 
  $00 $00 $00 $00 $3C $00 $00 $00 $00 $00 19 Make-Floor
;

\ ---[ sf, ]---------------------------------------------------------
\ Allocate and store a short float - 4 bytes - to the dictionary.
\ Suggested by Anton Ertl 06/03/2010 - Thanks Anton!
\ This is due to the C compiler expecting floats to be only 4 bytes
\ in size - at least for structures.

: sf, ( r -- ) here 1 sfloats allot sf! ;

\ ---[ Wall[] ]------------------------------------------------------
\ This array defines the walls used in the program.
\ I have not spent much time examining this, but it is a little more
\ complicated than the simple map used in my previous RayCasting
\ columns.  It is also a lot more versatile.
\
\ If how I am reading this is correct, based on the -40e X coords,
\ the world is arranged with compass points like this:
\
\                                 -y
\                                270
\                                 |
\                                 |
\                 <-- -x  180 ----+----> 0  +x -->
\                                 |
\                                 |
\                                90
\                                +y
\
\ (I know, the y +/- dirs look reversed, but they *are* correct)
\
\ So, when the program first runs, you are looking ahead at a wall,
\ but you are not facing "north", you are facing "east".
\
\ (This is with init-dir set to 0e.  If you change init-dir to 15.7
\ then you would be looking *west* when the program starts)
\
\ Additionally, the walls do not have to be drawn on a grid.  Lines
\ can be drawn at any angle you want them to be.  If you want to have
\ circles, all you have to do is break it down into sufficiently
\ small segments to get it as round as you want.
\
\ Remember, the Wall[] array defines *lines*, which the program then
\ translates into walls with height.  When you are working with this
\ remember that you are drawing line segments.
\
\ I have added the <+tndx> word to assign a different texture to each
\ of the walls as they are defined.  Normally, the value would be set
\ specifically for what the wall is meant to look like.

: +tndx ( n -- n++ ) 
  Generate-Textures 0= if
    dup 8 mod                              \ [0..7] for texture index
  else
    0                           \ generated texture is always index 0
  then
  ,                  \ save the texture index as next entry in struct
  1+                                       \ increment the wall count
;

0                         \ init count for how many walls are defined

create Wall[]
\   ax       ay       bx       by     coef-x  coef-y  aconst
  0e   sf, 2e   sf, 0e   sf, 3e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
  0e   sf, 3e   sf, 3e   sf, 3e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
  1e   sf, 0e   sf, 3e   sf, 0e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
  3e   sf, 0e   sf, 3e   sf, 3e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
  1e   sf, 0e   sf, 0.9e sf, 0.5e sf, 0e  sf, 0e  sf, 0e  sf, +tndx
  0.9e sf, 0.5e sf, 0.7e sf, 0.7e sf, 0e  sf, 0e  sf, 0e  sf, +tndx
  0.7e sf, 0.7e sf, 0.5e sf, 0.9e sf, 0e  sf, 0e  sf, 0e  sf, +tndx
  0.5e sf, 0.9e sf, 0e   sf, 1e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
\ y- cul-de-sac
  0e   sf, 1e   sf, -10e sf, 1e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-10e   sf, 1e   sf, -10e sf, -5e  sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-10e   sf, -5e  sf, -12e sf, -5e  sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-12e   sf, -5e  sf, -12e sf, 1e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-12e   sf, 1e   sf, -20e sf, 1e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
\ y+ cul-de-sac
  0e   sf, 2e   sf, -10e sf, 2e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-10e   sf, 2e   sf, -10e sf, 3e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-10e   sf, 3e   sf, -12e sf, 3e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-12e   sf, 3e   sf, -12e sf, 2e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-12e   sf, 2e   sf, -20e sf, 2e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
\ octagonal room at end
-20e   sf, 1e   sf, -21e sf, 0e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-21e   sf, 0e   sf, -22e sf, 0e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-22e   sf, 0e   sf, -23e sf, 1e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-23e   sf, 1e   sf, -23e sf, 2e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-23e   sf, 2e   sf, -22e sf, 3e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-22e   sf, 3e   sf, -21e sf, 3e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
-21e   sf, 3e   sf, -20e sf, 2e   sf, 0e  sf, 0e  sf, 0e  sf, +tndx
\ A spoked thingy to the side of the above maze - giant asterisk
  0e   sf, 5e   sf,   0e sf, 15e  sf, 0e  sf, 0e  sf, 0e  sf, +tndx
  5e   sf, 10e  sf,  -5e sf, 10e  sf, 0e  sf, 0e  sf, 0e  sf, +tndx
 -5e   sf, 5e   sf,   5e sf, 15e  sf, 0e  sf, 0e  sf, 0e  sf, +tndx
 -5e   sf, 15e  sf,   5e sf,  5e  sf, 0e  sf, 0e  sf, 0e  sf, +tndx
 
constant #MaxWalls                               \ # of walls defined

\ ---[ wall-ndx ]----------------------------------------------------
\ Returns the base address of each wall definition in Wall[], which
\ is then accessed using the wall% struct definition fields.
\
\ The < wall% swap / > sequence determines how many fields there are
\ in each of the Wall[] elements.  Each field is 4-bytes in size.
\ <wall%> returns two values, an alignment size and the total size of
\ the array element.  In this case, it returns <4 32> on the stack.
\
\ Since the alignment size is the same as the size of an sfloat, this
\ could be changed to:
\
\   : wall-ndx ( n -- *wall[n] ) wall% over / >R * R> * Wall[] + ;
\
\ but does not allow for if the size of the float changes back to
\ an 8-byte value, which would be needed if the definition in the
\ libcc file is changed from <float> to <double>.
\ Leaves an option open for changing the code more easily.

: wall-ndx ( n -- *wall[n] ) 1 sfloats * wall% swap / * Wall[] + ;

\ ---[ Update-Walls ]------------------------------------------------
\ Executed during program initialization phase only

fvariable cm-l
fvariable cm-xd
fvariable cm-yd
fvariable cm-minx
fvariable cm-miny
fvariable cm-maxx
fvariable cm-maxy

: Update-Walls ( -- )
  0 { _w -- }
  #MaxWalls 0 do
    i wall-ndx to _w                    \ shortcut to wall[i] address
    _w .bx sf@ _w .ax sf@ F- cm-xd sf!
    _w .by sf@ _w .ay sf@ F- cm-yd sf!
    cm-xd sf@ FDUP F* cm-yd sf@ FDUP F* F+ FSQRT FDUP F0<> if
      cm-l sf! 
      cm-yd sf@ cm-l sf@ F/ _w .coef-x sf!
      cm-xd sf@ cm-l sf@ F/ fnegate _w .coef-y sf!
      cm-xd sf@ _w .ay sf@ F* cm-yd sf@ _w .ax sf@ F* F- 
      cm-l sf@ F/ _w .aconst sf!
      _w .ax sf@ _w .bx sf@ FMIN 0.001e F- cm-minx sf!
      _w .ay sf@ _w .by sf@ FMIN 0.001e F- cm-miny sf!
      _w .ax sf@ _w .bx sf@ FMAX 0.001e F+ cm-maxx sf!
      _w .ay sf@ _w .by sf@ FMAX 0.001e F+ cm-maxy sf!
      cm-minx sf@ _w .ax sf!
      cm-miny sf@ _w .ay sf!
      cm-maxx sf@ _w .bx sf!
      cm-maxy sf@ _w .by sf!
    else
    \ if cm-l==0e then skip it - no dividing by zero
      FDROP
    then
  loop
;

\ ---[ flen-matrix ]-------------------------------------------------
\ Floor matrix allocation in dictionary.
\ This is allocated dynamically *after* InitGraph has been executed,
\ so that the array is set up at runtime for any of the possible
\ video modes (surface sizes) the user might choose.

0 value flen-matrix

: fmatrix-ndx ( n -- *flen-matrix[n] ) floats flen-matrix + ;

\ ---[ Calc-Floor-Matrix ]-------------------------------------------
\ Initialize the flen-matrix array
\ Cannot use sfloats for this, as the C program apparently *wants*
\ the floats to be stored as 8-byte values, while still seeming to
\ claim that the size of a float is only 4 bytes.

\ ..a programmer...only a programmer...only a programmer...only a p..

: Calc-Floor-Matrix ( -- )
  window-screenh 2/ { _wh -- }
  here _wh 1+ floats allot to flen-matrix
  _wh 0 do
    1e
    i _wh - S>F                           \ this is *always* negative
    Vision-A F*                                            \ * (PI/3)
    window-screenh S>F F/
    FTAN
    F/
    2e F/
    i fmatrix-ndx f!
  loop
;

\ ---[ Process-Input ]-----------------------------------------------
\ Monitor the keyboard/mouse and modify the related data to reflect
\ what the user has told the 'game' to do.
\ Should add keys to do what the mouse does
\ Requires sdl-1.0f.fs revision for the mouse usage.

: Process-Input ( -- )
  begin
    event sdl-pollevent                     \ while there is an event
  while
   event sdl-event-type C@ 
    case
     SDL_KeyDown of 
       event sdl-event-key sdl-keysym-sym uw@
        case
         SDLK_ESCAPE of 1 to %quit endof
         SDLK_q      of 1 to %quit endof
         SDLK_LEFT   of pos-inc fnegate person .slide-inc sf! endof
         SDLK_RIGHT  of pos-inc person .slide-inc sf! endof
         SDLK_UP     of pos-inc person .pos-inc sf! endof
         SDLK_DOWN   of pos-inc fnegate person .pos-inc sf! endof
  \ This toggles the KeyHelp display on/off
         SDLK_h      of 1 ShowKeys xor to ShowKeys endof
  \ This toggles the FPS display on/off
         SDLK_f      of 1 ShowFPS xor to ShowFPS endof
        endcase
      endof \ SDL_KeyDown
     SDL_KeyUp of 
       event sdl-event-key sdl-keysym-sym uw@
        case
          SDLK_LEFT   of person .slide-inc sf@ 0e F< if
                           0e person .slide-inc sf!
                         then
                      endof
          SDLK_RIGHT  of person .slide-inc sf@ 0e F> if
                           0e person .slide-inc sf!
                         then
                      endof
          SDLK_UP     of person .pos-inc sf@ 0e F> if
                           0e person .pos-inc sf!
                         then
                      endof
          SDLK_DOWN   of person .pos-inc sf@ 0e F< if
                           0e person .pos-inc sf!
                         then
                      endof
        endcase
      endof \ SDL_KeyUp
     SDL_MOUSEMOTION of
       mousex mousey sdl-getrelmousestate drop  \ part of sdl-1.0f.fs
     \ Value returned from getrelmousestate are the buttons states
       person .dir sf@ 
       mousex @ 10 * S>F window-screenw S>F F/ F+ 
       person .dir sf!
      endof \ SDL_MOUSEMOTION
    endcase
  repeat
  \ Update person direction
  person .dir sf@ person .dir-inc sf@ F+ person .dir sf!
  \ Update person y position
  person .y sf@
  person .pos-inc sf@ person .dir sf@ FSIN F*
  person .slide-inc sf@ person .dir sf@ FCOS F* F+ F+ person .y sf!
  \ Update person x position
  person .x sf@
  person .pos-inc sf@ person .dir sf@ FCOS F*
  person .slide-inc sf@ person .dir sf@ FSIN F* F- F+ person .x sf!
;  

\ ---[ LoadImage ]---------------------------------------------------
\ This loads a texture file into a new surface, and then optimizes
\ the image to the current display surface format.  This means that
\ it will convert from an 8-bit image to a 32-bit for us.

: LoadImage ( *fname len -- *optimized-image )
  Terminate-String sdl-loadimage                         \ load image
  dup 0= s" Unable to load image file" Error-End     \ error check it
  dup sdl-display-format                     \ create optimized image
  swap sdl-freesurface                        \ release the old image
;                                           \ returns *optimzed-image  

\ ---[ IntToStr ]----------------------------------------------------
\ Converts an integer value to a string; returns addr/len

: IntToStr ( n -- str len ) 0 <# #S #> ;

\ ---[ Simple-Ray ]--------------------------------------------------

: Display-Surface-Stats ( -- )
  cr ." Initialized display surface to: "
  window-screenw IntToStr type ." x"
  window-screenh IntToStr type ." x"
  window-screenbpp IntToStr type
;

: Load-Textures ( -- )
  cr ." Loading Textures - "
  Generate-Textures 0= if
    s" wolftextures.png0" LoadImage to texture-surface
    ." Using Wolf3D textures"
  else
    0 20 80 32 0 0 0 0 sdl-creatergbsurface
    dup 0= s" Unable to create SDL surface for texture" Error-End
    to texture-surface
    texture-surface Build-Texture
    ." Using generated textures"
  then
;

: Create-Floor-Texture ( -- )
  cr ." Creating Floor Texture"
  0 20 20 32 0 0 0 0 sdl-creatergbsurface
  dup 0= s" Unable to create SDL surface for floor" Error-End
  to floor-surface
  floor-surface Build-Floor
;

: Initialize-Data ( -- )
  cr ." Initializing data"
  0 to %quit
  0 to frames
  1 to ShowKeys
  person person% nip 0 fill
  init-x person .x sf!
  init-y person .y sf!
  init-dir person .dir sf!

  flen-matrix 0= if    \ do not execute these again after initial run
    Calc-Floor-Matrix        \ so arrays do not get allocated *again*
    Update-Walls
  then
  sdl-get-ticks to ticks               \ get initial clock tick count
;

: Scale-FPS-Box ( x2 -- x2+++ ) my-fps IntToStr nip + ;

: Display-Text ( -- )
  ShowKeys if
    1 fmaxy 4 - 19 fmaxy #White #DarkGray 6 GFBox
    #Yellow to font-color
    3 fmaxy 3 - GotoXY s" Arrow Keys Move" GPutS
    3 fmaxy 2 - GotoXY s" Mouse Rotates  " GPutS
    3 fmaxy 1 - GotoXY s" ESC/q Exits    " GPutS
  then
  ShowFPS if
  \ scale the box to fit the current fps # of digits
    1 1 8 Scale-FPS-Box 3 #White #Blue 4 GFBox
    2 2 #Yellow #Blue s" FPS: " GWriteAt my-fps IntToStr GPutS
  then
;

: Update-FPS ( -- )
  frames 1+ to frames                              \ update fps count
  sdl-get-ticks ticks 1000 + >= if           \ after a second elapses
    frames to my-fps                 \ save the new count for display
    0 to frames                              \ reset the counter to 0
    sdl-get-ticks to ticks     \ get the current ticks to start again
    ShowFPS 0= if
      cr my-fps . ." fps"
    then
  then
;

: ShutDown 
  texture-surface sdl-freesurface
  floor-surface sdl-freesurface
  font-quit
  CloseGraph
;

\ ---[ Render-A-Frame ]----------------------------------------------

: Render-A-Frame ( -- )
  person
  Wall[]
  flen-matrix
  #MaxWalls
  floor-surface
  texture-surface
  screen-surface
  ray-renderframe
;

: RayCast3 ( res -- )
\  320x200x32 s" gforth/SDL RayCaster 30" InitGraph
  640x480x32 s" gforth/SDL RayCaster 30" InitGraph
\  800x600x32 s" gforth/SDL RayCaster 30" InitGraph
\  1024x768x32 s" gforth/SDL RayCaster 30" InitGraph

  Display-Surface-Stats
  screen-surface font-init
  font-cursor-max-y to fmaxy
  Load-Textures
  Create-Floor-Texture
  Initialize-Data

  cr ." Executing main loop" cr
  cr ." Press 'h' to toggle key help on/off (slows display down)"
  cr ." Press 'f' to toggle FPS display (slows display down)" cr
  
  begin
    %quit 0=
  while
    Process-Input                     \ see if user has done anything
    Render-A-Frame                   \ render entire scene to surface
    Display-Text                                  \ show FPS and help
    screen-surface sdl-flip drop          \ display surface to screen
    screen-surface 0 0 sdl-fillrect drop          \ Clear the surface
    Update-FPS
\    10 sdl-delay                \ in case your system is too fast...
  repeat
  ShutDown
;

cr .( SDL RayCaster3: raycast3)

