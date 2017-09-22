\ ===[ Code Addendum 02 ]============================================
\             gforth: SDL/OpenGL Graphics Part II
\ ===================================================================
\    File Name: sdl.fs
\       Author: Timothy Trussell
\         Date: 02/07/2010
\  Description: SDL Interface for gforth sdl-tut02 demo
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-19
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                   Requires Code Addendum 03
\ ===================================================================

\ ---[Note]----------------------------------------------------------
\       An 'f' as a return value indicates a flag - usually boolean.
\       0 denote success, -1 denotes failure.
\       Reference the SDL_video.h include file in the C++ source
\       Probably at: /usr/include/SDL/SDL_video.h
\ ------------------------------------------------------[End Note]---

\ ---[ gforth Prototypes ]-------------------------------------------
\ sdl-init              ( flags -- error code )
\ sdl-set-video-mode    ( width height bpp flags -- *surface )
\ sdl-quit              ( -- )
\ sdl-loadbmp           ( *filename -- *surface )
\ sdl-freesurface       ( *surface -- )
\ sdl-blitsurface       ( *src *srcrect *dst *dstref -- f )
\ sdl-flip              ( *surface -- f )
\ sdl-delay             ( uint32_ms -- )
\ sdl-show-cursor       ( toggle -- status )
\ sdl-wm-set-caption    ( *title *icon -- )
\ ------------------------------------------------[End Prototypes]---

c-library sdl_t02
s" SDL" add-lib

\c #include <SDL/SDL.h>

c-function sdl-init		SDL_Init		n -- n
c-function sdl-set-video-mode	SDL_SetVideoMode	n n n n -- a
c-function sdl-quit		SDL_Quit		-- void
c-function sdl-loadbmp          SDL_LoadBMP             a -- a
c-function sdl-freesurface      SDL_FreeSurface         a -- void
c-function sdl-blitsurface      SDL_BlitSurface         a a a a -- n
c-function sdl-flip             SDL_Flip                a -- n
c-function sdl-delay            SDL_Delay               n -- void

c-function sdl-show-cursor	SDL_ShowCursor		n -- n

c-function sdl-wm-set-caption	SDL_WM_SetCaption	a a -- void

end-c-library

\ Transfer the constants used to this file

include sdlconstants.fs

