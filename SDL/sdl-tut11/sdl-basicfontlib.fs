\ ===[ Code Addendum 06 ]============================================
\                 gforth: SDL/OpenGL Graphics Part XI
\ ===================================================================
\    File Name: sdl-basicfontlib.fs
\      Version: 1.03
\       Author: Timothy Trussell
\         Date: 05/09/2010
\  Description: A basic SDL font package, with one 8x16 font type
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-21
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================

\ ---[ Changes Log ]-------------------------------------------------
\ 03/28/2010    Posted as part VIII of the gforth/SDL        rev 1.01
\               column series
\
\ 04/03/2010    Changed the Draw-Font-Char function to       rev 1.02
\               use an SDL surface as a buffer to draw the
\               current character to, which is then blitted
\               to the display surface.  Greatly speeded up
\               the drawing of characters.
\
\ 05/02/2010    Moved the font-set-color function to the     rev 1.03
\               sdllib.fs file, renamed to Set-Color. It is
\               now called from InitGraph.
\

\ ---[ Function Prototypes ]-----------------------------------------

\ : font-SetDestSurface ( *dst -- )
\ : font-getsurface     ( -- *dst )
\ : GetX                ( -- cx )
\ : GetY                ( -- cy )
\ : GetXY               ( -- cx cy )
\ : GotoXY              ( x y -- )
\ : \r                  ( -- )
\ : \n                  ( -- )
\ : Draw-Font-Char      { *dst _x _y *src -- }
\ : Update-Cursor-Position ( -- )
\ : GPutC               { _c -- }
\ : GPutS               { *str _len -- }
\ : GWriteC             { _x _y _fg _bg _c -- }
\ : GWriteS             { _x _y _fg _bg *str _len -- }
\ : GWriteAt            ( x y fg bg &str len -- )
\ : GWriteCenter        { _y _fg _bg &str _len -- }
\ : GClearLine          { _y _fg _bg -- }
\ : GBox                { _x1 _y1 _x2 _y2 _fg _bg _type -- }
\ : GFBox               { _x1 _y1 _x2 _y2 _fg _bg _type -- }
\ : font-init           ( *dst -- )

\ ------------------------------------------------[End Prototypes]---

require sdllib.fs

\ This loads in the font data, creating the Font8x16[] array

require font8x16.fs

\ ===[ Globals ]=====================================================

0 value font-cursor-x                     \ current cursor x position
0 value font-cursor-y                     \ current cursor y position
0 value font-cursor-max-x         \ max columns in current resolution
0 value font-cursor-max-y            \ max rows in current resolution

0 value font-color                 \ color to use for text foreground
0 value font-background-color      \ color to use for text background

\ ---[ font-transparency ]---
\ Use to select drawing method to be used by GPutC.
\ Set to 1 (default) and only non-zero pixels will be drawn.
\ Set to 0 and the color specified by <font-background-color> will be
\ used for the zero pixels in the image

0 value font-transparency

\ ---[ font-surface ]---
\ If this value is detected to be NULL, the code will default to use
\ screen-surface which is defined by InitGraph.
\
\ It should be noted that it is safe to intentionally leave the
\ font-surface pointer set to NULL, so as to use the default
\ screen-surface destination pointer. Good programming practice
\ dictates that font-surface has to be initialized, even if it is to
\ the default screen-surface pointer.

0 value font-surface                \ destination surface to draw to

\ ---[ Variables for the Draw-Font-Char function ]-------------------
\ An 8x16x(current bpp) surface to render the character to.

0 value font-buffer                 \ surface to render characters to
0 value font-buf^                     \ stream pointer to font-buffer
0 value font-bpp                      \ current bytes-per-pixel value

\ ---[ box% ]--------------------------------------------------------
\ This is a struct of type sdl-rect%, used to copy partial surface
\ data from the source surface to the dest surface.

create box% sdl-rect% %allot drop

\ ===[ Functions ]===================================================

\ ---[ font-SetDestSurface ]-----------------------------------------
\ This sets the variable font-surface to the screen surface which the
\ programmer wants the text to be drawn to.
\ If font-surface==NULL then it will default to using screen-surface.
\ The library functions use this to tell them where to put data to.

: font-SetDestSurface  ( *dst -- ) to font-surface ;

