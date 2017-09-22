\ ===[ Code Addendum 01 ]============================================
\             gforth: SDL/OpenGL Graphics Part VIII
\ ===================================================================
\    File Name: sdl-basicfontdemo.fs
\       Author: Timothy Trussell
\         Date: 03/28/2010
\  Description: Basic Font Demo
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-20
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\            Simple demo that displays the 8x16 font data
\ ===================================================================

page .( ---[ Loading gforth: SDL BasicFont Demo ]---) cr

[IFDEF] ---marker---
  ---marker---
[ENDIF]

marker ---marker---

decimal

\ ---[ Library Modules ]---------------------------------------------

require sdl-basicfontlib.fs

\ ---[ Variables ]---------------------------------------------------

\ ---[ Brighter Color Definitions ]----------------------------------
\ The basic VGA palette colors are too dim, so I tried to find some
\ that were close in the existing SDL palette...

  0 constant #Black
  3 constant #Blue
 16 constant #Green
  9 constant #Cyan
128 constant #Red
162 constant #Magenta
 68 constant #Brown
109 constant #LightGray
 36 constant #DarkGray
 23 constant #LightBlue
 21 constant #LightGreen
 31 constant #LightCyan
\ constant #LightRed
\ constant #LightMagenta
248 constant #Yellow
255 constant #White

\ ---[ ShowFontStuff ]-----------------------------------------------
\ Array of string data to send to the display surfaces

create PList$
s" : font-SetDestSurface ( *dst -- )                         " s,
s" : font-getsurface     ( -- *dst )                         " s,
s" : GetX                ( -- cx )                           " s,
s" : GetY                ( -- cy )                           " s,
s" : GetXY               ( -- cx cy )                        " s,
s" : GotoXY              ( x y -- )                          " s,
s" : \r                  ( -- )                              " s,
s" : \n                  ( -- )                              " s,
s" : Draw-Font-Char      { *dst _x _y *src -- }              " s,
s" : Update-Cursor-Position ( -- )                           " s,
s" : GPutC               { _c -- }                           " s,
s" : GPutS               { *str _len -- }                    " s,
s" : GWriteC             { _x _y _fg _bg _c -- }             " s,
s" : GWriteS             { _x _y _fg _bg *str _len -- }      " s,
s" : GWriteAt            ( x y fg bg &str len -- )           " s,
s" : GWriteCenter        { _y _fg _bg &str _len -- }         " s,
s" : GClearLine          { _y _fg _bg -- }                   " s,
s" : GBox                { _x1 _y1 _x2 _y2 _fg _bg _type -- }" s,
s" : GFBox               { _x1 _y1 _x2 _y2 _fg _bg _type -- }" s,
s" : font-init           ( *dst -- )                         " s,

: max-x font-cursor-max-x ;
: max-y font-cursor-max-y ;

: Show-Font-Stuff ( -- )
  font-getsurface NULL 0 sdl-fillrect drop        \ erase the surface
  0 0 max-x max-y 1- #LightGray #Black 15 GFBox     \ draw background
  screen-surface sdl-flip drop                     \ display the line
  10 1 max-x 10 - 3 #White #Cyan 4 GFBox               \ title window
  2 #Yellow #Cyan s" SDL BasicFont Graphical Text Demo" GWriteCenter
  3  5 max-x 3 - max-y 2 - #White #Blue 5 GFBox \ main display window
  5  5 #Yellow #Blue s" [ SDL BasicFont Info ]" GWriteAt
  7  6 #White #Blue s" Prototype Listing:" GWriteAt
  \ now display the array of strings in PList$
  screen-surface sdl-flip drop                     \ display the line
  20 0 do
    9 7 i + #White #Blue PList$ i 60 * + dup 1+ swap C@ GWriteAt
    screen-surface sdl-flip drop                   \ display the line
  loop
  max-y #White #Black GClearLine
  max-y #Yellow #Black s" Pausing for 5 seconds" GWriteCenter
  screen-surface sdl-flip drop                    \ display the image
;

\ ---[ FontDemo ]----------------------------------------------------

: Font-Res ( resolution -- )
  s" SDL BasicFont - 8x16 Font Demo0" InitGraph
  screen-surface font-init
  Show-Font-Stuff
  5000 sdl-delay
  CloseGraph
;  

: fontdemo ( -- )
  640x480x8 Font-Res
  800x600x8 Font-Res
  1024x768x8 Font-Res
;

cr .( SDL-BasicFontDemo: fontdemo ) cr
