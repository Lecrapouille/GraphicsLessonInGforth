\ ===[ Code Addendum 01 ]============================================
\             gforth: SDL/OpenGL Graphics Part VI
\ ===================================================================
\    File Name: sdl-vboll.fs
\       Author: Timothy Trussell
\         Date: 02/28/2010
\  Description: SDL Vector Bolls Graphics Demo
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-19
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================

[IFDEF] [SDL-VBOLL]
  [SDL-VBOLL]
[ENDIF]

marker [SDL-VBOLL]

decimal

require sdllib.fs
require edo.fs
require sdl-vboll-data.fs

\ ---[ Define an SDL Event ]-----------------------------------------
\ We will use this for polling the SDL Event subsystem

create event sdl-event% %allot drop

\ ---[ Define Surface Pointers ]-------------------------------------
\ Note that only screen-surface is the max resolution of 1024x768x8.
\ All the others are to be 320x200x8
\
\ screen-surface is defined in the sdllib.fs file, automatically.
\ InitGraph uses screen-surface to create the display window.

NULL value hatch-surface                         \ hatched background
NULL value red-surface                             \ red anim display
NULL value green-surface                         \ green anim display
NULL value blue-surface                           \ blue anim display
NULL value bolls-surface                              \ bolls display
NULL value combo-surface               \ where everything is combined

: Help-Message ( -- )
  ." +----------------------------------------------------------+" cr
  ." |                  gforth SDL Bolls Demo        02/28/2010 |" cr
  ." +----------------------------------------------------------|" cr
  ." |              Animated Sprites Demonstration              |" cr
  ." +----------------------------------------------------------|" cr
  ." |      Tested on: Ubuntu v9.10 i386 Kernel: 2.6.31-19      |" cr
  ." +----------------------------------------------------------+" cr
  ."                To exit demo, close the window" cr
  ." ------------------------------------------------------------" cr
  ."               Commands: sdl-vboll Help-Message" cr
  ." ------------------------------------------------------------" cr
;

\ ---[ GetSurface ]--------------------------------------------------
\ Creates a blank surface/palette in the specified w/h/bpp pattern
\ Returns pointer to allocated surface on success

: Get-Surface { _str _len -- *buf }
  SDL_SWSURFACE 320 200 8 0 0 0 0 sdl-creatergbsurface 
  dup NULL = _str _len Error-End
;

\ ---[ Clear-Surfaces ]----------------------------------------------
\ Zero the pixel array of the surfaces

: Clear-Surfaces ( -- )
  hatch-surface NULL 0 sdl-fillrect drop
  red-surface NULL   0 sdl-fillrect drop
  green-surface NULL 0 sdl-fillrect drop
  blue-surface NULL  0 sdl-fillrect drop
  bolls-surface NULL 0 sdl-fillrect drop
  combo-surface NULL 0 sdl-fillrect drop
;

\ ---[ Set-Transparency ]--------------------------------------------
\ Set the ColorKey so that when blitted to combo-surface only the
\ non-zero pixels will be copied.

: Set-Transparency ( -- )
  red-surface   SDL_SRCCOLORKEY 0 sdl-setcolorkey drop
  green-surface SDL_SRCCOLORKEY 0 sdl-setcolorkey drop
  blue-surface  SDL_SRCCOLORKEY 0 sdl-setcolorkey drop
  bolls-surface SDL_SRCCOLORKEY 0 sdl-setcolorkey drop
;

\ ---[ Set-Palettes ]------------------------------------------------
\ Initialize the palettes of all surfaces for the demo. I am using
\ the palette from the VBoll image, with the basic values for the
\ Red, Green and Blue VGA colors added on.

: Set-Surface-Palette ( *dst -- )
  SDL_LOGPAL SDL_PHYSPAL OR                            \ flags to use
  VBPalette[]                           \ source data of palette info
  0                                              \ first color number
  27                                        \ number of colors to set
  sdl-setpalette drop