\ ---[ font-getsurface ]---------------------------------------------
\ This checks font-surface to see if it is set to NULL (0). If not,
\ it will return the current value - otherwise, it will return the
\ default screen-surface value defined by InitGraph.
\ A final check is to see if screen-surface is set to NULL (0).
\ If it is, then InitGraph has not been called, so exit/abort out.

: font-getsurface ( -- *dst )
  font-surface 0<> if                         \ if font-surface!=NULL
    font-surface              \ use the specified destination surface
  else                                                    \ otherwise
    screen-surface                  \ default to using screen-surface
    dup 0= s" SDL must be initialized" Error-End   \ exit if not "on"
  then
;

\ ---[ GetX/Y ]------------------------------------------------------
\ Returns the current cursor position(s).

: GetX  ( -- cx )    font-cursor-x ;
: GetY  ( -- cy )    font-cursor-y ;
: GetXY ( -- cx cy ) GetX GetY ;

\ ---[ GotoXY ]------------------------------------------------------
\ Sets the cursor positions.

: GotoXY ( x y -- )
  to font-cursor-y
  to font-cursor-x
;

\ ---[ \r ]----------------------------------------------------------
\ Carriage Return: Returns the x cursor position to 0.
\ Does NOT increment the y cursor position.

: \r ( -- ) 0 to font-cursor-x ;

\ ---[ \n ]----------------------------------------------------------
\ New Line: increments the y cursor position to the next line.
\ Does NOT reset the x cursor position to zero.
\ If we move past the bottom line of the surface, which is set by
\ the variable <font-cursor-max-y>, then wrap to the top of the
\ surface by resetting <font-cursor-y> to zero.

: \n ( -- )
  font-cursor-y 1+
  dup font-cursor-max-y > if drop 0 then to font-cursor-y
;

\ ---[ Draw-Font-Char ]----------------------------------------------
\ Draws the 8x16 font image to the *dst surface at cx,cy
\ Non-zero pixels are plotted in <font-color>
\ If the transparency flag is off, zero pixels are plotted in 
\ <font-background-color>
\ The fonts are stored in a packed-pixel format, and therefore have
\ to be decoded prior to plotting to the display surface.

0 [if]

\ This is the original code to plot a character to the dest surface

: Draw-Font-Char { *dst _cx _cy *src -- }
  _cx 8 * to _cx                          \ scale x coord for surface
  _cy 16 * to _cy                         \ scale y coord for surface
  16 0 do
    8 0 do
      *src C@
      128 i RSHIFT AND if                   \ is it a non-zero pixel?
        *dst _cx i + _cy j + font-color PutPixel       \ yes, plot it
      else                                                       \ no
        font-transparency 0= if   \ if transparency off, plot 0 pixel
          *dst _cx i + _cy j + font-background-color PutPixel
        then
      then
    loop
    *src 1+ to *src                 \ increment to next byte in image
  loop
;

[else]

\ This is the modified version, using a temp surface, which is then
\ blitted to the dest surface.

\ ---[ Copy-Font-Buffer ]--------------------------------------------
\ Copies the src surface to the dst surface, placed at x,y.

: Copy-Font-Buffer ( *src *dst x y -- )
  box% sdl-offset-y w!                     \ set offsets to rectangle
  box% sdl-offset-x w!
  0 box% sdl-offset-w w!                            \ just to be safe
  0 box% sdl-offset-h w!
  NULL swap box% sdl-blitsurface drop     \ blit the surface to video
;

: Draw-Font-Char { *dst _cx _cy *src -- }
  font-buffer NULL 0 sdl-fillrect drop      \ erase the render buffer
  font-buffer sdl-surface-pixels @ to font-buf^   \ ptr to image data
  
  _cx 8 * to _cx                          \ scale x coord for surface
  _cy 16 * to _cy                         \ scale y coord for surface

  16 0 do
    8 0 do
      *src C@
      128 i RSHIFT AND if                   \ is it a non-zero pixel?
        font-buf^ font-color font-bpp <PlotPixel>
      else                                                       \ no
        font-transparency 0= if   \ if transparency off, plot 0 pixel
          font-buf^ font-background-color font-bpp <PlotPixel>
        then
      then
      font-buf^ font-bpp + to font-buf^    \ increment render pointer
    loop
    *src 1+ to *src                 \ increment to next byte in image
  loop
  font-buffer *dst _cx _cy Copy-Font-Buffer      \ blit render buffer
