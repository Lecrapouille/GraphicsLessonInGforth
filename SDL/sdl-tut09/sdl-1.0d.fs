\ ===[ Code Addendum 03 ]============================================
\                 gforth: SDL/OpenGL Graphics Part IX
\ ===================================================================
\    File Name: sdl-1.0d.fs
\      Version: 1.0d
\       Author: Timothy Trussell
\         Date: 04/15/2010
\  Description: SDL Interface for gforth
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-20
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                   SDL v1.0d Library for gforth
\ ===================================================================

\ ---[ Change Log ]--------------------------------------------------
\
\ 01/31/2010    Initial data from GLForth and 3DDemo         rev 1.0a
\               Modified to make them functional for
\               gforth-0.7.0
\
\ 02/26/2010    Added SDL structure definitions              rev 1.0b
\               Added sdl-mustlock, lock- and unlock-surface
\
\ 03/12/2010    Added comments for all of the SDL library    rev 1.0c
\               functions that will be added in the Prototype 
\               section, even though they are not implemented 
\               as yet.
\
\ 03/18/2010    Added call to the SDL Image extension library for
\               loading additional picture formats <sdl-loadimage>
\               Worked great after adding the add-lib line.
\               Note: I attempted to code sdl-imageinit and
\                     sdl-imagequit, but for both calls the gforth
\                     system came back with an invalid memory error.
\                     The system seems to run fine without them
\                     specifically in place.
\
\ 04/03/2010    Changed the sdl-surface% struct, as it       rev 1.0d
\               appears I added the wrong elements to the
\               struct when I did the original coding.
\               As I am not using anything below <offset>, this
\               is not really a problem, but needs to be fixed.
\
\ 04/15/2010    Added various interface calls to the listing.
\               These are just for general use, as I have not
\               started using most of them as yet.

\ -----------------------------------------------[End Change Log ]---

\ ---[Note]----------------------------------------------------------
\ An 'f' as a return value indicates a flag - usually boolean.
\ 0 denote success, -1 denotes failure -- *usually*
\ Reference the SDL_video.h include file in the C++ source
\ Probably at: /usr/include/SDL/SDL_video.h (if you installed SDL)
\ ------------------------------------------------------[End Note]---

\ ---[ gforth Prototypes ]-------------------------------------------
\ The ordering of these routines is taken from the SDL API Reference
\ Guide .CHM file downloaded from the libsdl.org documentation page.

\ *** Not Ready For Prime Time *** simply means it is not coded yet.

\ ---[ Section 1: General ]---

\ sdl-init              ( flags -- error-code )
\ sdl-initsubsystem     ( flags -- error-code )
\ sdl-quitsubsystem     ( flags -- )
\ sdl-quit              ( -- )
\ sdl-wasinit           ( flags -- flags )
\ sdl-geterror          ( -- *error-msg )

\ ---[ Section 2: Video ]---