;

: Set-Palettes ( -- )
  hatch-surface  Set-Surface-Palette
  red-surface    Set-Surface-Palette
  green-surface  Set-Surface-Palette
  blue-surface   Set-Surface-Palette
  bolls-surface  Set-Surface-Palette
  combo-surface  Set-Surface-Palette
  screen-surface Set-Surface-Palette
;

\ ---[ Allocate-Surfaces ]-------------------------------------------
\ Create blank surfaces from system memory - palettes are blank also

: Allocate-Surfaces ( -- )
  s" Unable to create Hatch surface" Get-Surface to hatch-surface
  s" Unable to create Red surface"   Get-Surface to red-surface
  s" Unable to create Green surface" Get-Surface to green-surface
  s" Unable to create Blue surface"  Get-Surface to blue-surface
  s" Unable to create Bolls surface" Get-Surface to bolls-surface
  s" Unable to create Combo surface" Get-Surface to combo-surface

  Clear-Surfaces                        \ zero the allocated surfaces
  Set-Transparency                    \ set transparency for blitting
  Set-Palettes                          \ initialize all palette data
;

\ ---[ Free-Surfaces ]-----------------------------------------------
\ Release the memory prior to shutting down the SDL subsystems

: Free-Surfaces ( -- )
  hatch-surface sdl-freesurface                  \ hatched background
  red-surface   sdl-freesurface                    \ red anim display
  green-surface sdl-freesurface                  \ green anim display
  blue-surface  sdl-freesurface                   \ blue anim display
  bolls-surface sdl-freesurface                       \ bolls display
  combo-surface sdl-freesurface        \ where everything is combined
;
  
\ ---[ RANDOM NUMBERS IN FORTH ]-------------------------------------
\  D. H. Lehmers Parametric multiplicative linear congruential
\  random number generator is implemented as outlined in the
\  October 1988 Communications of the ACM ( V 31 N 10 page 1192)

     16807 CONSTANT A
2147483647 CONSTANT M
    127773 CONSTANT Q   \ m a /
      2836 CONSTANT R   \ m a mod

CREATE SEED  123475689 ,

\ Returns a full cycle random number
: RAND ( -- rand )  \ 0 <= rand < 4,294,967,295
   SEED @ Q /MOD ( lo high)
   R * SWAP A * 2DUP > IF  - ELSE  - M +  THEN  DUP SEED ! ;

\ Returns single random number less than n
: RND ( n -- rnd )  \ 0 <= rnd < n
   RAND SWAP MOD ;

\ ---[ Variables ]---------------------------------------------------

variable xangle
variable yangle
variable zangle
variable distance
variable direction

0 value %ado-src
0 value %ado-color

variable %B 24 %B !
variable %G 25 %G !
variable %R 26 %R !

\ ---[ Anim[] ]------------------------------------------------------
\ Animation control structure
\ The sprite movement is controlled by the elements of this structure

create Anim[]
\                     x    x+   x<    x>    y   y+  y<    y>    c
1 , 21 , DiamondS[] ,  44 , 2 ,  40 , 256 ,   8 , 0 , 0 , 195 , %R ,
1 , 15 , Ovoid[]    , 148 , 2 ,  56 , 240 ,  72 , 0 , 0 , 195 , %R ,
1 , 23 , DiamondS[] ,  44 , 2 ,  40 , 256 , 128 , 0 , 0 , 195 , %R ,
1 , 13 , Ovoid[]    ,  60 , 0 ,   0 , 320 ,  72 , 1 , 0 , 160 , %B ,
1 , 11 , DiamondS[] , 104 , 0 ,   0 , 320 ,  72 , 2 , 0 , 160 , %B ,
1 ,  8 , Ovoid[]    , 148 , 0 ,   0 , 320 ,  72 , 3 , 0 , 164 , %B ,
1 ,  9 , DiamondS[] , 192 , 0 ,   0 , 320 ,  72 , 4 , 0 , 164 , %B ,
1 , 14 , Ovoid[]    , 236 , 0 ,   0 , 320 ,  72 , 5 , 0 , 160 , %B ,
1 ,  8 , DiamondS[] , 100 , 4 ,   4 , 300 ,  24 , 3 , 0 , 180 , %G ,
1 ,  9 , Ovoid[]    , 124 , 4 ,   4 , 300 ,  48 , 3 , 0 , 180 , %G ,
1 ,  7 , DiamondS[] , 148 , 4 ,   4 , 300 ,  72 , 3 , 0 , 180 , %G ,
1 ,  6 , Ovoid[]    , 172 , 4 ,   4 , 300 ,  96 , 3 , 0 , 180 , %G ,
1 ,  5 , DiamondS[] , 196 , 4 ,   4 , 300 , 120 , 3 , 0 , 180 , %G ,