;

[then]

\ ---[ Update-Cursor-Position ]--------------------------------------
\ Check to see if position has gone past limits for both cx and cy.
\ If cx reaches max-x, reset cx to 0 and increment cy.
\ If cy reaches max-y, reset cy to 0 (top of screen)
\ I do not plan on implementing screen scrolling in this version.

: Update-Cursor-Position ( -- )
  GetX 1+ to font-cursor-x
  GetX font-cursor-max-x > if
    0 to font-cursor-x
    GetY 1+ to font-cursor-y
    GetY font-cursor-max-y > if
      0 to font-cursor-y
    then
  then
;

\ ---[ GPutC ]-------------------------------------------------------
\ Plots the character <c> to the screen at the current cursor coords
\ in the color specified by by the variable <font-color>
\
\ If the font-transparency flag is set to 0, then 0 pixels will be
\ plotted using the font-background-color value.
\
\ If the font-transparency flag is set to 1, then it will skip the
\ zero pixel positions in the character being drawn.
\
\ The cursor position will be advanced after each character has been
\ plotted to the surface.
\
\ There is no 'special casing' for this font, as all of the control
\ characters have also been allocated graphics images.

: GPutC { _c -- }
  \ Draw the character to the surface
  font-getsurface GetXY Font8x16[] _c 16 * + Draw-Font-Char
  Update-Cursor-Position
;

\ ---[ GPutS ]-------------------------------------------------------
\ Displays a stream of data at the current cx,cy coordinates.
\ cx and cy are incremented as per the settings of the currently
\ active font object.
\
\ Example:
\      s" Basic screen display string" GPutS

: GPutS { *str _len -- }
  _len 0 do
    *str i + C@ GPutC
  loop
;

\ ---[ GWriteC ]-----------------------------------------------------
\ Display a character at the given x,y screen coordinates with the
\ specified foreground and background colors
\
\ Example:
\       1 5 #White #Blue 65 GWriteC
\
\ Displays the letter 'A' in white on a blue background at (1,5)

: GWriteC { _x _y _fg _bg _c -- }
  0 to font-transparency
  _x _y GotoXY
  _fg to font-color
  _bg to font-background-color
  _c GPutC
;

\ ---[ GWriteS/GWriteAt ]--------------------------------------------
\ Display a string at the given x,y screen coordinates with the
\ specified foreground and background colors
\
\ Example:
\     1 5 #White #Blue " Basic screen display string" GWriteS

: GWriteS { _x _y _fg _bg *str _len -- }
  0 to font-transparency
  _x _y GotoXY
  _fg to font-color
  _bg to font-background-color
  *str _len GPutS
;

\ ---[ GWriteAt ]----------------------------------------------------
\ Alias for GWriteS

: GWriteAt ( x y fg bg &str len -- ) GWriteS ;

\ ---[ GWriteCenter ]------------------------------------------------
\ Display a string in the center of the specified row

: GWriteCenter { _y _fg _bg &str _len -- }
  font-cursor-max-x 2/ _len 2/ - _y _fg _bg &str _len GWriteAt
;

\ ---[ GClearLine ]--------------------------------------------------
\ Clears the row with the specified fg/bg colors

: GClearLine { _y _fg _bg -- }
  0 to font-transparency
  _fg to font-color
  _bg to font-background-color
  0 _y GotoXY
  font-cursor-max-x 1+ 0 do
    32 GPutC
  loop
;

\ Define the window frame structure to access the array with

struct
  1 1 field wframe-top-left
  1 1 field wframe-top-center
  1 1 field wframe-top-right
  1 1 field wframe-mid-left
  1 1 field wframe-mid-center
  1 1 field wframe-mid-right
  1 1 field wframe-bot-left
  1 1 field wframe-bot-center
  1 1 field wframe-bot-right
end-struct wframe