\ sdl-get-video-surface ( -- *surface )
\ sdl-get-video-info    ( -- *videoinfo )
\ sdl-video-driver-name ( *namebuf maxlen -- *name )
\ sdl-list-modes                     *** Not Ready For Prime Time ***
\ sdl-video-mode-ok     ( width height bpp flags -- *surface )
\ sdl-set-video-mode    ( width height bpp flags -- *surface )
\ sdl-updaterect        ( *surface x y w h -- )
\ sdl-update-rects      ( *surface nrects *rects -- )
\ sdl-flip              ( *surface -- f )
\ sdl-set-colors        ( *surface *colors first count -- f )
\ sdl-setpalette        ( *surface flags *colors first num -- f )
\ sdl-set-gamma         ( redgamma greengamma bluegamma -- n )
\ sdl-get-gamma-ramp    ( *red[] *green[] *blue[] -- n )
\ sdl-set-gamma-ramp    ( *red[] *green[] *blue[] -- n )
\ sdl-map-rgb           ( *format r g b -- pixel )
\ sdl-map-rgba          ( *format r g b a -- pixel )
\ sdl-get-rgb           ( pixel *format *r *g *b -- )
\ sdl-get-rgba          ( pixel *format *r *g *b *a -- )
\ sdl-creatergbsurface  ( flags w h bpp Rm Gm Bm Am -- *surface )
\ sdl-creatergbsurfacefrom ( *p w h bpp pitch rmask gmask bmask amask -- *surface )
\ sdl-freesurface       ( *surface -- )
\ : sdl-mustlock        { *src -- f }       -- in Forth code, not C++
\ sdl-locksurface       ( *surface -- f )
\ sdl-unlocksurface     ( *surface -- )
\ sdl-loadbmp           ( *filename -- *surface )
\ sdl-savebmp           ( *surface *filename -- f )
\ sdl-set-colorkey      ( *surface flag key -- f )
\ sdl-set-alpha         ( *surface flag alpha -- f )
\ sdl-set-clip-rect     ( *surface *rect -- )
\ sdl-get-clip-rect     ( *surface *rect -- )
\ sdl-convert-surface   ( *surface *format flags -- *surface )
\ sdl-blitsurface       ( *src *srcrect *dst *dstref -- f )
\ sdl-fillrect          ( *dst *dstrect color -- f )
\ sdl-display-format    ( *surface -- *surface )
\ sdl-display-format-alpha ( *surface -- *surface )
\ sdl-warp-mouse        ( x y -- )
\ sdl-create-cursor     ( *data *mask w h hotx hoty -- *cursor )
\ sdl-free-cursor       ( *cursor -- )
\ sdl-set-cursor        ( *cursor -- )
\ sdl-get-cursor        ( -- *cursor )
\ sdl-show-cursor       ( toggle -- status )
\ sdl-gl-load-library   ( *path -- f )
\ sdl-gl-get-proc-address ( *procname -- *proc )
\ sdl-gl-get-attribute               *** Not Ready For Prime Time ***
\ sdl-gl-set-attribute               *** Not Ready For Prime Time ***
\ sdl-gl-swap-buffers                *** Not Ready For Prime Time ***
\ sdl-create-yuv-overlay             *** Not Ready For Prime Time ***
\ sdl-lock-yuv-overlay               *** Not Ready For Prime Time ***
\ sdl-unlock-yuv-overlay             *** Not Ready For Prime Time ***
\ sdl-display-yuv-overlay            *** Not Ready For Prime Time ***
\ sdl-free-yuv-overlay               *** Not Ready For Prime Time ***

\ ---[ Section 3: Window Management ]---

\ sdl-wm-set-caption    ( *title *icon -- )
\ sdl-wm-get-caption                 *** Not Ready For Prime Time ***
\ sdl-wm-set-icon                    *** Not Ready For Prime Time ***
\ sdl-wm-iconify-window              *** Not Ready For Prime Time ***
\ sdl-wm-toggle-full-screen          *** Not Ready For Prime Time ***
\ sdl-wm-grab-input                  *** Not Ready For Prime Time ***

\ ---[ Section 4: Events ]---

\ sdl-pump-events                    *** Not Ready For Prime Time ***
\ sdl-peep-events                    *** Not Ready For Prime Time ***
\ sdl-pollevent         ( *event -- f )
\ sdl-wait-event                     *** Not Ready For Prime Time ***
\ sdl-push-event                     *** Not Ready For Prime Time ***
\ sdl-set-event-filter               *** Not Ready For Prime Time ***
\ sdl-get-event-filter               *** Not Ready For Prime Time ***
\ sdl-event-state                    *** Not Ready For Prime Time ***
\ sdl-get-key-state                  *** Not Ready For Prime Time ***
\ sdl-get-mod-state                  *** Not Ready For Prime Time ***
\ sdl-set-mod-state                  *** Not Ready For Prime Time ***
\ sdl-get-key-name                   *** Not Ready For Prime Time ***
\ sdl-enable-unicode                 *** Not Ready For Prime Time ***
\ sdl-enable-key-repeat              *** Not Ready For Prime Time ***
\ sdl-get-mouse-state                *** Not Ready For Prime Time ***
\ sdl-get-relative-mouse-state       *** Not Ready For Prime Time ***
\ sdl-get-app-state                  *** Not Ready For Prime Time ***
\ sdl-joystick-event-state           *** Not Ready For Prime Time ***

