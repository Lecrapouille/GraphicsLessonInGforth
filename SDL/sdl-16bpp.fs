\ ===[ Code Addendum 01 ]============================================
\                     gforth/SDL 16-bpp Pixel Demo
\ ===================================================================
\    File Name: sdl-16bpp.fs
\       Author: Timothy Trussell
\         Date: 04/05/2010
\  Description: SDL 16-bpp demo
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-20
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                  From the Linux Journal 4401.tgz
\ ===================================================================

page .( ---[ Loading SDL 16-bpp Demo ]---) cr

[IFDEF] ---marker---
  ---marker---
[ENDIF]

marker ---marker---

decimal

require sdllib.fs

0 value demo-raw-pixels
0 value demo-pixel-color
0 value demo-offset

\ ---[ Create-16bpp-Pixel ]------------------------------------------
\ Maps the passed r/g/b components to the desired 16-bpp value

: Map-Pixel { *dst _r _g _b -- n }
  *dst sdl-surface-format @ _r _g _b sdl-map-rgb
;
  
: demo16 ( -- )
  \ Initialize the graphics display.
  1024x768x16 s" SDL 16-bpp Demo0" InitGraph          \ init display

  screen-surface sdl-mustlock if      \ Lock the surface if required
    screen-surface sdl-locksurface drop
  then

  screen-surface sdl-surface-pixels @ to demo-raw-pixels  \ save ptr

  \ We can now safely write to the video surface. We'll draw a nice
  \ gradient pattern by varying our red and blue components along the
  \ X and Y axes.

  window-screenh 0 do
    window-screenw 0 do
      screen-surface j 0 i Map-Pixel to demo-pixel-color
      \ Calculate the memory offset of the pixel we wish to change
      screen-surface sdl-surface-pitch sw@ j * i 2 * + to demo-offset
      demo-pixel-color demo-raw-pixels demo-offset + w!
    loop
  loop

  \ Now unlock the surface if required
  screen-surface sdl-mustlock if
    screen-surface sdl-unlocksurface
  then

  \ copy the buffer to the visible screen surface
  screen-surface sdl-flip drop

  \ ooohs and aaahs time...
  10 to demo-offset         \ to let you use whatever delay you want
  cr ." Timer delay: "
  demo-offset 0 do
    demo-offset i - .
    1000 sdl-delay
  loop
  0 .
  CloseGraph
;

cr .( SDL-16bpp: demo16 ) cr