\     --------
\ ---[ WFrame ]------------------------------------------------------
\     --------
\ Window frame characters table
\
\ Type 0: no frame (space characters)
\      1: single line box
\      2: double line box
\      3: double top/bot, single sides
\      4: single top/left, double right/bot
\      5: double top/left, single right/bot
\      6: double top/right, single left/bot
\      7: single top/right, double left/bot
\      8: solid blocks
\      9: solid sides, half top/bot
\     10: half solids
\     11: light shaded pattern
\     12: medium shaded pattern
\     13: dark shaded pattern
\     14: background frame box - single line
\     15: background frame box - double line

create wframe[]
\ TL    TM     TR     ML     MM     MR     BL     BM     BR
 32 C,  32 C,  32 C,  32 C,  32 C,  32 C,  32 C,  32 C,  32 C, \ 0
218 C, 196 C, 191 C, 179 C,  32 C, 179 C, 192 C, 196 C, 217 C, \ 1
201 C, 205 C, 187 C, 186 C,  32 C, 186 C, 200 C, 205 C, 188 C, \ 2
213 C, 205 C, 184 C, 179 C,  32 C, 179 C, 212 C, 205 C, 190 C, \ 3
218 C, 196 C, 183 C, 179 C,  32 C, 186 C, 212 C, 205 C, 188 C, \ 4
201 C, 205 C, 184 C, 186 C,  32 C, 179 C, 211 C, 196 C, 217 C, \ 5
213 C, 205 C, 187 C, 179 C,  32 C, 186 C, 192 C, 196 C, 189 C, \ 6
214 C, 196 C, 191 C, 186 C,  32 C, 179 C, 200 C, 205 C, 190 C, \ 7
219 C, 219 C, 219 C, 219 C,  32 C, 219 C, 219 C, 219 C, 219 C, \ 8
219 C, 223 C, 219 C, 219 C,  32 C, 219 C, 219 C, 220 C, 219 C, \ 9
222 C, 223 C, 221 C, 222 C,  32 C, 221 C, 222 C, 220 C, 222 C, \ 10
178 C, 178 C, 178 C, 178 C,  32 C, 178 C, 178 C, 178 C, 178 C, \ 11
177 C, 177 C, 177 C, 177 C,  32 C, 177 C, 177 C, 177 C, 177 C, \ 12
176 C, 176 C, 176 C, 176 C,  32 C, 176 C, 176 C, 176 C, 176 C, \ 13
218 C, 194 C, 191 C, 195 C, 197 C, 180 C, 192 C, 193 C, 217 C, \ 14
201 C, 203 C, 187 C, 204 C, 206 C, 185 C, 200 C, 202 C, 188 C, \ 15

: wframe-ndx 9 * wframe[] + ;

: wf-tl ( type -- c ) wframe-ndx wframe-top-left   C@ ;
: wf-tc ( type -- c ) wframe-ndx wframe-top-center C@ ;
: wf-tr ( type -- c ) wframe-ndx wframe-top-right  C@ ;
: wf-ml ( type -- c ) wframe-ndx wframe-mid-left   C@ ;
: wf-mc ( type -- c ) wframe-ndx wframe-mid-center C@ ;
: wf-mr ( type -- c ) wframe-ndx wframe-mid-right  C@ ;
: wf-bl ( type -- c ) wframe-ndx wframe-bot-left   C@ ;
: wf-bc ( type -- c ) wframe-ndx wframe-bot-center C@ ;
: wf-br ( type -- c ) wframe-ndx wframe-bot-right  C@ ;

\ ---[ GBox ]--------------------------------------------------------
\ Draws a text box frame to the surface

: GBox { _x1 _y1 _x2 _y2 _fg _bg _type -- }
  0 to font-transparency
  _x1 _y1 _fg _bg _type wf-tl GWriteC               \ top/left corner
  _x2 _y1 _fg _bg _type wf-tr GWriteC              \ top/right corner
  _x1 _y2 _fg _bg _type wf-bl GWriteC               \ bot/left corner
  _x2 _y2 _fg _bg _type wf-br GWriteC              \ bot/right corner
  _x2 _x1 - 1- 0 do                            \ top/bot lines of box
    _x1 1+ i + _y1 _fg _bg _type wf-tc GWriteC
    _x1 1+ i + _y2 _fg _bg _type wf-bc GWriteC
  loop
  _y2 _y1 1+ do                             \ left/right sides of box
    _x1 i _fg _bg _type wf-ml GWriteC
    _x2 i _fg _bg _type wf-mr GWriteC
  loop
