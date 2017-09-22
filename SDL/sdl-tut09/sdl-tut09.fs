\ ===[ Code Addendum 01 ]============================================
\                 gforth: SDL/OpenGL Graphics Part IX
\ ===================================================================
\    File Name: sdl-tut09.fs
\       Author: Timothy Trussell
\         Date: 04/17/2010
\  Description: SDL Wu Line Demo
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-20
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                              Wu Redux
\ ===================================================================

page .( ---[ Loading SDL Wu-Lines Demo ]---) cr

[IFDEF] ---marker---
  ---marker---
[ENDIF]

marker ---marker---

decimal

\ Requires v1.0d of the SDL Library

require sdllib.fs
require sdlkeysym.fs

\ ---[ Data ]--------------------------------------------------------

0 value %quit

256 value num-levels
1 value draw-antialias-mode

\ ---[ Colors]-------------------------------------------------------
\ The <Initialize-Colors> function stores the calculated values here

0 value #Black
0 value #Blue
0 value #Green
0 value #Cyan
0 value #Red
0 value #Magenta
0 value #Brown
0 value #LightGray
0 value #DarkGray
0 value #LightBlue
0 value #LightGreen
0 value #LightCyan
0 value #LightRed
0 value #LightMagenta
0 value #Yellow
0 value #White

\ ---[ Define an SDL Event ]-----------------------------------------
\ We will use this for polling the SDL Event subsystem

create event sdl-event% %allot drop

\ ---[ Make-Title ]--------------------------------------------------
\ Adds the current screen-surface resolution to the title string
\ Requires a couple of string functions.

128 constant title-max-len
create title$ here title-max-len 1+ dup allot 0 fill

\ ---[ concat-string ]-----------------------------------------------
\ A basic string concatenation function; adds one string to another

: concat-string { *str _len *dst -- }
  *str *dst 1+ *dst C@ + _len cmove
  *dst C@ _len + *dst C!
;

\ ---[ IntToStr ]----------------------------------------------------
\ Converts an integer to a counted string for graphics printing

: IntToStr ( n -- str len ) 0 <# #S #> ;


: Make-Title ( resolution *title tlen -- resolution )
  2 pick Set-Mode-Parms       \ need the window parms set to use them
  title$ title-max-len 1+ 0 fill                  \ zero string & len
  ( *title tlen ) title$ concat-string
  window-screenw IntToStr title$ concat-string                \ width
  s" x" title$ concat-string
  window-screenh IntToStr title$ concat-string               \ height
  s" x" title$ concat-string
  window-screenbpp IntToStr title$ concat-string                \ bpp
  s" 0" title$ concat-string                     \ terminate with "0"
;

\ ---[ Initialize-Colors ]-------------------------------------------
\ Sets the correct color values based on the rgb components used.
\ Works for all four bpp resolutions.

: Initialize-Colors ( -- )
  screen-surface sdl-surface-format @ >R
  R@    0    0    0 sdl-map-rgb to #Black
  R@    0    0 $0FF sdl-map-rgb to #Blue
  R@    0 $0FF    0 sdl-map-rgb to #Green
\  R@    0 $0FF $0FF sdl-map-rgb to #Cyan
  R@    0 $0CD $0CD sdl-map-rgb to #Cyan \ not as glaring as 00FFFF
  R@ $0FF    0    0 sdl-map-rgb to #Red
  R@ $0FF    0 $0FF sdl-map-rgb to #Magenta
  R@ $08A $036 $00F sdl-map-rgb to #Brown
  R@ $09E $09E $09E sdl-map-rgb to #LightGray
  R@ $06B $06B $06B sdl-map-rgb to #DarkGray
  R@ $01E $090 $0FF sdl-map-rgb to #LightBlue
  R@    0 $0FA $09A sdl-map-rgb to #LightGreen
  R@ $097 $0FF $0FF sdl-map-rgb to #LightCyan
  R@ $0FF $030 $030 sdl-map-rgb to #LightRed
  R@ $0E0 $066 $0FF sdl-map-rgb to #LightMagenta
  R@ $0FF $0FF    0 sdl-map-rgb to #Yellow
  R> $0FF $0FF $0FF sdl-map-rgb to #White