\ ---[ CrossHatch ]--------------------------------------------------
\ Draws the background pattern to the dst surface

: CrossHatch { *dst -- }
  *dst NULL 0 sdl-fillrect drop
  40 0 do
    *dst i 8 * 0 199 23 VLine
  loop
  *dst 319 0 199 23 VLine
  20 0 do
    *dst 0 319 i 10 * 23 HLine
  loop
  *dst 0 319 199 23 HLine
;

\ ---[ Create-Lookup-Tables ]----------------------------------------
\ There are no examples on actual use of floats in the gforth dox.
\ Remember that gforth uses a separate stack for floating point.

: Create-Lookup-Tables ( -- )
  360 0 do
    i s>f 314159265e-8 F* 180 s>f F/ FSINCOS
    Cosine[] i sc-ndx F!
    Sine[]   i sc-ndx F!
  loop
;

\ ---[ Create-Vector-Object ]----------------------------------------
\ Define the matrix points for the Vector Bolls

: Set-Object ( x y z obj# -- )
  vboll[] swap vboll-ndx >R R@ .vbZ ! R@ .vbY ! R> .vbX !
;

: Create-Vector-Object ( -- )
\ The arrangement of the bolls will be in a cube pattern
\ We are drawing two sets of these bolls, so we need two structures
  -30 -30  30 0 Set-Object                     \ back top left coords
   30 -30  31 1 Set-Object                    \ back top right coords
  -30  30  32 2 Set-Object                  \ back bottom left coords
   30  30  33 3 Set-Object                 \ back bottom right coords
  -30 -30 -30 4 Set-Object                    \ front top left coords
   30 -30 -31 5 Set-Object                   \ front top right coords
  -30  30 -32 6 Set-Object                 \ front bottom left coords
   30  30 -33 7 Set-Object                \ front bottom right coords
  \ copy the object data to the second object structure
  vboll[] vboll2[] vboll[]-def cmove
;

\ ---[ GetAnimSurface ]----------------------------------------------
\ Determines which surface to use by accessing the sprite's color
\
\ Note that the .color entry is a variable pointer, not a color value

: Get-Anim-Surface { _color -- *dst }
  _color dup @ to %ado-color         \ _color is an address not value 
  case
    %B of blue-surface  endof
    %G of green-surface endof
    %R of red-surface   endof
    \ otherwise
         blue-surface
  endcase                               \ *dst
;

\ --[ Anim-Draw-Object ]---------------------------------------------
\ Draws the current Animation sprite to the surface which is chosen
\ by the sprite's color.
\ Note that this is a single-color sprite image being drawn.

: Anim-Draw-Object { &obj -- }
  &obj .color @ Get-Anim-Surface        \ *dst
  &obj .&image @ .image to %ado-src     \ *dst    set data stream ptr
  &obj .xcoord @                        \ *dst x
  &obj .ycoord @                        \ *dst x y
  &obj .&image @ .width @               \ *dst x y w
  &obj .&image @ .height @              \ *dst x y w h
  0 do                                  \ *dst x y w
    dup                                 \ *dst x y w w
    0 do                                \ *dst x y w
      %ado-src C@                       \ *dst x y w c
      if                                \ *dst x y w       plot if !0
        3 pick                          \ *dst x y w *dst
        3 pick j +                      \ *dst x y w *dst x+j
        3 pick i +                      \ *dst x y w *dst x+j y+i
        %ado-color                      \ *dst x y w *dst x+j y+i c
        PutPixel                        \ *dst x y w
      then                              \ *dst x y w
      %ado-src 1 + to %ado-src          \ *dst x y w
    loop                                \ *dst x y w
  loop                                  \ *dst x y w
  2drop 2drop                           \ --
;

\ ---[ Rotate-Vectors ]----------------------------------------------
\ Calculate the new x/y/z vectors for the Vector Bolls objects

0 value dvo:i
0 value dvo:j
0 value dvo:k
0 value dvo:a
0 value dvo:b
0 value dvo:nx
0 value dvo:ny
0 value dvo:nz
variable dvo:xofs

: Rotate-X-Vector { &obj -- } \    ---[ rotate around the x-axis ]---
  \ ny=(int)(vboll[i].y*cosine[xangle]-vboll[i].z*sine[xangle]);
  &obj .vbY @ S>F Cosine[] xangle @ sc-ndx F@ F*
  &obj .vbZ @ S>F Sine[]   xangle @ sc-ndx F@ F* F- F>S to dvo:ny
  \ nz=(int)(vboll[i].y*sine[xangle]+vboll[i].z*cosine[xangle]);
  &obj .vbY @ S>F Sine[]   xangle @ sc-ndx F@ F*
  &obj .vbZ @ S>F Cosine[] xangle @ sc-ndx F@ F* F+ F>S to dvo:nz
  \ nx=(int)(vboll[i].x);
  &obj .vbX @ to dvo:nx
;

: Rotate-Y-Vector { &obj -- }    \ ---[ rotate around the y-axis ]---
  \ nx=(int)(temp[i].x * cosine[yangle] +temp[i].z * sine[yangle]);
  &obj .vbX @ S>F Cosine[] yangle @ sc-ndx F@ F*
  &obj .vbZ @ S>F Sine[]   yangle @ sc-ndx F@ F* F+ F>S to dvo:nx
  \ nz=(int)(-temp[i].x * sine[yangle]+temp[i].z * cosine[yangle]);
  &obj .vbX @ negate S>F Sine[]   yangle @ sc-ndx F@ F*
  &obj .vbZ @ S>F Cosine[] yangle @ sc-ndx F@ F* F+ F>S to dvo:nz
  dvo:nx &obj .vbX !
  dvo:nz &obj .vbZ !
;

: Rotate-Z-Vector { &obj -- }     \ ---[ rotate around the z-axis]---
  \ nx=(int)(temp[i].x * cosine[zangle]-temp[i].y * sine[zangle]);
  &obj .vbX @ S>F Cosine[] zangle @ sc-ndx F@ F*
  &obj .vbY @ S>F Sine[]   zangle @ sc-ndx F@ F* F- F>S to dvo:nx
  \ ny=(int)(temp[i].x * sine[zangle]+temp[i].y * cosine[zangle]);
  &obj .vbX @ S>F Sine[]   zangle @ sc-ndx F@ F*
  &obj .vbY @ S>F Cosine[] zangle @ sc-ndx F@ F* F+ F>S to dvo:ny
  dvo:nx &obj .vbX !
  dvo:ny &obj .vbY !
;

: Rotate-Vectors { &obj -- }
  &obj vboll[] = if 240 else 80 then dvo:xofs !
\ first we have to do all the rotations
  8 0 do
    &obj i vboll-ndx Rotate-X-Vector

    temp[] i vboll-ndx >R
    dvo:nx R@ .vbX !
    dvo:ny R@ .vbY !
    dvo:nz R@ .vbZ !

    R@ Rotate-Y-Vector
    R@ Rotate-Z-Vector

  \ now we have to push the z coordinates into the view area
  \ temp[i].z-=distance;
    R@ .vbZ @ distance @ - R@ .vbZ !

  \ finally we project & copy the new x,y,z to a temporary array
  \ temp[i].x=(int)((temp[i].x*256)/temp[i].z+xoffset);
    R@ .vbX @ 256 * R@ .vbZ @ / dvo:xofs @ + R@ .vbX !

  \ temp[i].y=(int)((temp[i].y*256)/temp[i].z+100);
    R@ .vbY @ 256 * R@ .vbZ @ / 100 + R@ .vbY !

  \ temp[i].color=vboll[i].color;
    R> vboll[] i vboll-ndx .vbC @ swap .vbC !
  loop
;

\ ---[ ZBuffering ]--------------------------------------------------
\ Sorts the image structures based on the .vbZ value of each.
\ the z with most positive value should be in lowest index
\ I used bubble sort here because there are only 8 bolls :-)
\ -------------------------------------------------------------------

