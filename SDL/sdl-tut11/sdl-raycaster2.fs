\ ===[ Code Addendum 01 ]============================================
\                 gforth: SDL/OpenGL Graphics Part XI
\ ===================================================================
\    File Name: sdl-raycaster2.fs
\       Author: Timothy Trussell
\         Date: 05/09/2010
\  Description: 3D RayCaster - Textured Walls
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-21
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================

page .( ---[ Loading gforth: SDL Textured 3D Engine Demo ]---) cr

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

create offset% sdl-rect% %allot drop

\ ---[ Generate-Textured-Images ]------------------------------------
\ Set to 0 to load the file <wolftextures.png>, or set to 1 to have
\ the program generate the textures to be used for the walls.
\ The <wolftextures.png> file will be included in the archive that
\ will be posted to the Taygeta Forth Achives, while the code that is
\ posted to C.L.F. will be set to use the generated images.

1 value Generate-Textured-Images

0 value texture-surface         \ where to put the generated textures

64 constant TexWidth
64 constant TexHeight

24 constant MapWidth
24 constant MapHeight

create WorldMap[] here MapWidth MapHeight * dup allot 0 fill

: worldmap-ndx { _x _y -- &src[x,y] }
  WorldMap[] _x + _y MapWidth * + ;

\ ---[ Make-World ]--------------------------------------------------
\ For ease of editting, as well as posting, I converted the map to
\ a series of strings, which we will now parse into 8-bit numbers
\ and store in the WorldMap[] array.
\ This is coded to work only with the numbers [0..9] for now
 
marker ---make-world---                          \ set a forget point

: Parse-Map-Data ( *str len index -- )
  0 { *str _len _ndx *dst -- }
  0 MapHeight 1- _ndx - worldmap-ndx to *dst
  _len 0 do
    *str i + C@ $30 - *dst i + C!
  loop
;    

s" 444444444444444477777777" 0  Parse-Map-Data
s" 400000000000000070000007" 1  Parse-Map-Data
s" 401000000000000000000007" 2  Parse-Map-Data
s" 402000000000000000000007" 3  Parse-Map-Data
s" 403000000000000070000007" 4  Parse-Map-Data
s" 404000055555555577077777" 5  Parse-Map-Data
s" 405000050505050570007771" 6  Parse-Map-Data
s" 406000050000000570000008" 7  Parse-Map-Data
s" 407000000000000000007771" 8  Parse-Map-Data
s" 408000050000000570000008" 9  Parse-Map-Data
s" 400000050000000570007771" 10 Parse-Map-Data
s" 400000055550555577777771" 11 Parse-Map-Data
s" 666666666660666666666666" 12 Parse-Map-Data
s" 800000000000000000000004" 13 Parse-Map-Data
s" 666666066660666666666666" 14 Parse-Map-Data
s" 444444044460622222223333" 15 Parse-Map-Data
s" 400000000460620000020002" 16 Parse-Map-Data
s" 400000000000620050020002" 17 Parse-Map-Data
s" 400000000460620000022022" 18 Parse-Map-Data
s" 406060000460000050000002" 19 Parse-Map-Data
s" 400500000460620000022022" 20 Parse-Map-Data
s" 406060000460620050020002" 21 Parse-Map-Data
s" 400000000460620000020002" 22 Parse-Map-Data
s" 444444444411122222233333" 23 Parse-Map-Data

---make-world---                      \ we have used it - now lose it

\ ---[ Generate-Textures-To-Surface ]--------------------------------
\ Draws all of the textures to an SDL surface
\ Dest surface is created in the current bpp mode to match whatever
\ InitGraph sets <screen-surface> to.

: Set-Tex { _x _y _pixel _ofs -- }
  texture-surface _x 64 _ofs * + _y _pixel PutPixel ;

: Generate-Textures-To-Surface ( -- )
  0 0 0 0 { _xorc _yc _xyc _tofs -- }   \ create some local variables
  TexWidth 0 do         \ x
    TexHeight 0 do      \ y
      j 256 * TexWidth / i 256 * TexHeight / XOR to _xorc
      i 256 * TexHeight / to _yc
      i 128 * TexHeight / j 128 * TexWidth / + to _xyc

      j i j i <> j TexWidth 1- i - <> AND 0= 
      if 0 else $FE0000 then                 0 Set-Tex
      j i _xyc 257 * _xyc 16 LSHIFT +        1 Set-Tex
      j i _xyc 256 * _xyc 16 LSHIFT +        2 Set-Tex
      j i _xyc 256 _xorc * + 65536 _xorc * + 3 Set-Tex
      j i _xorc 256 *                        4 Set-Tex
      j i $C00000 j 16 mod i 16 mod AND *    5 Set-Tex
      j i _yc 65536 *                        6 Set-Tex
      j i 257 128 * $800000 +                7 Set-Tex
    loop
  loop
;

\ ---[ FPS - Frames Per Second ]-------------------------------------
\ Incorporate an fps display into the code

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