;

\ ---[ GFBox ]--------------------------------------------------------
\ Draws a filled text box to the surface
\ The first 14 frame styles all use the space character as the center
\ fill of the box, so we use the sdl-fillrect function for these
\ frame styles...

: GFBox { _x1 _y1 _x2 _y2 _fg _bg _type -- }
  _x1 _y1 _x2 _y2 _fg _bg _type GBox
  _type 14 < if
    _x1 1+ 8 * box% sdl-offset-x w!
    _y1 1+ 16 * box% sdl-offset-y w!
    _x2 _x1 - 1- 8 * box% sdl-offset-w w!
    _y2 _y1 - 1- 16 * box% sdl-offset-h w!
    font-getsurface box% _bg sdl-fillrect drop
  else
    _y2 _y1 1+ do
      _x2 _x1 1+ do
        i j _fg _bg _type wf-mc GWriteC
      loop
    loop
  then
;

\ ===[ Initialization Functions ]====================================

\ ---[ font-quit ]---------------------------------------------------
\ Should be called just prior to CloseGraph, as it has to release an
\ SDL surface which we allocated in font-init.

: font-quit ( -- )
  font-buffer sdl-freesurface
  0 to font-buffer
;

\ ---[ Copy-Surface-Palette ]----------------------------------------
\ The palettes for font-buffer and screen-surface do not match, so we
\ have to copy the contents of the screen-surface palette to the
\ font-buffer surface.  For some reason, sdl-blitsurface blits only
\ zeroes to the destination surface, no matter what the original
\ pixel data in the font-buffer is.  Once we have set the palette
\ data in font-buffer, everything works correctly. And faster!

\ -- An SDL palette entry has four elements, not three: r/g/b/a --

\ -- This is executed from font-init at the program start --

: Copy-Surface-Palette ( -- )
  \ Load the surface palette data we want to copy from screen-surface
  font-getsurface sdl-surface-format @
  sdl-pixelformat-palette @
  \ font-surface->pixelformat->palette->ncolors
  dup sdl-palette-ncolors @ >R          \ number of colors in palette
  \ font-surface->pixelformat->palette->colors
  sdl-palette-colors @                      \ pointer to palette data
  here R@ 4 * cmove                      \ copy the palette to <here>

  font-buffer                 \ destination surface of palette to set
  SDL_LOGPAL SDL_PHYSPAL OR                            \ flags to use
  here                                      \ address of palette data
  0                                              \ first color to set
  R>                  \ number of colors to set - from screen-surface
  sdl-setpalette drop
;

\ ---[ font-init ]---------------------------------------------------
\ Initializes the parameters required for font package operation.
\ InitGraph *must* have been called prior to this, as a valid
\ surface must be passed as a parameter, so the font functions have
\ a valid surface to plot data to.
\ The <in-graphics-mode> flag, set by InitGraph, is checked for this.

: font-init ( *dst -- )
  in-graphics-mode if
    font-SetDestSurface
    \ Initialize the font-cursor-max-x/y variables
    window-screenw 8 / 1- to font-cursor-max-x
    window-screenh 16 / 1- to font-cursor-max-y
    \ Set font-bpp to screen-surface bpp value (1,2,3 or 4)
    \ Need this for the Draw-Font-Char function
    font-getsurface sdl-surface-format @
    sdl-pixelformat-bytesperpixel C@
    to font-bpp
    \ Set the font-transparency flag off
    0 to font-transparency
    \ Set initial text/background colors
    #White to font-color
    #Black to font-background-color
    \ Create an 8x16xfont-bpp(*8) surface for the font render buffer
    \ (*8 because font-bpp is in the range [1..4])
    SDL_SWSURFACE 8 16 font-bpp 8 * 0 0 0 0 sdl-creatergbsurface 
    dup NULL = s" Unable to create Font Render Buffer" Error-End
    to font-buffer
    \ copy the screen-surface palette if in 8-bpp resolution
    font-bpp 1 = if
      Copy-Surface-Palette
    then
  else
    1 s" Font-Init Error: Graphics mode not initialized" Error-End
  then
;