: ZBuffering ( -- )
  7 0 do
    8 i 1+ do
      temp[] j vboll-ndx .vbZ @
      temp[] i vboll-ndx .vbZ @
      >= if
        temp[] j vboll-ndx dummy[] vboll-def cmove
        temp[] i vboll-ndx temp[] j vboll-ndx vboll-def cmove
        dummy[] temp[] i vboll-ndx vboll-def cmove
      then
    loop
  loop
;

\ ---[ Draw-Vector-Object ]------------------------------------------

: <DVO> { &obj -- }
  &obj Rotate-Vectors
  ZBuffering
  8 0 do
    bolls-surface                       \ *dst
    temp[] i vboll-ndx >R               \ *dst
    R@ .vbX @ 12 -                      \ *dst x
    R> .vbY @ 15 -                      \ *dst x y
    VBBitMap[] .width @                 \ *dst x y w
    VBBitMap[] .height @                \ *dst x y w h
    VBBitMap[] .image                   \ *dst x y w h *src
    PutSprite                           \ --
  loop
;

: Draw-Vector-Objects ( -- )
  vboll[]  <DVO>                             \ process first position
  vboll2[] <DVO>                            \ process second position
;

\ ---[ offset% ]-----------------------------------------------------
\ sdl-blitsurface requires a dststruct that defines the data pointers
\ telling it where to put the surface data it is blitting