\ ---[ IntToStr ]----------------------------------------------------
\ Converts an integer value to a string; returns addr/len

: IntToStr ( n -- str len ) 0 <# #S #> ;

\ ---[ Make-FPS ]----------------------------------------------------
\ Converts the Floating Point FPS value to a string; returns addr/len

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

\ ---[ 3D Engine Variables ]-----------------------------------------
  
fvariable r.PX                                 \ x/y start position
fvariable r.PY
fvariable r.DirX                           \ initial direction vector
fvariable r.DirY
fvariable r.RayPX
fvariable r.RayPY
fvariable r.RayDX
fvariable r.RayDY
fvariable r.MapX
fvariable r.MapY
fvariable r.StepX
fvariable r.StepY
fvariable r.LineHeight
fvariable r.DrawStart     \ top y coord of the line slice to be drawn
fvariable r.DrawEnd       \ bot y coord of the line slice to be drawn
fvariable r.CameraX
fvariable r.MoveSpeed
fvariable r.RotSpeed
fvariable r.PlaneX             \ 2D raycaster version of Camera Plane
fvariable r.PlaneY
fvariable r.SideDX
fvariable r.SideDY
fvariable r.DeltaDX
fvariable r.DeltaDY
fvariable r.WallDist
fvariable r.WallX

0 value i.hit
0 value i.side
0 value i.color
0 value i.texture#
0 value i.texX
0 value i.texY

0 value %quit

\ ---[ Key Flags ]---------------------------------------------------
\ These are set (to 1) when the key is pressed
\ These are cleared (to 0) when the key is released

0 value Key-Up-Flag
0 value Key-Down-Flag
0 value Key-Left-Flag
0 value Key-Right-Flag

\ In the Movement-Up and -Down code, I have inserted an FDUP and SWAP
\ to replace duplicating an entire FP calculation, along with an
\ FDROP if the test is false.

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

\ ---[ Clear-Screen ]------------------------------------------------
\ Clears the display surface, then fills the bottom half with the
\ floor color (dark gray)

: Clear-Screen ( -- )
  screen-surface 0 #Black sdl-fillrect drop
  0 offset% sdl-offset-x w!
  window-screenh 2/ offset% sdl-offset-y w!
  window-screenw offset% sdl-offset-w w!
  window-screenh 2/ offset% sdl-offset-h w!
  screen-surface offset% #DarkGray sdl-fillrect drop
;

: Init-Variables ( -- )
  0 to %quit                                             \ exit flag
  7e0    r.PX F!
  20e0    r.PY F!
  -1e0   r.DirX F!
  0e0    r.DirY F!
  0e0    r.PlaneX F!
  66e-2  r.PlaneY F!

  5e-1 r.MoveSpeed F!
  1e-1 r.RotSpeed F!
;

: raycast ( -- )
\  320x200x32 s" SDL RayCaster Textured Demo0" InitGraph
  640x480x32 s" SDL RayCaster Textured Demo0" InitGraph
