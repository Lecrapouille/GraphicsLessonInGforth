\ ===================================================================
\        Program: mini-sdl-1.01.fs
\         Author: Timothy Trussell
\           Date: 07/24/2010
\    Description: SDL libcc interface
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\ This libcc interface contains the cumulatively added SDL code
\ function calls that are required for use with the gforth version of
\ the NeHe OpenGL Tutorial Lessons.
\ ===================================================================

UseLibUtil [if]
  c-library mini_sdl_lib
[else]
  c-library mini_sdl_lib23
[then]

s" SDL" add-lib

\c #include <SDL/SDL.h>

\ Initial Entries for Lesson 1
c-function sdl-delay            SDL_Delay                   n -- void
c-function sdl-freesurface      SDL_FreeSurface             a -- void
c-function sdl-get-app-state    SDL_GetAppState                  -- n
c-function sdl-geterror         SDL_GetError                     -- a
c-function sdl-get-ticks        SDL_GetTicks                     -- n
c-function sdl-getvideoinfo     SDL_GetVideoInfo                 -- a
c-function sdl-gl-set-attribute SDL_GL_SetAttribute       n n -- void
c-function sdl-gl-swap-buffers  SDL_GL_SwapBuffers            -- void
c-function sdl-init		SDL_Init		       n -- n
c-function sdl-loadbmp          SDL_LoadBMP                    a -- a
c-function sdl-pollevent        SDL_PollEvent                  a -- n
c-function sdl-quit		SDL_Quit		      -- void
c-function sdl-set-video-mode	SDL_SetVideoMode         n n n n -- a
c-function sdl-wm-set-caption	SDL_WM_SetCaption	  a a -- void
c-function sdl-wm-togglefullscreen SDL_WM_ToggleFullScreen     a -- n

\ Additions for Lesson 09
c-function sdl-enable-keyrepeat SDL_EnableKeyRepeat          n n -- n

\ Additions for Lesson 21
c-function sdl-init-sub-system  SDL_InitSubSystem              n -- n
c-function sdl-quit-sub-system  SDL_QuitSubSystem           n -- void

end-c-library

\ ---[ Load the SDL Constants ]--------------------------------------
\ Some of the struct definitions require these constant definitions

include sdlconstants.fs

\ ---[ Structure Definitions ]---------------------------------------
\ 
\ ---[ Define the aligned field sizes ]---

4 4 2constant int%
2 2 2constant word%
1 1 2constant byte%
cell% 2constant ptr%

\ ---[ Video ]---

struct
  byte% field sdl-color-r                             \ red intensity
  byte% field sdl-color-g                           \ green intensity
  byte% field sdl-color-b                            \ blue intensity
  byte% field sdl-color-unused         \ actually the alpha intensity
end-struct sdl-color%

struct
  int%  field sdl-palette-ncolors       \ # of assigned colors in pal
  ptr%  field sdl-palette-colors        \ pointer to sdl-color% array
end-struct sdl-palette%

struct
  word% field sdl-offset-x               \ upper left x coord of rect
  word% field sdl-offset-y               \ upper left y coord of rect
  word% field sdl-offset-w                            \ width of rect
  word% field sdl-offset-h                           \ height of rect
end-struct sdl-rect%

struct
  ptr%  field sdl-pixelformat-palette
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
  int%  field sdl-surface-flags           \ flags set for the surface
  ptr%  field sdl-surface-format             \ pixelformat of surface
  int%  field sdl-surface-w                        \ width of surface
  int%  field sdl-surface-h                       \ height of surface
  word% field sdl-surface-pitch                \ number of bytes/line
  ptr%  field sdl-surface-pixels                \ actual image buffer
  int%  field sdl-surface-offset                            \ private
  ptr%  field sdl-surface-hwdata     \ hardware specific surface info
  sdl-rect% field sdl-surface-cliprect  \ read-only : will this work?
  int%  field sdl-surface-unused1          \ for binary compatibility
  int%  field sdl-surface-locked              \ allow recursive locks
  ptr%  field sdl-surface-map            \ info for fast blit mapping
  int%  field sdl-surface-formatversion              \ format version
  int%  field sdl-surface-refcount       \ ref count: freeing surface
end-struct sdl-surface%

