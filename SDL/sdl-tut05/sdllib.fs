\ ===[ Code Addendum 01 ]============================================
\             gforth: SDL/OpenGL Graphics Part V
\ ===================================================================
\      Program: sdllib.fs
\      Version: 1.0b
\       Author: Timothy Trussell
\         Date: 02/26/2010
\  Description: SDL Library Routines for gforth
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-19
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                   SDL v1.0b Library for gforth
\ ===================================================================

\ ---[ SDL Primitives ]----------------------------------------------
\ Basic primitive functions library

\     -------------------
\ ---[ Prototype Listing ]-------------------------------------------
\     -------------------

\ : SDLLib-Version              ( -- )

\ : Error-End                   ( f addr n -- )
\ : Terminate-String            ( a n -- a )

\ : Set-Mode-Parms              ( mode -- )
\ : InitGraph                   ( mode a n -- )
\ : CloseGraph                  ( -- )

\ : <PlotPixel>                 { *ofs _pixel _bytepp -- }
\ : PutPixel                    { *dst _x _y _pixel -- }
\ : Line                        { *dst _x1 _y1 _x2 _y2 _pixel -- }
\ : VLine                       { *dst _x _y1 _y2 _pixel -- }
\ : HLine                       { *dst _x1 _x2 _y _pixel -- }
\ : <Draw-Image>                { *dst _x _y _w _h *src tflag -- }
\ : PutImage                    ( *dst _x _y _w _h *src -- )
\ : PutSprite                   ( *dst _x _y _w _h *src -- )

\ : Splash-Screen               ( -- )
\ ----------------------------------------------[ End Prototypes ]---

\ Load the SDL C Library Interface

require sdl-1.0b.fs

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

1   value mouse-cursor-display

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
[vmodes] 320x200x8
[vmodes] 640x480x8
[vmodes] 800x600x8
[vmodes] 1024x768x8
[vmodes] 1280x1024x8
[vmodes] 1600x12004x8

[vmodes] 320x200x16
[vmodes] 640x480x16
[vmodes] 800x600x16
[vmodes] 1024x768x16
[vmodes] 1280x1024x16
[vmodes] 1600x12004x16

[vmodes] 320x200x24
[vmodes] 640x480x24
[vmodes] 800x600x24
[vmodes] 1024x768x24
[vmodes] 1280x1024x24
[vmodes] 1600x12004x24

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

\     ----------------
\ ---[ SDLLib-Version ]----------------------------------------------
\     ----------------

: SDLLib-Version ( -- )
  cr ." +-------------------------------------------------+"
  cr ." |   SDLLib v1.0b 02/14/2010 by Timothy Trussell   |"
  cr ." +-------------------------------------------------+"
  cr
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
      320x200x8    of 320  200  8  endof
      640x480x8    of 640  480  8  endof
      800x600x8    of 800  600  8  endof
      1024x768x8   of 1024 768  8  endof
      1280x1024x8  of 1280 1024 8  endof
\      1600x1200x8  of 1600 1200 8  endof
      320x200x16   of 320  200  16 endof
      640x480x16   of 640  480  16 endof
      800x600x16   of 800  600  16 endof
      1024x768x16  of 1024 768  16 endof
      1280x1024x16 of 1280 1024 16 endof
\      1600x1200x8  of 1600 1200 16 endof
      320x200x24   of 320  200  24 endof
      640x480x24   of 640  480  24 endof
      800x600x24   of 800  600  24 endof
      1024x768x24  of 1024 768  24 endof
      1280x1024x24 of 1280 1024 24 endof
\      1600x1200x8  of 1600 1200 24 endof
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
  SDL_INIT_VIDEO sdl-init               \ initialize SDL video system
  0<> s" Unable to initialize SDL" Error-End      \ check return code

  window-screenw                                  \ create the window
  window-screenh
  window-screenbpp
  window-fullscreen if SDL_FULLSCREEN else 0 then       \ fullscreen?
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
\               ofs = *dst+(y*swidth)+(x*bytepp)
\ where
\               *dst   - pointer to destination surface
\               x,y    - coordinates to plot the pixel to
\               swidth - <pitch> value of the screen line, defined as
\                        window-screenw * bytes-per-pixel
\               bytepp - # of bytes per pixel for this color depth
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

: VLine { *dst _x _y1 _y2 _pixel -- }
  \ Lock the surface if required
  *dst sdl-mustlock if
    *dst sdl-locksurface drop
  then
  \ Calculate the target address of the pixel
  *dst sdl-surface-pixels @     \ *pixels              32-bit pointer
  *dst sdl-surface-pitch sw@    \ *pixels swidth         16-bit value
  _y1 * +                       \ *pixels[swidth*y]
  *dst sdl-surface-format @     \ *pixels[swidth*y] *format
  sdl-pixelformat-bytesperpixel \ *pixels[swidth*y] *format->bytepp
  C@                            \ *pixels[swidth*y] bytepp
  dup to %vh-bpp                \ *pixels[swidth*y] bytepp
  _x * +                        \ *pixels[ofs]
  _y2 _y1 do                    \ *pixels[ofs]
    dup                         \ *ofs *ofs
    _pixel                      \ *ofs *ofs pixel
    %vh-bpp                     \ *ofs *ofs pixel bytepp
    <PlotPixel>                 \ *ofs
    *dst sdl-surface-pitch sw@ + \ *ofs+<pitch>      inc to next line
  loop
  drop
  \ Now unlock the surface if required
  *dst sdl-mustlock if
    *dst sdl-unlocksurface
  then
;

\ The old version of VLine that I am replacing with one that will do
\ all the pixels with the surface being locked/unlocked only once.

: VLine1 { *dst _x _y1 _y2 _pixel -- }
  _y2 _y1 - 0 do
    *dst _x _y1 i + _pixel PutPixel
  loop
;

\ ---[ HLine ]-------------------------------------------------------
\ Draws a horizontal line to the dst surface

: HLine { *dst _x1 _x2 _y _pixel -- }
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
  dup to %vh-bpp                \ *pixels[swidth*y] bytepp
  _x1 * +                       \ *pixels[ofs]
  _x2 _x1 do                    \ *pixels[ofs]
    dup                         \ *ofs *ofs
    _pixel                      \ *ofs *ofs pixel
    %vh-bpp                     \ *ofs *ofs pixel bytepp
    <PlotPixel>                 \ *ofs
    %vh-bpp +                   \ *ofs+<pitch>      inc to next pixel
  loop
  drop
  \ Now unlock the surface if required
  *dst sdl-mustlock if
    *dst sdl-unlocksurface
  then
;

\ The old version of HLine that I am replacing with one that will do
\ all the pixels with the surface being locked/unlocked only once.

: HLine1 { *dst _x1 _x2 _y _pixel -- }
  _x2 _x1 - 0 do
    *dst _x1 i + _y _pixel PutPixel
  loop
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
\ What is wrong with a little ego boosting, right?
\ Set to 0 to bypass the awe inspiring display.
\ Initially, it will tell you that the basic SDL functions work.

1 [if]

marker [SPLASH]

: Splash-Screen ( -- )
  cr ." Splashing the screen"
  640x480x32 s" SDL Library v1.0b" InitGraph  \ create display window
  s" sdllib.bmp0" Terminate-String sdl-loadbmp           \ load image
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

[then]
SDLLib-Version
.( SDL Library loaded successfully.) cr

