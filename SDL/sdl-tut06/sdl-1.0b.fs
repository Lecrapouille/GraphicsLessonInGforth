\ ===[ Code Addendum 04 ]============================================
\             gforth: SDL/OpenGL Graphics Part VI
\ ===================================================================
\    File Name: sdl-1.0b.fs
\       Author: Timothy Trussell
\         Date: 02/26/2010
\  Description: SDL Interface for gforth
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-19
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                   SDL v1.0b Library for gforth
\ ===================================================================

\ ---[Note]----------------------------------------------------------
\ An 'f' as a return value indicates a flag - usually boolean.
\ 0 denote success, -1 denotes failure -- *usually*
\ Reference the SDL_video.h include file in the C++ source
\ Probably at: /usr/include/SDL/SDL_video.h (if you installed SDL)
\ ------------------------------------------------------[End Note]---

\ ---[ gforth Prototypes ]-------------------------------------------
\ sdl-init              ( flags -- error code )
\ sdl-set-video-mode    ( width height bpp flags -- *surface )
\ sdl-updaterect        ( *surface x y w h -- )
\ sdl-flip              ( *surface -- f )
\ sdl-setpalette        ( *surface flags *colors first num -- f )
\ sdl-quit              ( -- )
\ sdl-loadbmp           ( *filename -- *surface )
\ sdl-freesurface       ( *surface -- )
\ sdl-blitsurface       ( *src *srcrect *dst *dstref -- f )
\ sdl-fillrect          ( *dst *dstrect color -- f )
\ sdl-delay             ( uint32_ms -- )
\ sdl-show-cursor       ( toggle -- status )
\ sdl-wm-set-caption    ( *title *icon -- )
\ sdl-mustlock          ( *surface -- f )
\ sdl-creatergbsurface  ( flags w h bpp Rm Gm Bm Am -- *surface )
\ sdl-locksurface       ( *surface -- f )
\ sdl-unlocksurface     ( *surface -- )
\ sdl-setcolorkey       ( *surface flag key -- f )
\ sdl-pollevent         ( *event -- f )
\
\ : sdl-mustlock        { *src -- f }
\ ------------------------------------------------[End Prototypes]---

c-library sdl_lib
s" SDL" add-lib

\c #include <SDL/SDL.h>

c-function sdl-init		SDL_Init		n -- n
c-function sdl-set-video-mode	SDL_SetVideoMode	n n n n -- a
c-function sdl-updaterect       SDL_UpdateRect          a n n n n -- void
c-function sdl-flip             SDL_Flip                a -- n
c-function sdl-setpalette       SDL_SetPalette          a n a n n -- n
c-function sdl-quit		SDL_Quit		-- void
c-function sdl-loadbmp          SDL_LoadBMP             a -- a
c-function sdl-freesurface      SDL_FreeSurface         a -- void
c-function sdl-blitsurface      SDL_BlitSurface         a a a a -- n
c-function sdl-fillrect         SDL_FillRect            a a n -- n
c-function sdl-delay            SDL_Delay               n -- void

c-function sdl-show-cursor	SDL_ShowCursor		n -- n

c-function sdl-wm-set-caption	SDL_WM_SetCaption	a a -- void

c-function sdl-creatergbsurface SDL_CreateRGBSurface    n n n n n n n n -- a

c-function sdl-locksurface      SDL_LockSurface         a -- n
c-function sdl-unlocksurface    SDL_UnlockSurface       a -- void
c-function sdl-setcolorkey      SDL_SetColorKey         a n n -- n
c-function sdl-pollevent        SDL_PollEvent           a -- n

end-c-library

\ ---[ Structure Definitions ]---------------------------------------

4 4 2constant int%
2 2 2constant word%
1 1 2constant byte%
cell% 2constant ptr%

struct
  char% field sdl-keysym-scancode
  int% field sdl-keysym-sym
  int% field sdl-keysym-mod
  word% field sdl-keysym-unicode
end-struct sdl-keysym%

struct
  byte% field sdl-active-event-type
  byte% field sdl-active-event-gain
  byte% field sdl-active-event-state
end-struct sdl-active-event%

struct
  byte% field sdl-keyboard-event-type
  byte% field sdl-keyboard-event-which
  byte% field sdl-keyboard-event-state
  sdl-keysym% field sdl-keyboard-event-keysym
end-struct sdl-keyboard-event%

struct
  byte% field sdl-mouse-motion-event-type
  byte% field sdl-mouse-motion-event-which
  byte% field sdl-mouse-motion-event-state
  word% field sdl-mouse-motion-event-x
  word% field sdl-mouse-motion-event-y
  word% field sdl-mouse-motion-event-xrel
  word% field sdl-mouse-motion-event-yrel
end-struct sdl-mouse-motion-event%

struct
  byte% field sdl-mouse-button-event-type
  byte% field sdl-mouse-button-event-which
  byte% field sdl-mouse-button-event-button
  byte% field sdl-mouse-button-event-state
  word% field sdl-mouse-button-event-x
  word% field sdl-mouse-button-event-y
end-struct sdl-mouse-button-event%

struct
  byte% field sdl-joy-axis-event-type
  byte% field sdl-joy-axis-event-which
  byte% field sdl-joy-axis-event-axis
  word% field sdl-joy-axis-event-value
end-struct sdl-joy-axis-event%

struct
  byte% field sdl-joy-ball-event-type
  byte% field sdl-joy-ball-event-which
  byte% field sdl-joy-ball-event-ball
  word% field sdl-joy-ball-event-xrel
  word% field sdl-joy-ball-event-yrel
