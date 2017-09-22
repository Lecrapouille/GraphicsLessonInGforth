\ ===[ Code Addendum 03 ]============================================
\                gforth: SDL/OpenGL Graphics Part XII
\ ===================================================================
\    File Name: sdllib.fs
\      Version: 1.0f
\       Author: Timothy Trussell
\         Date: 06/13/2010
\  Description: SDL Library Routines for gforth
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-22
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                   SDL v1.0f Library for gforth
\ ===================================================================

\ ---[ Changes Log ]-------------------------------------------------
\ 01/31/2010    Started coding the SDL Library Interface     rev 1.0a
\               Posted in my SDL columns Parts I thru IV
\
\ 02/14/2010    Formally put together the SDL Library        rev 1.0b
\               Posted in my SDL column Part V and VI
\
\ 03/12/2010    Added Draw-Box and Draw-Filled-Box           rev 1.0c
\               Added additional struct elements to the
\               <sdl-1.0c.fs> interface file
\
\ 03/18/2010    Changed Splash-Screen to load .PNG image
\               Drops the 900k .BMP file to a 43k .PNG file
\
\ 04/03/2010    Added GetPixel                               rev 1.0d
\               Reviewing sdl-surface% struct as I think it
\               is not coded correctly.
\               Added numerous functions to the interface file.
\
\ 04/15/2010    Changed H/VLine to use sdl-fillrect to draw the
\               lines
\               Changed Draw-Filled-Box to use sdl-fillrect
\               Modified Line to eliminate external VALUEs
\               Added alternative PutPixel, using sdl-fillrect
\
\ 05/02/2010    Moved Set-Color from BasicFont and added     rev 1.0e
\               it to InitGraph to initialize the color set.
\               Added functions to the Library Interface.
\               Added Wu Line function to Library
\
\ 06/09/2010    Added mouse functions for column XII         rev 1.0f

\     -------------------
\ ---[ Prototype Listing ]-------------------------------------------
\     -------------------

\ : SDLLib-Version              ( -- )

\ : Error-End                   ( f addr n -- )
\ : Terminate-String            ( a n -- a )

\ : Set-Mode-Parms              ( mode -- )
\ : Set-Colors                  ( -- )
\ : InitGraph                   ( mode a n -- )
\ : CloseGraph                  ( -- )

\ : <PlotPixel>                 { *ofs _pixel _bytepp -- }
\ : <GetPixel>                  { *ofs _bytepp -- pixel }
\ : PutPixel                    { *dst _x _y _pixel -- }
\ : GetPixel                    { *dst _x _y -- pixel }
\ : Line                        { *dst _x1 _y1 _x2 _y2 _pixel -- }
\ : VLine                       { *dst _x _y1 _y2 _pixel -- }
\ : HLine                       { *dst _x1 _x2 _y _pixel -- }
\ : Draw-Box                    { *dst _x1 _y1 _x2 _y2 _pixel -- }
\ : Draw-Filled-Box             { *dst _x1 _y1 _x2 _y2 _pixel -- }
\ : wu-line       { *dst _x0 _y0 _x1 _y1 _fg _bg _nlevels _nbits -- }
\ : <Draw-Image>                { *dst _x _y _w _h *src tflag -- }
\ : PutImage                    ( *dst _x _y _w _h *src -- )
\ : PutSprite                   ( *dst _x _y _w _h *src -- )

\ : Splash-Screen               ( -- )
\ ----------------------------------------------[ End Prototypes ]---

\ ---[ %sdl-quiet ]--------------------------------------------------
\ Set to 1 to eliminate my library compilation messages
\ Will also enable/disable the Splash screen display.

1 value %sdl-quiet

\ Load the SDL C Library Interface

require sdl-1.0f.fs

\     -----------
\ ---[ Variables ]---------------------------------------------------
\     -----------

\ These are set in InitGraph during video mode initialization

0   value screen-surface                  \ pointer to screen surface
640 value window-screenw                         \ screen width value
480 value window-screenh                        \ screen height value
32  value window-screenbpp              \ bits per pixel of this mode
0   value window-fullscreen     \ whether to use window or fullscreen
0   value in-graphics-mode           \ flag for when graphics enabled