\ ---[ Section 5: Joystick ]---

\ sdl-num-joysticks                  *** Not Ready For Prime Time ***
\ sdl-joystick-name                  *** Not Ready For Prime Time ***
\ sdl-joystick-open                  *** Not Ready For Prime Time ***
\ sdl-joystick-opened                *** Not Ready For Prime Time ***
\ sdl-joystick-index                 *** Not Ready For Prime Time ***
\ sdl-joystick-num-axes              *** Not Ready For Prime Time ***
\ sdl-joystick-num-balls             *** Not Ready For Prime Time ***
\ sdl-joystick-num-hats              *** Not Ready For Prime Time ***
\ sdl-joystick-num-buttons           *** Not Ready For Prime Time ***
\ sdl-joystick-update                *** Not Ready For Prime Time ***
\ sdl-joystick-get-axis              *** Not Ready For Prime Time ***
\ sdl-joystick-get-hat               *** Not Ready For Prime Time ***
\ sdl-joystick-get-button            *** Not Ready For Prime Time ***
\ sdl-joystick-get-ball              *** Not Ready For Prime Time ***
\ sdl-joystick-close                 *** Not Ready For Prime Time ***

\ ---[ Section 6: Audio ]---

\ sdl-open-audio                     *** Not Ready For Prime Time ***
\ sdl-pause-audio                    *** Not Ready For Prime Time ***
\ sdl-get-audio-status               *** Not Ready For Prime Time ***
\ sdl-load-wav                       *** Not Ready For Prime Time ***
\ sdl-free-wav                       *** Not Ready For Prime Time ***
\ sdl-build-audio-cvt                *** Not Ready For Prime Time ***
\ sdl-convert-audio                  *** Not Ready For Prime Time ***
\ sdl-mix-audio                      *** Not Ready For Prime Time ***
\ sdl-lock-audio                     *** Not Ready For Prime Time ***
\ sdl-unlock-audio                   *** Not Ready For Prime Time ***
\ sdl-close-audio                    *** Not Ready For Prime Time ***

\ ---[ Section 7: CDROM ]---

\ sdl-cd-num-drives                  *** Not Ready For Prime Time ***
\ sdl-cd-name                        *** Not Ready For Prime Time ***
\ sdl-cd-open                        *** Not Ready For Prime Time ***
\ sdl-cd-status                      *** Not Ready For Prime Time ***
\ sdl-cd-play                        *** Not Ready For Prime Time ***
\ sdl-cd-play-tracks                 *** Not Ready For Prime Time ***
\ sdl-cd-pause                       *** Not Ready For Prime Time ***
\ sdl-cd-resume                      *** Not Ready For Prime Time ***
\ sdl-cd-stop                        *** Not Ready For Prime Time ***
\ sdl-cd-eject                       *** Not Ready For Prime Time ***
\ sdl-cd-close                       *** Not Ready For Prime Time ***

\ note - SDL_Max_Tracks is defined as 99 in sdlconstants.fs

\ ---[ Section 8: Multi-Threaded Programming ]---

