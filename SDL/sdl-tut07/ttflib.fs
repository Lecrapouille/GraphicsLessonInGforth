\ ===[ Code Addendum 02 ]============================================
\             gforth: SDL/OpenGL Graphics Part VII
\ ===================================================================
\    File Name: ttflib.fs
\      Version: 1.00
\       Author: Timothy Trussell
\         Date: 03/24/2010
\  Description: TTF Library Routines for gforth
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-19
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                  SDL_ttf v1.00 Library for gforth
\ ===================================================================

\ ---[ Changes Log ]-------------------------------------------------
\
\ 03/24/2010    Initial Version                              rev 1.00

\     -------------------
\ ---[ Prototype Listing ]-------------------------------------------
\     -------------------
\
\ : TTFLib-Version    ( -- )
\ : Load-TTF           ( *fname flen ptsize -- *ttf )
\ : Apply-TTF-Surface  ( x y *src *dst -- )
\ : FRender-Text       { *str _len *fname _flen _size *c -- }
\ : FRender-Close      ( -- )
\ : FWriteAt        { *str _len *fname _flen _size _x _y *c *dst -- }
\ : FWriteCenter       { *str _len *fname _flen _size _y _c *dst -- }
\
\ ----------------------------------------------[ End Prototypes ]---

\ Load the SDL_ttf C Library Interface

require sdl-ttf-1.00.fs

\     ----------------
\ ---[ SDLLib-Version ]----------------------------------------------
\     ----------------

: TTFLib-Version ( -- )
  ." +-------------------------------------------------+" cr
  ." |   TTFLib v1.00 03/27/2010 by Timothy Trussell   |" cr
  ." +-------------------------------------------------+" cr
;

\ ---[ Load-TTF ]----------------------------------------------------
\ Passed the name/location of the font to load, followed by the point
\ size to load the font as - basically the pixel height of the font.

: Load-TTF ( *fname flen ptsize -- *ttf )
  >R Terminate-String R> ttf-openfont dup 
  0= s" Unable to load TTF" Error-End
;

\ ---[ Apply-TTF-Surface ]-------------------------------------------
\ Copies the src surface to the dst surface, placed at x,y. The full
\ rectangular dimension of the src image is copied to the dst image.
\ Note that this does NOT make the images visible in the display
\ window.  sdl-flip does that function.

create offset% sdl-rect% %allot drop

: Apply-TTF-Surface ( x y *src *dst -- )
  >R >R
  offset% sdl-offset-y w!                  \ set offsets to rectangle
  offset% sdl-offset-x w!
  0 offset% sdl-offset-w w!                         \ just to be safe
  0 offset% sdl-offset-h w!
  R> NULL R> offset% sdl-blitsurface drop           \ blit src to dst
;

0 value work-font
0 value work-surface

: FRender-Text { *str _len *fname _flen _size *c -- }
  *fname _flen _size Load-TTF to work-font             \ load the ttf
  work-font                                         \ render the text
  *str _len Terminate-String
  *c
  ttf-rendertext-solid dup 0= s" Error Rendering Text" Error-End
  to work-surface
;

: FRender-Close ( -- )
  work-surface sdl-freesurface 0 to work-surface \ free work surfaces
  work-font ttf-closefont 0 to work-font
;

\ ---[ FWriteAt ]----------------------------------------------------
\ This is one way to display the rendered text.
\ For each line of text we load a font into a surface, which is 
\ converted to the correct pixel height during the load process.
\ Next, we Render the text, which creates a second surface.
\ This surface is the exact height/width of the rendered text.
\ The data is then copied to the *dst surface.
\ Finally, the surface the font was loaded into, as well as the
\ surface the text was rendered to, are freed, releasing the memory.
\ -------------------------------------------------------------------
\ Note:
\ The string to be rendered MUST have an extra character, which will
\ be converted to a 0 byte value.
\ -------------------------------------------------------------------
\ Parameters:
\
\       *str _len               the text to be rendered
\       *fname _flen            path/name of the font to use
\       _size                   pixel height of the font
\       _x, _y                  address to copy the rendered text to
\       *c                      color struct
\       *dst                    destination surface to copy to
\
\ Examples:
\
\       s" My text line0" s" ttf-fonts/times.ttf0"
\       28 0 0 TextColor[] screen-surface FWriteAt
\
\  s" this is a really long line of text that says nothing at all0"
\  TTFont4 28 0 100 screen-surface FWriteAt
\ -------------------------------------------------------------------
       
: FWriteAt { *str _len *fname _flen _size _x _y *c *dst -- }
  *str _len *fname _flen _size *c FRender-Text
  _x _y work-surface *dst Apply-TTF-Surface    \ copy to dest surface
  FRender-Close                                  \ free work surfaces
;

\ ---[ FWriteCenter ]------------------------------------------------
\ Displays the rendered text, centered in the surface, at the y coord
\ specified.  Note that the y coord is the top line of the rendered
\ text being displayed.

: FWriteCenter { *str _len *fname _flen _size _y _c *dst -- }
  *str _len *fname _flen _size _c FRender-Text
  window-screenw 2/ work-surface sdl-surface-w uw@ 2/ -
  _y work-surface *dst Apply-TTF-Surface
  FRender-Close                                  \ free work surfaces
;

%sdl-quiet 0= [if]
TTFLib-Version
.( TTF Library loaded successfully.) cr
[then]

