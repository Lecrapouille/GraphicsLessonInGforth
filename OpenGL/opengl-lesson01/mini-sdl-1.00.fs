\ ===[ Code Addendum 05 ]============================================
\                 gforth: OpenGL Graphics Lesson 01
\ ===================================================================
\           File: mini-sdl-1.01.fs
\         Author: Timothy Trussell
\           Date: 07/06/2010
\    Description: SDL libcc interface
\   Forth System: gforth-0.7.0
\      Assembler: Built-in FORTH assembler
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\ This libcc interface contains the SDL code required for use with
\ the NeHe OpenGL Tutorials.
\ ===================================================================

c-library mini_sdl_lib
s" SDL" add-lib

\c #include <SDL/SDL.h>

\ SDL General Functions

c-function sdl-init		SDL_Init		       n -- n
c-function sdl-quit		SDL_Quit		      -- void
c-function sdl-geterror         SDL_GetError                     -- a

\ SDL Video Functions

c-function sdl-getvideoinfo     SDL_GetVideoInfo                 -- a
c-function sdl-set-video-mode	SDL_SetVideoMode         n n n n -- a
c-function sdl-freesurface      SDL_FreeSurface             a -- void
c-function sdl-gl-set-attribute SDL_GL_SetAttribute       n n -- void
c-function sdl-gl-swap-buffers  SDL_GL_SwapBuffers            -- void
c-function sdl-loadbmp          SDL_LoadBMP                    a -- a

\ SDL Window Management

c-function sdl-wm-set-caption	SDL_WM_SetCaption	  a a -- void
c-function sdl-wm-togglefullscreen SDL_WM_ToggleFullScreen     a -- n

\ SDL Event Functions

c-function sdl-pollevent        SDL_PollEvent                  a -- n
c-function sdl-get-app-state    SDL_GetAppState                  -- n

\ SDL Time

c-function sdl-get-ticks        SDL_GetTicks                     -- n
c-function sdl-delay            SDL_Delay                   n -- void

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

\ ---[ General ]---
\ -- no structures required in this section

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

\ palette       Pointer to the palette; NULL if the BitsPerPixel>8
\ BitsPerPixel  The number of bits used to represent each pixel in a
\               surface. Usually 8, 16, 24 or 32.
\ BytesPerPixel The number of bytes used to represent each pixel in a
\               surface. Usually one to four.
\ [RGBA]mask    Binary mask used to retrieve individual color values
\ [RGBA]shift   Binary left shift of each color component in the
\               pixel value
\ [RGBA]loss    Precision loss of each color component
\ colorkey      Pixel value of transparent pixels
\ alpha         Overall surface alpha value

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

\ ---[ Window Management ]---
\ -- no structures required in this section

\ ====================================================[07/04/2010]===
\ The SDL Event structures will now use a method I came across in the
\ gforth libcc.fs source where the first STRUCT defined, event-type%,
\ defines two fields, one of which is mapped to all successive STRUCT
\ entries - which will now start with <sdl-event-type%>, and a second
\ which acts as a place-holder for the location of data following.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ For those wanting more nuts and bolts, what happens is that in our
\ code, we define an SDL event structure:
\
\    create event here sdl-event% nip dup allot 0 fill
\
\ (I prefer to zero my arrays and structures, which is the purpose of
\ the <address> <size> dup allot 0 fill sequence)
\
\ Now, in the code we use to check for an SDL event:
\
\    begin
\      event sdl-pollevent            \ are there any pending events?
\    while
\      event sdl-event-type c@              \ yes, process the events
\      case
\        SDL_ACTIVEEVENT of   \ application visibility event occurred
\                           (code for an active event)
\                        endof
\        SDL_VIDEORESIZE of            \ window resize event occurred
\                           (code for a key-up event)
\                        endof
\        SDL_KEYDOWN of                    \ key press event occurred
\                           (code for a key-down event)
\                        endof
\        SDL_QUIT of    \ window close box clicked, or ALT-F4 pressed
\                           (code for a quit event)
\                        endof
\      endcase
\    repeat                   \ until no more events are in the queue
\
\ when the <event sdl-pollevent> executes, up to 20 bytes of data are
\ copied to the address returned by <event>.
\ 
\ The contents of these 20 bytes are then parsed by checking the
\ first byte - the sdl-event-type - which is the single common field
\ defined in each of the event structs to determine which kind of
\ event has occurred.
\
\ The 20 bytes that are copied - for ANY event that occurs - are put
\ into the event array we created.
\
\ This is where the C UNION typing comes in - the data is mapped to
\ the SAME array space (base addressed by event) for ALL of the event
\ types that are implemented. (The SDL code defines the SDL_Event
\ struct as a UNION.)
\
\ It is then up to our program to determine what to do with the data.
\
\ Note that this sdl-pollevent sequence is in a loop, which will
\ continue until there are no further events in the poll queue. This
\ means that if you sit there and hit as many keys as you can, they
\ will all get looked at by this loop - but only acted upon if they
\ are actually defined in our CASE..ENDCASE construct.
\
\ If the event that occurred is not defined, it is ignored and the
\ data is basically thrown away when the next sdl-pollevent occurs,
\ which reads in the data for the next event.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ It would be easy to create a log of all events that occur, by the
\ placement of code that outputs this data to a text file here:
\
\    begin
\      event sdl-pollevent            \ are there any pending events?
\    while
\      event sdl-event-type c@              \ yes, process the events
\      dup copy-event-to-log
\      ... rest of event parsing code
\
\ The <copy-event-to-log> code would need to copy the data in the
\ event structure to the log file.
\ You could then re-create an entire sequence of events - replay them
\ as it were - by simply coding a small supervisor function that can
\ read in the "demo" data file and execute it in place of the event
\ handling code above.
\ Even the timing involved between events could be implemented, as
\ the elapsed ticks could be retrieved as well, and used as a delay
\ between each new event sequence.
\ ===================================================================

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

\ Description of fields for sdl-videoinfo%

\ hw_available  Is it possible to create hardware surfaces?
\ wm_available  Is there a window manager available
\ blit_hw       Are hardware to hardware blits accelerated?
\ blit_hw_CC    Are hardware to hardware colorkey blits accelerated?
\ blit_hw_A     Are hardware to hardware alpha blits accelerated?
\ blit_sw       Are software to hardware blits accelerated?
\ blit_sw_CC    Are software to hardware colorkey blits accelerated?
\ blit_sw_A     Are software to hardware alpha blits accelerated?
\ blit_fill     Are color fills accelerated?
\ video_mem     Total amount of video memory in Kilobytes
\ vformat       pixel format of the video device