\ ---[ mouse-cursor-display ]----------------------------------------
\ This flag is used to allow the programmer to turn off the mouse (if
\ set to 1) when the mouse cursor is inside the display window. Set
\ to 0 to allow the mouse cursor to be seen in the window.

0 value mouse-cursor-display

\ ---[ Enum ]--------------------------------------------------------
\ Enumeration type definition

: ENUM+ ( n -- ) create , does> ( -- n ) dup @ constant 1 swap +! ;
: ENUM  ( -- )   0 ENUM+ ;

\ ---[ Video Mode Listing ]------------------------------------------
\ These are ordered by bpp, but new resolutions can be simply added
\ to the list (above last-mode). If new resolutions *are* added, then 
\ the Set-Mode-Parms word will also have to be modified to give the
\ correct parameters for that resolution.

enum [vmodes]

[vmodes] 256x256x8
[vmodes] 320x200x8
[vmodes] 640x480x8
[vmodes] 800x600x8
[vmodes] 1024x768x8
[vmodes] 1280x1024x8
[vmodes] 1600x12004x8

[vmodes] 256x256x16
[vmodes] 320x200x16
[vmodes] 640x480x16
[vmodes] 800x600x16
[vmodes] 1024x768x16
[vmodes] 1280x1024x16
[vmodes] 1600x12004x16

[vmodes] 256x256x24
[vmodes] 320x200x24
[vmodes] 640x480x24
[vmodes] 800x600x24
[vmodes] 1024x768x24
[vmodes] 1280x1024x24
[vmodes] 1600x12004x24

[vmodes] 256x256x32
[vmodes] 320x200x32
[vmodes] 640x480x32
[vmodes] 800x600x32
[vmodes] 1024x768x32
[vmodes] 1280x1024x32
[vmodes] 1600x12004x32

[vmodes] last-mode

\ ---[ Basic Color Listing ]-----------------------------------------

enum [color]
[color] Black
[color] Blue
[color] Green
[color] Cyan
[color] Red
[color] Magenta
[color] Brown
[color] LightGray
[color] DarkGray
[color] LightBlue
[color] LightGreen
[color] LightCyan
[color] LightRed
[color] LightMagenta
[color] Yellow
[color] White

\ ---[ Advanced Color Listing ]--------------------------------------
\ Where we will store the calculated rgb color values

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

\     ----------------
\ ---[ SDLLib-Version ]----------------------------------------------
\     ----------------

: SDLLib-Version ( -- )
  ." +-------------------------------------------------+" cr
  ." |   SDLLib v1.0f 06/13/2010 by Timothy Trussell   |" cr
  ." +-------------------------------------------------+" cr
;

\ ---[ Error-End ]---------------------------------------------------
\ Handle an SDL return code -- 0 == ok, -1 == an error occurred

\ If a zero flag is passed, then the string pointer is dropped and
\ the function exits and allows the program to continue running.

\ If a non-zero flag is passed, the function closes down the SDL
\ subsystems, closes the display window, displays the error message
\ and quits the gforth system. This is to give you the idea that an
\ error occurred.

\ Examples of use are in the InitGraph function below

: Error-End ( f addr n -- )
  rot if                    \ if flag is !=0, then there was an error
    in-graphics-mode if       \ turn off graphics mode for error exit
      SDL_ENABLE sdl-show-cursor drop        \ turn the mouse back on
      sdl-quit                                 \ exit the SDL systems
      0 to in-graphics-mode                         \ reset mode flag
    then
    type                                    \ now display the message
    cr
    bye                             \ and exit gforth to the terminal
  else                         \ Otherwise, all was good, so continue
    2drop
  then
;

\ ---[ Terminate-String ]--------------------------------------------
\ Convert the gforth string to a zero-delimited string.
\ Note that an extra digit needs to be placed at the end of the 
\ string, and that digit is replaced with a 0 byte value.
\ ie., 
\       s" gforth1.bmp0" Terminate-String
\
\ This is required for the interface when passing strings to the C
\ library functions.

: Terminate-String ( a n -- a )                    \ 0 delimit string
  over + 1- 0 swap c! ; 

\ ---[ Set-Mode-Parms ]----------------------------------------------
\ A shortcut tool to keep from having to change multiple variables
\ to select the mode to be used.  This allows just changing the
\ entry in the InitGraph call to set the required parameters for the
\ mode to be used.