end-struct sdl-joy-ball-event%

struct
  byte% field sdl-joy-hat-event-type
  byte% field sdl-joy-hat-event-which
  byte% field sdl-joy-hat-event-hat
  byte% field sdl-joy-hat-event-value
end-struct sdl-joy-hat-event%

struct
  byte% field sdl-joy-button-event-type
  byte% field sdl-joy-button-event-which
  byte% field sdl-joy-button-event-button
  byte% field sdl-joy-button-event-state
end-struct sdl-joy-button-event%

struct
  byte% field sdl-resize-event-type
  int% field sdl-resize-event-width
   int% field sdl-resize-event-height
end-struct sdl-resize-event%

struct
  byte% field sdl-expose-event-type
end-struct sdl-expose-event%

struct
  byte% field sdl-quit-event-type
end-struct sdl-quit-event%

struct
  byte% field sdl-user-event-type
  int% field sdl-user-event-code
  ptr% field sdl-user-event-data1
  ptr% field sdl-user-event-data2
end-struct sdl-user-event%

struct
  byte% field sdl-sys-wm-event-type
  ptr% field sdl-sys-wm-event-msg
end-struct sdl-sys-wm-event%

struct
  byte% field sdl-event-type
  sdl-active-event% field sdl-event-active
  sdl-keyboard-event% field sdl-event-key
  sdl-mouse-motion-event% field sdl-event-motion
  sdl-mouse-button-event% field sdl-event-button
  sdl-joy-axis-event% field sdl-event-jaxis
  sdl-joy-ball-event% field sdl-event-jball
  sdl-joy-hat-event% field sdl-event-jhat
  sdl-joy-button-event% field sdl-event-jbutton
  sdl-resize-event% field sdl-event-resize
  sdl-expose-event% field sdl-event-expose
  sdl-quit-event% field sdl-event-quit
  sdl-user-event% field sdl-event-user
  sdl-sys-wm-event% field sdl-event-syswm
end-struct sdl-event%

struct
  byte% field sdl-color-r
  byte% field sdl-color-g
  byte% field sdl-color-b
  byte% field sdl-color-unused
end-struct sdl-color%

struct
  int%  field sdl-palette-ncolors       \ # of assigned colors in pal
  ptr%  field sdl-palette-colors         \ pointer to sdl-color% type
end-struct sdl-palette%

struct
  word% field sdl-offset-x
  word% field sdl-offset-y
  word% field sdl-offset-w
  word% field sdl-offset-h
end-struct sdl-rect%

struct
  ptr%  field sdl-pixelformat-palette   \ pointer to sdl-palette type
  byte% field sdl-pixelformat-bitsperpixel
  byte% field sdl-pixelformat-bytesperpixel
  int%  field sdl-pixelformat-rmask
  int%  field sdl-pixelformat-gmask
  int%  field sdl-pixelformat-bmask
  int%  field sdl-pixelformat-amask
  byte% field sdl-pixelformat-rshift
  byte% field sdl-pixelformat-gshift
  byte% field sdl-pixelformat-bshift
  byte% field sdl-pixelformat-ashift
  byte% field sdl-pixelformat-rloss
  byte% field sdl-pixelformat-gloss
  byte% field sdl-pixelformat-bloss
  byte% field sdl-pixelformat-aloss
  int%  field sdl-pixelformat-colorkey
  byte% field sdl-pixelformat-alpha
end-struct sdl-pixelformat%

struct
  int%  field sdl-surface-flags                           \ read-only
  ptr%  field sdl-surface-format                          \ read-only
  int%  field sdl-surface-w                               \ read-only
  int%  field sdl-surface-h                               \ read-only
  word% field sdl-surface-pitch                           \ read-only
  ptr%  field sdl-surface-pixels                          \ read-only
  int%  field sdl-surface-offset                            \ private
  ptr%  field sdl-surface-hwdata     \ hardware specific surface info
  sdl-rect% field sdl-surface-cliprect  \ read-only : will this work?
  int%  field sdl-surface-unused1          \ for binary compatibility
  int%  field sdl-surface-locked              \ allow recursive locks
  ptr%  field sdl-surface-map            \ info for fast blit mapping
  int%  field sdl-surface-formatversion              \ format version
  int%  field sdl-surface-refcount       \ ref count: freeing surface
end-struct sdl-surface%

\ Load the constants

include sdlconstants.fs

\ ---[ sdl-mustlock ]------------------------------------------------
\ This is coded as a macro - not as an actual C++ function.
\ Therefore, it can be coded as such in gforth also.
\ All it actually does is check to see if specific flags are set to
\ determine if the surface HAS to be locked or not.
\
\ The <flags> to be checks are the first 32-bit value in the surface
\ record, which should be available immediately upon passing of the
\ surface pointer.
\
\ Returns:
\       0 if surface does not have to be locked
\       1 if surface has to be locked
\
\ Note - the SDL dox indicate that unless you are using HWSURFACE,
\        you do not have to worry about locking a surface for access
\ -------------------------------------------------------------------
\ typedef struct SDL_Surface {
\	Uint32 flags;				/* Read-only */

: sdl-mustlock { *src -- f }
  0 SDL_HWSURFACE OR SDL_ASYNCBLIT OR SDL_RLEACCEL OR 
  *src sdl-surface-flags @ AND
  *src sdl-surface-offset @ AND
;