\ sdl-create-thread                  *** Not Ready For Prime Time ***
\ sdl-thread-id                      *** Not Ready For Prime Time ***
\ sdl-get-thread-id                  *** Not Ready For Prime Time ***
\ sdl-wait-thread                    *** Not Ready For Prime Time ***
\ sdl-kill-thread                    *** Not Ready For Prime Time ***
\ sdl-create-mutex                   *** Not Ready For Prime Time ***
\ sdl-destroy-mutex                  *** Not Ready For Prime Time ***
\ sdl-mutex-p                        *** Not Ready For Prime Time ***
\ sdl-mutex-v                        *** Not Ready For Prime Time ***
\ sdl-create-semaphore               *** Not Ready For Prime Time ***
\ sdl-destroy-semaphore              *** Not Ready For Prime Time ***
\ sdl-sem-wait                       *** Not Ready For Prime Time ***
\ sdl-sem-try-wait                   *** Not Ready For Prime Time ***
\ sdl-sem-wait-timeout               *** Not Ready For Prime Time ***
\ sdl-sem-post                       *** Not Ready For Prime Time ***
\ sdl-sem-value                      *** Not Ready For Prime Time ***
\ sdl-create-cond                    *** Not Ready For Prime Time ***
\ sdl-destroy-cond                   *** Not Ready For Prime Time ***
\ sdl-cond-signal                    *** Not Ready For Prime Time ***
\ sdl-cond-broadcast                 *** Not Ready For Prime Time ***
\ sdl-cond-wait                      *** Not Ready For Prime Time ***
\ sdl-cond-wait-timeout              *** Not Ready For Prime Time ***

\ ---[ Section 9: Time ]---

\ sdl-get-ticks                      *** Not Ready For Prime Time ***
\ sdl-delay             ( uint32_ms -- )
\ sdl-add-timer                      *** Not Ready For Prime Time ***
\ sdl-remove-timer                   *** Not Ready For Prime Time ***
\ sdl-set-timer                      *** Not Ready For Prime Time ***
\
\ ---[ Section 10: SDL Image Extension Library ]---

\ sdl-loadimage         ( *filename -- *surface )

\ ------------------------------------------------[End Prototypes]---

c-library sdl_lib
s" SDL" add-lib
s" SDL_image" add-lib

\c #include <SDL/SDL.h>
\c #include <SDL/SDL_image.h>

\c // SDL General Functions -- Section 1

c-function sdl-init		SDL_Init		n -- n
c-function sdl-initsubsystem    SDL_InitSubSystem       n -- n
c-function sdl-quitsubsystem    SDL_QuitSubSystem       n -- void
c-function sdl-quit		SDL_Quit		-- void
c-function sdl-wasinit          SDL_WasInit             n -- n
c-function sdl-geterror         SDL_GetError            -- a

\c // SDL Video Functions -- Section 2

c-function sdl-getvideosurface  SDL_GetVideoSurface     -- a
c-function sdl-getvideoinfo     SDL_GetVideoInfo        -- a
c-function sdl-videodrivername  SDL_VideoDriverName     a n -- a
c-function sdl-video-mode-ok    SDL_VideoModeOk         n n n n -- n
c-function sdl-set-video-mode	SDL_SetVideoMode	n n n n -- a
c-function sdl-updaterect       SDL_UpdateRect          a n n n n -- void
c-function sdl-update-rects     SDL_UpdateRects         a n a -- void
c-function sdl-flip             SDL_Flip                a -- n
c-function sdl-set-colors       SDL_SetColors           a a n n -- n
c-function sdl-setpalette       SDL_SetPalette          a n a n n -- n
c-function sdl-setgamma         SDL_SetGamma            r r r -- n
c-function sdl-getgammaramp     SDL_GetGammaRamp        a a a -- n
c-function sdl-setgammaramp     SDL_SetGammaRamp        a a a -- n
c-function sdl-map-rgb          SDL_MapRGB              a n n n -- n
c-function sdl-map-rgba         SDL_MapRGBA             a n n n n -- n
c-function sdl-get-rgb          SDL_GetRGB              n a a a a -- void
c-function sdl-get-rgba         SDL_GetRGBA             n a a a a a -- void
c-function sdl-creatergbsurface SDL_CreateRGBSurface    n n n n n n n n -- a
c-function sdl-creatergbsurfacefrom SDL_CreateRGBSurfaceFrom a n n n n n n n n -- a
c-function sdl-freesurface      SDL_FreeSurface         a -- void
c-function sdl-locksurface      SDL_LockSurface         a -- n
c-function sdl-unlocksurface    SDL_UnlockSurface       a -- void
c-function sdl-loadbmp          SDL_LoadBMP             a -- a
c-function sdl-savebmp          SDL_SaveBMP             a a -- n
c-function sdl-setcolorkey      SDL_SetColorKey         a n n -- n
c-function sdl-setalpha         SDL_SetAlpha            a n n -- n
c-function sdl-set-clip-rect    SDL_SetClipRect         a a -- void
c-function sdl-get-clip-rect    SDL_GetClipRect         a a -- void
c-function sdl-convert-surface  SDL_ConvertSurface      a a n -- a
c-function sdl-blitsurface      SDL_BlitSurface         a a a a -- n
c-function sdl-fillrect         SDL_FillRect            a a n -- n