create offset% sdl-rect% %allot drop

\ ---[ Apply-Surface ]-----------------------------------------------
\ Copies the src surface to the dst surface, placed at x,y. The full
\ rectangular dimension of the src image is copied to the dst image.
\ Note that this uses the *dstrect struct to define where in the
\ dst surface the data is to be placed.

: Apply-Surface ( *src *dst x y -- )
  offset% sdl-offset-y w!                  \ set offsets to rectangle
  offset% sdl-offset-x w!
  0 offset% sdl-offset-w w!                         \ just to be safe
  0 offset% sdl-offset-h w!
  NULL swap offset% sdl-blitsurface drop  \ blit the surface to video
;

\ ---[ Copy-To-Screen ]----------------------------------------------
\ Build the 3x3 display matrix on the screen-surface for display.

: Copy-To-Screen ( -- )
  3 0 do
    3 0 do
      combo-surface
      screen-surface
      i 336 * 16 +
      j 242 * 42 +
      Apply-Surface
    loop
  loop
;

\ ---[ Erase-Work-Surfaces ]-----------------------------------------
\ Zeroes the animation build surfaces

: Erase-Work-Surfaces ( -- )
  bolls-surface NULL 0 sdl-fillrect drop
  red-surface   NULL 0 sdl-fillrect drop
  green-surface NULL 0 sdl-fillrect drop
  blue-surface  NULL 0 sdl-fillrect drop
;

