\ ===[ Code Addendum 01 ]============================================
\             gforth: SDL/OpenGL Graphics Part II
\ ===================================================================
\    File Name: sdl-tut02.fs
\       Author: Timothy Trussell
\         Date: 02/07/2010
\  Description: SDL Example Program
\             : Displaying A Bitmapped Image
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-19
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\               Requires Code Addendums 02 and 03
\ ===================================================================

[IFDEF] [SDL-TUT02]
  [SDL-TUT02]
[ENDIF]

marker [SDL-TUT02]

page

\ Load in the SDL library interface functions
\ This will cause the gcc compiler to process the data, and the
\ resulting object file will be linked into our gforth program.

require sdl.fs

\ ---[ Variables ]---------------------------------------------------
\ Define storage for the surface pointers

NULL value image-surface
NULL value screen-surface

\ Use this for the error handler

0 value in-graphics-mode

\ ---[ Utility Words ]-----------------------------------------------

: Help-Message ( -- )
  ." +----------------------------------------------------------+" cr
  ." |                  gforth SDL Tutorial 02       02/05/2010 |" cr
  ." +----------------------------------------------------------|" cr
  ." |  Draw a bitmapped image to the graphics display window   |" cr
  ." +----------------------------------------------------------|" cr
  ." |      Tested on: Ubuntu v9.10 i386 Kernel: 2.6.31-19      |" cr
  ." +----------------------------------------------------------+" cr
  ." Commands: sdl-tut02 Help-Message" cr
  ." ------------------------------------------------------------" cr
;

\ ---[ Error-End ]---------------------------------------------------
\ Handle and SDL return code -- 0 == ok, -1 == an error occurred

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
\ This is required for the libcc interface when passing strings to
\ the C library functions.

: Terminate-String ( a n -- a )    \ replaces the last character of a
  over + 1- 0 swap c!                                \ string with \0
;   

\ ---[ InitGraph ]---------------------------------------------------
\ InitGraph initializes the SDL systems that will be used.
\ It then creates a window on the Linux desktop, with a resolution of
\ 640x480x32 for this demo.
\ The window is then given a title bar.
\ Finally, the mouse pointer is disabled if placed on the window.

: InitGraph ( -- )
  SDL_INIT_VIDEO sdl-init                                 \ start sdl		
  0<> s" Unable to initialize SDL" Error-End

  640 480 32 SDL_SWSURFACE sdl-set-video-mode         \ create window
  dup 0< s" Unable to set video mode" Error-End
  to screen-surface                  \ save pointer to screen surface

  s" sdl-tut01 gforth demo0" Terminate-String NULL sdl-wm-set-caption 
  SDL_DISABLE sdl-show-cursor drop     \ disappear mouse if in window
  1 to in-graphics-mode
;

\ ---[ CloseGraph ]--------------------------------------------------
\ Re-enables the mouse pointer, and shuts down the SDL systems.
\ The open display window on the desktop is closed.

: CloseGraph ( -- )
  SDL_ENABLE sdl-show-cursor drop            \ turn the mouse back on
  sdl-quit                                            \ graceful exit
  0 to in-graphics-mode
;

\ ---[ Load-Picture ]------------------------------------------------
\ Loads the specified file into a surface that SDL creates, and then
\ returns a pointer, which is saved to a VALUE.

\ sdl-loadbmp returns a 0 on an error, *surface on success

: Load-Picture ( -- )
  s" gforth1.bmp0" Terminate-String sdl-loadbmp
  dup 0= s" Unable to load image file" Error-End
  to image-surface                  \ save pointer to picture surface
;

\ ---[ Show-Picture ]------------------------------------------------
\ Copies the specified surface buffer to the display window buffer,
\ and then blits the image to the display window.

: Show-Picture ( -- )
  image-surface NULL screen-surface NULL sdl-blitsurface drop
  screen-surface sdl-flip drop
;

\ ---[ Free-Picture ]------------------------------------------------
\ Causes SDL to free the memory allocated for the specified surface.

: Free-Picture ( -- )
  image-surface sdl-freesurface          \ release memory for surface
;

\ ---[ The main function ]-------------------------------------------
\ Links all the above together.

: sdl-tut02 ( -- )
  InitGraph                                 \ rev the graphics engine
  Load-Picture                            \ get the image into memory
  Show-Picture                          \ let us view the masterpiece
  10000 sdl-delay                        \ time for ooh's and aahhh's
  Free-Picture                           \ clean up the system memory
  CloseGraph                             \ leave the program politely
;

\ Displays the information window after compilation is complete.

Help-Message

