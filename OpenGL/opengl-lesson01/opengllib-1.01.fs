\ ===[ Code Addendum 02 ]============================================
\                 gforth: OpenGL Graphics Lesson 01
\ ===================================================================
\           File: opengllib-1.01.fs
\         Author: Jeff Molofee
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 07/06/2010
\    Description: Setting up an OpenGL window
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 01
\ ===================================================================
\ This code was created by Jeff Molofee '99 
\ (ported to Linux/SDL by Ti Leggett '01)
\ Visit Jeff at http://nehe.gamedev.net/
\ ===================================================================

require ~/.gforth/opengl-libs/mini-opengl-current.fs
require ~/.gforth/opengl-libs/mini-sdl-1.00.fs
require ~/.gforth/opengl-libs/sdlkeysym.fs

\ ---[ Prototype Listing ]-------------------------------------------
\ : HandleKeyPress              ( &event -- )
\ : InitGL                      ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE of TRUE to opengl-exit-flag endof
    SDLK_F1     of screen sdl-wm-togglefullscreen drop endof
  endcase
;

\ ---[ InitGL ]------------------------------------------------------
\ General OpenGL initialization function 

: InitGL ( -- boolean )
  \ Enable smooth shading
  GL_SMOOTH gl-shade-model
  \ Set the background black
  0e 0e 0e 0e gl-clear-color
  \ Depth buffer setup
  1e gl-clear-depth
  \ Enable depth testing
  GL_DEPTH_TEST gl-enable
  \ Type of depth test to do
  GL_LEQUAL gl-depth-func
  \ Really nice perspective calculations
  GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint
  \ Return a good value
  TRUE
;
  
\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag          \ reset these flag for next time
  sdl-quit                               \ close down the SDL systems
;

fvariable fps-seconds
fvariable fps-count
0 value   fps-ticks
0 value   fps-t0
0 value   fps-frames
0 value   fps-line

\ ---[ Reset-FPS-Counter ]-------------------------------------------

: Reset-FPS-Counter ( -- )
  sdl-get-ticks to fps-t0
  0 to fps-frames
;

\ ---[ Display-FPS ]-------------------------------------------------

: Display-FPS ( -- )
  sdl-get-ticks to fps-ticks
  fps-ticks fps-t0 - 1000 >= if
    fps-ticks fps-t0 - S>F 1000e F/ fps-seconds F!
    fps-frames S>F fps-seconds F@ F/ fps-count F!
    0 fps-line at-xy 50 spaces           \ clear previous fps display
    0 fps-line at-xy                          \ display new fps count
    fps-frames . ." frames in " 
    fps-seconds F@ F>S . ." seconds = " 
    fps-count F@ F>S . ." FPS" cr
    fps-ticks to fps-t0
    0 to fps-frames
  then
;

\ ---[ DrawGLScene ]-------------------------------------------------
\ Here goes our drawing code 

: DrawGLScene ( -- boolean )
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear

  \ Reset the matrix
  gl-load-identity
  
  \ Draw it to the screen -- if double buffering is permitted
  sdl-gl-swap-buffers

  \ Gather  our frames per second count
  fps-frames 1+ to fps-frames

  \ Display the FPS count to the terminal window
  Display-FPS

  \ Return a good value
  TRUE
;