\ ---[ Combine-Pages ]-----------------------------------------------
\ This takes all the data and puts it onto a single screen. I get to
\ choose the order of what goes on first.
\ So, for now, the order of precedence will be BLUE in front, GREEN
\ second, and RED in the back, and all of them will be in front of
\ the CrossHatch image data.
\
\ The hatch-surface is NOT set to transparent, as we WANT it to
\ erase anything in the combo-surface to clear it for the next frame.

: Combine-Surfaces ( -- )
  hatch-surface NULL combo-surface NULL sdl-blitsurface drop
  red-surface   NULL combo-surface NULL sdl-blitsurface drop
  green-surface NULL combo-surface NULL sdl-blitsurface drop
  blue-surface  NULL combo-surface NULL sdl-blitsurface drop
  bolls-surface NULL combo-surface NULL sdl-blitsurface drop
;

\ ---[ One-Frame ]---------------------------------------------------
\ This routine moves all the Anim objects one step per call

: One-Frame ( -- )
  13 0 do                                    \ cycle thru the objects
    Anim[] i anim-ndx >R            \ cannot use 'i' after this point
    R@ .delay @ 1- dup R@ .delay !
    if                               \ if .delay==0, move this object
      R@ .basedelay @ R@ .delay !               \ reset to .basedelay
      R@ .xcoord @ dup                  \ inc x; reverse at xmin/xmax
      R@ .xmin @ < over
      R@ .xmax @ > or if
        R@ .xinc @ negate R@ .xinc !
      then
      R@ .xinc @ + R@ .xcoord !          \ add XInc to current XCoord
      R@ .ycoord @ dup                  \ inc y; reverse at ymin/ymax
      R@ .ymin @ < over
      R@ .ymax @ > or if
        R@ .yinc @ negate R@ .yinc !
      then
      R@ .yinc @ + R@ .ycoord !          \ add YInc to current YCoord
      R> Anim-Draw-Object    \ Now draw the image at the new location
    else
      R> drop                  \ lose last pointer and check next obj
    then
  loop
  Combine-Surfaces                   \ add all the data to one screen
;

0 value quit%

: sdl-vboll
  1024x768x8 s" SDL VBolls Animation Demo0" InitGraph

  Allocate-Surfaces              \ create and initialize all surfaces

  Create-Lookup-Tables                  \ init the sine/cosine tables
  Create-Vector-Object                  \ init the object[] structure

  0 to quit%                         \ make sure we run at least once
  360 rnd xangle !                             \ initialize variables
  360 rnd yangle !
  360 rnd zangle !
  256 distance !
    1 direction !

  hatch-surface CrossHatch              \ draw the background pattern

  begin                               \ repeat until a key is pressed
    quit% 0=                                    \ loop while quit%==0
  while
    direction @ 1 = if                  \ update distance & direction
      1 distance +!
      distance @ 599 > if 0 direction ! 600 distance ! then
    else
      -1 distance +!
      distance @ 255 < if 1 direction ! 256 distance ! then
    then

    xangle @ 3 + dup 359 > if drop 0 then xangle !    \ update angles
    yangle @ 3 + dup 359 > if drop 0 then yangle !
    zangle @ 2 - dup 1   < if drop 359 then zangle !

    Draw-Vector-Objects                    \ Draw to the video buffer
    One-Frame                       \ this draws all the Anim objects
    Copy-To-Screen                  \ copy data to the screen-surface
    screen-surface sdl-flip drop            \ display the final image
    Erase-Work-Surfaces                \ erase all the build surfaces

  \ Use sdl-pollevent to see if we have X'd out (Close Window event)
  \ Alt-F4 will also close the window
  \ Need to add the ESC key as an exit option

    event sdl-pollevent if
      event sdl-event-type C@ 12 = if
        1 to quit%                                 \ exit on SDL_QUIT
      then
    then
    5 sdl-delay                           \ give up a small timeslice
  repeat
  Free-Surfaces
  CloseGraph
;

Help-Message
