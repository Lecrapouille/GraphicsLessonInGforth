\ ===[ Code Addendum 01 ]============================================
\                 gforth: SDL/OpenGL Graphics Part X
\ ===================================================================
\    File Name: sdl-raycaster.fs
\       Author: Timothy Trussell
\         Date: 05/02/2010
\  Description: 3D RayCaster Demo - Flat Textures
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-21
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================

page .( ---[ Loading gforth: SDL 3D RayCaster Demo ]---) cr

[IFDEF] ---marker---
  ---marker---
[ENDIF]

marker ---marker---

decimal

\ ---[ Library Modules ]---------------------------------------------

\ Load the SDL Library interface
require sdllib.fs
require sdlkeysym.fs

\ Load the Basic Font package
require sdl-basicfontlib.fs

\ ---[ Define an SDL Event ]-----------------------------------------
\ We will use this for polling the SDL Event subsystem

create event sdl-event% %allot drop

\ ---[ Define an SDL Rect Struct ]----------------------------------
\ For drawing the vertical lines (fasssst)

create offset% sdl-rect% %allot drop

\ ---[ Define the 3D world ]-----------------------------------------
\ The world in a 24x24 matrix; each element is an 8-bit value

24 constant MapWidth
24 constant MapHeight

create WorldMap[] here MapWidth MapHeight * dup allot 0 fill

: worldmap-ndx { _x _y -- &src[x,y] }
  WorldMap[] _x + _y MapWidth * + ;

\ ---[ Make-World ]--------------------------------------------------
\ For ease of editting, as well as posting, I converted the map to
\ a series of strings, which we will now parse into 8-bit numbers
\ and store in the WorldMap[] array.
\ This is coded to work only with numbers [0..9] for now.
 
marker ---make-world---                          \ set a forget point

: Parse-Map-Data ( *str len index -- )
  0 { *str _len _ndx *dst -- }
  0 MapHeight 1- _ndx - worldmap-ndx to *dst
  _len 0 do
    *str i + C@ $30 - *dst i + C!
  loop
;    

s" 111111111111111111111111" 0  Parse-Map-Data
s" 100000000000000000000001" 1  Parse-Map-Data
s" 100000000000000000000001" 2  Parse-Map-Data
s" 100000000000000000000001" 3  Parse-Map-Data
s" 100000222220000303030001" 4  Parse-Map-Data
s" 100000200020000000000001" 5  Parse-Map-Data
s" 100000200020000300030001" 6  Parse-Map-Data
s" 100000200020000000000001" 7  Parse-Map-Data
s" 100000220220000303030001" 8  Parse-Map-Data
s" 100000000000000000000001" 9  Parse-Map-Data
s" 100000000000000000000001" 10 Parse-Map-Data
s" 100000000000000000000001" 11 Parse-Map-Data
s" 100000000000000000000001" 12 Parse-Map-Data
s" 100000000000000000000001" 13 Parse-Map-Data
s" 100000000000000000000001" 14 Parse-Map-Data
s" 100000000000000000000001" 15 Parse-Map-Data
s" 144444444000000000000001" 16 Parse-Map-Data
s" 140400004000000000000001" 17 Parse-Map-Data
s" 140000504000000000000001" 18 Parse-Map-Data
s" 140400004000000000000001" 19 Parse-Map-Data
s" 140444444000000000000001" 20 Parse-Map-Data
s" 140000000000000000000001" 21 Parse-Map-Data
s" 144444444000000000000001" 22 Parse-Map-Data
s" 111111111111111111111111" 23 Parse-Map-Data

---make-world---                      \ we have used it - now lose it

\ ---[ Choose-Wall-Color ]-------------------------------------------
\ Select color based on (x,y) grid value in the World Map array
\ The colors are defined in the sdl-basicfontlib.fs file

: Choose-Wall-Color ( x y -- _pixel )
  worldmap-ndx C@
  case
    1 of #LightRed   endof
    2 of #LightGreen endof
    3 of #LightBlue  endof
    4 of #White endof
    ( default: )
      #Yellow
  endcase
;

\ ---[ Clear-Screen ]------------------------------------------------
\ Clears the display surface, then fills the bottom half with the
\ floor color (dark gray)

: Clear-Screen ( -- )
  screen-surface 0 0 sdl-fillrect drop
  0 offset% sdl-offset-x w!
  window-screenh 2/ offset% sdl-offset-y w!
  window-screenw offset% sdl-offset-w w!
  window-screenh 2/ offset% sdl-offset-h w!
  screen-surface offset% #DarkGray sdl-fillrect drop
;