;

\ ---[ Clear-Screen ]------------------------------------------------
\ Erases the dst surface to 0s
\ Locks/unlocks the surface if it is required.

: Clear-Screen { *dst -- }
  *dst sdl-mustlock if *dst sdl-locksurface drop then
  *dst NULL 0 sdl-fillrect drop
  *dst sdl-mustlock if *dst sdl-unlocksurface then
;

\ ---[ Wu-Line ]-----------------------------------------------------
\ Based on Michael Abrash's DDJ Feb 1992 column.
\ Uses 16.16 Fixed Point math
\ Change to 32.32 requires 64-bit math functions (or assembly)
\ Considering changing to Floating Point.

0 value wu-intshift
0 value wu-erracc
0 value wu-erradj
0 value wu-erracctmp
0 value wu-wgt
0 value wu-wgtcompmask
0 value wu-dx
0 value wu-dy
0 value wu-tmp
0 value wu-xdir

create wucolors[] here 256 cell * dup allot 0 fill
: wucolors-ndx ( n -- &wucolors[n] ) cell * wucolors[] + ;

: fg-color ( -- c ) 0 wucolors-ndx @ ;
: bg-color ( nlevels -- c ) 1- wucolors-ndx @ ;

create wu-r sdl-rect% %allot drop
create wu-fg sdl-color% %allot drop
create wu-bg sdl-color% %allot drop

\ Shortcuts for accessing the struct elements

: wu-r-x ( -- &x ) wu-r sdl-offset-x ;
: wu-r-y ( -- &y ) wu-r sdl-offset-y ;
: wu-r-w ( -- &w ) wu-r sdl-offset-w ;
: wu-r-h ( -- &h ) wu-r sdl-offset-h ;

: wu-fg-r ( -- &r ) wu-fg sdl-color-r ;
: wu-fg-g ( -- &g ) wu-fg sdl-color-g ;
: wu-fg-b ( -- &b ) wu-fg sdl-color-b ;

: wu-bg-r ( -- &r ) wu-bg sdl-color-r ;
: wu-bg-g ( -- &g ) wu-bg sdl-color-g ;
: wu-bg-b ( -- &b ) wu-bg sdl-color-b ;

\ ---[ get-fg-data ]-------------------------------------------------
\ Breaks down the <fg> pixel to the separate r/g/b elements.
\ Stores the elements directly to the struct elements.

: get-fg-data ( fg -- )
  screen-surface sdl-surface-format @
  wu-fg-r wu-fg-g wu-fg-b sdl-get-rgb
;

\ ---[ get-bg-data ]-------------------------------------------------
\ Breaks down the <bg> pixel to the separate r/g/b elements.
\ Stores the elements directly to the struct elements.

: get-bg-data ( bg -- )
  screen-surface sdl-surface-format @
  wu-bg-r wu-bg-g wu-bg-b sdl-get-rgb
;

\ ---[ generate-gamma ]----------------------------------------------
\ Creates the gamma table for the r/g/b values currently stored in
\ the fg/bg structs.

: generate-gamma { _nlevels -- }
  _nlevels 0 do
    screen-surface sdl-surface-format @
    wu-fg-r C@ i over wu-bg-r C@ - * _nlevels 1- / -    \ red
    wu-fg-g C@ i over wu-bg-g C@ - * _nlevels 1- / -    \ green
    wu-fg-b C@ i over wu-bg-b C@ - * _nlevels 1- / -    \ blue
    sdl-map-rgb
    i wucolors-ndx !
  loop
;

\ The main Wu Line function