: Set-Mode-Parms ( mode -- )
\ parse if in valid range [0..(last-mode)-1]
  dup 0 >= over last-mode < AND if
    case
      256x256x8    of 256  256  8  endof
      320x200x8    of 320  200  8  endof
      640x480x8    of 640  480  8  endof
      800x600x8    of 800  600  8  endof
      1024x768x8   of 1024 768  8  endof
      1280x1024x8  of 1280 1024 8  endof
\      1600x1200x8  of 1600 1200 8  endof
      256x256x16   of 256  256  16 endof
      320x200x16   of 320  200  16 endof
      640x480x16   of 640  480  16 endof
      800x600x16   of 800  600  16 endof
      1024x768x16  of 1024 768  16 endof
      1280x1024x16 of 1280 1024 16 endof
\      1600x1200x8  of 1600 1200 16 endof
      256x256x24   of 256  256  24 endof
      320x200x24   of 320  200  24 endof
      640x480x24   of 640  480  24 endof
      800x600x24   of 800  600  24 endof
      1024x768x24  of 1024 768  24 endof
      1280x1024x24 of 1280 1024 24 endof
\      1600x1200x8  of 1600 1200 24 endof
      256x256x32   of 256  256  32 endof
      320x200x32   of 320  200  32 endof
      640x480x32   of 640  480  32 endof
      800x600x32   of 800  600  32 endof
      1024x768x32  of 1024 768  32 endof
      1280x1024x32 of 1280 1024 32 endof
\      1600x1200x8  of 1600 1200 32 endof
    endcase
  else
  \ out of range - use default of 640x480x32
    drop
    640 480 32
  then
  to window-screenbpp
  to window-screenh
  to window-screenw
;

\ ---[ Set-Colors ]---------------------------------------------
\ Sets the correct color values based on the rgb components used.
\ Works on all four bpp resolutions.

: Set-Colors ( -- )
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

\ ---[ InitGraph ]---------------------------------------------------
\ Initializes the video display to the specified video mode
\
\ Example:
\
\       640x480x8 s" Window-Title0" InitGraph
\
\ sets the window-screenW/H/BPP variables to 640 480 8 and then
\ initializes the display to that resolution, and sets the title bar
\ to the passed string.
\
\ If no window title is desired, simply pass NULL NULL instead:
\
\       640x480x8 NULL NULL InitGraph
\
\ Note: See Terminate-String for title string composition

: InitGraph ( mode a n -- )
  rot Set-Mode-Parms                       \ select screen parameters
  SDL_INIT_EVERYTHING sdl-init          \ initialize SDL video system
  0<> s" Unable to initialize SDL" Error-End      \ check return code

  window-screenw                                  \ create the window
  window-screenh
  window-screenbpp
  window-fullscreen if SDL_FULLSCREEN else 1 then       \ fullscreen?
  sdl-set-video-mode                       \ Note: SDL_SWSURFACE == 0
  dup 0< s" Unable to set video mode" Error-End   \ check return code
  to screen-surface                  \ save pointer to screen surface

  dup NULL = if                 \ if string length == 0 then no title
    2drop                  \ no window title, drop NULLs and continue
  else
    Terminate-String NULL sdl-wm-set-caption   \ set the title string
  then
  mouse-cursor-display if
    SDL_DISABLE sdl-show-cursor drop   \ disappear mouse if in window
  then
  Set-Colors                      \ Initialize the advanced color set
  1 to in-graphics-mode          \ set flag because graphics are 'on'
;

\ ---[ CloseGraph ]--------------------------------------------------
\ Re-enables the mouse pointer, and shuts down the SDL systems.
\ The open display window on the desktop is closed.
\ The programmer must have already freed allocated surfaces.

: CloseGraph ( -- )
  mouse-cursor-display if
    SDL_ENABLE sdl-show-cursor drop          \ turn the mouse back on
  then
  sdl-quit                                            \ graceful exit
  0 to in-graphics-mode
;

\                        ---------------------
\ ----------------------[ Graphics Primitives ]----------------------
\                        ---------------------