\ ---[ VertLine ]----------------------------------------------------
\ Draws a vertical slice to the dst surface using an sdl-rect struct

0 value watch-lines-draw            \ set to 1 to see each line drawn

: VertLine { *dst _x _y1 _y2 _pixel -- }
  \ init the struct elements
  _x        offset% sdl-offset-x w!
  _y1       offset% sdl-offset-y w!
  1         offset% sdl-offset-w w!
  _y2 _y1 - offset% sdl-offset-h w!
  \ use sdl-fillrect to draw the line to the surface
  *dst offset% _pixel sdl-fillrect drop
  \ If you want to have each line displayed as it is drawn
  \ Slows down the system drastically, but can be useful to see
  \ how the program is drawing the slices.  Boring.
  watch-lines-draw if
    *dst sdl-flip drop
  then
;

\ ---[ Working Variables ]-------------------------------------------

fvariable r.PX                                   \ x/y start position
fvariable r.PY
fvariable r.DirX                           \ initial direction vector
fvariable r.DirY
fvariable r.RayPX                                      \ ray position
fvariable r.RayPY
fvariable r.RayDX                                     \ ray direction
fvariable r.RayDY
fvariable r.MapX                                       \ map position
fvariable r.MapY
fvariable r.StepX                                   \ step increments
fvariable r.StepY
fvariable r.LineHeight
fvariable r.DrawStart     \ top y coord of the line slice to be drawn
fvariable r.DrawEnd       \ bot y coord of the line slice to be drawn
fvariable r.CameraX
fvariable r.MoveSpeed
fvariable r.RotSpeed
fvariable r.PlaneX             \ 2D raycaster version of Camera Plane
fvariable r.PlaneY
fvariable r.SideDX                                    \ side distance
fvariable r.SideDY
fvariable r.DeltaDX                                  \ delta distance
fvariable r.DeltaDY
fvariable r.WallDist

0 value i.hit
0 value i.side
0 value i.color
0 value %quit

\ ---[ FPS - Frames Per Second ]-------------------------------------
\ Trying to incorporate an fps display into the code

variable fps-frames
variable fps-update
fvariable fps-fps

80 constant fps-max-len
create fps-string here fps-max-len 1+ dup allot 0 fill

\ ---[ concat-string ]-----------------------------------------------
\ A basic string concatenation function, to add one string to another
\ Pass a string to add the fps count to, ie,:
\       s" FPS Count: " Make-FPS

: concat-string { *str _len *dst -- }
  *str *dst 1+ *dst C@ + _len cmove
  *dst C@ _len + *dst C!                \ use *dst[0] as length byte
;

: IntToStr ( n -- str len ) 0 <# #S #> ;

: Make-FPS { *str _len -- }
  fps-string fps-max-len 1+ 0 fill
  *str _len fps-string concat-string
  \ try to convert the floating point # to a string
  fps-fps F@ F>S dup >R IntToStr fps-string concat-string
  0 if                \ set to 1 to see the floating point fractional
    s" ." fps-string concat-string
    \ convert up to 9 digits past the decimal point
    fps-fps F@ R> S>F F- 1e9 F* 
    F>S IntToStr fps-string concat-string
  else
    R> drop
  then
;  
  
\ ---[ Key Flags ]---------------------------------------------------
\ These are set (to 1) when the key is pressed
\ These are cleared (to 0) when the key is released

0 value Key-Up-Flag
0 value Key-Down-Flag
0 value Key-Left-Flag
0 value Key-Right-Flag

: Movement-Up ( -- )        \ move forward if no wall in front of you
  r.PX F@ r.DirX F@ r.MoveSpeed F@ F* F+ fdup F>S 
  r.PY F@ F>S worldmap-ndx C@ 0= if
    r.PX F!
  else
    fdrop
  then

  r.PY F@ r.DirY F@ r.MoveSpeed F@ F* F+ fdup F>S 
  r.PX F@ F>S swap worldmap-ndx C@ 0= if
    r.PY F!
  else
    fdrop
  then
;

: Movement-Down ( -- )         \ move backwards if no wall behind you
  r.PX F@ r.DirX F@ r.MoveSpeed F@ F* F- fdup F>S 
  r.PY F@ F>S worldmap-ndx C@ 0= if
    r.PX F!
  else
    fdrop
  then

  r.PY F@ r.DirY F@ r.MoveSpeed F@ F* F- fdup F>S 
  r.PX F@ F>S swap worldmap-ndx C@ 0= if
    r.PY F!
  else
    fdrop
  then
;