struct
  byte% field sdl-event-type
  1 0   field sdl-event-payload  \ 1st field in each STRUCT maps here
end-struct sdl-event-type%

\ ---[ Joystick ]---

sdl-event-type%
  byte% field sdl-joy-axis-event-which
  byte% field sdl-joy-axis-event-axis
  word% field sdl-joy-axis-event-value
end-struct sdl-joy-axis-event%

sdl-event-type%
  byte% field sdl-joy-ball-event-which
  byte% field sdl-joy-ball-event-ball
  word% field sdl-joy-ball-event-xrel
  word% field sdl-joy-ball-event-yrel
end-struct sdl-joy-ball-event%

sdl-event-type%
  byte% field sdl-joy-hat-event-which
  byte% field sdl-joy-hat-event-hat
  byte% field sdl-joy-hat-event-value
end-struct sdl-joy-hat-event%

sdl-event-type%
  byte% field sdl-joy-button-event-which
  byte% field sdl-joy-button-event-button
  byte% field sdl-joy-button-event-state
end-struct sdl-joy-button-event%

\ ---[ Section 4: Events ]---

sdl-event-type%
  byte% field sdl-active-event-gain
  byte% field sdl-active-event-state
end-struct sdl-active-event%

struct
  char% field sdl-keysym-scancode
  int%  field sdl-keysym-sym
  int%  field sdl-keysym-mod
  word% field sdl-keysym-unicode
end-struct sdl-keysym%

sdl-event-type%
  byte% field sdl-keyboard-event-which
  byte% field sdl-keyboard-event-state
  sdl-keysym% field sdl-keyboard-event-keysym
end-struct sdl-keyboard-event%

sdl-event-type%
  byte% field sdl-mouse-motion-event-which
  byte% field sdl-mouse-motion-event-state
  word% field sdl-mouse-motion-event-x
  word% field sdl-mouse-motion-event-y
  word% field sdl-mouse-motion-event-xrel
  word% field sdl-mouse-motion-event-yrel
end-struct sdl-mouse-motion-event%

sdl-event-type%
  byte% field sdl-mouse-button-event-which
  byte% field sdl-mouse-button-event-button
  byte% field sdl-mouse-button-event-state
  word% field sdl-mouse-button-event-x
  word% field sdl-mouse-button-event-y
end-struct sdl-mouse-button-event%

sdl-event-type%
  int%  field sdl-resize-event-width
  int%  field sdl-resize-event-height
end-struct sdl-resize-event%

\ These two do not need to be defined, as all that is checked is the
\ sdl-event-type field to see if it is an SDL_VIDEOEXPOSE or SDL_QUIT
\ event that has occurred - there is no data for these two events.

\ sdl-event-type%
\   1 0 field sdl-expose-event-type
\ end-struct sdl-expose-event%

\ sdl-event-type%
\   1 0 field sdl-quit-event-type
\ end-struct sdl-quit-event%

sdl-event-type%
  int%  field sdl-user-event-code
  ptr%  field sdl-user-event-data1
  ptr%  field sdl-user-event-data2
end-struct sdl-user-event%

sdl-event-type%
  ptr%  field sdl-sys-wm-event-msg
end-struct sdl-sys-wm-event%

\ The SDL_Event structure is arranged as a UNION.
\ This means that each of the structures is accessed at the same
\ event address. Therefore, we only have to allocate memory for the
\ largest struct size we will need to access.
\
\ All of the data can now be referenced by using the address of the
\ event struct in gforth memory, followed by the field name of the
\ specific event being processed.
\
\ All of the -type fields in the structs are mapped to the address of
\ the sdl-event-type field in the sdl-event% struct.

sdl-event-type%
  1 31 field sdl-event-data      \ actually 20 bytes, but I padded it
end-struct sdl-event%

struct
  int% field sdl-video-info-hw-available
  int% field sdl-video-info-wm-available
  int% field sdl-video-info-blit-hw
  int% field sdl-video-info-blit-hw-cc
  int% field sdl-video-info-blit-hw-a
  int% field sdl-video-info-blit-sw
  int% field sdl-video-info-blit-sw-cc
  int% field sdl-video-info-blit-sw-a
  int% field sdl-video-info-blit-fill
  int% field sdl-video-info-video-mem
  ptr% field sdl-video-info-vformat
end-struct sdl-videoinfo%