: wu-line { *dst _x0 _y0 _x1 _y1 _fg _bg _nlevels _nbits -- }
  _fg get-fg-data              \ init wu-fg struct to <fg> pixel data
  _bg get-bg-data              \ init wu-bg struct to <bg> pixel data
  _nlevels generate-gamma    \ init wucolors[] table for fg/bg gammas
  _y0 _y1 > if                                         \ ensure y0<y1
    _y0 _y1 to _y0 to _y1                      \ if not - swap values
    _x0 _x1 to _x0 to _x1
  then
  \ draw initial pixel in the fg color - no weighting
  *dst _x0 _y0 fg-color PutPixel
  _x1 _x0 - to wu-dx
  wu-dx 0 >= if 1 else -1 then to wu-xdir
  wu-dx abs to wu-dx

  \ special case horizontal, vertical and diagonal lines
  \ No weighting as they go thru the center of every pixel

  _y1 _y0 - dup to wu-dy 0= if            \ check for horizontal line
    _x0 _x1 min wu-r-x w!
    _y0 wu-r-y w!
    wu-dx wu-r-w w!
    1 wu-r-h w!
    *dst wu-r fg-color sdl-fillrect drop       \ draw horiz
  else                                      \ check for vertical line
    wu-dx 0= if
      _x0 wu-r-x w!
      _y0 wu-r-y w!
      1 wu-r-w w!
      wu-dy wu-r-h w!
      *dst wu-r fg-color sdl-fillrect drop      \ draw vert
    else                                    \ check for diagonal line
      wu-dx wu-dy = if
        wu-dy 0 do
          _x0 wu-xdir + to _x0
          _y0 1+ to _y0
          *dst _x0 _y0 fg-color PutPixel
        loop
      else            \ Line is not horizontal, diagonal, or vertical
        0 to wu-erracc                  \ error acc is initially zero

      \ #of bits by which to shift ErrorAcc to get intensity level
        16 _nbits - to wu-intshift

      \ mask used to flip all bits in an intensity weighting,
      \ producing the result (1 - intensity weighting)
        _nlevels 1- to wu-wgtcompmask

      \ is this an X-Major or Y-Major line
        wu-dy wu-dx > if       \ Y-Major line - [wuDeltaY > wuDeltaX]
          wu-dx 16 LSHIFT wu-dy / to wu-erradj
          \ draw all pixels other than the first and last
          begin
            wu-dy -1 + dup to wu-dy 0>
          while
            wu-erracc $FFFF AND to wu-erracctmp
            wu-erracc wu-erradj + $FFFF AND to wu-erracc
            wu-erracc wu-erracctmp <= if                  \ rollover?
              _x0 wu-xdir + to _x0
            then
            _y0 1+ to _y0               \ y-major so always advance y
            wu-erracc wu-intshift RSHIFT to wu-wgt
            *dst _x0 _y0 wu-wgt wucolors-ndx @ PutPixel
            *dst _x0 wu-xdir + _y0 wu-wgt wu-wgtcompmask XOR
             wucolors-ndx @ PutPixel
          repeat
        else                                   \ it's an X-Major line
          wu-dy 16 LSHIFT wu-dx / to wu-erradj
          begin
            wu-dx -1 + to wu-dx
            wu-dx 0>
          while
            wu-erracc $FFFF AND to wu-erracctmp
            wu-erracc wu-erradj + to wu-erracc
            wu-erracc $FFFF AND to wu-erracc
            wu-erracc wu-erracctmp <= if                  \ rollover?
              _y0 1+ to _y0
            then
            _x0 wu-xdir + to _x0
            wu-erracc wu-intshift RSHIFT to wu-wgt
            *dst _x0 _y0 wu-wgt wucolors-ndx @ PutPixel
            *dst _x0 _y0 1+ wu-wgt wu-wgtcompmask XOR
            wucolors-ndx @ PutPixel
          repeat
        then \ x/y-major
      then \ diagonal
    then \ vertical
  then \ horizontal
\ draw the final pixel -- no weighting
  *dst _x1 _y1 fg-color PutPixel
;

\ ---[ draw-line ]---------------------------------------------------
\ If <draw-antialias-mode==0> draw line with Bresenham algorithm
\ If <draw-antialias-mode==1> draw line with Wu algorithm

: draw-line ( *dst x1 y1 x2 y2 c -- )
  draw-antialias-mode if
    0 256 8 wu-line                 \ add some parameters for wu-line
  else
    Line
  then
;

\ ---[ Wu-Lines Demo ]-----------------------------------------------
\ Draws four demo patterns using the Wu Lines Algorithm

: scale-win-scrw window-screenw 2/ window-screenw 10 / ;