\  1024x768x32 s" SDL RayCaster Textured Demo0" InitGraph
  screen-surface font-init
  \ create surface for the textures to use
  Generate-Textured-Images if
   SDL_SWSURFACE 512 64 window-screenbpp 0 0 0 0 sdl-creatergbsurface
   dup NULL = s" Unable to create Texture Surface" Error-End
   to texture-surface
   Generate-Textures-To-Surface
  else
   s" wolftextures.png0" Terminate-String sdl-loadimage
   dup 0= s" Unable to load image file" Error-End
   to texture-surface
  then

  Init-Variables
  sdl-get-ticks fps-update !      \ get initial count value

  begin
    %quit 0=
  while
    window-screenw 0 do
      \ calculate ray position and direction
      2 i * S>F window-screenw S>F F/ 1e0 F- r.CameraX F!
      r.PX F@ r.RayPX F! \ set ray position to position of player
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
      0 to i.hit                              \ was there a wall hit?
      0 to i.side                       \ was a NS or an EW wall hit?

      \ calculate step and initial Side Distance X
      r.RayDX F@ F0< if
        -1e0 r.StepX F!
        r.RayPX F@ r.MapX F@ F- r.DeltaDX F@ F* r.SideDX F!
      else
        1e0 r.StepX F!
        r.MapX F@ 1e0 F+ r.RayPX F@ F- r.DeltaDX F@ F* 
        r.SideDX F!
      then

      \ calculate step and initial Side Distance Y
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
      r.LineHeight F@ fnegate 2e0 F/ window-screenh 2/ S>F F+ 
      r.DrawStart F!
      r.DrawStart F@ F0< if
        0e0 r.DrawStart F!
      then
      r.LineHeight F@ 2e0 F/ window-screenh 2/ S>F F+ r.DrawEnd F!
      r.DrawEnd F@ window-screenh S>F F>= if
        window-screenh 1- S>F r.DrawEnd F!
      then

      \ ---[ Texturing Calculations ]--------------------------------

      \ calculate value of r.WallX - where exactly the wall was hit
      i.side 1 = if
        r.RayPX F@ r.MapY F@ r.RayPY F@ F- 1e0 r.StepY F@ F- 
        2e0 F/ F+ r.RayDY F@ F/ r.RayDX F@ F* F+
      else
        r.RayPY F@ r.MapX F@ r.RayPX F@ F- 1e0 r.StepX F@ F- 
        2e0 F/ F+ r.RayDX F@ F/ r.RayDY F@ F* F+
      then
      FDUP floor F- r.WallX F!

      \ x coordinate on the texture
      r.WallX F@ TexWidth S>F F* F>S to i.texX

      i.side 0 = r.RayDX F@ F0> AND if 
        TexWidth i.texX - 1- to i.texX
      then

      i.side 1 = r.RayDY F@ F0< AND if 
        TexWidth i.texX - 1- to i.texX
      then

      r.DrawEnd F@ F>S r.DrawStart F@ F>S do
        i 8 LSHIFT window-screenh 7 LSHIFT - S>F
        r.LineHeight F@ 128e0 F* F+
        TexHeight S>F F* r.LineHeight F@ F/ F>S 8 RSHIFT
        to i.texY

        \ Select texture based on wall # in the map
        texture-surface
        r.MapX F@ F>S r.MapY F@ F>S worldmap-ndx C@ 1-
        64 * i.texX + i.texY GetPixel to i.color
 
        \ Make the color darker for y-sides (shadowing, kind of)
        \ r/g/b divided thru two with a SHIFT and an AND
        \ (The AND ensures bit 7 of each byte is cleared)
        i.side 1 = if i.color 1 RSHIFT $7F7F7F AND to i.color then
 
        \ I have put in two different PutPixel codes:
        1 if
          j offset% sdl-offset-x w!
          i offset% sdl-offset-y w!
          1 offset% sdl-offset-w w!
          1 offset% sdl-offset-h w!
          screen-surface offset% i.color sdl-fillrect drop
        else
          screen-surface j i i.color PutPixel
        then
      loop
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
    0 1 #Yellow #Black s" PosX:" GWriteAt
    0 2 #Yellow #Black s" PosY:" GWriteAt
    6 1 #White #Black r.PX F@ F>S IntToStr GWriteAt
    6 2 #White #Black r.PY F@ F>S IntToStr GWriteAt

    window-screenh 16 / 1- 
    #Yellow #DarkGray s" Arrow Keys Move, ESC exits" GWriteCenter
    1 fps-frames +!
    screen-surface sdl-flip drop                     \ blit to screen
    Clear-Screen                    \ clear surface for next sequence
  repeat
  texture-surface sdl-freesurface
  CloseGraph
;  

\ ---[ Diagnostics ]-------------------------------------------------
\ showtextures displays the generated textures to the screen.

create src-offset% sdl-rect% %allot drop
create dst-offset% sdl-rect% %allot drop

: copy-texture-from-surface { *dst _x _y _ofs -- }
  64 _ofs * src-offset% sdl-offset-x w!
   0 src-offset% sdl-offset-y w!
  64 src-offset% sdl-offset-w w!
  64 src-offset% sdl-offset-h w!
          
  _x dst-offset% sdl-offset-x w!
  _y dst-offset% sdl-offset-y w!
  64 dst-offset% sdl-offset-w w!
  64 dst-offset% sdl-offset-h w!
  texture-surface src-offset% *dst dst-offset% sdl-blitsurface drop
;

: CountDown { _secs -- }
  ." Delay: " _secs .
  _secs 0 do
    1000 sdl-delay
    _secs 1- i - .
  loop
;

: showtextures ( -- )
  640x480x32 s" SDL RayCaster Textured Demo0" InitGraph
  screen-surface font-init
  Generate-Textured-Images if
   SDL_SWSURFACE 512 64 window-screenbpp 0 0 0 0 sdl-creatergbsurface 
   dup NULL = s" Unable to create Texture Surface" Error-End
   to texture-surface
   Generate-Textures-To-Surface
  else
   s" wolftextures.png0" Terminate-String sdl-loadimage    \ load image
   dup 0= s" Unable to load image file" Error-End     \ error check it
   to texture-surface
  then
  Clear-Screen
  8 0 do
    \ select texture
    screen-surface window-screenw 8 / i * 0 i copy-texture-from-surface
  loop
  \ display texture numbers
  8 0 do
    window-screenw 8 / i * 8 / dup
    5 #Yellow #Black s" Texture" GWriteAT
    7 + 5 #Yellow #Black i IntToStr GWriteAT
  loop
  \ blit surface to display
  screen-surface sdl-flip drop
  \ oooohs and aaaahs time
  10 CountDown
  \ close it down
  texture-surface sdl-freesurface
  CloseGraph
;

cr .( SDL-Ray-Textured: raycast showtextures)
cr