\ ---[ <PlotPixel> ]-------------------------------------------------
\ Pixel plot function.
\ Designed to handle the computers' Endian-ness
\ Verify setting of the SDL_BYTEORDER constant for your computer.
\
\   *ofs    - target address of pixel in destination surface
\             Calculated by:
\               ofs = *dst+(y*swidth)+(x*bytepp)
\             where
\               *dst   - pointer to destination surface
\               x,y    - coordinates to plot the pixel to
\               swidth - width of screen line for this color depth
\               bytepp - # of bytes per pixel for this color depth
\
\   _pixel  - the pixel data to be plotted
\   _bytepp - number of bytes per pixel on the surface
\
\ Called from PutPixel, VLine, HLine
\
\ Note-
\       C! stores an 8-bit value to the destination address
\       w! stores a 16-bit value to the destination address
\       !  stores a 32-bit value to the destination address

: <PlotPixel> { *ofs _pixel _bytepp -- }
  _bytepp case
    1 of _pixel *ofs C! endof
    2 of _pixel *ofs w! endof
    3 of SDL_BYTEORDER SDL_BIG_ENDIAN = if
           _pixel 16 RSHIFT *ofs C!
           _pixel 8 RSHIFT *ofs 1+ C!
           _pixel *ofs 2 + C!
         else
           _pixel *ofs C!
           _pixel 8 RSHIFT *ofs 1+ C!
           _pixel 16 RSHIFT *ofs 2 + C!
         then
      endof
    4 of _pixel *ofs ! endof
  endcase
;

\ Retrieves a pixel from the specifed address offset
\ _bytepp indicates if surface is 8-, 16-, 24- or 32-bpp

: <GetPixel> { *ofs _bytepp -- pixel }
  _bytepp case
    1 of *ofs C@ endof
    2 of *ofs w@ endof
    3 of SDL_BYTEORDER SDL_BIG_ENDIAN = if
           *ofs C@ 16 LSHIFT
           *ofs 1+ C@ 8 LSHIFT OR
           *ofs 2 + C@ OR
         else
           *ofs C@
           *ofs 1+ C@ 8 LSHIFT OR
           *ofs 2 + C@ 16 LSHIFT OR
         then
      endof
    4 of *ofs @ endof
  endcase
;

\ ---[ PutPixel ]----------------------------------------------------
\ Plots the pixel value to the *dst surface at the given coordinates.

\ PutPixel accesses the <pitch> and <pixel> fields of the sdl-surface
\ struct, which is defined in the sdl.fs file.

\ <pitch> defines the window-width of the surface, and represents the
\ actual number of bytes each line contains.
\
\   In 1024x768x8  mode, this value is 1024, with 1 byte/pixel
\   In 1024x768x16 mode, this value is 2048, with 2 bytes/pixel
\   In 1024x768x24 mode, this value is 3072, with 3 bytes/pixel
\   In 1024x768x32 mode, this value is 4096, with 4 bytes/pixel
\
\ <pixel> is a pointer to the actual memory for the surface data

\ Note that the window-width <pitch> represents is generated by the
\ SDL video subsystem, not by the call to InitGraph which we make to
\ create a display window.

\ The address is calculated with the following formula:
\     ofs = *dst->pixels+(y*swidth)+(x*bytepp)
\ where
\     *dst->pixels - pointer to destination surface pixel data
\     x,y          - coordinates to plot the pixel to
\     swidth       - <pitch> value of the screen line, defined as
\                    window-screenw * bytes-per-pixel
\     bytepp       - # of bytes per pixel for this color depth
\

: PutPixel { *dst _x _y _pixel -- }
  \ Lock the surface if required
  *dst sdl-mustlock if
    *dst sdl-locksurface drop
  then
  \ Calculate the target address of the pixel
  *dst sdl-surface-pixels @     \ *pixels              32-bit pointer
  *dst sdl-surface-pitch sw@    \ *pixels swidth         16-bit value
  _y * +                        \ *pixels[swidth*y]
  *dst sdl-surface-format @     \ *pixels[swidth*y] *format
  sdl-pixelformat-bytesperpixel \ *pixels[swidth*y] *format->bytepp
  C@                            \ *pixels[swidth*y] bytepp
  dup >R                        \ *pixels[swidth*y] bytepp
  _x * +                        \ *pixels[ofs]
  _pixel                        \ *pixels[ofs] pixel
  R>                            \ *pixels[ofs] pixel bytepp
  <PlotPixel>                   \ --
  \ Now unlock the surface if required
  *dst sdl-mustlock if
    *dst sdl-unlocksurface
  then