: Side-1 ( *dst -- )
  5 { *dst _n -- }
  begin
    _n window-screenw <
  while
    *dst
    scale-win-scrw - _n 5 / +
    window-screenh 5 /
    _n
    window-screenh 1-
    #Blue
    draw-line
    _n 10 + to _n
  repeat
;

: Side-2 ( *dst -- )
  5 { *dst _n -- }
  begin
    _n window-screenh <
  while
    *dst
    scale-win-scrw -
    _n 5 /
    0
    _n
    #Green
    draw-line
    _n 10 + to _n
  repeat
;

: Side-3 ( *dst -- )
  5 { *dst _n -- }
  begin
    _n window-screenh <
  while
    *dst
    scale-win-scrw +
    _n 5 /
    window-screenw 1-
    _n
    #Red
    draw-line
    _n 10 + to _n
  repeat
;

: Side-4 ( *dst -- )
  0 { *dst _n -- }
  begin
    _n window-screenw <
  while
    *dst
    scale-win-scrw - _n 5 / +
    window-screenh 1-
    _n
    0
    #White
    draw-line
    _n 10 + to _n
  repeat
;

\ ---[ Poll-For-Exit ]-----------------------------------------------
\ Process events while they exist, looking for an exit condition.
\ Skips all other keys/events.
\ Empties the event buffer each time it is called.

: Poll-For-Exit ( -- )
  begin
    event sdl-pollevent
  while
    event sdl-event-type C@ case
      SDL_QUIT of 1 to %quit
               endof
      SDL_KeyDown of event sdl-event-key sdl-keysym-sym uw@
        SDLK_ESCAPE = if
          1 to %quit
        then
    endcase
  repeat
;

\ ---[ WuTest ]------------------------------------------------------
\ Shows first the Wu-Lines version, and then the Bresenham version
\ of the demo. Alternates with enough of a delay so that you can get
\ a good look at the difference between the line algorithms
\
\ When <draw-antialias-mode==1> it draws wu-lines.
\ When <draw-antialias-mode==0> it draws bresenham lines.

: WuTest ( -- )
  1024x768x32 s" SDL Wu/Bresenham Demo - "
  Make-Title title$ count InitGraph
  Initialize-Colors
  0 to %quit
  0 to draw-antialias-mode
  begin
    %quit 0=
  while
    draw-antialias-mode 1 XOR to draw-antialias-mode \ toggle aa mode
    screen-surface Clear-Screen
    screen-surface Side-1
    screen-surface Side-2
    screen-surface Side-3
    screen-surface Side-4
    screen-surface sdl-flip drop                  \ display the image
    1000 sdl-delay                                   \ ooohhh, aaahhh
    Poll-For-Exit
  repeat
  CloseGraph
;

\ ---[ WuDemo ]------------------------------------------------------
\ Shows just the Wu Lines version of the demo

: WuDemo ( -- )
  1024x768x32 s" SDL Wu-Lines Demo - "
  Make-Title title$ count InitGraph
  Initialize-Colors
  0 to %quit
  1 to draw-antialias-mode
  screen-surface Clear-Screen
  screen-surface Side-1
  screen-surface Side-2
  screen-surface Side-3
  screen-surface Side-4
  screen-surface sdl-flip drop
  begin
    %quit 0=
  while
    Poll-For-Exit
    5 sdl-delay                  \ give up time slice - frees the cpu
  repeat
  CloseGraph
;

\ ---[ BresDemo ]----------------------------------------------------
\ Shows just the Bresenham version of the demo

: BresDemo ( -- )
  1024x768x32 s" SDL Bresenham Lines - "
  Make-Title title$ count InitGraph
  Initialize-Colors
  0 to %quit
  0 to draw-antialias-mode
  screen-surface Clear-Screen
  screen-surface Side-1
  screen-surface Side-2
  screen-surface Side-3
  screen-surface Side-4
  screen-surface sdl-flip drop
  begin
    %quit 0=
  while
    Poll-For-Exit
    5 sdl-delay                  \ give up time slice - frees the cpu
  repeat
  CloseGraph
;

cr .( SDL-Draw-WuLine: wutest wudemo bresdemo) cr