: Movement-Left ( -- )                           \ rotate to the left
  \ both camera direction and camera plane must be rotated
  r.DirX F@ fdup
  r.RotSpeed F@ FCOS F* r.DirY F@ r.RotSpeed F@ FSIN F* F- r.DirX F!
  r.RotSpeed F@ FSIN F* r.DirY F@ r.RotSpeed F@ FCOS F* F+ r.DirY F!

  r.PlaneX F@ fdup
  r.RotSpeed F@ FCOS F* r.PlaneY F@ 
  r.RotSpeed F@ FSIN F* F- r.PlaneX F!
  
  r.RotSpeed F@ FSIN F* r.PlaneY F@ 
  r.RotSpeed F@ FCOS F* F+ r.PlaneY F!
;

: Movement-Right ( -- )                         \ rotate to the right
  \ both camera direction and camera plane must be rotated
  r.DirX F@ fdup
  r.RotSpeed F@ fnegate FCOS F* r.DirY F@ 
  r.RotSpeed F@ fnegate FSIN F* F- r.DirX F!

  r.RotSpeed F@ fnegate FSIN F* r.DirY F@ 
  r.RotSpeed F@ fnegate FCOS F* F+ r.DirY F!

  r.PlaneX F@ fdup
  r.RotSpeed F@ fnegate FCOS F* r.PlaneY F@ 
  r.RotSpeed F@ fnegate FSIN F* F- r.PlaneX F!

  r.RotSpeed F@ fnegate FSIN F* r.PlaneY F@ 
  r.RotSpeed F@ fnegate FCOS F* F+ r.PlaneY F!
;

\ ---[ Process-Keys ]------------------------------------------------
\ Polls the SDL Event system for keypresses.
\ Sets the appropriate flags.
\
\ If you press the Left and Right arrow at the same time, the code
\ for both will be executed, which should result in no movement.
\
\ This checks once per loop for a keypress

: Process-Keys ( -- )
  begin
    event sdl-pollevent                     \ while there is an event
  while
    event sdl-event-type C@ 
      case
        SDL_QUIT of 1 to %quit endof
        SDL_KeyDown of 
          event sdl-event-key sdl-keysym-sym uw@
            case
              SDLK_ESCAPE of 1 to %quit           endof
              SDLK_UP     of 1 to Key-Up-Flag     endof
              SDLK_DOWN   of 1 to Key-Down-Flag   endof
              SDLK_LEFT   of 1 to Key-Left-Flag   endof
              SDLK_RIGHT  of 1 to Key-Right-Flag  endof
            endcase
          endof
        SDL_KeyUp of 
          event sdl-event-key sdl-keysym-sym uw@
            case
              SDLK_UP     of 0 to Key-Up-Flag     endof
              SDLK_DOWN   of 0 to Key-Down-Flag   endof
              SDLK_LEFT   of 0 to Key-Left-Flag   endof
              SDLK_RIGHT  of 0 to Key-Right-Flag  endof
            endcase
          endof
      endcase
  repeat
  \ Now process the flags  
  Key-Up-Flag    if Movement-Up then
  Key-Down-Flag  if Movement-Down then
  Key-Left-Flag  if Movement-Left then
  Key-Right-Flag if Movement-Right then
;

: Init-Variables ( -- )
  0 to %quit                                             \ exit flag
  22e0 r.PX F!
  12e0 r.PY F!
  -1e0 r.DirX F!
  0e0 r.DirY F!
  0e0 r.PlaneX F!
  66e-2 r.PlaneY F!
  5e-1 r.MoveSpeed F!
  1e-1 r.RotSpeed F!
;