;

: GetPixel { *dst _x _y -- pixel }
  \ Lock the surface if required
  *dst sdl-mustlock if
    *dst sdl-locksurface drop
  then
  \ Calculate the target address of the pixel
  *dst sdl-surface-pixels @     \ *pixels              32-bit pointer
  *dst sdl-surface-pitch sw@    \ *pixels swidth         16-bit value
  _y * +                        \ *pixels[swidth*y]
  *dst sdl-surface-format @     \ *pixels[swidth*y] *format
  sdl-pixelformat-bytesperpixel \ *pixels[swidth*y] *format->bytepp
  C@                            \ *pixels[swidth*y] bytepp
  dup >R                        \ *pixels[swidth*y] bytepp
  _x * +                        \ *pixels[ofs]
  R>                            \ *pixels[ofs] pixel bytepp
  <GetPixel>                    \ pixel
  \ Now unlock the surface if required
  *dst sdl-mustlock if
    *dst sdl-unlocksurface
  then
;

\     ------
\ ---[ Line ]--------------------------------------------------------
\     ------
\ Draws a Bresenham line from x1,y1 to x2,y2 in the color pixel.

0 value [[d]
0 value [[x]
0 value [[y]
0 value [[ax]
0 value [[ay]
0 value [[sx]
0 value [[sy]
0 value [[dx]
0 value [[dy]

: Line { *dst _x1 _y1 _x2 _y2 _pixel -- }
  _x2 _x1 - to [[dx]
  [[dx] abs 2* to [[ax]
  [[dx] 0< if -1 else 1 then to [[sx]

  _y2 _y1 - to [[dy]
  [[dy] abs 2* to [[ay]
  [[dy] 0< if -1 else 1 then to [[sy]

  _x1 to [[x]
  _y1 to [[y]

  [[ax] [[ay] > if
    [[ay] [[ax] 2/ - to [[d]
    begin
      [[x] _x2 = 0=                                \ while [[x] != x2
    while
      *dst [[x] [[y] _pixel PutPixel
      [[d] 0 >= if
        [[sy] [[y] + to [[y]
        [[ax] negate [[d] + to [[d]
      then
      [[sx] [[x] + to [[x]
      [[ay] [[d] + to [[d]
    repeat
  else                                                  \ ax not > ay
    [[ax] [[ay] 2/ - to [[d]
    begin
      [[y] _y2 = 0=                                \ while [[y] != y2
    while
      *dst [[x] [[y] _pixel PutPixel
      [[d] 0 >= if
        [[sx] [[x] + to [[x]
        [[ay] negate [[d] + to [[d]
      then
      [[sy] [[y] + to [[y]
      [[ax] [[d] + to [[d]
    repeat
  then
;

\ ---[ VLine ]-------------------------------------------------------
\ Draws a vertical line to the dst surface

0 value %vh-bpp

create line% sdl-rect% %allot drop

: VLine { *dst _x _y1 _y2 _pixel -- }
  _x line% sdl-offset-x w!
  _y1 line% sdl-offset-y w!
  1 line% sdl-offset-w w!
  _y2 _y1 - 1+ line% sdl-offset-h w!
  *dst line% _pixel sdl-fillrect drop
;

\ ---[ HLine ]-------------------------------------------------------
\ Draws a horizontal line to the dst surface

: HLine { *dst _x1 _x2 _y _pixel -- }
  _x1 line% sdl-offset-x w!
  _y line% sdl-offset-y w!
  _x2 _x1 - 1+ line% sdl-offset-w w!
  1 line% sdl-offset-h w!
  *dst line% _pixel sdl-fillrect drop
;

\ ---[ Draw-Box ]----------------------------------------------------
\ Draws an open box frame to the destination surface

: Draw-Box { *dst _x1 _y1 _x2 _y2 _pixel -- }
  *dst _x1 _x2 _y1 _pixel HLine                     \ draw top of box
  *dst _x1 _x2 _y2 _pixel HLine                  \ draw bottom of box
  *dst _x1 _y1 _y2 _pixel VLine               \ draw left side of box
  *dst _x2 _y1 _y2 _pixel VLine              \ draw right side of box
;

\ ---[ Draw-Filled-Box ]---------------------------------------------
\ Draws a filled box to the dst surface

: Draw-Filled-Box { *dst _x1 _y1 _x2 _y2 _pixel -- }
  _x1 line% sdl-offset-x w!
  _y1 line% sdl-offset-y w!
  _x2 _x1 - line% sdl-offset-w w!
  _y2 _y1 - line% sdl-offset-h w!
  *dst line% _pixel sdl-fillrect drop
;

\ ---[ Wu-Line ]-----------------------------------------------------
\ Based on Michael Abrash's DDJ Feb 1992 column.
\ Uses 16.16 Fixed Point math

256 value num-levels

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

\     -------------
\ ---[ <DrawImage> ]-------------------------------------------------
\     -------------
\ Core routine for both PutImage and PutSprite.
\ By setting the transparency flag <tflag> parameter to 0, the full
\ sprite image, including zeroes, will be plotted.
\ Setting the transparency flag to 1 will cause it to NOT plot the
\ zero pixels, giving the image transparency to the background.
\
\ I should probably get rid of PutSprite, and simply specify that the
\ PutImage function has the transparency flag parameter requirement.
\
\ Change 02/15/2010: 
\ I realized that I do not have to perform a multiply sequence for
\ getting each and every pixel from the sprite image as I have been
\ doing.  It is simpler to set a variable to the start of the image
\ data stream, and increment the pointer as each pixel is accessed.
\
\ ---[ Parameters ]---
\ *dst          - pointer to destination screen surface
\ _x            - top left x coord to display image at
\ _y            - top left y coord to display image at
\ _w            - width of sprite image
\ _h            - height of sprite image
\ *src          - pointer to sprite image data - NOT a screen surface
\ tflag         - transparency flag

0 value %di-ofs

: <Draw-Image> { *dst _x _y _w _h *src tflag -- }
  *src to %di-ofs                           \ initialize data pointer
  _h 0 do
    _w 0 do
      tflag if                               \ transparency flag set?
        %di-ofs C@  0>          \ 0/1           yes - do not plot 0's
      else
        1                       \ 1              no - draw everything
      then
      if                        \                  \ plot if pixel!=0
        *dst _x i + _y j +      \ *dst x+i y+j       \ target x/y set
        %di-ofs C@              \ *dst x+i y+j pixel
        PutPixel                \ --
      then
      %di-ofs 1+ to %di-ofs                  \ increment data pointer
    loop
  loop
;

\     ----------
\ ---[ PutImage ]----------------------------------------------------
\     ----------
\ Draw a bitmap image into the buffer at location x,y.  Every pixel
\ of the image is drawn, so no transparent areas are possible.

: PutImage  ( *dst _x _y _w _h *src -- ) 0 <Draw-Image> ;

\     -----------
\ ---[ PutSprite ]---------------------------------------------------
\     -----------
\ Draw a bitmap image into the buffer at location x,y.
\ Only non-zero pixels are drawn, allowing transparency to the
\ background images which may be present.

: PutSprite  ( *dst _x _y _w _h *src -- ) 1 <Draw-Image> ;

\ Need to add GetSprite (synonym for GetImage, probably)

\ ---[Note]----------------------------------------------------------
\ PutImage and PutSprite will only work correctly with the 8-bit mode
\ of any display resolution.  This is because I have not set the
\ code up to handle color depths larger than one byte.
\ This will need to be addressed.
\ ------------------------------------------------------[End Note]---

\ ---[ Splash Screen ]-----------------------------------------------

%sdl-quiet 0= [if]

marker [SPLASH]

: Splash-Screen ( -- )
  640x480x32 s" SDL Library v1.0f0" InitGraph  \ create display window
  s" sdllib-10f.png0" Terminate-String sdl-loadimage    \ load image
  dup 0= s" Unable to load image file" Error-End     \ error check it
  >R                                           \ save surface pointer
  R@ NULL screen-surface NULL sdl-blitsurface drop   \ blit to screen
  screen-surface sdl-flip drop     \ transfer image to display screen
  2000 sdl-delay                                     \ 2 second delay
  R> sdl-freesurface                   \ free allocated image surface
  CloseGraph                  \ shut down the SDL subsystems and exit
;

Splash-Screen

[SPLASH]

SDLLib-Version
.( SDL Library loaded successfully.) cr
[then]