c-function sdl-display-format   SDL_DisplayFormat       a -- a
c-function sdl-display-format-alpha SDL_DisplayFormatAlpha a -- a
c-function sdl-warp-mouse       SDL_WarpMouse           n n -- void
c-function sdl-create-cursor    SDL_CreateCursor        a a n n n n -- a
c-function sdl-free-cursor      SDL_FreeCursor          a -- void
c-function sdl-set-cursor       SDL_SetCursor           a -- void
c-function sdl-get-cursor       SDL_GetCursor           -- a

c-function sdl-show-cursor	SDL_ShowCursor		n -- n

\c // SDL Window Management -- Section 3

c-function sdl-wm-set-caption	SDL_WM_SetCaption	a a -- void

\c // SDL Event Functions -- Section 4

c-function sdl-pollevent        SDL_PollEvent           a -- n

\c // SDL_image library functions

c-function sdl-loadimage        IMG_Load                a -- a

\c // SDL Time -- Section 9

c-function sdl-delay            SDL_Delay               n -- void

end-c-library

\ ---[ Load the SDL Constants ]--------------------------------------
\ Some of the struct definitions require the constant definitions

include sdlconstants.fs

\ ---[ Structure Definitions ]---------------------------------------

\ ---[ Define the aligned field sizes ]---

4 4 2constant int%
2 2 2constant word%
1 1 2constant byte%
cell% 2constant ptr%

\ ---[ Section 1: General ]---

\ ---[ Section 2: Video ]---

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

\ ---[ Section 3: Window Management ]---

\ ---[ Section 6: Joystick ]---

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

\ ---[ Section 4: Events ]---

struct
  char% field sdl-keysym-scancode
  int%  field sdl-keysym-sym
  int%  field sdl-keysym-mod
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
  byte% field sdl-resize-event-type
  int%  field sdl-resize-event-width
  int%  field sdl-resize-event-height
end-struct sdl-resize-event%

struct
  byte% field sdl-expose-event-type
end-struct sdl-expose-event%

struct
  byte% field sdl-quit-event-type
end-struct sdl-quit-event%

struct
  byte% field sdl-user-event-type
  int%  field sdl-user-event-code
  ptr%  field sdl-user-event-data1
  ptr%  field sdl-user-event-data2
end-struct sdl-user-event%

struct
  byte% field sdl-sys-wm-event-type
  ptr%  field sdl-sys-wm-event-msg
end-struct sdl-sys-wm-event%

struct
  byte%                   field sdl-event-type
  sdl-active-event%       field sdl-event-active
  sdl-keyboard-event%     field sdl-event-key
  sdl-mouse-motion-event% field sdl-event-motion
  sdl-mouse-button-event% field sdl-event-button
  sdl-joy-axis-event%     field sdl-event-jaxis
  sdl-joy-ball-event%     field sdl-event-jball
  sdl-joy-hat-event%      field sdl-event-jhat
  sdl-joy-button-event%   field sdl-event-jbutton
  sdl-resize-event%       field sdl-event-resize
  sdl-expose-event%       field sdl-event-expose
  sdl-quit-event%         field sdl-event-quit
  sdl-user-event%         field sdl-event-user
  sdl-sys-wm-event%       field sdl-event-syswm
end-struct sdl-event%

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

%sdl-quiet 0= [if]
.( [set %sdl-quiet flag to 1 to hide these messages])cr
.( SDL Interface loaded successfully.) cr
[else]
.( [set %sdl-quiet flag to 0 to view these messages])cr
[then]