: raycast ( -- )
\  320x200x32 s" SDL RayCaster Flat Textured Demo0" InitGraph
\  640x480x32 s" SDL RayCaster Flat Textured Demo0" InitGraph
  1024x768x32 s" SDL RayCaster Flat Textured Demo0" InitGraph
  screen-surface font-init
  Init-Variables
  sdl-get-ticks fps-update !      \ get initial count value
  Clear-Screen
  begin
    %quit 0=
  while
    window-screenw 0 do
    \ calculate ray position and direction
      2e0 i S>F F* window-screenw S>F F/ 1e0 F- r.CameraX F!
    \ set ray position to position of player
      r.PX F@ r.RayPX F!
      r.PY F@ r.RayPY F!
      r.DirX F@ r.PlaneX F@ r.CameraX F@ F* F+ r.RayDX F!
      r.DirY F@ r.PlaneY F@ r.CameraX F@ F* F+ r.RayDY F!

    \ which box of the map we are in...
      r.RayPX F@ r.MapX F!
      r.RayPY F@ r.MapY F!

    \ length of ray from one x or y-side to next x or y-side
      1e0 r.RayDY F@ FDUP F* r.RayDX F@ FDUP F* F/ F+ FSQRT
      r.DeltaDX F!
      1e0 r.RayDX F@ FDUP F* r.RayDY F@ FDUP F* F/ F+ FSQRT
      r.DeltaDY F!
      0e0 r.WallDist F!

    \ what direction to step in x or y-direction (either +1 or -1)
      0e0 r.StepX F!
      0e0 r.StepY F!
      0 to i.hit
      0 to i.side

    \ calculate step and initial side distance
      r.RayDX F@ F0< if
        -1e0 r.StepX F!
        r.RayPX F@ r.MapX F@ F- r.DeltaDX F@ F* r.SideDX F!
      else
        1e0 r.StepX F!
        r.MapX F@ 1e0 F+ r.RayPX F@ F- r.DeltaDX F@ F* 
        r.SideDX F!
      then
      
      r.RayDY F@ F0< if
        -1e0 r.StepY F!
        r.RayPY F@ r.MapY F@ F- r.DeltaDY F@ F* r.SideDY F!
      else
        1e0 r.StepY F!
        r.MapY F@ 1e0 F+ r.RayPY F@ F- r.DeltaDY F@ F* 
        r.SideDY F!
      then
      
    \ perform Digital Differential Analysis (DDA)

      begin
        i.hit 0=
      while
      \ jump to next map square, OR in x-, OR in y-direction
        r.SideDX F@ r.SideDY F@ F< if
          r.SideDX F@ r.DeltaDX F@ F+ r.SideDX F!
          r.MapX F@ r.StepX F@ F+ r.MapX F!
          0 to i.side
        else
          r.SideDY F@ r.DeltaDY F@ F+ r.SideDY F!
          r.MapY F@ r.StepY F@ F+ r.MapY F!
          1 to i.side
        then
      \ check if ray has hit a wall
        r.MapX F@ F>S r.MapY F@ F>S worldmap-ndx C@ 0> if
          1 to i.hit
        then
      repeat

    \ Calculate distance projected on camera direction
      i.side 0= if
        r.MapX F@ r.RayPX F@ F- 1e0 r.StepX F@ F- 2e0 F/ F+ 
        r.RayDX F@ F/ FABS r.WallDist F!
      else
        r.MapY F@ r.RayPY F@ F- 1e0 r.StepY F@ F- 2e0 F/ F+ 
        r.RayDY F@ F/ FABS r.WallDist F!
      then

    \ Calculate height of line to draw on surface
      window-screenh S>F r.WallDist F@ F/ FABS r.LineHeight F!

    \ calculate lowest/highest pixel to fill in current stripe
      r.LineHeight F@ fnegate 2e0 F/ window-screenh S>F 2e0 F/ F+ 
      r.DrawStart F!
      r.DrawStart F@ F0< if
        0e0 r.DrawStart F!
      then
      r.LineHeight F@ 2e0 F/ window-screenh S>F 2e0 F/ F+ 
      r.DrawEnd F!
      r.DrawEnd F@ window-screenh S>F F>= if
        window-screenh 1- S>F r.DrawEnd F!
      then

    \ choose wall color
      r.MapX F@ F>S r.MapY F@ F>S Choose-Wall-Color to i.color

    \ give x and y sides different brightness
      i.side 1 = if
        i.color 2/ to i.color
      then

    \ draw the pixels of the stripe as a vertical line
      screen-surface                                    \ dst surface
      i                                             \ ray being drawn
      r.DrawStart F@ F>S                      \ top y of line to draw
      r.DrawEnd F@ F>S                     \ bottom y of line to draw
      i.color                                    \ color value to use
      VertLine                                  \ draw the line slice
    loop
    
    Process-Keys

    sdl-get-ticks fps-update @ > if
      fps-frames @ S>F sdl-get-ticks S>F fps-update @ S>F F- F/ 
      1000e0 F* fps-fps F!
      sdl-get-ticks fps-update !
    \ display the fps data to the screen somewhere...
      0 fps-frames !
    then
    0 0 #Yellow #Black s" FPS: " 
    Make-FPS fps-string dup 1+ swap C@ GWriteAt
    window-screenh 16 / 1- 
    #Yellow #DarkGray s" Arrow Keys Move, ESC exits" GWriteCenter
    1 fps-frames +!
\    10 sdl-delay
    screen-surface sdl-flip drop                     \ blit to screen
    Clear-Screen                    \ clear surface for next sequence
  repeat
  CloseGraph
;  

cr .( SDL-RayCast: raycast)

